using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 覆盖存档弹窗：美术拖好引用后，由 MainMenuController 调用 Show 方法。
/// </summary>
public class OverwriteSaveDialogView : MonoBehaviour
{
    [Header("面板根节点（含遮罩，控制显隐）")]
    [SerializeField] private GameObject panelRoot;

    [Header("文案")]
    [SerializeField] private Text dynamicLineText;
    [SerializeField] private Text fixedLineText;
    [SerializeField] private string fixedLineDefault = "会覆盖当前存档，是否确认？";

    [Header("按钮")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private bool isNewGameRequest;
    private int targetLevelIndex;

    /// <summary>确认开始新游戏。</summary>
    public event Action OnNewGameConfirmed;

    /// <summary>确认进入指定关卡，参数为关卡索引。</summary>
    public event Action<int> OnLevelSelectConfirmed;

    /// <summary>点击取消。</summary>
    public event Action OnCanceled;

    private void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        Hide();
    }

    /// <summary>新游戏触发的覆盖确认。</summary>
    public void ShowForNewGame()
    {
        isNewGameRequest = true;
        targetLevelIndex = 0;
        ShowInternal("开启新游戏，");
    }

    /// <summary>关卡选择触发的覆盖确认。</summary>
    public void ShowForLevelSelect(int levelIndex)
    {
        isNewGameRequest = false;
        targetLevelIndex = levelIndex;
        ShowInternal("如进入指定关卡，");
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void ShowInternal(string dynamicLine)
    {
        if (dynamicLineText != null)
        {
            dynamicLineText.text = dynamicLine;
        }

        if (fixedLineText != null)
        {
            fixedLineText.text = fixedLineDefault;
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    private void OnConfirmClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (isNewGameRequest)
        {
            OnNewGameConfirmed?.Invoke();
        }
        else
        {
            OnLevelSelectConfirmed?.Invoke(targetLevelIndex);
        }

        Hide();
    }

    private void OnCancelClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        OnCanceled?.Invoke();
        Hide();
    }

#if UNITY_EDITOR
    [ContextMenu("Validate References")]
    private void ValidateReferences()
    {
        if (panelRoot == null || dynamicLineText == null || confirmButton == null || cancelButton == null)
        {
            Debug.LogError("[OverwriteSaveDialogView] 引用未配齐，请拖入 panelRoot、文案与按钮。", this);
        }
    }
#endif
}
