using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using static PickUpMod.PickUpMod.MyEasyNetworkManager;

namespace PickUpMod.PickUpMod
{
    class MyNetworkHandler : IDisposable
    {

        public MyEasyNetworkManager MyNetwork;
        public static MyNetworkHandler Static;

        public static void Init()
        {
            if (Static == null)
            {
                Static = new MyNetworkHandler();
            }
        }

        protected MyNetworkHandler()
        {
            MyNetwork = new MyEasyNetworkManager(52547);
            MyNetwork.Register();

            MyNetwork.OnRecievedPacket += PacketIn;
        }

        private void PacketIn(PacketIn e)
        {
            if (e.PacketId == 1)
            {
                if (MyAPIGateway.Multiplayer.IsServer)
                {
                    PacketPickUp packet = e.UnWrap<PacketPickUp>();
                    MyEntity grid;
                    if (MyEntities.TryGetEntityById(packet.GridId, out grid))
                    {
                        MyEntity character;
                        if (MyEntities.TryGetEntityById(packet.characterID, out character))
                        {
                            if (!grid.Closed && grid.Physics != null)
                            {
                                MatrixD m = grid.WorldMatrix;

                                Vector3 transformedOff = Vector3.Transform(packet.Translation, grid.WorldMatrix) - m.Translation;
                                Vector3 currentPos = m.Translation + transformedOff;

                                if ((packet.DesiredPos - currentPos).LengthSquared() > 50)
                                {
                                    return;
                                }

                                Utils.AddForceTowards(grid, currentPos, packet.DesiredPos, packet.Foward);
                                grid.Physics.AngularVelocity = packet.Rotation;

                                if (packet.IsThrow)
                                {
                                    Vector3 linearVelosity = grid.Physics.LinearVelocity;
                                    Vector3 toApply = character.Physics.GetWorldMatrix().GetOrientation().Forward;

                                    toApply.Multiply(5f);

                                    linearVelosity.Add(toApply);

                                    grid.Physics.SetSpeeds(linearVelosity, grid.Physics.AngularVelocity);
                                }

                            }
                        }
                            
                    }
                }
            }
        }

        public void Dispose()
        {
            MyNetwork.UnRegister();
            MyNetwork = null;
            Static = null;
        }
    }
}
