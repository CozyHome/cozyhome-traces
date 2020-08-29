using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.chs.final
{
    [RequireComponent(typeof(CharacterCapsuleCollider))]
    public class CapsuleBounce : MonoBehaviour
    {
        const float GRAVITY = 9.81F;
        const float MinimumMovementThreshold = 0.01F;
        const float SimulationWaitDelay = 0.5F;

        readonly RaycastHit[] internalHits = new RaycastHit[15];

        [Header("Bounce Simulation Settings")]
        [SerializeField] CharacterCapsuleCollider characterCapsule;
        [SerializeField] LayerMask validLayers;
        [SerializeField] float gScale = 1.0F;
        [SerializeField] Vector3 initialVelocity;

        Vector3 initialPosition;
        Vector3 internalPosition;
        Vector3 internalVelocity;

        public void Initialize()
        {
            initialPosition = internalPosition = transform.position;
            internalVelocity = initialVelocity;
        }

        public void ResetBouncer()
        {
            transform.position = internalPosition = initialPosition;
            internalVelocity = initialVelocity;
        }

        public void Simulate(float fdt)
        {
            if (Time.fixedTime < SimulationWaitDelay) //If your game is slow to begin running, 
                                                      // it may chop up and simulate several time steps to catch up.
                                                      // Just wait a while for Unity to catch up, then run.
                return;

            internalVelocity -= Vector3.up * (GRAVITY * gScale) * fdt;

            if (internalVelocity.sqrMagnitude > MinimumMovementThreshold)
                BounceMove(ref internalVelocity, fdt);

            transform.SetPositionAndRotation(internalPosition, transform.rotation);
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

                //Trace Capsule
                int nbTracedColliders = characterCapsule.Trace(
                position: naiveTracePosition,
                direction: nextTraceDirection,
                traceDistance: remainingTraceLength + 0.01F,
                orientation: transform.rotation,
                validHitMask: validLayers,
                interactionType: QueryTriggerInteraction.Ignore,
                tmpBuffer: internalHits);

                //Filter Hits
                CharacterCollider.FindClosestFilterInvalids(
                    tracedColliderCount: ref nbTracedColliders,
                    tmpBuffer: internalHits,
                    closestIndex: out int closestHitIndex,
                    self: characterCapsule.Collider(),
                    traceBias: CharacterCapsuleCollider.TRACEBIAS);

                //If hit anything
                if (closestHitIndex >= 0)
                {
                    //cache hit to avoid reading wrong data if trace occurs somewhere else in code
                    RaycastHit closestHit = internalHits[closestHitIndex];
                    Vector3 traceMove = nextTraceDirection * (closestHit.distance - 0.01F);
                    remainingTraceLength -= traceMove.magnitude;

                    naiveTracePosition += traceMove;

                    TryPushBouncer(velocity, closestHit.normal, closestHit.collider);
                    BounceVelocity(ref velocity, ref nextTraceDirection, ref remainingTraceLength, closestHit.normal);
                } //If not
                else
                {
                    naiveTracePosition += nextTraceDirection * remainingTraceLength;
                    break;
                }
            }

            //Set position, end method
            internalPosition = naiveTracePosition;
        }

        //if we hit another CapsuleBounce, why not push them? :)
        void TryPushBouncer(Vector3 velocity, Vector3 normal, Collider collider)
        {
            CapsuleBounce other = collider.GetComponent<CapsuleBounce>();

            if (other != null)
            {
                other.Push(Vector3.Project(velocity, normal) * 0.9F);
            }
        }

        void Push(Vector3 impulseVelocity)
        {
            internalVelocity += impulseVelocity;
        }

        void BounceVelocity(ref Vector3 velocity, ref Vector3 nextTraceDirection, ref float remainingTraceLength, Vector3 hitNormal)
        {
            float initVelocity = velocity.magnitude;

            float inwardComponent = -Vector3.Dot(velocity, hitNormal);
            velocity += 2 * inwardComponent * hitNormal; // "inverting" the component along the normal

            //We can fake friction by chopping a percentage of magnitude every frame we've hit something
            velocity -= velocity * 0.175F; // taking away % of our current velocity			

            remainingTraceLength *= velocity.magnitude / initVelocity;

            //NaN check because I was running into errors earlier, I'm assuming it had to do with my dot product and floating point inaccuracies.
            //It was only occuring because I was doing a calculation wrong, but its still healthy to check regardless.
            if (float.IsNaN(remainingTraceLength))
                remainingTraceLength = 0F;

            nextTraceDirection = velocity.normalized;
        }
    }

}

