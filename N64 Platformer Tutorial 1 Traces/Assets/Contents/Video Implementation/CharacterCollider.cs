using UnityEngine;

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
    public abstract int Trace(
        Vector3 position,
        Vector3 direction,
        float traceDistance,
        Quaternion orientation,
        LayerMask validHitMask,
        QueryTriggerInteraction interactionType,
        RaycastHit[] tmpBuffer);

    /// <summary>
    /// FindClosestFilterInvalids will return the index of the closest RaycastHit during the physics query. <br/>
    /// This method will also filter out any hits that you deem invalid.
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

            // Valid hit branch:
            if (traceLength > 0F && tmpHit.collider != self)
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
    /// The blueprint method that your implementation must override, as it returns access to the character's physics collider.
    /// </summary>
    /// <returns></returns>
    public abstract Collider Collider(); // reference accessor to use in Filter comparisons
}
