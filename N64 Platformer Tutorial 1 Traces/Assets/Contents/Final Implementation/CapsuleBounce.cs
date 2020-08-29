using UnityEngine;

namespace com.chs.final
{
    /// <summary>
    /// 1. A simple collision response implementation in which any recorded hit will invert the object's clip velocity. <br/> 
    /// 2. This is not a thoroughly thought out script, so I suggest you implement your own when you have the knowledge/ability to. <br/>
    /// <br/> 3. CapsuleBounce script is tied to the CharacterCapsuleCollider, so why not try and implement a Bounce response that treats each CharacterCollider the same?
    /// </summary>
    [RequireComponent(typeof(CharacterCapsuleCollider))]
    public class CapsuleBounce : MonoBehaviour
    {
        /// <summary>
        /// Our write buffer used for this bouncer's trace hits
        /// </summary>
        readonly RaycastHit[] internalHits = new RaycastHit[15];

        /// <summary>
        /// The gravitational acceleration
        /// </summary>
        readonly float GRAVITY = 9.81F;

        /// <summary>
        /// The minimum squared magnitude required for the velocity vector to simulate movement
        /// </summary>
        readonly float MinimumVelocityThreshold = 0.01F;

        [Header("Bounce Simulation Settings")]
        /// <summary>
        /// The CharacterCapsuleCollider used as a front for physics querying without cluttering collision response.
        /// scripts.
        /// </summary>
        [SerializeField] CharacterCapsuleCollider CharacterCol;

        /// <summary>
        /// The layermask we'll apply to our physics queries to filter out entire layers of colliders that we do not want to hit.
        /// </summary>
        [SerializeField] LayerMask validLayers;

        /// <summary>
        /// The gravitational factor we'll be able to modify during runtime to affect our gravity calculations
        /// </summary>
        [SerializeField] float gScale = 1.0F;

        /// <summary>
        /// The initial position that is handled by our collision response simulation
        /// </summary>
        [System.NonSerialized] Vector3 initialPosition;

        /// <summary>
        /// The internal position that is handled by our collision response simulation
        /// </summary>
        [System.NonSerialized] Vector3 internalPosition;
        /// <summary>
        /// The internal velocity that is handled by our collision response simulation
        /// </summary>
        [System.NonSerialized] Vector3 internalVelocity;

        /// <summary>
        /// The initial velocity found during the beginning of the simulation
        /// </summary>
        [SerializeField] Vector3 initialVelocity;

        /// <summary>
        /// Start() has been moved to SimulationBounceManager() but Initialize() does the same thing
        /// </summary>
        public void Initialize()
        {
            initialPosition = internalPosition = transform.position;
            internalVelocity = initialVelocity;
        }

        /// <summary>
        /// ResetBouncer() will reset the capsule's relevant values to the starting simulation values
        /// </summary>
        public void ResetBouncer()
        {
            transform.position = internalPosition = initialPosition;
            internalVelocity = initialVelocity;
        }

        /// <summary>
        /// FixedUpdate() but moved into SimulationBounceManager() to handle CapsuleBounce simulation in a centralized class
        /// </summary>
        /// <param name="fdt"></param>
        public void Simulate(float fdt)
        {
            internalVelocity -= Vector3.up * (GRAVITY * gScale) * fdt;

            if (internalVelocity.sqrMagnitude > MinimumVelocityThreshold)
                BounceMove(ref internalVelocity, fdt);

            transform.SetPositionAndRotation(internalPosition, transform.rotation);
        }

        /// <summary>
        /// Continuous Collision Response method that reflect's a character's velocity off
        /// of any detected surfaces.
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="fdt"></param>
        void BounceMove(ref Vector3 velocity, float fdt)
        {
            if (fdt <= 0F) // exit function if there is not a given timestep
                return;

            float remainingTraceLength = velocity.magnitude * fdt; // distance to continue traversing during the trace step phase
            Vector3 nextTraceDirection = velocity.normalized; // direction to cast during the trace step phase
            Vector3 naiveTracePosition = internalPosition; // position to cast and move from during the trace step phase

            int remainingBounceSteps = 5; // 1. maximum number of steps
                                          /* 2. Trace iterations will rarely ever go past two iterations
                                           * 3. However, if the bouncer is 'stuck' between a crease or corner, the movement behaviour is ambiguous.
                                           * 4. creases and corner resolution have not been accounted for due to simplicity for this script.
                                           */

            for (; remainingBounceSteps-- >= 0;)
            {
                if (remainingTraceLength <= 0F) // exit out of loop if no distance is left to travel to save execution time
                    break;

                //Trace Capsule
                int nbTracedColliders = CharacterCol.Trace(
                position: naiveTracePosition,
                direction: nextTraceDirection,
                traceDistance: remainingTraceLength + 0.01F,
                orientation: transform.rotation,
                validHitMask: validLayers,
                interactionType: QueryTriggerInteraction.Ignore,
                tmpBuffer: internalHits);

                //Filter Hits
                CharacterCollider.FindClosest(
                    tracedColliderCount: ref nbTracedColliders,
                    tmpBuffer: internalHits,
                    closestIndex: out int closestHitIndex,
                    self: CharacterCol.Collider(),
                    traceBias: CharacterCapsuleCollider.TRACEBIAS);

                //If hit anything
                if (closestHitIndex >= 0)
                {
                    //cache hit to avoid reading wrong data if trace occurs somewhere else in code
                    RaycastHit closestHit = internalHits[closestHitIndex];
                    Vector3 traceMove = nextTraceDirection * (closestHit.distance - 0.01F);

                    // advance position to trace distance
                    naiveTracePosition += traceMove;

                    // account for loss of magnitude for next step
                    remainingTraceLength -= traceMove.magnitude;

                    // check to push other bouncers if they've been hit:
                    TryPushBouncer(velocity, closestHit.normal, closestHit.collider);

                    // reflect velocity along hit plane
                    BounceVelocity(ref velocity, ref nextTraceDirection, ref remainingTraceLength, closestHit.normal);
                }
                else // if we've hit nothing
                {
                    // advance position along remaining displacement vector
                    naiveTracePosition += nextTraceDirection * remainingTraceLength;
                    break;
                }
            }

            // finally, set our internalPosition to our last computed step position
            internalPosition = naiveTracePosition;
        }

        /// <summary>
        /// If a collision occurs with another bouncer, just send the velocity projected along the other bouncer's hit normal
        /// to the other Bouncer
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="normal"></param>
        /// <param name="collider"></param>
        void TryPushBouncer(Vector3 velocity, Vector3 normal, Collider collider)
        {
            CapsuleBounce other = collider.GetComponent<CapsuleBounce>();

            if (other != null) // if hit contains CapsuleBounce, it is a Bouncer
            {
                other.PushSelf(Vector3.Project(velocity, normal) * 0.9F);
            }
        }

        /// <summary>
        /// Apply an impulse velocity to the Bouncer
        /// </summary>
        /// <param name="impulseVelocity"></param>
        void PushSelf(Vector3 impulseVelocity)
        {
            internalVelocity += impulseVelocity;
        }

        /// <summary>
        /// Simply reflects velocity off of a plane given its normal while also 
        /// simulating friction.
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="nextTraceDirection"></param>
        /// <param name="remainingTraceLength"></param>
        /// <param name="normal"></param>
        void BounceVelocity(ref Vector3 velocity, ref Vector3 nextTraceDirection, ref float remainingTraceLength, Vector3 hitNormal)
        {
            // grab our initial magnitude for later
            float initVelocity = velocity.magnitude;

            // grab component of velocity clipping into the negative normal
            float inwardComponent = -Vector3.Dot(velocity, hitNormal);

            // apply this component by a factor of two to 'invert the sign' of the normal component
            velocity += 2 * inwardComponent * hitNormal;

            //We can fake friction by chopping a percentage of magnitude every frame we've hit something
            velocity -= velocity * 0.175F; // taking away % of our current velocity			

            // multiply our remaining trace length by the ratio of our current velocity over the initVelocity
            remainingTraceLength *= velocity.magnitude / initVelocity;

            //NaN check because I was running into errors earlier, I'm assuming it had to do with my dot product and floating point inaccuracies.
            //It was only occuring because I was doing a calculation wrong, but its still healthy to check regardless.
            if (float.IsNaN(remainingTraceLength))
                remainingTraceLength = 0F;

            // apply the unit direction to be the velocity's unit vector
            nextTraceDirection = velocity.normalized;
        }
    }

}

