using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterCollider : MonoBehaviour
{
    public abstract int Trace(
        Vector3 position,
        Vector3 direction,
        float traceDistance,
        Quaternion orientation,
        LayerMask validHitMask,
        QueryTriggerInteraction interactionType,
        RaycastHit[] tmpBuffer);

    public static void FindClosestFilterInvalids(ref int tracedColliderCount,
        RaycastHit[] tmpBuffer,
        out int closestIndex,
        Collider self,
        float traceBias = 0F)
    {
        int tmpIndex = tracedColliderCount - 1;
        float closestTrace = Mathf.Infinity;
        closestIndex = -1;
        
        while(tmpIndex >= 0)
        {
            tmpBuffer[tmpIndex].distance -= traceBias;
            RaycastHit tmpHit = tmpBuffer[tmpIndex];
            float traceLength = tmpHit.distance;

            if(traceLength > 0F && tmpHit.collider != self)
            {
                if(traceLength < closestTrace)
                {
                    closestIndex = tmpIndex;
                    closestTrace = traceLength;
                }
            }
            else
            {
                if(tmpIndex < --tracedColliderCount)
                {
                    tmpBuffer[tmpIndex] = tmpBuffer[tracedColliderCount];
                }
            }

            tmpIndex--;
        }
    }

    public abstract Collider Collider();
}
