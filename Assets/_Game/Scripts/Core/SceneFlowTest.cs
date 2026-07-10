using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Play 模式测试 SceneFlowManager：
/// O - LoadNextLevel（主菜单时进入 level1，关卡内进入下一关）
/// P - ReloadCurrentLevel（重载当前场景，SimpleFade）
/// M - LoadMainMenu
/// </summary>
public class SceneFlowTest : MonoBehaviour
{
    [SerializeField] private SceneFlowManager sceneFlowManager;
    [SerializeField] private LevelDatabaseSO levelDatabase;
    [SerializeField] private KeyCode loadNextKey = KeyCode.O;
    [SerializeField] private KeyCode reloadKey = KeyCode.P;
    [SerializeField] private KeyCode mainMenuKey = KeyCode.M;

    private void Awake()
    {
        EnsureSceneFlowManager();
    }

    private void Update()
    {
        if (sceneFlowManager == null || sceneFlowManager.IsLoading)
        {
            return;
        }

        if (Input.GetKeyDown(loadNextKey))
        {
            Debug.Log("[SceneFlowTest] 触发 LoadNextLevel");
            sceneFlowManager.LoadNextLevel();
        }

        if (Input.GetKeyDown(reloadKey))
        {
            Debug.Log("[SceneFlowTest] 触发 ReloadCurrentLevel");
            sceneFlowManager.ReloadCurrentLevel();
        }

        if (Input.GetKeyDown(mainMenuKey))
        {
            Debug.Log("[SceneFlowTest] 触发 LoadMainMenu");
            sceneFlowManager.LoadMainMenu();
        }
    }

    private void EnsureSceneFlowManager()
    {
        if (sceneFlowManager == null)
        {
            sceneFlowManager = SceneFlowManager.Instance;
        }

        if (sceneFlowManager == null)
        {
            sceneFlowManager = FindObjectOfType<SceneFlowManager>();
        }

        if (sceneFlowManager == null)
        {
            GameObject managerObject = new GameObject("SceneFlowManager");
            sceneFlowManager = managerObject.AddComponent<SceneFlowManager>();
        }

#if UNITY_EDITOR
        if (levelDatabase == null)
        {
            levelDatabase = AssetDatabase.LoadAssetAtPath<LevelDatabaseSO>(
                "Assets/_Game/Data/ScriptableObjects/LevelDatabase.asset");
        }

        if (levelDatabase != null)
        {
            SerializedObject serializedManager = new SerializedObject(sceneFlowManager);
            SerializedProperty databaseProperty = serializedManager.FindProperty("levelDatabase");
            if (databaseProperty != null && databaseProperty.objectReferenceValue == null)
            {
                databaseProperty.objectReferenceValue = levelDatabase;
                serializedManager.ApplyModifiedPropertiesWithoutUndo();
            }
        }
#endif
    }
}
