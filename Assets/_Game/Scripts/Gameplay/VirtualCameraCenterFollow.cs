using UnityEngine;

public class VirtualCameraCenterFollow : MonoBehaviour
{
    [SerializeField] private Transform targetA;
    [SerializeField] private Transform targetB;
    [SerializeField] private Vector2 offset;

    private void Update()
    {
        if (targetA == null || targetB == null)
        {
            return;
        }

        Vector3 center = (targetA.position + targetB.position) * 0.5f;
        Vector3 currentPosition = transform.position;

        transform.position = new Vector3(
            center.x + offset.x,
            center.y + offset.y,
            currentPosition.z);
    }
}
