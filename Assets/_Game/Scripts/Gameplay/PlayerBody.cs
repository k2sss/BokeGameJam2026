using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBody : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Transform viewRoot;
    [SerializeField] private float fixedPunchScale = 0.18f;
    [SerializeField] private float fixedPunchDuration = 0.2f;

    private bool isFixed;
    private bool initialized;
    private RigidbodyConstraints2D cachedConstraints;
    private RigidbodyType2D cachedBodyType;
    private Vector3 cachedViewScale = Vector3.one;
    private Tween fixedScaleTween;
    private Transform attachedPoint;

    public bool IsFixed => isFixed;
    public Vector2 Position => body != null ? body.position : (Vector2)transform.position;
    public Vector2 Velocity => body != null ? body.velocity : Vector2.zero;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void LateUpdate()
    {
        FollowAttachedPoint();
    }

    private void OnValidate()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        if (viewRoot == null)
        {
            viewRoot = transform;
        }
    }

    public void ApplySwingInput(float horizontalInput, float force, Vector2 pivotPosition)
    {
        EnsureInitialized();

        if (body == null || isFixed)
        {
            return;
        }

        Vector2 radius = body.position - pivotPosition;
        if (radius.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Vector2 tangentA = new Vector2(-radius.y, radius.x).normalized;
        Vector2 tangentB = -tangentA;
        Vector2 desiredTangent = horizontalInput > 0f
            ? (tangentA.x >= tangentB.x ? tangentA : tangentB)
            : (tangentA.x <= tangentB.x ? tangentA : tangentB);

        body.AddForce(desiredTangent * (Mathf.Abs(horizontalInput) * force), ForceMode2D.Force);
    }

    public void SnapToPosition(Vector2 position)
    {
        EnsureInitialized();

        if (body == null)
        {
            transform.position = position;
            return;
        }

        if (body.bodyType != RigidbodyType2D.Static)
        {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        body.position = position;
        transform.position = position;
    }

    public void AttachToPoint(Transform point)
    {
        EnsureInitialized();

        attachedPoint = point;
        if (attachedPoint != null)
        {
            SnapToPosition(attachedPoint.position);
        }
    }

    public void DetachFromPoint()
    {
        attachedPoint = null;
    }

    public void SetVelocity(Vector2 velocity)
    {
        EnsureInitialized();

        if (body == null)
        {
            return;
        }

        body.velocity = velocity;
    }

    public void SetFixed(bool shouldBeFixed)
    {
        EnsureInitialized();

        if (body == null)
        {
            return;
        }

        isFixed = shouldBeFixed;

        if (shouldBeFixed)
        {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeAll;
            body.bodyType = RigidbodyType2D.Static;
            body.Sleep();
            PlayFixedBounceAnimation();
            return;
        }

        StopFixedBounceAnimation();
        DetachFromPoint();
        body.bodyType = cachedBodyType;
        body.constraints = cachedConstraints;
        body.WakeUp();
    }

    private void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        if (body != null)
        {
            cachedConstraints = body.constraints;
            cachedBodyType = body.bodyType;
        }

        if (viewRoot == null)
        {
            viewRoot = transform;
        }

        if (viewRoot != null)
        {
            cachedViewScale = viewRoot.localScale;
        }

        initialized = true;
    }

    private void PlayFixedBounceAnimation()
    {
        if (viewRoot == null)
        {
            return;
        }

        StopFixedBounceAnimation();
        viewRoot.localScale = cachedViewScale;
        fixedScaleTween = viewRoot
            .DOPunchScale(Vector3.one * fixedPunchScale, fixedPunchDuration, 6, 0.85f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                viewRoot.localScale = cachedViewScale;
                fixedScaleTween = null;
            });
    }

    private void StopFixedBounceAnimation()
    {
        if (fixedScaleTween != null)
        {
            fixedScaleTween.Kill();
            fixedScaleTween = null;
        }

        if (viewRoot != null)
        {
            viewRoot.localScale = cachedViewScale;
        }
    }

    private void FollowAttachedPoint()
    {
        if (!isFixed || attachedPoint == null)
        {
            return;
        }

        SnapToPosition(attachedPoint.position);
    }
}
