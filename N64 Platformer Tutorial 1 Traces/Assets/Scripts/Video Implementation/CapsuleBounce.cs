using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterCapsuleCollider))]
public class CapsuleBounce : MonoBehaviour
{
    readonly RaycastHit[] internalHits = new RaycastHit[15];

    Vector3 internalPosition;
    Vector3 internalVelocity;

    [SerializeField] Vector3 initialVelocity;
    [SerializeField] CharacterCapsuleCollider CapsuleCol;
    [SerializeField] LayerMask validLayers;

    const float GRAVITY = 9.81F;
    [SerializeField] float gScale = 1F;

    void Start()
    {
        internalPosition = transform.position;
        internalVelocity = initialVelocity;
    }

    void FixedUpdate()
    {
        float fdt = Time.fixedDeltaTime;

        internalVelocity -= Vector3.up * (gScale * GRAVITY * fdt); //gravity

        if (internalVelocity.sqrMagnitude > 0.01F)
            BounceMove(ref internalVelocity, fdt);

        transform.position = internalPosition;
    }

    void BounceMove(ref Vector3 velocity, float fdt)
    {
        if (fdt <= 0F)
            return;

        float remainingTraceLength = velocity.magnitude * fdt;
        Vector3 nextTraceDirection = velocity.normalized;
        Vector3 naiveTracePosition = internalPosition;

        int remainingBounceSteps = 5;

        for (; remainingBounceSteps-- >= 0;)
        {
            if (remainingTraceLength <= 0F)
                break;

            int nbTracedColliders = CapsuleCol.Trace(
                naiveTracePosition,
                nextTraceDirection,
                remainingTraceLength + 0.01F,
                transform.rotation,
                validLayers,
                QueryTriggerInteraction.Ignore,
                internalHits);

            CharacterCollider.FindClosestFilterInvalids(ref nbTracedColliders,
                internalHits,
                out int closestIndex,
                CapsuleCol.Collider(),
                CharacterCapsuleCollider.TRACEBIAS);

            //we've hit something
            if (closestIndex >= 0)
            {
                RaycastHit closestHit = internalHits[closestIndex];

                internalHits[closestIndex].distance = -1F;

                Vector3 traceMove = nextTraceDirection * (closestHit.distance - 0.01F);

                naiveTracePosition += traceMove;

                remainingTraceLength -= traceMove.magnitude;

                BounceVelocity(ref velocity, ref nextTraceDirection, ref remainingTraceLength, closestHit.normal);
            }
            else // we haven't hit anything
            {
                naiveTracePosition += nextTraceDirection * remainingTraceLength;
                remainingTraceLength = 0F;
            }
        }

        internalPosition = naiveTracePosition;
    }

    void BounceVelocity(ref Vector3 velocity, ref Vector3 nextTraceDirection, ref float remainingTraceLength, Vector3 normal)
    {
        float initVelocity = velocity.magnitude;
        float inwardComponent = Vector3.Dot(velocity, -normal);

        velocity += 2 * inwardComponent * normal;

        velocity -= velocity * 0.05F;

        remainingTraceLength *= velocity.magnitude / initVelocity;
        nextTraceDirection = velocity.normalized;
    }

    interface IBouncerCallback
    {
        void OnSurfaceHit(ref Vector3 velocity);
    }
}
