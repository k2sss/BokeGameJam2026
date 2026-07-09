using UnityEngine;

public enum GameState
{
    Playing,
    GameOver
}

public class GameStateManager : BaseMonoManager<GameStateManager>
{
    public GameState CurrentState { get; private set; } = GameState.Playing;

    public bool IsPlaying => CurrentState == GameState.Playing;

    public void EnterGameOver()
    {
        if (CurrentState == GameState.GameOver)
        {
            return;
        }

        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(GameConstants.AudioNames.GameOver);
        }

        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.Show();
        }

        Debug.Log("[GameStateManager] Game Over");
    }

    public void ResetToPlaying()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.Hide();
        }
    }
}
