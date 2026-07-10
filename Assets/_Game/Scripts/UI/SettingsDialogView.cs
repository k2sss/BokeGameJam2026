using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏内设置弹窗 View（Phase 5 接线）。
/// 提供关闭、回到主页面、手动存档等按钮引用。
/// </summary>
public class SettingsDialogView : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] private GameObject panelRoot;

    [Header("按钮")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backToMainMenuButton;
    [SerializeField] private Button manualSaveButton;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }

        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuPlaceholder);
        }

        if (manualSaveButton != null)
        {
            manualSaveButton.onClick.AddListener(OnManualSavePlaceholder);
        }

        Hide();
    }

    public void Show()
    {
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

    private void OnBackToMainMenuPlaceholder()
    {
        Debug.Log("[SettingsDialogView] 回到主页面（Phase 5 接线）");
        Hide();
    }

    private void OnManualSavePlaceholder()
    {
        Debug.Log("[SettingsDialogView] 手动存档（Phase 5 接线）");
    }

#if UNITY_EDITOR
    [ContextMenu("Validate References")]
    private void ValidateReferences()
    {
        if (panelRoot == null || closeButton == null)
        {
            Debug.LogError("[SettingsDialogView] 引用未配齐，请拖入 panelRoot 与 closeButton。", this);
        }
    }
#endif
}
