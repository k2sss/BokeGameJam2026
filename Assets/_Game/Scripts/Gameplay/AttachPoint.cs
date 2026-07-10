using UnityEngine;
using DG.Tweening;

public class AttachPoint : MonoBehaviour
{
    [Header("Attach")]
    [SerializeField] private float attachRadius = 1.25f;

    [Header("Hint")]
    [SerializeField] private Transform hintRoot;
    [SerializeField] private Sprite hintSprite;
    [SerializeField] private Vector3 hintOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Hint Animation")]
    [SerializeField] private bool enableHintBreath = true;
    [SerializeField] private float breathScaleMultiplier = 1.12f;
    [SerializeField] private float breathDuration = 0.6f;

    [Header("Gizmo")]
    [SerializeField] private bool drawGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.9f, 0.3f, 0.9f);

    private bool hintVisible;
    private Vector3 cachedHintScale = Vector3.one;
    private Tween hintBreathTween;

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

    private void Awake()
    {
        EnsureHintRoot();
        ApplyHintSprite();
        HideHint();
    }

    private void OnValidate()
    {
        EnsureHintRoot();
        ApplyHintSprite();

        if (hintRoot != null)
        {
            hintRoot.localPosition = hintOffset;
        }
    }

    private void LateUpdate()
    {
        SetHintVisible(ShouldShowHint());
    }

    private void OnDestroy()
    {
        StopHintBreath();
    }

    private bool ShouldShowHint()
    {
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsPlaying)
        {
            return false;
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag(GameConstants.Tags.Player);
        for (int i = 0; i < players.Length; i++)
        {
            PlayerBody playerBody = players[i].GetComponent<PlayerBody>();
            if (playerBody == null || playerBody.IsFixed)
            {
                continue;
            }

            if (CanAttach(playerBody))
            {
                return true;
            }
        }

        return false;
    }

    private void EnsureHintRoot()
    {
        if (hintRoot != null)
        {
            return;
        }

        Transform child = transform.Find("Hint");
        if (child != null)
        {
            hintRoot = child;
        }
    }

    private void ApplyHintSprite()
    {
        if (hintSprite == null || hintRoot == null)
        {
            return;
        }

        SpriteRenderer renderer = hintRoot.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sprite = hintSprite;
        }
    }

    private void SetHintVisible(bool visible)
    {
        if (visible)
        {
            ShowHint();
            return;
        }

        HideHint();
    }

    private void ShowHint()
    {
        if (hintRoot == null || hintVisible)
        {
            return;
        }

        hintVisible = true;
        hintRoot.gameObject.SetActive(true);
        cachedHintScale = hintRoot.localScale;
        StartHintBreath();
    }

    private void HideHint()
    {
        if (hintRoot == null || !hintVisible)
        {
            return;
        }

        hintVisible = false;
        StopHintBreath();
        hintRoot.gameObject.SetActive(false);
    }

    private void StartHintBreath()
    {
        if (!enableHintBreath || hintRoot == null)
        {
            return;
        }

        StopHintBreath();
        hintRoot.localScale = cachedHintScale;
        hintBreathTween = hintRoot
            .DOScale(cachedHintScale * breathScaleMultiplier, breathDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void StopHintBreath()
    {
        if (hintBreathTween != null)
        {
            hintBreathTween.Kill();
            hintBreathTween = null;
        }

        if (hintRoot != null)
        {
            hintRoot.localScale = cachedHintScale;
        }
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
