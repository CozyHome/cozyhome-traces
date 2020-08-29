using UnityEngine;

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
    readonly RaycastHit[] internalHits = new RaycastHit[15]; // the buffer we'll input to our traces

    /// <summary>
    /// The gravitational acceleration
    /// </summary>
    readonly float GRAVITY = 9.81F;

    /// <summary>
    /// The internal position that is handled by our collision response simulation
    /// </summary>
    Vector3 internalPosition;
    /// <summary>
    /// The internal velocity that is handled by our collision response simulation
    /// </summary>
    Vector3 internalVelocity;

    /// <summary>
    /// The initial velocity found during the beginning of the simulation
    /// </summary>
    [SerializeField] Vector3 initialVelocity;

    /// <summary>
    /// The CharacterCapsuleCollider used as a front for physics querying without cluttering collision response.
    /// scripts.
    /// </summary>
    [SerializeField] CharacterCapsuleCollider CapsuleCol;

    /// <summary>
    /// The layermask we'll apply to our physics queries to filter out entire layers of colliders that we do not want to hit.
    /// </summary>
    [SerializeField] LayerMask validLayers;

    /// <summary>
    /// The gravitational factor we'll be able to modify during runtime to affect our gravity calculations
    /// </summary>
    [SerializeField] float gScale = 1F;

    void Start()
    {
        internalPosition = transform.position;
        internalVelocity = initialVelocity;
    }

    /// <summary>
    /// Use FixedUpdate() whenever you're running physics calculations for your scripts
    /// </summary>
    void FixedUpdate()
    {
        float fdt = Time.fixedDeltaTime;

        internalVelocity -= Vector3.up * (gScale * GRAVITY * fdt); //gravity

        if (internalVelocity.sqrMagnitude > 0.01F)
            BounceMove(ref internalVelocity, fdt);

        transform.position = internalPosition;
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
                                       * 3. However, if the bouncer is 'stuck' between a crease or corner, behaviour is ambiguous.
                                       * 4. creases and corner resolution have not been accounted for due to simplicity for this script.
                                       */

        for (; remainingBounceSteps-- >= 0;)
        {
            if (remainingTraceLength <= 0F) // exit out of loop if no distance is left to travel to save execution time
                break;

            int nbTracedColliders = CapsuleCol.Trace(
                naiveTracePosition, //-> position
                nextTraceDirection, //-> direction
                remainingTraceLength + 0.01F, //-> magnitude + floating point offset
                transform.rotation, //-> changing orientation has not been accounted for so just assume transform's rotation
                validLayers, //-> layermask
                QueryTriggerInteraction.Ignore, //-> ignore triggers
                internalHits); //-> tmp buffer

            //filter out invalid hits and find the closest RaycastHit struct
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

                // advance position to trace distance
                naiveTracePosition += traceMove;

                // account for loss of magnitude for next step
                remainingTraceLength -= traceMove.magnitude;

                // bounce velocity and write to trace vector's magnitude and direction
                BounceVelocity(ref velocity, ref nextTraceDirection, ref remainingTraceLength, closestHit.normal);
            }
            else // we haven't hit anything
            {
                // advance position along remaining displacement vector
                naiveTracePosition += nextTraceDirection * remainingTraceLength;
                remainingTraceLength = 0F;
            }
        }

        // finally, set our internalPosition to our last computed step position
        internalPosition = naiveTracePosition;
    }

    /// <summary>
    /// Simply reflects velocity off of a plane given its normal while also 
    /// simulating friction.
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="nextTraceDirection"></param>
    /// <param name="remainingTraceLength"></param>
    /// <param name="normal"></param>
    void BounceVelocity(ref Vector3 velocity, ref Vector3 nextTraceDirection, ref float remainingTraceLength, Vector3 normal)
    {
        // grab our initial magnitude for later
        float initVelocity = velocity.magnitude;

        // grab component of velocity clipping into the negative normal
        float inwardComponent = Vector3.Dot(velocity, -normal);

        // apply this component by a factor of two to 'invert the sign' of the normal component
        velocity += 2 * inwardComponent * normal;

        // apply a loss of velocity to simulate friction
        velocity -= velocity * 0.05F;

        // multiply our remaining trace length by the ratio of our current velocity over the initVelocity
        remainingTraceLength *= velocity.magnitude / initVelocity;

        // apply the unit direction to be the velocity's unit vector
        nextTraceDirection = velocity.normalized;
    }

}
