using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCapsuleCollider : CharacterCollider
{
    [SerializeField] CapsuleCollider Capsule_C;

    public const float TRACEBIAS = 0.001F;

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

        Vector3 capsuleSegmentLength = (orientation * Vector3.up) * (Capsule_C.height * .5F - Capsule_C.radius);

        int tracedCollidersCount = Physics.CapsuleCastNonAlloc(
            position + capsuleSegmentLength,
            position - capsuleSegmentLength,
            Capsule_C.radius,
            direction,
            tmpBuffer,
            traceDistance + TRACEBIAS,
            validHitMask,
            interactionType);

        return tracedCollidersCount;
    }

    public override Collider Collider()
    {
        return Capsule_C;
    }
}
