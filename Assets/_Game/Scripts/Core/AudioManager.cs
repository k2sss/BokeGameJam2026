using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 从 AudioDatabaseSO 解析音频，不再使用 Resources/Audio。
/// </summary>
public static class AudioAssetLoader
{
    private static AudioDatabaseSO database;

    public static void SetDatabase(AudioDatabaseSO audioDatabase)
    {
        database = audioDatabase;
    }

    public static AudioClip GetAudioAsset(string audioName)
    {
        if (database == null)
        {
            return null;
        }

        return database.ResolveClip(audioName);
    }
}

/// <summary>
/// 跨场景音频管理：BGM（场景/游戏结束）与 SFX（点击等短音效）分通道。
/// </summary>
public class AudioManager : BaseMonoManager<AudioManager>
{
    private enum MusicContext
    {
        None,
        Scene,
        GameOver
    }

    [Header("配置")]
    [SerializeField] private AudioDatabaseSO audioDatabase;
    [SerializeField] private SceneBgmConfigSO sceneBgmConfig;
    [SerializeField] private LevelDatabaseSO levelDatabase;

    [Header("游戏结束音乐")]
    [SerializeField] private bool gameOverMusicLoop = true;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private MusicContext currentContext = MusicContext.None;
    private string currentMusicName;
    private float musicVolume = GameConstants.DefaultAudioVolume;
    private float sfxVolume = GameConstants.DefaultAudioVolume;
    private int currentVolumeLevelIndex = -1;

    /// <summary>当前音量绑定的关卡索引；-1 表示非关卡场景（如主菜单）。</summary>
    public int CurrentVolumeLevelIndex => currentVolumeLevelIndex;

    public float MusicVolume => musicVolume;

    public float SfxVolume => sfxVolume;

    protected override bool PersistAcrossScenes => true;

    protected override void Awake()
    {
        base.Awake();
#if UNITY_EDITOR
        EnsureAudioDatabaseInEditor();
        EnsureLevelDatabaseInEditor();
#endif
        ApplyAudioDatabase();
        EnsureAudioSources();
    }

    public void SetAudioDatabase(AudioDatabaseSO database)
    {
        if (database != null)
        {
            audioDatabase = database;
            ApplyAudioDatabase();
        }
    }

    public void SetSceneBgmConfig(SceneBgmConfigSO config)
    {
        if (config != null)
        {
            sceneBgmConfig = config;
        }
    }

    public void SetLevelDatabase(LevelDatabaseSO database)
    {
        if (database != null)
        {
            levelDatabase = database;
        }
    }

    /// <summary>根据当前已加载场景播放 BGM（由 SceneFlowManager.FinishLoad 调用）。</summary>
    public void PlayMusicForScene(string sceneName)
    {
        ApplyVolumesForScene(sceneName);

        if (sceneBgmConfig == null)
        {
            Debug.LogWarning("[AudioManager] 未配置 SceneBgmConfigSO，跳过场景 BGM。");
            return;
        }

        SceneBgmConfigSO.SceneBgmEntry entry = sceneBgmConfig.ResolveEntry(sceneName);
        if (entry == null || string.IsNullOrWhiteSpace(entry.bgmAudioName))
        {
            StopMusic();
            currentContext = MusicContext.Scene;
            currentMusicName = null;
            return;
        }

        PlayMusic(entry.bgmAudioName, MusicContext.Scene, entry.loop);
    }

    /// <summary>播放当前场景的 BGM（用于无场景重载的兜底恢复）。</summary>
    public void PlayMusicForActiveScene()
    {
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>进入游戏结束音乐（占用 BGM 通道，停止关卡 BGM）。</summary>
    public void EnterGameOverMusic()
    {
        PlayMusic(GameConstants.AudioNames.GameOverBGM, MusicContext.GameOver, gameOverMusicLoop);
    }

    public void StopMusic()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }

        currentContext = MusicContext.None;
        currentMusicName = null;
    }

    public void PlayButtonClick()
    {
        PlaySfx(GameConstants.AudioNames.ButtonClick);
    }

    public void PlayLevelClear(float volumeScale = 1f)
    {
        PlaySfx(GameConstants.AudioNames.LevelClear, volumeScale);
    }

    public void PlayAttachPoint(float volumeScale = 1f)
    {
        PlaySfx(GameConstants.AudioNames.AttachPoint, volumeScale);
    }

    /// <summary>倒计时 tick / 结束提示音。</summary>
    public void PlayCountdown(float volumeScale = 1f)
    {
        PlaySfx(GameConstants.AudioNames.Countdown, volumeScale);
    }

    /// <summary>碰撞、撞击类音效。</summary>
    public void PlayCollision(float volumeScale = 1f)
    {
        PlaySfx(GameConstants.AudioNames.Collision, volumeScale);
    }

    /// <summary>玩家被攻击 / 受击音效。</summary>
    public void PlayPlayerHit(float volumeScale = 1f)
    {
        PlaySfx(GameConstants.AudioNames.PlayerHit, volumeScale);
    }

    /// <summary>掉落、坠落音效。</summary>
    public void PlayFall(float volumeScale = 1f)
    {
        PlaySfx(GameConstants.AudioNames.Fall, volumeScale);
    }

    /// <summary>落水、入水音效。</summary>
    public void PlayWaterSplash(float volumeScale = 1f)
    {
        PlaySfx(GameConstants.AudioNames.WaterSplash, volumeScale);
    }

    /// <summary>剧情对话逐字打印音效。</summary>
    public void PlayStoryTyping(float volumeScale = 1f)
    {
        PlaySfx(GameConstants.AudioNames.StoryTyping, volumeScale);
    }

    public void PlaySfx(string audioName, float volumeScale = 1f)
    {
        AudioClip clip = GetAudioClip(audioName);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] 未找到音效：{audioName}");
            return;
        }

        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    /// <summary>兼容旧调用，等同于 PlaySfx。</summary>
    public void PlayOneShot(string audioName, float volume = 1f)
    {
        PlaySfx(audioName, volume);
    }

    public void PlayOneShot(AudioClip audioClip, float volume = 1f)
    {
        if (audioClip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(audioClip, sfxVolume * volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = musicVolume;
        }

        PersistMusicVolumeForCurrentLevel();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PersistSfxVolumeForCurrentLevel();
    }

    /// <summary>为指定关卡设置 BGM 音量（写入存档；若正在该关则立即生效）。</summary>
    public void SetLevelMusicVolume(int levelIndex, float volume)
    {
        float clamped = Mathf.Clamp01(volume);
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetLevelBgmVolume(levelIndex, clamped);
        }

        if (currentVolumeLevelIndex == levelIndex)
        {
            musicVolume = clamped;
            if (bgmSource != null)
            {
                bgmSource.volume = musicVolume;
            }
        }
    }

    /// <summary>为指定关卡设置 SFX 音量（写入存档；若正在该关则立即生效）。</summary>
    public void SetLevelSfxVolume(int levelIndex, float volume)
    {
        float clamped = Mathf.Clamp01(volume);
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetLevelSfxVolume(levelIndex, clamped);
        }

        if (currentVolumeLevelIndex == levelIndex)
        {
            sfxVolume = clamped;
        }
    }

    private void PlayMusic(string audioName, MusicContext context, bool loop)
    {
        if (currentContext == context
            && currentMusicName == audioName
            && bgmSource != null
            && bgmSource.isPlaying)
        {
            bgmSource.volume = musicVolume;
            return;
        }

        currentContext = context;
        currentMusicName = audioName;

        AudioClip clip = GetAudioClip(audioName);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] 未找到音乐：{audioName}");
            StopMusic();
            return;
        }

        bgmSource.loop = loop;
        bgmSource.clip = clip;
        bgmSource.volume = musicVolume;
        bgmSource.Play();
    }

    private AudioClip GetAudioClip(string audioName)
    {
        return AudioAssetLoader.GetAudioAsset(audioName);
    }

    private void ApplyVolumesForScene(string sceneName)
    {
        int levelIndex = ResolveLevelIndex(sceneName);
        currentVolumeLevelIndex = levelIndex;

        if (levelIndex >= 0 && SaveManager.Instance != null)
        {
            musicVolume = SaveManager.Instance.GetLevelBgmVolume(levelIndex);
            sfxVolume = SaveManager.Instance.GetLevelSfxVolume(levelIndex);
        }
        else
        {
            musicVolume = GameConstants.DefaultAudioVolume;
            sfxVolume = GameConstants.DefaultAudioVolume;
        }

        if (bgmSource != null)
        {
            bgmSource.volume = musicVolume;
        }
    }

    private int ResolveLevelIndex(string sceneName)
    {
        if (levelDatabase == null || string.IsNullOrWhiteSpace(sceneName))
        {
            return -1;
        }

        return levelDatabase.GetIndexByScene(sceneName);
    }

    private void PersistMusicVolumeForCurrentLevel()
    {
        if (currentVolumeLevelIndex < 0 || SaveManager.Instance == null)
        {
            return;
        }

        SaveManager.Instance.SetLevelBgmVolume(currentVolumeLevelIndex, musicVolume);
    }

    private void PersistSfxVolumeForCurrentLevel()
    {
        if (currentVolumeLevelIndex < 0 || SaveManager.Instance == null)
        {
            return;
        }

        SaveManager.Instance.SetLevelSfxVolume(currentVolumeLevelIndex, sfxVolume);
    }

    private void ApplyAudioDatabase()
    {
        if (audioDatabase != null)
        {
            AudioAssetLoader.SetDatabase(audioDatabase);
        }
    }

#if UNITY_EDITOR
    private void EnsureAudioDatabaseInEditor()
    {
        if (audioDatabase != null)
        {
            return;
        }

        audioDatabase = AssetDatabase.LoadAssetAtPath<AudioDatabaseSO>(
            "Assets/_Game/Data/ScriptableObjects/AudioDatabase.asset");
    }

    private void EnsureLevelDatabaseInEditor()
    {
        if (levelDatabase != null)
        {
            return;
        }

        levelDatabase = AssetDatabase.LoadAssetAtPath<LevelDatabaseSO>(
            "Assets/_Game/Data/ScriptableObjects/LevelDatabase.asset");
    }
#endif

    private void EnsureAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            bgmSource = sources[0];
            sfxSource = sources[1];
        }
        else if (sources.Length == 1)
        {
            bgmSource = sources[0];
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        ConfigureMusicSource(bgmSource);
        ConfigureSfxSource(sfxSource);
    }

    private static void ConfigureMusicSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = true;
        source.ignoreListenerPause = true;
    }

    private static void ConfigureSfxSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = false;
        source.ignoreListenerPause = true;
    }
}
