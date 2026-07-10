using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡顺序与转场素材的配置表。通过 Project 窗口 Create > SO > LevelDatabase 创建资产。
/// </summary>
[CreateAssetMenu(fileName = "LevelDatabase", menuName = "SO/LevelDatabase")]
public class LevelDatabaseSO : ScriptableObject
{
    [Header("主菜单")]
    [Tooltip("全部关卡通关后返回的主界面场景名")]
    [SerializeField] private string mainMenuSceneName = GameConstants.SceneNames.MainMenu;

    [Header("关卡列表（按通关顺序排列）")]
    [SerializeField] private List<LevelEntry> levels = new List<LevelEntry>();

    public string MainMenuSceneName => mainMenuSceneName;

    public IReadOnlyList<LevelEntry> Levels => levels;

    public int LevelCount => levels != null ? levels.Count : 0;

    /// <summary>根据场景名查找关卡索引，未找到返回 -1。</summary>
    public int GetIndexByScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || levels == null)
        {
            return -1;
        }

        for (int i = 0; i < levels.Count; i++)
        {
            LevelEntry entry = levels[i];
            if (entry != null && entry.IsValid &&
                string.Equals(entry.sceneName, sceneName, System.StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>获取指定索引的关卡配置，越界或无效时返回 null。</summary>
    public LevelEntry GetLevel(int index)
    {
        if (levels == null || index < 0 || index >= levels.Count)
        {
            return null;
        }

        LevelEntry entry = levels[index];
        return entry != null && entry.IsValid ? entry : null;
    }

    /// <summary>指定索引是否为最后一关。</summary>
    public bool IsLastLevel(int index)
    {
        if (levels == null || levels.Count == 0)
        {
            return true;
        }

        return index >= levels.Count - 1;
    }

    /// <summary>
    /// 解析「下一目标场景」：有下一关则返回下一关场景名，否则返回主菜单场景名。
    /// </summary>
    public string ResolveNextSceneName(int currentIndex)
    {
        if (IsLastLevel(currentIndex) || levels == null || levels.Count == 0)
        {
            return mainMenuSceneName;
        }

        int nextIndex = currentIndex + 1;
        LevelEntry nextLevel = GetLevel(nextIndex);
        return nextLevel != null ? nextLevel.sceneName : mainMenuSceneName;
    }

    /// <summary>
    /// 根据当前场景名解析下一目标场景名；若当前场景不在列表中则返回主菜单。
    /// </summary>
    public string ResolveNextSceneNameByCurrentScene(string currentSceneName)
    {
        int index = GetIndexByScene(currentSceneName);
        if (index < 0)
        {
            Debug.LogWarning(
                $"[LevelDatabaseSO] 当前场景 '{currentSceneName}' 不在关卡列表中，将跳转主菜单。");
            return mainMenuSceneName;
        }

        return ResolveNextSceneName(index);
    }

    /// <summary>获取下一关配置；若已是最后一关则返回 null。</summary>
    public LevelEntry GetNextLevel(int currentIndex)
    {
        if (IsLastLevel(currentIndex))
        {
            return null;
        }

        return GetLevel(currentIndex + 1);
    }
}
