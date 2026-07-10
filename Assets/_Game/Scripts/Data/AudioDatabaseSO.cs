using UnityEngine;

/// <summary>
/// 音频资源总表：与 LevelDatabase 一样放在 Data/ScriptableObjects 下统一管理。
/// </summary>
[CreateAssetMenu(fileName = "AudioDatabase", menuName = "SO/AudioDatabase")]
public class AudioDatabaseSO : ScriptableObject
{
    [Header("BGM")]
    [SerializeField] private AudioClipSO mainMenuBgm;
    [SerializeField] private AudioClipSO level1Bgm;
    [SerializeField] private AudioClipSO level2Bgm;
    [SerializeField] private AudioClipSO level3Bgm;
    [SerializeField] private AudioClipSO level4Bgm;
    [SerializeField] private AudioClipSO gameOverBgm;

    [Header("UI / 流程音效")]
    [SerializeField] private AudioClipSO buttonClick;
    [SerializeField] private AudioClipSO levelClear;

    [Header("玩法音效")]
    [SerializeField] private AudioClipSO attachPoint;
    [SerializeField] private AudioClipSO countdown;
    [SerializeField] private AudioClipSO collision;
    [SerializeField] private AudioClipSO playerHit;
    [SerializeField] private AudioClipSO fall;
    [SerializeField] private AudioClipSO waterSplash;
    [SerializeField] private AudioClipSO storyTyping;

    public AudioClip ResolveClip(string audioName)
    {
        AudioClipSO clipSO = ResolveClipSO(audioName);
        return clipSO != null ? clipSO.GetClip() : null;
    }

    public AudioClipSO ResolveClipSO(string audioName)
    {
        if (string.IsNullOrEmpty(audioName))
        {
            return null;
        }

        if (audioName == GameConstants.AudioNames.MainMenuBGM)
        {
            return mainMenuBgm;
        }

        if (audioName == GameConstants.AudioNames.Level1BGM)
        {
            return level1Bgm;
        }

        if (audioName == GameConstants.AudioNames.Level2BGM)
        {
            return level2Bgm;
        }

        if (audioName == GameConstants.AudioNames.Level3BGM)
        {
            return level3Bgm;
        }

        if (audioName == GameConstants.AudioNames.Level4BGM)
        {
            return level4Bgm;
        }

        if (audioName == GameConstants.AudioNames.GameOverBGM
            || audioName == GameConstants.AudioNames.GameOver)
        {
            return gameOverBgm;
        }

        if (audioName == GameConstants.AudioNames.ButtonClick)
        {
            return buttonClick;
        }

        if (audioName == GameConstants.AudioNames.LevelClear)
        {
            return levelClear;
        }

        if (audioName == GameConstants.AudioNames.AttachPoint)
        {
            return attachPoint;
        }

        if (audioName == GameConstants.AudioNames.Countdown)
        {
            return countdown;
        }

        if (audioName == GameConstants.AudioNames.Collision)
        {
            return collision;
        }

        if (audioName == GameConstants.AudioNames.PlayerHit)
        {
            return playerHit;
        }

        if (audioName == GameConstants.AudioNames.Fall)
        {
            return fall;
        }

        if (audioName == GameConstants.AudioNames.WaterSplash)
        {
            return waterSplash;
        }

        if (audioName == GameConstants.AudioNames.StoryTyping)
        {
            return storyTyping;
        }

        return null;
    }
}
