using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 场景切换转场 UI：支持过关图文渐变与简单黑屏淡出。
/// 使用 DOTween 的 SetUpdate(true)，在 timeScale=0 时仍可播放。
/// </summary>
public class SceneTransitionUI : MonoBehaviour
{
    [Header("动画时长（秒，不受 timeScale 影响）")]
    [SerializeField] private float fadeOutDuration = 0.45f;
    [SerializeField] private float contentFadeDuration = 0.55f;
    [SerializeField] private float holdDuration = 0.65f;
    [SerializeField] private float fadeInDuration = 0.45f;

    [Header("UI 引用（留空则运行时自动创建）")]
    [SerializeField] private CanvasGroup overlayGroup;
    [SerializeField] private CanvasGroup contentGroup;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

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

    /// <summary>
    /// 过关转场 - 淡出阶段：黑屏遮罩 → 展示背景与角色图。
    /// </summary>
    public IEnumerator PlayLevelTransitionOut(Sprite background, Sprite character)
    {
        EnsureUiBuilt();
        PrepareVisibleState(showContent: true);
        ApplyLevelSprites(background, character);

        contentGroup.alpha = 0f;
        overlayGroup.alpha = 0f;
        SetBlocking(true);

        yield return AnimateAlpha(overlayGroup, 1f, fadeOutDuration);
        yield return AnimateAlpha(contentGroup, 1f, contentFadeDuration);

        if (holdDuration > 0f)
        {
            yield return WaitRealtime(holdDuration);
        }
    }

    /// <summary>
    /// 过关转场 - 淡入阶段：隐藏转场层，露出新场景。
    /// </summary>
    public IEnumerator PlayLevelTransitionIn()
    {
        EnsureUiBuilt();

        yield return AnimateAlpha(contentGroup, 0f, fadeInDuration * 0.6f);
        yield return AnimateAlpha(overlayGroup, 0f, fadeInDuration);

        HideImmediate();
    }

    /// <summary>
    /// 简单转场 - 淡出到黑屏（用于重试等）。
    /// </summary>
    public IEnumerator PlaySimpleFadeOut()
    {
        EnsureUiBuilt();
        PrepareVisibleState(showContent: false);

        overlayGroup.alpha = 0f;
        SetBlocking(true);

        yield return AnimateAlpha(overlayGroup, 1f, fadeOutDuration);
    }

    /// <summary>
    /// 简单转场 - 从黑屏淡入。
    /// </summary>
    public IEnumerator PlaySimpleFadeIn()
    {
        EnsureUiBuilt();

        yield return AnimateAlpha(overlayGroup, 0f, fadeInDuration);

        HideImmediate();
    }

    /// <summary>立即隐藏转场 UI。</summary>
    public void HideImmediate()
    {
        KillActiveTween();

        if (overlayGroup != null)
        {
            overlayGroup.alpha = 0f;
        }

        if (contentGroup != null)
        {
            contentGroup.alpha = 0f;
        }

        if (canvasRoot != null)
        {
            canvasRoot.SetActive(false);
        }

        SetBlocking(false);
    }

    private void PrepareVisibleState(bool showContent)
    {
        if (canvasRoot != null)
        {
            canvasRoot.SetActive(true);
        }

        if (contentGroup != null)
        {
            contentGroup.gameObject.SetActive(showContent);
        }
    }

    private void ApplyLevelSprites(Sprite background, Sprite character)
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = background;
            backgroundImage.enabled = background != null;
            backgroundImage.color = background != null ? Color.white : Color.black;
        }

        if (characterImage != null)
        {
            characterImage.sprite = character;
            characterImage.enabled = character != null;
            characterImage.color = character != null ? Color.white : new Color(1f, 1f, 1f, 0f);
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

    private static IEnumerator WaitRealtime(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void KillActiveTween()
    {
        if (activeTween != null && activeTween.IsActive())
        {
            activeTween.Kill();
        }

        activeTween = null;
    }

    /// <summary>运行时自动搭建全屏转场 Canvas（与 GameOverUI 风格一致）。</summary>
    private void EnsureUiBuilt()
    {
        if (overlayGroup != null && contentGroup != null)
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

        GameObject contentObject = CreateStretchPanel(canvasRoot.transform, "TransitionContent", Color.clear);
        contentGroup = contentObject.AddComponent<CanvasGroup>();
        contentGroup.alpha = 0f;

        backgroundImage = CreateStretchImage(contentObject.transform, "Background", Color.black);
        characterImage = CreateStretchImage(contentObject.transform, "Character", Color.clear);

        RectTransform characterRect = characterImage.rectTransform;
        characterRect.anchorMin = new Vector2(0.5f, 0f);
        characterRect.anchorMax = new Vector2(0.5f, 0f);
        characterRect.pivot = new Vector2(0.5f, 0f);
        characterRect.sizeDelta = new Vector2(720f, 900f);
        characterRect.anchoredPosition = new Vector2(0f, 80f);
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

    private static Image CreateStretchImage(Transform parent, string name, Color color)
    {
        GameObject imageObject = CreateStretchPanel(parent, name, color);
        Image image = imageObject.GetComponent<Image>();
        image.preserveAspect = true;
        return image;
    }
}
