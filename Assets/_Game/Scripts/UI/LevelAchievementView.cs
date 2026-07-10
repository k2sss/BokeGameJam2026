using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 关卡成就面板 View：读取解锁状态与关卡名，选关时通知 Controller。
/// 无星级版默认隐藏三星与「恢复初始」按钮。
/// </summary>
public class LevelAchievementView : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] private GameObject panelRoot;

    [Header("列表")]
    [SerializeField] private LevelAchievementItemView[] levelItems;

    [Header("按钮")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button resetProgressButton;

    [Header("星级显示（扩展 Phase 前保持关闭）")]
    [SerializeField] private bool showStars;

    /// <summary>玩家选择进入某关时触发。</summary>
    public event Action<int> OnLevelEnterRequested;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        if (resetProgressButton != null)
        {
            resetProgressButton.gameObject.SetActive(false);
            resetProgressButton.interactable = false;
        }

        BindLevelItems();
        Hide();
    }

    /// <summary>打开面板并刷新关卡列表。</summary>
    public void Show(LevelDatabaseSO levelDatabase, SaveManager saveManager)
    {
        Refresh(levelDatabase, saveManager);

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

    private void OnCloseClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        Hide();
    }

    /// <summary>根据存档与关卡表刷新各行状态。</summary>
    public void Refresh(LevelDatabaseSO levelDatabase, SaveManager saveManager)
    {
        if (levelItems == null || levelItems.Length == 0)
        {
            return;
        }

        for (int i = 0; i < levelItems.Length; i++)
        {
            LevelAchievementItemView item = levelItems[i];
            if (item == null)
            {
                continue;
            }

            item.SetLevelIndex(i);

            bool unlocked = saveManager != null && saveManager.IsLevelUnlocked(i);
            string displayName = BuildDisplayName(levelDatabase, i);
            int starCount = saveManager != null ? saveManager.GetStarCount(i) : 0;

            item.Apply(unlocked, displayName, starCount, showStars);
        }
    }

    private void BindLevelItems()
    {
        if (levelItems == null)
        {
            return;
        }

        for (int i = 0; i < levelItems.Length; i++)
        {
            LevelAchievementItemView item = levelItems[i];
            if (item == null)
            {
                continue;
            }

            item.SetLevelIndex(i);
            item.OnEnterRequested -= HandleLevelEnterRequested;
            item.OnEnterRequested += HandleLevelEnterRequested;
        }
    }

    private void HandleLevelEnterRequested(int levelIndex)
    {
        OnLevelEnterRequested?.Invoke(levelIndex);
    }

    private static string BuildDisplayName(LevelDatabaseSO levelDatabase, int levelIndex)
    {
        string levelNumber = $"第{levelIndex + 1}关";
        if (levelDatabase == null)
        {
            return levelNumber;
        }

        LevelEntry entry = levelDatabase.GetLevel(levelIndex);
        if (entry == null || string.IsNullOrWhiteSpace(entry.displayName))
        {
            return levelNumber;
        }

        return $"{levelNumber}·{entry.displayName}";
    }

#if UNITY_EDITOR
    [ContextMenu("Validate References")]
    private void ValidateReferences()
    {
        if (panelRoot == null || closeButton == null)
        {
            Debug.LogError("[LevelAchievementView] 引用未配齐，请拖入 panelRoot 与 closeButton。", this);
            return;
        }

        if (levelItems == null || levelItems.Length == 0)
        {
            Debug.LogWarning("[LevelAchievementView] 未配置 levelItems，请拖入至少一行关卡条目。", this);
        }
    }
#endif
}
