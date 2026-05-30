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
        public static void AddForceTowards(MyEntity held, Vector3 initialPosition, Vector3 destinationPosition, Vector3 Foward, bool preserveAngularVelocity = false)
        {
            if (held?.Physics == null) return;

            Vector3 positionError = destinationPosition - initialPosition;
            float distance = positionError.Length();
            
            // Ограничиваем максимальную силу
            float maxForce = 20000f;
			float forceMultiplier = Math.Min(maxForce, distance * 5000f);
            
            // Применяем силу пропорционально расстоянию
            float distanceFactor = Math.Min(1.0f, distance * 0.5f);
            
            
            // Демпфирование скорости
            Vector3 velocityDamping = held.Physics.LinearVelocity * 800f;
            
            // Итоговая сила
            Vector3 F = positionError * forceMultiplier - velocityDamping;

			 // Ограничиваем силу
			float forceLength = F.Length();
            if (forceLength > maxForce)
            {
                F = F / forceLength * maxForce;
            }
            
             
            // Применяем силу к центру масс объекта
			//held.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, F, held.PositionComp.GetPosition(), null);
            held.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, F, held.Physics.CenterOfMassWorld, null);

            // Устанавливаем AngularVelocity только если не нужно сохранять текущее вращение
            if (!preserveAngularVelocity)
            {
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
}
