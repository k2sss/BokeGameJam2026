using UnityEngine;

public class RopeLengthLimiter2D : MonoBehaviour
{
    [SerializeField] private Rigidbody2D anchorBody;
    [SerializeField] private Rigidbody2D targetBody;
    [Min(0.01f)]
    [SerializeField] private float maxLength = 5f;
    [SerializeField] private bool removeOutwardVelocity = true;
    [SerializeField] private bool drawGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(1f, 0.6f, 0.1f, 0.9f);

    private void FixedUpdate()
    {
        if (anchorBody == null || targetBody == null)
        {
            return;
        }

        Vector2 anchorPosition = anchorBody.position;
        Vector2 targetPosition = targetBody.position;
        Vector2 delta = targetPosition - anchorPosition;
        float distance = delta.magnitude;

        if (distance <= maxLength || distance <= 0.0001f)
        {
            return;
        }

        Vector2 direction = delta / distance;
        Vector2 clampedPosition = anchorPosition + direction * maxLength;

        targetBody.position = clampedPosition;

        if (!removeOutwardVelocity)
        {
            return;
        }

        Vector2 relativeVelocity = targetBody.velocity - anchorBody.velocity;
        float outwardSpeed = Vector2.Dot(relativeVelocity, direction);
        if (outwardSpeed > 0f)
        {
            targetBody.velocity -= direction * outwardSpeed;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmo || anchorBody == null)
        {
            return;
        }

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(anchorBody.position, maxLength);
    }
}
