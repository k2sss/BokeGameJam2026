using System;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 主界面逻辑：存档、场景切换、弹窗与面板调度。UI 引用由 View 在 Inspector 拖入。
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private LevelDatabaseSO levelDatabase;
    [SerializeField] private SceneBgmConfigSO sceneBgmConfig;
    [SerializeField] private AudioDatabaseSO audioDatabase;

    [Header("主菜单")]
    [SerializeField] private MainMenuView menuView;

    [Header("弹窗与面板")]
    [SerializeField] private OverwriteSaveDialogView overwriteDialogView;
    [SerializeField] private ExitGameDialogView exitGameDialogView;
    [SerializeField] private LevelAchievementView levelAchievementView;
    [SerializeField] private CreditsView creditsView;

    private void Awake()
    {
        EnsureLevelDatabase();
        EnsureSceneBgmConfig();
        EnsureAudioDatabase();
        EnsureEventSystem();
        EnsureManagers();
        BindViews();
        RefreshContinueButton();
        AudioManager.Instance?.PlayMusicForActiveScene();
    }

    private void OnEnable()
    {
        RefreshContinueButton();
        SubscribeDialogs();
    }

    private void OnDisable()
    {
        UnsubscribeDialogs();
    }

    public void OnContinueClicked()
    {
        if (SaveManager.Instance == null || !SaveManager.Instance.CanContinue)
        {
            return;
        }

        if (!SaveManager.Instance.TryGetContinueLevel(out int levelIndex))
        {
            return;
        }

        LoadLevelFromMenu(levelIndex);
    }

    public void OnNewGameClicked()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave)
        {
            CloseAllPanels();

            if (overwriteDialogView != null)
            {
                overwriteDialogView.ShowForNewGame();
            }
            else
            {
                Debug.LogWarning("[MainMenuController] 未配置覆盖存档弹窗，将直接开始新游戏。");
                StartNewGameAndLoadLevel(0);
            }

            return;
        }

        StartNewGameAndLoadLevel(0);
    }

    public void OnLevelAchievementClicked()
    {
        CloseAllPanels();

        if (levelAchievementView != null)
        {
            levelAchievementView.Show(levelDatabase, SaveManager.Instance);
        }
        else
        {
            Debug.LogWarning("[MainMenuController] 未配置 LevelAchievementView。");
        }
    }

    public void OnCreditsClicked()
    {
        CloseAllPanels();
        creditsView?.Show();
    }

    public void OnExitGameClicked()
    {
        CloseAllPanels();

        if (exitGameDialogView != null)
        {
            exitGameDialogView.Show();
        }
        else
        {
            ExitGameDialogView.QuitApplication();
        }
    }

    /// <summary>供关卡成就页选关时调用：有存档则弹覆盖确认。</summary>
    public void RequestEnterLevel(int levelIndex)
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave && overwriteDialogView != null)
        {
            CloseAllPanels();
            overwriteDialogView.ShowForLevelSelect(levelIndex);
            return;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetCurrentLevel(levelIndex);
        }

        LoadLevelFromMenu(levelIndex);
    }

    public void RefreshContinueButton()
    {
        if (menuView == null)
        {
            return;
        }

        bool canContinue = SaveManager.Instance != null && SaveManager.Instance.CanContinue;
        menuView.SetContinueEnabled(canContinue);
    }

    private void StartNewGameAndLoadLevel(int levelIndex)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.BeginNewGame();
        }

        LoadLevelFromMenu(levelIndex);
    }

    private void LoadLevelFromMenu(int levelIndex)
    {
        if (SceneFlowManager.Instance == null)
        {
            Debug.LogError("[MainMenuController] SceneFlowManager 不存在，无法加载关卡。");
            return;
        }

        string sceneName = SaveManager.Instance != null
            ? SaveManager.Instance.GetSceneNameByLevelIndex(levelIndex)
            : null;

        if (string.IsNullOrEmpty(sceneName) && levelDatabase != null)
        {
            LevelEntry entry = levelDatabase.GetLevel(levelIndex);
            sceneName = entry != null ? entry.sceneName : null;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"[MainMenuController] 无法解析关卡场景：index={levelIndex}");
            return;
        }

        SceneFlowManager.Instance.LoadScene(sceneName, TransitionMode.SimpleFade);
    }

    private void BindViews()
    {
        if (menuView == null)
        {
            menuView = FindObjectOfType<MainMenuView>();
        }

        ResolvePanelReferences();

        if (menuView != null)
        {
            menuView.Bind(this);
            menuView.InitializeCarousel(levelDatabase);
            menuView.ValidateReferences();
        }
        else
        {
            Debug.LogWarning(
                "[MainMenuController] 未找到 MainMenuView，请在场景中挂载并拖入按钮引用。",
                this);
        }

        CloseAllPanels();
    }

    private void ResolvePanelReferences()
    {
        if (overwriteDialogView == null)
        {
            overwriteDialogView = FindObjectOfType<OverwriteSaveDialogView>();
        }

        if (exitGameDialogView == null)
        {
            exitGameDialogView = FindObjectOfType<ExitGameDialogView>();
        }

        if (levelAchievementView == null)
        {
            levelAchievementView = FindObjectOfType<LevelAchievementView>();
        }

        if (creditsView == null)
        {
            creditsView = FindObjectOfType<CreditsView>();
        }
    }

    private void SubscribeDialogs()
    {
        if (overwriteDialogView != null)
        {
            overwriteDialogView.OnNewGameConfirmed += OnOverwriteNewGameConfirmed;
            overwriteDialogView.OnLevelSelectConfirmed += OnOverwriteLevelSelectConfirmed;
        }

        if (levelAchievementView != null)
        {
            levelAchievementView.OnLevelEnterRequested += RequestEnterLevel;
        }
    }

    private void UnsubscribeDialogs()
    {
        if (overwriteDialogView != null)
        {
            overwriteDialogView.OnNewGameConfirmed -= OnOverwriteNewGameConfirmed;
            overwriteDialogView.OnLevelSelectConfirmed -= OnOverwriteLevelSelectConfirmed;
        }

        if (levelAchievementView != null)
        {
            levelAchievementView.OnLevelEnterRequested -= RequestEnterLevel;
        }
    }

    private void CloseAllPanels()
    {
        overwriteDialogView?.Hide();
        exitGameDialogView?.Hide();
        levelAchievementView?.Hide();
        creditsView?.Hide();
    }

    private void OnOverwriteNewGameConfirmed()
    {
        StartNewGameAndLoadLevel(0);
    }

    private void OnOverwriteLevelSelectConfirmed(int levelIndex)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetCurrentLevel(levelIndex);
        }

        LoadLevelFromMenu(levelIndex);
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void EnsureManagers()
    {
        SaveManager saveManager = SaveManager.Instance ?? FindObjectOfType<SaveManager>();
        if (saveManager == null)
        {
            GameObject saveObject = new GameObject("SaveManager");
            saveManager = saveObject.AddComponent<SaveManager>();
        }

        SceneFlowManager sceneFlowManager = SceneFlowManager.Instance ?? FindObjectOfType<SceneFlowManager>();
        if (sceneFlowManager == null)
        {
            GameObject flowObject = new GameObject("SceneFlowManager");
            sceneFlowManager = flowObject.AddComponent<SceneFlowManager>();
        }

        AudioManager audioManager = AudioManager.Instance ?? FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            GameObject audioObject = new GameObject("AudioManager");
            audioManager = audioObject.AddComponent<AudioManager>();
        }

        if (levelDatabase != null)
        {
            saveManager.SetLevelDatabase(levelDatabase);
            sceneFlowManager.SetLevelDatabase(levelDatabase);
            audioManager.SetLevelDatabase(levelDatabase);
        }

        if (sceneBgmConfig != null)
        {
            sceneFlowManager.SetSceneBgmConfig(sceneBgmConfig);
            audioManager.SetSceneBgmConfig(sceneBgmConfig);
        }

        if (audioDatabase != null)
        {
            audioManager.SetAudioDatabase(audioDatabase);
        }

#if UNITY_EDITOR
        AssignDatabaseIfMissing(saveManager);
        AssignDatabaseIfMissing(sceneFlowManager);
        AssignSceneBgmConfigIfMissing(sceneFlowManager);
        AssignSceneBgmConfigIfMissing(audioManager);
        AssignAudioDatabaseIfMissing(audioManager);
        AssignLevelDatabaseIfMissing(audioManager);
#endif
    }

    private void EnsureAudioDatabase()
    {
        if (audioDatabase != null)
        {
            return;
        }

#if UNITY_EDITOR
        audioDatabase = AssetDatabase.LoadAssetAtPath<AudioDatabaseSO>(
            "Assets/_Game/Data/ScriptableObjects/AudioDatabase.asset");
#endif
    }

    private void AssignAudioDatabaseIfMissing(AudioManager audioManager)
    {
        if (audioManager == null || audioDatabase == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(audioManager);
        SerializedProperty property = serializedObject.FindProperty("audioDatabase");
        if (property != null && property.objectReferenceValue == null)
        {
            property.objectReferenceValue = audioDatabase;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        audioManager.SetAudioDatabase(audioDatabase);
    }

    private void AssignLevelDatabaseIfMissing(AudioManager audioManager)
    {
        if (audioManager == null || levelDatabase == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(audioManager);
        SerializedProperty property = serializedObject.FindProperty("levelDatabase");
        if (property != null && property.objectReferenceValue == null)
        {
            property.objectReferenceValue = levelDatabase;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        audioManager.SetLevelDatabase(levelDatabase);
    }

    private void EnsureSceneBgmConfig()
    {
        if (sceneBgmConfig != null)
        {
            return;
        }

#if UNITY_EDITOR
        sceneBgmConfig = AssetDatabase.LoadAssetAtPath<SceneBgmConfigSO>(
            "Assets/_Game/Data/ScriptableObjects/SceneBgmConfig.asset");
#endif
    }

    private void AssignSceneBgmConfigIfMissing(MonoBehaviour manager)
    {
        if (manager == null || sceneBgmConfig == null)
        {
            return;
        }

        if (manager is SceneFlowManager sceneFlowManager)
        {
            sceneFlowManager.SetSceneBgmConfig(sceneBgmConfig);
            return;
        }

        if (manager is AudioManager audioManager)
        {
            audioManager.SetSceneBgmConfig(sceneBgmConfig);
            return;
        }

        SerializedObject serializedObject = new SerializedObject(manager);
        SerializedProperty property = serializedObject.FindProperty("sceneBgmConfig");
        if (property != null && property.objectReferenceValue == null)
        {
            property.objectReferenceValue = sceneBgmConfig;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private void EnsureLevelDatabase()
    {
#if UNITY_EDITOR
        if (levelDatabase == null)
        {
            levelDatabase = AssetDatabase.LoadAssetAtPath<LevelDatabaseSO>(
                "Assets/_Game/Data/ScriptableObjects/LevelDatabase.asset");
        }
#endif
    }

    private void AssignDatabaseIfMissing(MonoBehaviour manager)
    {
        if (manager == null || levelDatabase == null)
        {
            return;
        }

        if (manager is SaveManager saveManager)
        {
            saveManager.SetLevelDatabase(levelDatabase);
            return;
        }

        if (manager is SceneFlowManager sceneFlowManager)
        {
            sceneFlowManager.SetLevelDatabase(levelDatabase);
        }

        SerializedObject serializedObject = new SerializedObject(manager);
        SerializedProperty property = serializedObject.FindProperty("levelDatabase");
        if (property != null && property.objectReferenceValue == null)
        {
            property.objectReferenceValue = levelDatabase;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
