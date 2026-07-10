using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景切换核心管理器：统一入口、异步加载、配合 SceneTransitionUI 播放转场。
/// 常驻 DontDestroyOnLoad，请勿在多个场景中重复保留。
/// </summary>
public class SceneFlowManager : BaseMonoManager<SceneFlowManager>
{
    [Header("配置")]
    [SerializeField] private LevelDatabaseSO levelDatabase;
    [SerializeField] private SceneBgmConfigSO sceneBgmConfig;

    [Header("组件")]
    [SerializeField] private SceneTransitionUI transitionUI;

    private bool isLoading;

    protected override bool PersistAcrossScenes => true;

    public bool IsLoading => isLoading;

    public string CurrentSceneName => SceneManager.GetActiveScene().name;

    public int CurrentLevelIndex =>
        levelDatabase != null ? levelDatabase.GetIndexByScene(CurrentSceneName) : -1;

    public bool IsLastLevel =>
        levelDatabase != null && CurrentLevelIndex >= 0 &&
        levelDatabase.IsLastLevel(CurrentLevelIndex);

    protected override void Awake()
    {
        base.Awake();
        EnsureTransitionUI();
    }

    /// <summary>运行时或主界面初始化时注入关卡配置表。</summary>
    public void SetLevelDatabase(LevelDatabaseSO database)
    {
        if (database != null)
        {
            levelDatabase = database;
        }
    }

    public void SetSceneBgmConfig(SceneBgmConfigSO config)
    {
        if (config != null)
        {
            sceneBgmConfig = config;
        }

        if (AudioManager.Instance != null && config != null)
        {
            AudioManager.Instance.SetSceneBgmConfig(config);
        }
    }

    public void LoadScene(string sceneName, TransitionMode mode = TransitionMode.SimpleFade)
    {
        StartLoad(new SceneLoadRequest
        {
            TargetSceneName = sceneName,
            Mode = mode
        });
    }

    public void LoadLevel(int levelIndex)
    {
        if (!EnsureDatabase())
        {
            return;
        }

        LevelEntry entry = levelDatabase.GetLevel(levelIndex);
        if (entry == null)
        {
            Debug.LogError($"[SceneFlowManager] 无效关卡索引：{levelIndex}");
            return;
        }

        StartLoad(new SceneLoadRequest
        {
            TargetSceneName = entry.sceneName,
            Mode = TransitionMode.LevelComplete,
            BackgroundSprite = entry.backgroundSprite,
            CharacterSprite = entry.characterSprite
        });
    }

    public void LoadNextLevel()
    {
        if (!EnsureDatabase())
        {
            return;
        }

        int currentIndex = CurrentLevelIndex;
        string nextSceneName = levelDatabase.ResolveNextSceneNameByCurrentScene(CurrentSceneName);
        TransitionMode mode = TransitionMode.SimpleFade;
        Sprite background = null;
        Sprite character = null;

        // 还有下一关时使用过关图文转场，并展示「即将进入关卡」的素材
        if (currentIndex >= 0 && !levelDatabase.IsLastLevel(currentIndex))
        {
            LevelEntry nextEntry = levelDatabase.GetNextLevel(currentIndex);
            if (nextEntry != null)
            {
                mode = TransitionMode.LevelComplete;
                background = nextEntry.backgroundSprite;
                character = nextEntry.characterSprite;
            }
        }

        StartLoad(new SceneLoadRequest
        {
            TargetSceneName = nextSceneName,
            Mode = mode,
            BackgroundSprite = background,
            CharacterSprite = character
        });
    }

    public void ReloadCurrentLevel()
    {
        StartLoad(new SceneLoadRequest
        {
            TargetSceneName = CurrentSceneName,
            Mode = TransitionMode.SimpleFade
        });
    }

    public void LoadMainMenu(TransitionMode mode = TransitionMode.SimpleFade)
    {
        if (!EnsureDatabase())
        {
            return;
        }

        StartLoad(new SceneLoadRequest
        {
            TargetSceneName = levelDatabase.MainMenuSceneName,
            Mode = mode
        });
    }

    private void StartLoad(SceneLoadRequest request)
    {
        if (isLoading)
        {
            Debug.LogWarning("[SceneFlowManager] 正在加载场景，忽略重复请求。");
            return;
        }

        if (string.IsNullOrWhiteSpace(request.TargetSceneName))
        {
            Debug.LogError("[SceneFlowManager] 目标场景名为空，无法加载。");
            return;
        }

        PrepareGameStateForLoad();
        StartCoroutine(LoadSceneRoutine(request));
    }

    /// <summary>统一的场景加载协程：转场淡出 → 异步加载 → 转场淡入 → 恢复 Playing。</summary>
    private IEnumerator LoadSceneRoutine(SceneLoadRequest request)
    {
        isLoading = true;
        EnsureTransitionUI();

        Debug.Log(
            $"[SceneFlowManager] 开始加载：{CurrentSceneName} -> {request.TargetSceneName} " +
            $"({request.Mode})");

        yield return PlayTransitionOut(request);

        if (!Application.CanStreamedLevelBeLoaded(request.TargetSceneName))
        {
            Debug.LogError(
                $"[SceneFlowManager] 场景 '{request.TargetSceneName}' 无法加载。" +
                "请确认场景存在且已加入 Build Settings。");
            yield return PlayTransitionIn(request.Mode);
            FinishLoad();
            yield break;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(request.TargetSceneName);
        if (loadOperation == null)
        {
            Debug.LogError($"[SceneFlowManager] LoadSceneAsync 返回 null：{request.TargetSceneName}");
            yield return PlayTransitionIn(request.Mode);
            FinishLoad();
            yield break;
        }

        loadOperation.allowSceneActivation = false;
        while (loadOperation.progress < 0.9f)
        {
            yield return null;
        }

        loadOperation.allowSceneActivation = true;
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        yield return PlayTransitionIn(request.Mode);
        FinishLoad();

        Debug.Log($"[SceneFlowManager] 加载完成：{SceneManager.GetActiveScene().name}");
    }

    private IEnumerator PlayTransitionOut(SceneLoadRequest request)
    {
        if (transitionUI == null)
        {
            yield break;
        }

        if (request.Mode == TransitionMode.LevelComplete)
        {
            yield return transitionUI.PlayLevelTransitionOut(
                request.BackgroundSprite,
                request.CharacterSprite);
        }
        else
        {
            yield return transitionUI.PlaySimpleFadeOut();
        }
    }

    private IEnumerator PlayTransitionIn(TransitionMode mode)
    {
        if (transitionUI == null)
        {
            yield break;
        }

        if (mode == TransitionMode.LevelComplete)
        {
            yield return transitionUI.PlayLevelTransitionIn();
        }
        else
        {
            yield return transitionUI.PlaySimpleFadeIn();
        }
    }

    private void PrepareGameStateForLoad()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.PrepareForSceneLoad();
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    private void FinishLoad()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ResetToPlaying();
        }
        else
        {
            Time.timeScale = 1f;
        }

        isLoading = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusicForScene(SceneManager.GetActiveScene().name);
        }
    }

    private void EnsureTransitionUI()
    {
        if (transitionUI == null)
        {
            transitionUI = GetComponent<SceneTransitionUI>();
        }

        if (transitionUI == null)
        {
            transitionUI = gameObject.AddComponent<SceneTransitionUI>();
        }
    }

    private bool EnsureDatabase()
    {
        if (levelDatabase != null)
        {
            return true;
        }

        Debug.LogError("[SceneFlowManager] 未配置 LevelDatabaseSO，请在 Inspector 中指定。");
        return false;
    }

    /// <summary>单次场景加载请求参数。</summary>
    private struct SceneLoadRequest
    {
        public string TargetSceneName;
        public TransitionMode Mode;
        public Sprite BackgroundSprite;
        public Sprite CharacterSprite;
    }
}
