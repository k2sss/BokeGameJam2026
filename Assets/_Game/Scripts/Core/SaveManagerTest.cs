using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Play 模式测试 SaveManager：
/// 1 - 新游戏并重载第 1 关
/// 2 - 继续游戏
/// 3 - 模拟当前关卡通关
/// 4 - 清空存档
/// 5 - 打印存档路径与内容
/// 6 - 打印四关解锁状态
/// </summary>
public class SaveManagerTest : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private SceneFlowManager sceneFlowManager;
    [SerializeField] private LevelDatabaseSO levelDatabase;

    [SerializeField] private KeyCode newGameKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode continueGameKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode clearLevelKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode clearSaveKey = KeyCode.Alpha4;
    [SerializeField] private KeyCode printSaveKey = KeyCode.Alpha5;
    [SerializeField] private KeyCode printUnlockKey = KeyCode.Alpha6;

    private void Awake()
    {
        EnsureManagers();
    }

    private void Update()
    {
        if (saveManager == null)
        {
            return;
        }

        if (Input.GetKeyDown(newGameKey))
        {
            TestNewGame();
        }

        if (Input.GetKeyDown(continueGameKey))
        {
            TestContinueGame();
        }

        if (Input.GetKeyDown(clearLevelKey))
        {
            TestSimulateLevelCleared();
        }

        if (Input.GetKeyDown(clearSaveKey))
        {
            saveManager.ClearSave();
            Debug.Log("[SaveManagerTest] 已清空存档。");
        }

        if (Input.GetKeyDown(printSaveKey))
        {
            PrintCurrentSave();
        }

        if (Input.GetKeyDown(printUnlockKey))
        {
            PrintUnlockState();
        }
    }

    private void TestNewGame()
    {
        saveManager.BeginNewGame();
        ReloadLevel(0);
        Debug.Log("[SaveManagerTest] 新游戏：关卡 0，正在重载。");
    }

    private void TestContinueGame()
    {
        if (!saveManager.TryGetContinueLevel(out int levelIndex))
        {
            Debug.LogWarning("[SaveManagerTest] 无可继续的存档。");
            return;
        }

        string sceneName = saveManager.GetSceneNameByLevelIndex(levelIndex);
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SaveManagerTest] 无法解析关卡场景名。");
            return;
        }

        if (sceneFlowManager != null)
        {
            sceneFlowManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("[SaveManagerTest] 无 SceneFlowManager，请手动重载场景。");
        }

        Debug.Log($"[SaveManagerTest] 继续游戏：关卡={levelIndex}");
    }

    private void TestSimulateLevelCleared()
    {
        if (sceneFlowManager == null)
        {
            Debug.LogWarning("[SaveManagerTest] 无 SceneFlowManager，无法获取当前关卡索引。");
            return;
        }

        int levelIndex = sceneFlowManager.CurrentLevelIndex;
        if (levelIndex < 0)
        {
            Debug.LogWarning("[SaveManagerTest] 当前场景不在关卡列表中。");
            return;
        }

        saveManager.OnLevelCleared(levelIndex);
        Debug.Log($"[SaveManagerTest] 模拟通关：关卡={levelIndex}");
    }

    private void ReloadLevel(int levelIndex)
    {
        string sceneName = saveManager.GetSceneNameByLevelIndex(levelIndex);
        if (sceneFlowManager != null && !string.IsNullOrEmpty(sceneName))
        {
            sceneFlowManager.LoadScene(sceneName);
        }
    }

    private void PrintCurrentSave()
    {
        SaveData data = saveManager.CurrentData;
        string completed = data.completedLevelIndices == null
            ? "[]"
            : $"[{string.Join(", ", data.completedLevelIndices)}]";

        Debug.Log(
            $"[SaveManagerTest] 路径={saveManager.SaveFilePath}\n" +
            $"hasSave={data.hasSave}, currentLevel={data.currentLevelIndex}, completed={completed}, " +
            $"CanContinue={saveManager.CanContinue}");
    }

    private void PrintUnlockState()
    {
        int count = saveManager.LevelCount;
        var lines = new System.Text.StringBuilder();
        lines.AppendLine("[SaveManagerTest] 关卡解锁状态：");

        for (int i = 0; i < count; i++)
        {
            string sceneName = saveManager.GetSceneNameByLevelIndex(i) ?? "?";
            lines.AppendLine(
                $"  [{i}] {sceneName} | unlocked={saveManager.IsLevelUnlocked(i)}, " +
                $"completed={saveManager.IsLevelCompleted(i)}, stars={saveManager.GetStarCount(i)}");
        }

        Debug.Log(lines.ToString());
    }

    private void EnsureManagers()
    {
        if (saveManager == null)
        {
            saveManager = SaveManager.Instance ?? FindObjectOfType<SaveManager>();
        }

        if (saveManager == null)
        {
            GameObject saveObject = new GameObject("SaveManager");
            saveManager = saveObject.AddComponent<SaveManager>();
        }

        if (sceneFlowManager == null)
        {
            sceneFlowManager = SceneFlowManager.Instance ?? FindObjectOfType<SceneFlowManager>();
        }

#if UNITY_EDITOR
        if (levelDatabase == null)
        {
            levelDatabase = AssetDatabase.LoadAssetAtPath<LevelDatabaseSO>(
                "Assets/_Game/Data/ScriptableObjects/LevelDatabase.asset");
        }

        AssignDatabaseIfMissing(saveManager, levelDatabase, "levelDatabase");

        if (sceneFlowManager != null)
        {
            AssignDatabaseIfMissing(sceneFlowManager, levelDatabase, "levelDatabase");
        }
#endif
    }

#if UNITY_EDITOR
    private static void AssignDatabaseIfMissing(Object target, Object database, string propertyName)
    {
        if (target == null || database == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null && property.objectReferenceValue == null)
        {
            property.objectReferenceValue = database;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
#endif
}
