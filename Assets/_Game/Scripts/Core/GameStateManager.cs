using UnityEngine;

public enum GameState
{
    /// <summary>正常游戏中，接受玩家输入。</summary>
    Playing,

    /// <summary>玩家死亡，显示 Game Over UI。</summary>
    GameOver,

    /// <summary>场景切换/过关转场中，冻结游戏逻辑与输入。</summary>
    Transitioning
}

public class GameStateManager : BaseMonoManager<GameStateManager>
{
    public GameState CurrentState { get; private set; } = GameState.Playing;

    /// <summary>是否处于可操作的游玩状态。</summary>
    public bool IsPlaying => CurrentState == GameState.Playing;

    /// <summary>是否处于场景转场中。</summary>
    public bool IsTransitioning => CurrentState == GameState.Transitioning;

    public void EnterGameOver()
    {
        if (CurrentState == GameState.GameOver || CurrentState == GameState.Transitioning)
        {
            return;
        }

        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.EnterGameOverMusic();
        }

        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.Show();
        }

        Debug.Log("[GameStateManager] Game Over");
    }

    /// <summary>
    /// 进入场景转场状态，暂停游戏并阻止玩家输入。
    /// </summary>
    public void EnterTransitioning()
    {
        if (CurrentState == GameState.Transitioning || CurrentState == GameState.GameOver)
        {
            return;
        }

        CurrentState = GameState.Transitioning;
        Time.timeScale = 0f;

        Debug.Log("[GameStateManager] Enter Transitioning");
    }

    /// <summary>
    /// 场景切换开始前调用。可从 Playing 或 GameOver 进入转场（如重试、过关）。
    /// </summary>
    public void PrepareForSceneLoad()
    {
        if (CurrentState == GameState.Transitioning)
        {
            return;
        }

        if (CurrentState == GameState.GameOver && GameOverUI.Instance != null)
        {
            GameOverUI.Instance.Hide();
        }

        CurrentState = GameState.Transitioning;
        Time.timeScale = 0f;

        Debug.Log("[GameStateManager] Prepare for scene load");
    }

    /// <summary>重置为正常游玩状态（场景加载完成或重试后调用）。</summary>
    public void ResetToPlaying()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.Hide();
        }

        Debug.Log("[GameStateManager] Reset to Playing");
    }
}
