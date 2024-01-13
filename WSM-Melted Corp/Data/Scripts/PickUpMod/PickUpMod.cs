using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace PickUpMod.PickUpMod
{

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class PickUpMod : MySessionComponentBase
    {
        public const int MAX_OBJECT_MASS = 20000;

        private IMyCubeGrid held;
        private IMyCubeGrid heldToView;
        private Vector3 hitPos;
        private Vector3 gridForward;
        private float distance;
        private int delay;
        private int delay2;
        private float oldscroll = 0f;
        private float scroll = 0f;
		private float angel = 0.02f;
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            MyNetworkHandler.Init();
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Utilities.IsDedicated)
            {
                return;
            }

            if (MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null && !MyAPIGateway.Session.IsCameraUserControlledSpectator &&
                !MyAPIGateway.Gui.IsCursorVisible && MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None &&
                MyAPIGateway.Session.Player?.Controller?.ControlledEntity?.Entity is IMyCharacter)
            {
                IMyCharacter character = MyAPIGateway.Session.Player?.Controller?.ControlledEntity?.Entity as IMyCharacter;

                if (character.EquippedTool != null)
                {
                    return;
                }

                if (delay != 0)
                {
                    delay--;
                    return;
                }

                MatrixD mat = MyAPIGateway.Session.Player.Character.GetHeadMatrix(true);
                if (MyAPIGateway.Input.IsNewGameControlPressed(MyControlsSpace.USE))
                {
                    if (held != null)
                    {
                        MyVisualScriptLogicProvider.SetHighlightLocal(held.Name, 0, 0, Color.Yellow * .1f);
                        held = null;
                        heldToView = null;
                    }
                    else
                    {
                        IHitInfo hit;

                        MyAPIGateway.Physics.CastRay(mat.Translation, mat.Translation + mat.Forward * 4, out hit);
                        if (hit != null && hit.HitEntity != null && hit.HitEntity is IMyCubeGrid)
                        {
                            IMyCubeGrid grid = hit.HitEntity as IMyCubeGrid;
                            float mass;
                            MyAPIGateway.Physics.CalculateNaturalGravityAt(grid.PositionComp.GetPosition(), out mass);
                            mass = Math.Max(1, mass);
                            mass = (float)(mass * 9.8) * (grid as MyCubeGrid).GetCurrentMass();
                            if (grid.GridSizeEnum == MyCubeSize.Small && !grid.IsStatic && mass <= MAX_OBJECT_MASS && mass < MAX_OBJECT_MASS * 2)
                            {

                                IMySlimBlock block = grid.GetCubeBlock(grid.WorldToGridInteger(hit.Position + (hit.Normal * .01f)));
                                if (block != null && block.FatBlock != null && block.FatBlock is IMyButtonPanel)
                                {
                                    return;
                                }

                                held = grid;
                                gridForward = grid.WorldMatrix.Backward;
                                hitPos = Vector3.Transform(hit.Position, held.PositionComp.WorldMatrixNormalizedInv);
                                distance = (float)(mat.Translation - hit.Position).Length();
                                MyVisualScriptLogicProvider.SetHighlightLocal(held.Name, 20, 0, Color.Yellow * .1f);
                                MyVisualScriptLogicProvider.PlaySingleSoundAtPosition("PickGrid", (Vector3)MyAPIGateway.Session.Player.GetPosition());
                                //MyVisualScriptLogicProvider.ShowNotification(held.DisplayName, 2000, "Green", MyAPIGateway.Session.Player.IdentityId);
                            }
                        }

                    }

                    if (heldToView != null)
                    {
                        MyVisualScriptLogicProvider.SetHighlightLocal(heldToView.Name, 0, 0, Color.Yellow * .1f);
                        heldToView = null;
                    }
                    else
                    {
                        IHitInfo hit;

                        MyAPIGateway.Physics.CastRay(mat.Translation, mat.Translation + mat.Forward * 4, out hit);
                        if (hit != null && hit.HitEntity != null && hit.HitEntity is IMyCubeGrid)
                        {
                            IMyCubeGrid grid = hit.HitEntity as IMyCubeGrid;

                            float mass;
                            MyAPIGateway.Physics.CalculateNaturalGravityAt(grid.PositionComp.GetPosition(), out mass);
                            mass = Math.Max(1, mass);
                            mass = (float)(mass * 9.8) * (grid as MyCubeGrid).GetCurrentMass();
                            if (grid.GridSizeEnum == MyCubeSize.Small && !grid.IsStatic && mass > MAX_OBJECT_MASS && mass < MAX_OBJECT_MASS*2)
                            {
                                IMySlimBlock block = grid.GetCubeBlock(grid.WorldToGridInteger(hit.Position + (hit.Normal * .01f)));
                                if (block != null && block.FatBlock != null && block.FatBlock is IMyButtonPanel)
                                {
                                    return;
                                }

                                heldToView = grid;
                                gridForward = grid.WorldMatrix.Backward;
                                hitPos = Vector3.Transform(hit.Position, heldToView.PositionComp.WorldMatrixNormalizedInv);
                                distance = (float)(mat.Translation - hit.Position).Length();
                                MyVisualScriptLogicProvider.SetHighlightLocal(heldToView.Name, 20, 0, Color.Red * .1f);
                                MyVisualScriptLogicProvider.ShowNotification("I can't move this!", 2000, "Red", MyAPIGateway.Session.Player.IdentityId);
                                MyVisualScriptLogicProvider.PlaySingleSoundAtPosition("ArcHudItem", (Vector3)MyAPIGateway.Session.Player.GetPosition());
                            }
                        }

                    }
                }


            if (held != null && !held.Closed && held.Physics != null)
                {
                    MyEntity heldEnt = held as MyEntity;

                    double dist = Vector3D.Distance(character.GetPosition(), heldEnt.PositionComp.GetPosition());
                    if (dist<=1)
                    {
                        MyVisualScriptLogicProvider.SetHighlightLocal(held.Name, 0, 0, Color.Yellow * .1f);
                        held = null;
                        return;
                    }

                    MatrixD m = held.WorldMatrix;

                    Vector3 transformedOff = Vector3.Transform(hitPos, held.WorldMatrix) - m.Translation;
                    Vector3 desiredPos = mat.Translation + (mat.Forward * distance);
                    Vector3 currentPos = m.Translation + transformedOff;

                   // Utils.DrawDebugLineDirect(desiredPos, currentPos, 0, 0, 0);

                    if ((desiredPos - currentPos).LengthSquared() > 50)
                    {
                        MyVisualScriptLogicProvider.SetHighlightLocal(held.Name, 0, 0, Color.Yellow * .1f);
                        held = null;
                        return;
                    }

                    Utils.AddForceTowards(heldEnt, currentPos, desiredPos, gridForward);



                    if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.Insert)){
                        scroll = scroll + angel;
                        heldEnt.Physics.AngularVelocity = new Vector3(scroll, 0, 0);
                    }
                    else if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.PageUp)){
                        scroll = scroll - angel;
                        heldEnt.Physics.AngularVelocity = new Vector3(scroll, 0, 0);
                    }

                    else if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.Delete)){
                        scroll = scroll + angel;
                        heldEnt.Physics.AngularVelocity = new Vector3(0, 0, scroll);
                    }
                    else if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.PageDown)){
                        scroll = scroll - angel;
                        heldEnt.Physics.AngularVelocity = new Vector3(0, 0, scroll);
                    }

					else if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.End)){
                        scroll = scroll + angel;
                        heldEnt.Physics.AngularVelocity = new Vector3(0, scroll, 0);
                    }
                    else if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.Home)){
                        scroll = scroll - angel;
                        heldEnt.Physics.AngularVelocity = new Vector3(0, scroll, 0);
                    }

                    else
                    {
                       this.scroll = ((MyAPIGateway.Input.DeltaMouseScrollWheelValue() / 100) % 50)*6;
                        if (scroll > 10)
                        {
                            scroll = 10;
                        }
                        if (scroll < -10)
                        {
                            scroll = -10;
                        }
                       /*if (oldscroll != scroll)
                        {
                            heldEnt.Physics.AngularVelocity = new Vector3(0, scroll, 0);
                        }
                        else
                        {
                            scroll = 0;
                            heldEnt.Physics.AngularVelocity = new Vector3(0, scroll, 0);
                        }*/
                        oldscroll = scroll;
                    }

                    if (MyAPIGateway.Input.IsMousePressed(VRage.Input.MyMouseButtonsEnum.Left))
                    {
                        Vector3 linearVelosity = heldEnt.Physics.LinearVelocity;
                        Vector3 toApply = character.Physics.GetWorldMatrix().GetOrientation().Forward;

                        toApply.Multiply(5f);

                        linearVelosity.Add(toApply);

                        heldEnt.Physics.SetSpeeds(linearVelosity, heldEnt.Physics.AngularVelocity);
                    }

                    MyNetworkHandler.Static.MyNetwork.TransmitToServer(new PacketPickUp(held.EntityId, gridForward, desiredPos, hitPos, heldEnt.Physics.AngularVelocity, MyAPIGateway.Input.IsMousePressed(VRage.Input.MyMouseButtonsEnum.Left), character), false, false);
                    if (MyAPIGateway.Input.IsMousePressed(VRage.Input.MyMouseButtonsEnum.Left))
                    {
                        MyVisualScriptLogicProvider.SetHighlightLocal(held.Name, 0, 0, Color.Yellow * .1f);
                        held = null;
                        return;
                    }
                }
                if (heldToView != null && !heldToView.Closed && heldToView.Physics != null)
                {
                    delay2++;
                    if (delay2>=150)
                    {
                        if (heldToView != null)
                        {
                            MyVisualScriptLogicProvider.SetHighlightLocal(heldToView.Name, 0, 0, Color.Yellow * .1f);
                        }
                        heldToView = null;
                        delay2 = 0;
                    }
                }
            }
            else
            {
                if (held != null)
                {
                    MyVisualScriptLogicProvider.SetHighlightLocal(held.Name, 0, 0, Color.Yellow * .1f);
                }
                if (heldToView != null)
                {
                    MyVisualScriptLogicProvider.SetHighlightLocal(heldToView.Name, 0, 0, Color.Yellow * .1f);
                }
                held = null;
                heldToView = null;
                delay = 30;
                scroll = 0;
            }
        }
    }
}
