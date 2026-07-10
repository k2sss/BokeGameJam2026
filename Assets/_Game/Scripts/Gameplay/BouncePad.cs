using UnityEngine;
using DG.Tweening;

public class BouncePad : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool affectOnlyPlayerBody = true;
    [SerializeField] private Transform viewRoot;
    [SerializeField] private float punchScale = 0.2f;
    [SerializeField] private float punchDuration = 0.18f;

    private Vector3 cachedViewScale = Vector3.one;
    private Tween bounceTween;

    private void Awake()
    {
        EnsureViewRoot();
    }

    private void OnValidate()
    {
        EnsureViewRoot();
    }

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
        PlayBounceAnimation();
    }

    private void PlayBounceAnimation()
    {
        if (viewRoot == null)
        {
            return;
        }

        StopBounceAnimation();
        viewRoot.localScale = cachedViewScale;
        bounceTween = viewRoot
            .DOPunchScale(Vector3.one * punchScale, punchDuration, 6, 0.85f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                viewRoot.localScale = cachedViewScale;
                bounceTween = null;
            });
    }

    private void StopBounceAnimation()
    {
        if (bounceTween != null)
        {
            bounceTween.Kill();
            bounceTween = null;
        }

        if (viewRoot != null)
        {
            viewRoot.localScale = cachedViewScale;
        }
    }

    private void EnsureViewRoot()
    {
        if (viewRoot == null)
        {
            viewRoot = transform;
        }

        if (viewRoot != null)
        {
            cachedViewScale = viewRoot.localScale;
        }
    }
}
