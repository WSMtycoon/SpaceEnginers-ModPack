using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Utils;
using VRageMath;

namespace PickUpMod.PickUpMod
{
    static class Utils
    {
        public static void AddForceTowards(MyEntity held, Vector3 initialPosition, Vector3 destinationPosition, Vector3 Foward)
        {
            Vector3 Pt0 = initialPosition;
            Vector3 Vt0 = held.Physics.LinearVelocity;
            Vector3 F = (destinationPosition - Pt0) * 5000 + (Vector3.Zero - Vt0) * 500;
            held.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, F, held.PositionComp.GetPosition(), null);

            Vector3 x = Vector3.Cross(Foward, held.WorldMatrix.Forward);
            float theta = (float)Math.Asin(x.Length());
            x.Normalize();
            Vector3 w = x * theta;

            w.Normalize();
            w *= .1f;
            if (w.IsValid())
            {
                held.Physics.AngularVelocity = w;
            }

        }

    }
}
