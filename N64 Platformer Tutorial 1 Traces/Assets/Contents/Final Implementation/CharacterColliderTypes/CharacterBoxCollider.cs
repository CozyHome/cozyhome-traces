using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.chs.final
{
    public class CharacterBoxCollider : CharacterCollider
    {
        /* For the sake of not going insane, I decided to focus heavy documentation on the Capsule implementation for the time being. 
           Please check that out if you're interested in seeing commented code. */

        [SerializeField] BoxCollider Box_C;

        public const float TRACEBIAS = 0.001F;

        public override int Trace(Vector3 position, Vector3 direction, float traceDistance, Quaternion orientation, LayerMask validHitMask, QueryTriggerInteraction interactionType, RaycastHit[] tmpBuffer)
        {
            position += orientation * Box_C.center;
            position -= direction * TRACEBIAS;

            //feel free to cache these values instead of computing every trace.
            Vector3 scaledExtents = Box_C.size / 2F;
            for (int i = 0; i < 3; i++)
                scaledExtents[i] *= transform.localScale[i];

            int tracedCollidersCount = Physics.BoxCastNonAlloc
            (position,
            scaledExtents,
            direction,
            tmpBuffer,
            orientation,
            traceDistance + TRACEBIAS,
            validHitMask,
            interactionType);

            return tracedCollidersCount;
        }

        public override Collider Collider()
        {
            return Box_C;
        }
    }

}