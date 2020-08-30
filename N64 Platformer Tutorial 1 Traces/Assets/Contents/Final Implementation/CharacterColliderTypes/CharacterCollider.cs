using UnityEngine;

namespace com.chs.final
{
    /// <summary>
    /// A custom MonoBehaviour script that combines the Collider component system with tracing and filter methods
    /// to easily allow for implmementation of casting primitives in the physics worldspace.
    /// </summary>
    public abstract class CharacterCollider : MonoBehaviour
    {
        /// <summary>
        /// CharacterCollider.Trace is a blueprint method designed to be overridden by your own custom CharacterCollider implementations.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="traceDistance"></param>
        /// <param name="orientation"></param>
        /// <param name="validHitMask"></param>
        /// <param name="interactionType"></param>
        /// <param name="tmpBuffer"></param>
        /// <returns></returns>
        public abstract int Trace(Vector3 position,
            Vector3 direction,
            float traceDistance,
            Quaternion orientation,
            LayerMask validHitMask,
            QueryTriggerInteraction interactionType,
            RaycastHit[] tmpBuffer);

        /// <summary>
        /// FindClosestFilterInvalids() will return the index of the closest RaycastHit during the physics query. <br/>
        /// This method can also filter out any hits that you deem invalid.
        /// </summary>
        /// <param name="tracedColliderCount"></param>
        /// <param name="tmpBuffer"></param>
        /// <param name="closestIndex"></param>
        /// <param name="self"></param>
        /// <param name="traceBias"></param>
        public static void FindClosestFilterInvalids(ref int tracedColliderCount,
        RaycastHit[] tmpBuffer,
        out int closestIndex,
        Collider self,
        float traceBias = 0F)
        {
            int tmpIndex = tracedColliderCount - 1; // start our array accessor at the last element 
            float closestTrace = Mathf.Infinity; // cache a closestTrace distance float to use obtain the closest index
            closestIndex = -1; // assume negative one to signify nothing was hit

            while (tmpIndex >= 0)
            {
                // Subtract our trace bias to not incorrectly report hit distance:
                tmpBuffer[tmpIndex].distance -= traceBias;

                // Cache a temporary hit reference to avoid excessive array access:
                RaycastHit tmpHit = tmpBuffer[tmpIndex];
                float traceLength = tmpHit.distance;

                bool customValidHitCheck = true;

                // Valid hit branch:
                if (traceLength > 0F && // if trace length is not negative (hit is further than us along direction line)
                    tmpHit.collider != self && // if trace hit is not us
                    customValidHitCheck) // override the filter method and boolean to allow for custom checks (be creative :> )
                {
                    if (traceLength < closestTrace)
                    {
                        closestIndex = tmpIndex;
                        closestTrace = traceLength;
                    }
                }
                else // Invalid hit branch:
                {
                    if (tmpIndex < --tracedColliderCount)
                    {
                        tmpBuffer[tmpIndex] = tmpBuffer[tracedColliderCount];
                    }
                }

                tmpIndex--; // decrement (make sure not to remove this, you'll end up causing your unity to crash)
            }
        }

        /// <summary>
        /// FindClosest() will return the index of the closest RaycastHit during the physics query.
        /// </summary>
        /// <param name="tracedColliderCount"></param>
        /// <param name="tmpBuffer"></param>
        /// <param name="closestIndex"></param>
        /// <param name="self"></param>
        /// <param name="traceBias"></param>
        public static void FindClosest(ref int tracedColliderCount,
            RaycastHit[] tmpBuffer,
            out int closestIndex,
            Collider self,
            float traceBias = 0F)
        {
            int tmpIndex = tracedColliderCount - 1;
            float closestTrace = Mathf.Infinity;
            closestIndex = -1;

            while (tmpIndex >= 0)
            {
                tmpBuffer[tmpIndex].distance -= traceBias;
                RaycastHit tmpHit = tmpBuffer[tmpIndex];
                float traceLength = tmpHit.distance;

                if (traceLength > 0F && tmpHit.collider != self)
                {
                    if (traceLength < closestTrace)
                    {
                        closestIndex = tmpIndex;
                        closestTrace = traceLength;
                    }
                }
                else
                    tracedColliderCount--;

                tmpIndex--;
            }
        }

        /// <summary>
        /// FindFurthestFilterInvalids() will return the index of the furthest RaycastHit during the physics query. <br/>
        /// This method can also filter out any hits that you deem invalid.
        /// </summary>
        /// <param name="tracedColliderCount"></param>
        /// <param name="tmpBuffer"></param>
        /// <param name="closestIndex"></param>
        /// <param name="self"></param>
        /// <param name="traceBias"></param>
        public static void FindFurthestFilterInvalids(ref int tracedColliderCount,
        RaycastHit[] tmpBuffer,
        out int furthestIndex,
        Collider self,
        float traceBias = 0F)
        {
            int tmpIndex = tracedColliderCount - 1;
            float furthestTrace = 0F;
            furthestIndex = -1;

            while (tmpIndex >= 0)
            {
                tmpBuffer[tmpIndex].distance -= traceBias;
                RaycastHit tmpHit = tmpBuffer[tmpIndex];
                float traceLength = tmpHit.distance;

                if (traceLength > 0F && tmpHit.collider != self)
                {
                    if (traceLength > furthestTrace)
                    {
                        furthestIndex = tmpIndex;
                        furthestTrace = traceLength;
                    }
                }
                else
                {
                    if (tmpIndex < --tracedColliderCount)
                    {
                        tmpBuffer[tmpIndex] = tmpBuffer[tracedColliderCount];
                    }
                }

                tmpIndex--;
            }
        }

        /// <summary>
        /// FindFurthest() will return the index of the furthest RaycastHit during the physics query.
        /// </summary>
        /// <param name="tracedColliderCount"></param>
        /// <param name="tmpBuffer"></param>
        /// <param name="closestIndex"></param>
        /// <param name="self"></param>
        /// <param name="traceBias"></param>
        public static void FindFurthest(ref int tracedColliderCount,
        RaycastHit[] tmpBuffer,
        out int furthestIndex,
        Collider self,
        float traceBias = 0F)
        {
            int tmpIndex = tracedColliderCount - 1;
            float furthestTrace = 0F;
            furthestIndex = -1;

            while (tmpIndex >= 0)
            {
                tmpBuffer[tmpIndex].distance -= traceBias;
                RaycastHit tmpHit = tmpBuffer[tmpIndex];
                float traceLength = tmpHit.distance;

                if (traceLength > 0F && tmpHit.collider != self)
                {
                    if (traceLength > furthestTrace)
                    {
                        furthestIndex = tmpIndex;
                        furthestTrace = traceLength;
                    }
                }
                else
                    tracedColliderCount--;

                tmpIndex--;
            }
        }

        /// <summary>
        /// The blueprint method that your implementation must override, as it returns access to the character's physics collider.
        /// </summary>
        /// <returns></returns>
        public abstract Collider Collider(); // reference accessor to use in Filter comparisons

    }

}

