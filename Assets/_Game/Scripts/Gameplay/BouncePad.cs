using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool affectOnlyPlayerBody = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryReverseVelocity(collision.rigidbody);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryReverseVelocity(other.attachedRigidbody);
    }

    private void TryReverseVelocity(Rigidbody2D body)
    {
        if (body == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(targetTag) && !body.CompareTag(targetTag))
        {
            return;
        }

        if (affectOnlyPlayerBody && body.GetComponent<PlayerBody>() == null)
        {
            return;
        }

        body.velocity = -body.velocity;
    }
}
