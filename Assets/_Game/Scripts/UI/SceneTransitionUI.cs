using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 场景切换转场 UI：简单黑屏淡出。
/// 使用 DOTween 的 SetUpdate(true)，在 timeScale=0 时仍可播放。
/// </summary>
public class SceneTransitionUI : MonoBehaviour
{
    [Header("动画时长（秒，不受 timeScale 影响）")]
    [SerializeField] private float fadeOutDuration = 0.45f;
    [SerializeField] private float fadeInDuration = 0.45f;

    [Header("UI 引用（留空则运行时自动创建）")]
    [SerializeField] private CanvasGroup overlayGroup;

    private GameObject canvasRoot;
    private Tween activeTween;

    private void Awake()
    {
        EnsureUiBuilt();
        HideImmediate();
    }

    private void OnDestroy()
    {
        KillActiveTween();
    }

    public IEnumerator PlaySimpleFadeOut()
    {
        EnsureUiBuilt();
        PrepareVisibleState();
        SetBlocking(true);

        // 已是黑幕时跳过，避免 HoldBlack 后再淡出从透明闪一帧。
        if (overlayGroup != null && overlayGroup.alpha >= 0.99f)
        {
            yield break;
        }

        if (overlayGroup != null)
        {
            overlayGroup.alpha = 0f;
        }

        yield return AnimateAlpha(overlayGroup, 1f, fadeOutDuration);
    }

    public IEnumerator PlaySimpleFadeIn()
    {
        EnsureUiBuilt();

        yield return AnimateAlpha(overlayGroup, 0f, fadeInDuration);

        HideImmediate();
    }

    /// <summary>收起转场黑幕，让介绍面板等内容显示在上层。</summary>
    public void SuppressOverlayForContent()
    {
        EnsureUiBuilt();
        KillActiveTween();

        if (overlayGroup != null)
        {
            overlayGroup.alpha = 0f;
        }

        SetBlocking(false);
    }

    /// <summary>保持全屏黑幕，避免介绍关闭后露出底层场景。</summary>
    public void HoldBlack()
    {
        EnsureUiBuilt();
        PrepareVisibleState();
        KillActiveTween();

        if (overlayGroup != null)
        {
            overlayGroup.alpha = 1f;
        }

        SetBlocking(true);
    }

    public void HideImmediate()
    {
        KillActiveTween();

        if (overlayGroup != null)
        {
            overlayGroup.alpha = 0f;
        }

        if (canvasRoot != null)
        {
            canvasRoot.SetActive(false);
        }

        SetBlocking(false);
    }

    private void PrepareVisibleState()
    {
        if (canvasRoot != null)
        {
            canvasRoot.SetActive(true);
        }
    }

    private void SetBlocking(bool block)
    {
        if (overlayGroup != null)
        {
            overlayGroup.blocksRaycasts = block;
            overlayGroup.interactable = block;
        }
    }

    private IEnumerator AnimateAlpha(CanvasGroup group, float targetAlpha, float duration)
    {
        if (group == null)
        {
            yield break;
        }

        KillActiveTween();

        if (duration <= 0f)
        {
            group.alpha = targetAlpha;
            yield break;
        }

        activeTween = group
            .DOFade(targetAlpha, duration)
            .SetUpdate(true);

        yield return activeTween.WaitForCompletion();
        activeTween = null;
    }

    private void KillActiveTween()
    {
        if (activeTween != null && activeTween.IsActive())
        {
            activeTween.Kill();
        }

        activeTween = null;
    }

    private void EnsureUiBuilt()
    {
        if (overlayGroup != null)
        {
            return;
        }

        canvasRoot = new GameObject("SceneTransitionCanvas");
        canvasRoot.transform.SetParent(transform, false);

        Canvas canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasRoot.AddComponent<GraphicRaycaster>();

        GameObject overlayObject = CreateStretchPanel(canvasRoot.transform, "FadeOverlay", Color.black);
        overlayGroup = overlayObject.AddComponent<CanvasGroup>();
        overlayGroup.alpha = 0f;
    }

    private static GameObject CreateStretchPanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        Image image = panel.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return panel;
    }
}
