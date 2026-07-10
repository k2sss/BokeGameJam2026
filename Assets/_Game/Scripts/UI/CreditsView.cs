using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 致谢名单 View：支持 ScrollRect 自动滚动与关闭按钮。
/// </summary>
public class CreditsView : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] private GameObject panelRoot;

    [Header("内容")]
    [SerializeField] private Text creditsText;
    [TextArea(3, 16)]
    [SerializeField] private string defaultCreditsText =
        "致谢名单占位\n\n" +
        "制作人：\n" +
        "程序：\n" +
        "美术：\n" +
        "策划：\n" +
        "音乐：\n\n" +
        "感谢游玩！";

    [Header("滚动")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float scrollSpeed = 0.15f;
    [SerializeField] private bool autoScrollOnShow = true;

    [Header("按钮")]
    [SerializeField] private Button closeButton;

    private Coroutine scrollRoutine;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        Hide();
    }

    public void Show()
    {
        if (creditsText != null)
        {
            creditsText.text = defaultCreditsText;
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        ResetScrollPosition();
        RestartAutoScroll();
    }

    public void Hide()
    {
        StopAutoScroll();

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void OnCloseClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        Hide();
    }

    private void ResetScrollPosition()
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void RestartAutoScroll()
    {
        StopAutoScroll();

        if (!autoScrollOnShow || scrollRect == null || scrollSpeed <= 0f)
        {
            return;
        }

        scrollRoutine = StartCoroutine(AutoScrollRoutine());
    }

    private void StopAutoScroll()
    {
        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }
    }

    private IEnumerator AutoScrollRoutine()
    {
        while (scrollRect != null && scrollRect.verticalNormalizedPosition > 0f)
        {
            scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.unscaledDeltaTime;
            yield return null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Validate References")]
    private void ValidateReferences()
    {
        if (panelRoot == null || closeButton == null)
        {
            Debug.LogError("[CreditsView] 引用未配齐，请拖入 panelRoot 与 closeButton。", this);
        }
    }
#endif
}
