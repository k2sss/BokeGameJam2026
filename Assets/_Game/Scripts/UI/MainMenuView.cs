using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主界面 View：仅持有 uGUI 引用，负责按钮绑定与显示状态刷新。
/// 美术在 Canvas 中摆好布局后，将 Button 等组件拖入 Inspector。
/// </summary>
public class MainMenuView : MonoBehaviour
{
    [Header("菜单按钮")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button levelAchievementButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button exitGameButton;

    [Header("可选：继续按钮禁用遮罩（不绑则仅改 interactable）")]
    [SerializeField] private Graphic continueDisabledOverlay;

    [Header("展示区")]
    [SerializeField] private CharacterCarousel characterCarousel;

    /// <summary>将菜单按钮点击事件绑定到 Controller。</summary>
    public void Bind(MainMenuController controller)
    {
        if (controller == null)
        {
            Debug.LogError("[MainMenuView] Bind 失败：controller 为空。");
            return;
        }

        BindButton(continueButton, controller.OnContinueClicked);
        BindButton(newGameButton, controller.OnNewGameClicked);
        BindButton(levelAchievementButton, controller.OnLevelAchievementClicked);
        BindButton(creditsButton, controller.OnCreditsClicked);
        BindButton(exitGameButton, controller.OnExitGameClicked);
    }

    /// <summary>设置「继续游戏」是否可点击。</summary>
    public void SetContinueEnabled(bool enabled)
    {
        if (continueButton != null)
        {
            continueButton.interactable = enabled;
        }

        if (continueDisabledOverlay != null)
        {
            continueDisabledOverlay.gameObject.SetActive(!enabled);
        }
    }

    /// <summary>初始化角色轮播（需在 Inspector 绑定 CharacterCarousel）。</summary>
    public void InitializeCarousel(LevelDatabaseSO levelDatabase)
    {
        if (characterCarousel != null)
        {
            characterCarousel.Initialize(levelDatabase);
        }
    }

    /// <summary>校验 Inspector 引用是否齐全。</summary>
    public bool ValidateReferences(bool logError = true)
    {
        bool valid = continueButton != null
                     && newGameButton != null
                     && levelAchievementButton != null
                     && creditsButton != null
                     && exitGameButton != null;

        if (!valid && logError)
        {
            Debug.LogError(
                "[MainMenuView] 菜单按钮引用未配齐，请在 Inspector 拖入全部 Button。",
                this);
        }

        return valid;
    }

#if UNITY_EDITOR
    [ContextMenu("Validate References")]
    private void ValidateReferencesContextMenu()
    {
        ValidateReferences(true);
    }
#endif

    private static void BindButton(Button button, Action handler)
    {
        if (button == null || handler == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayButtonClick();
            handler();
        });
    }
}
