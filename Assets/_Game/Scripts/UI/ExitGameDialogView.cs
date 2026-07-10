using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 主界面退出游戏确认弹窗：美术拖好引用后，由 MainMenuController 调用 Show。
/// </summary>
public class ExitGameDialogView : MonoBehaviour
{
    [Header("面板根节点（含遮罩，控制显隐）")]
    [SerializeField] private GameObject panelRoot;

    [Header("文案")]
    [SerializeField] private Text messageText;
    [SerializeField] private string defaultMessage = "确定要退出游戏吗？";

    [Header("按钮")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

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

    public void Show()
    {
        if (messageText != null)
        {
            messageText.text = defaultMessage;
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void OnConfirmClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        QuitApplication();
    }

    private void OnCancelClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        Hide();
    }

    /// <summary>退出应用（Editor 下停止 Play）。</summary>
    public static void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

#if UNITY_EDITOR
    [ContextMenu("Validate References")]
    private void ValidateReferences()
    {
        if (panelRoot == null || confirmButton == null || cancelButton == null)
        {
            Debug.LogError("[ExitGameDialogView] 引用未配齐，请拖入 panelRoot、确认/取消按钮。", this);
        }
    }
#endif
}
