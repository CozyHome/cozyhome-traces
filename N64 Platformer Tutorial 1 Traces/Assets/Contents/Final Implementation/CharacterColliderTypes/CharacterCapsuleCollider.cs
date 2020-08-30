using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.chs.final
{
    public class CharacterCapsuleCollider : CharacterCollider
    {
        /// <summary>
        /// The CapsuleCollider object we'll be using to store information relevant to the
        /// physics system
        /// </summary>
        [SerializeField] CapsuleCollider Capsule_C;

        /// <summary>
        /// A Trace offset constant used to aid Unity's 
        /// physics query in catching trace hits 
        /// directly infront of the collider
        /// </summary>
        public const float TRACEBIAS = 0.001F;

        /// <summary>
        /// 1. The written implementation of our CharacterCollider blueprint method. <br/> 
        /// 2. Trace() casts the CapsuleCollider across the Physics worldspace and writes to the provided hit buffer in AMBIGUOUS order. <br/>
        /// 3. This method returns the amount of hits found but does not sort them.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="traceDistance"></param>
        /// <param name="orientation"></param>
        /// <param name="validHitMask"></param>
        /// <param name="interactionType"></param>
        /// <param name="tmpBuffer"></param>
        /// <returns></returns>
        public override int Trace(Vector3 position,
        Vector3 direction,
        float traceDistance,
        Quaternion orientation,
        LayerMask validHitMask,
        QueryTriggerInteraction interactionType,
        RaycastHit[] tmpBuffer)
        {
            position += orientation * Capsule_C.center;
            position -= direction * TRACEBIAS;

            // i havent worked out solving capsule scaling, so sorry about that :(
            // some day i'll get around to it

            Vector3 capsuleSegmentLength = (orientation * Vector3.up) * (Capsule_C.height * 0.5F - Capsule_C.radius);

            int tracedCollidersCount = Physics.CapsuleCastNonAlloc
            (position + capsuleSegmentLength,
            position - capsuleSegmentLength,
            Capsule_C.radius,
            direction,
            tmpBuffer,
            traceDistance + TRACEBIAS,
            validHitMask,
            interactionType);

            return tracedCollidersCount;
        }

        /// <summary>
        /// The implementation of the blueprint method that returns a reference to our CapsuleCollider component
        /// </summary>
        /// <returns></returns>
        public override Collider Collider()
        {
            return Capsule_C;
        }
    }
}
