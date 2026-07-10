using UnityEngine;

public class AttachPoint : MonoBehaviour
{
    [SerializeField] private float attachRadius = 1.25f;
    [SerializeField] private bool drawGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.9f, 0.3f, 0.9f);

    public Vector2 Position => transform.position;

    public bool CanAttach(PlayerBody playerBody)
    {
        if (playerBody == null)
        {
            return false;
        }

        Vector2 offset = playerBody.transform.position - transform.position;
        return offset.sqrMagnitude <= attachRadius * attachRadius;
    }

    public float GetDistanceSqr(PlayerBody playerBody)
    {
        if (playerBody == null)
        {
            return float.MaxValue;
        }

        Vector2 offset = playerBody.transform.position - transform.position;
        return offset.sqrMagnitude;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmo)
        {
            return;
        }

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, attachRadius);
    }
}
