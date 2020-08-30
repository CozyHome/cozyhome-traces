using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.chs.final
{
    /* For the sake of not going insane, I decided to focus heavy documentation on the Capsule implementation for the time being. 
       Please check that out if you're interested in seeing commented code. */

    public class CharacterSphereCollider : CharacterCollider
    {
        [SerializeField] SphereCollider Sphere_C;

        public const float TRACEBIAS = 0.001F;

        public override int Trace(Vector3 position, Vector3 direction, float traceDistance, Quaternion orientation, LayerMask validHitMask, QueryTriggerInteraction interactionType, RaycastHit[] tmpBuffer)
        {
            position += orientation * Sphere_C.center;
            position -= direction * TRACEBIAS;

            float biggestScale = 0F;
            for (int i = 0; i < 3; i++)
                if (biggestScale > transform.localScale[i])
                    biggestScale = transform.localScale[i];

            int tracedCollidersCount = Physics.SphereCastNonAlloc
            (position,
            Sphere_C.radius * biggestScale,
            direction,
            tmpBuffer,
            traceDistance + TRACEBIAS,
            validHitMask,
            interactionType);

            return tracedCollidersCount;
        }

        public override Collider Collider()
        {
            return Sphere_C;
        }
    }
}
