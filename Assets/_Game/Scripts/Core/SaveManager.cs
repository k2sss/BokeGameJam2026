using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 本地存档管理器，读写单文件 save.json。
///
/// 存档路径：
///   {Application.persistentDataPath}/save.json
///
/// Windows 示例：
///   C:\Users\用户名\AppData\LocalLow\CompanyName\ProductName\save.json
///   （CompanyName / ProductName 来自 Project Settings → Player）
/// </summary>
public class SaveManager : BaseMonoManager<SaveManager>, ILevelStarProvider
{
    [Header("配置")]
    [SerializeField] private LevelDatabaseSO levelDatabase;

    [Header("调试")]
    [SerializeField] private bool logSaveOperations = true;

    private SaveData currentData;

    protected override bool PersistAcrossScenes => true;

    /// <summary>本地存档完整路径。</summary>
    public string SaveFilePath => Path.Combine(Application.persistentDataPath, GameConstants.SaveFileName);

    /// <summary>当前内存中的存档数据。</summary>
    public SaveData CurrentData => currentData;

    /// <summary>是否有存档，供「继续游戏」按钮判断是否可点击。</summary>
    public bool CanContinue => currentData != null && currentData.hasSave;

    /// <summary>与 CanContinue 相同。</summary>
    public bool HasSave => CanContinue;

    protected override void Awake()
    {
        base.Awake();
        LoadFromDisk();
    }

    /// <summary>运行时或主界面初始化时注入关卡配置表。</summary>
    public void SetLevelDatabase(LevelDatabaseSO database)
    {
        if (database != null)
        {
            levelDatabase = database;
        }
    }

    /// <summary>开始新游戏：覆盖旧存档，从第 1 关（索引 0）开始。</summary>
    public void BeginNewGame()
    {
        currentData = SaveData.CreateNewGame();
        WriteToDisk();

        if (logSaveOperations)
        {
            Debug.Log($"[SaveManager] 新游戏，存档路径：{SaveFilePath}");
        }
    }

    /// <summary>获取继续游戏的目标关卡索引。</summary>
    public bool TryGetContinueLevel(out int levelIndex)
    {
        levelIndex = 0;

        if (!CanContinue)
        {
            return false;
        }

        levelIndex = Mathf.Max(0, currentData.currentLevelIndex);
        return true;
    }

    /// <summary>设置当前进度关卡（选关、进入关卡时调用）。</summary>
    public void SetCurrentLevel(int levelIndex)
    {
        if (currentData == null)
        {
            currentData = SaveData.CreateEmpty();
        }

        currentData.currentLevelIndex = Mathf.Max(0, levelIndex);
        currentData.hasSave = true;
        WriteToDisk();

        if (logSaveOperations)
        {
            Debug.Log($"[SaveManager] 当前关卡设为：{currentData.currentLevelIndex}");
        }
    }

    /// <summary>
    /// 关卡通关时调用：记录已通关关卡，并将 currentLevelIndex 推进到下一关（末关则保持）。
    /// </summary>
    public void OnLevelCleared(int levelIndex)
    {
        if (currentData == null)
        {
            currentData = SaveData.CreateEmpty();
        }

        MarkLevelCompleted(levelIndex);

        if (levelDatabase != null && !levelDatabase.IsLastLevel(levelIndex))
        {
            currentData.currentLevelIndex = levelIndex + 1;
        }
        else
        {
            currentData.currentLevelIndex = levelIndex;
        }

        currentData.hasSave = true;
        WriteToDisk();

        if (logSaveOperations)
        {
            Debug.Log(
                $"[SaveManager] 关卡通关：{levelIndex}，下一进度关：{currentData.currentLevelIndex}");
        }
    }

    /// <summary>查询某关是否已通关。</summary>
    public bool IsLevelCompleted(int levelIndex)
    {
        if (currentData?.completedLevelIndices == null)
        {
            return false;
        }

        for (int i = 0; i < currentData.completedLevelIndices.Length; i++)
        {
            if (currentData.completedLevelIndices[i] == levelIndex)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>获取已通关关卡索引列表（副本）。</summary>
    public int[] GetCompletedLevels()
    {
        if (currentData?.completedLevelIndices == null)
        {
            return Array.Empty<int>();
        }

        int[] copy = new int[currentData.completedLevelIndices.Length];
        Array.Copy(currentData.completedLevelIndices, copy, copy.Length);
        return copy;
    }

    /// <summary>
    /// 关卡是否已在关卡成就中解锁。
    /// 规则：第 1 关（索引 0）默认解锁；第 N 关需已通关第 N-1 关。
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex <= 0)
        {
            return true;
        }

        if (levelDatabase != null && (levelIndex < 0 || levelIndex >= levelDatabase.LevelCount))
        {
            return false;
        }

        return IsLevelCompleted(levelIndex - 1);
    }

    /// <summary>关卡总数（来自 LevelDatabase）。</summary>
    public int LevelCount => levelDatabase != null ? levelDatabase.LevelCount : GameConstants.LevelCount;

    /// <summary>读取指定关卡的 BGM 音量（0~1）。</summary>
    public float GetLevelBgmVolume(int levelIndex)
    {
        EnsureLevelAudioData();
        if (!IsValidLevelIndex(levelIndex))
        {
            return GameConstants.DefaultAudioVolume;
        }

        return currentData.levelBgmVolumes[levelIndex];
    }

    /// <summary>读取指定关卡的 SFX 音量（0~1）。</summary>
    public float GetLevelSfxVolume(int levelIndex)
    {
        EnsureLevelAudioData();
        if (!IsValidLevelIndex(levelIndex))
        {
            return GameConstants.DefaultAudioVolume;
        }

        return currentData.levelSfxVolumes[levelIndex];
    }

    /// <summary>设置指定关卡的 BGM 音量并写入存档。</summary>
    public void SetLevelBgmVolume(int levelIndex, float volume)
    {
        EnsureLevelAudioData();
        if (!IsValidLevelIndex(levelIndex))
        {
            return;
        }

        currentData.levelBgmVolumes[levelIndex] = Mathf.Clamp01(volume);
        WriteToDisk();

        if (logSaveOperations)
        {
            Debug.Log($"[SaveManager] 关卡 {levelIndex} BGM 音量：{currentData.levelBgmVolumes[levelIndex]:P0}");
        }
    }

    /// <summary>设置指定关卡的 SFX 音量并写入存档。</summary>
    public void SetLevelSfxVolume(int levelIndex, float volume)
    {
        EnsureLevelAudioData();
        if (!IsValidLevelIndex(levelIndex))
        {
            return;
        }

        currentData.levelSfxVolumes[levelIndex] = Mathf.Clamp01(volume);
        WriteToDisk();

        if (logSaveOperations)
        {
            Debug.Log($"[SaveManager] 关卡 {levelIndex} SFX 音量：{currentData.levelSfxVolumes[levelIndex]:P0}");
        }
    }

    // --- 星级扩展占位（ILevelStarProvider，扩展 Phase 再实现） ---

    /// <inheritdoc />
    public int GetStarCount(int levelIndex)
    {
        return 0;
    }

    /// <inheritdoc />
    public void SettleLevelStars(int levelIndex, int starCount)
    {
        if (logSaveOperations)
        {
            Debug.Log($"[SaveManager] SettleLevelStars 占位调用：关卡={levelIndex}，星数={starCount}（扩展 Phase 实现）");
        }
    }

    /// <inheritdoc />
    public void ResetAllStars()
    {
        if (logSaveOperations)
        {
            Debug.Log("[SaveManager] ResetAllStars 占位调用（扩展 Phase 实现）");
        }
    }

    /// <summary>清空存档并删除本地文件。</summary>
    public void ClearSave()
    {
        currentData = SaveData.CreateEmpty();

        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
        }

        if (logSaveOperations)
        {
            Debug.Log($"[SaveManager] 存档已清空：{SaveFilePath}");
        }
    }

    /// <summary>根据关卡索引获取场景名。</summary>
    public string GetSceneNameByLevelIndex(int levelIndex)
    {
        if (levelDatabase == null)
        {
            return null;
        }

        LevelEntry entry = levelDatabase.GetLevel(levelIndex);
        return entry != null ? entry.sceneName : null;
    }

    private void MarkLevelCompleted(int levelIndex)
    {
        if (IsLevelCompleted(levelIndex))
        {
            return;
        }

        var completed = new List<int>();
        if (currentData.completedLevelIndices != null)
        {
            completed.AddRange(currentData.completedLevelIndices);
        }

        completed.Add(levelIndex);
        completed.Sort();
        currentData.completedLevelIndices = completed.ToArray();
    }

    private void EnsureLevelAudioData()
    {
        if (currentData == null)
        {
            currentData = SaveData.CreateEmpty();
        }

        currentData.EnsureLevelAudioCapacity(LevelCount);
    }

    private bool IsValidLevelIndex(int levelIndex)
    {
        return levelIndex >= 0 && levelIndex < LevelCount;
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(SaveFilePath))
        {
            currentData = SaveData.CreateEmpty();
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SaveData loaded = JsonUtility.FromJson<SaveData>(json);
            currentData = loaded ?? SaveData.CreateEmpty();

            if (currentData.completedLevelIndices == null)
            {
                currentData.completedLevelIndices = Array.Empty<int>();
            }

            EnsureLevelAudioData();
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[SaveManager] 读取存档失败，视为无存档。路径：{SaveFilePath}，原因：{exception.Message}");
            currentData = SaveData.CreateEmpty();
        }
    }

    private void WriteToDisk()
    {
        try
        {
            string directory = Path.GetDirectoryName(SaveFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(currentData, true);
            File.WriteAllText(SaveFilePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SaveManager] 写入存档失败。路径：{SaveFilePath}，原因：{exception.Message}");
        }
    }
}
