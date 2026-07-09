using UnityEngine;

/// <summary>
/// Editor 测试用：
/// T - 触发 GameOver
/// Y - 触发 Transitioning（场景转场状态）
/// R - 重置为 Playing
/// </summary>
public class GameStateTest : MonoBehaviour
{
    [SerializeField] private KeyCode triggerGameOverKey = KeyCode.T;
    [SerializeField] private KeyCode triggerTransitionKey = KeyCode.Y;
    [SerializeField] private KeyCode resetKey = KeyCode.R;

    private void Update()
    {
        if (GameStateManager.Instance == null)
        {
            return;
        }

        if (Input.GetKeyDown(triggerGameOverKey))
        {
            GameStateManager.Instance.EnterGameOver();
            LogCurrentState("EnterGameOver");
        }

        if (Input.GetKeyDown(triggerTransitionKey))
        {
            GameStateManager.Instance.EnterTransitioning();
            LogCurrentState("EnterTransitioning");
        }

        if (Input.GetKeyDown(resetKey))
        {
            GameStateManager.Instance.ResetToPlaying();
            LogCurrentState("ResetToPlaying");
        }
    }

    private void LogCurrentState(string action)
    {
        bool isPlaying = GameStateManager.Instance.IsPlaying;
        bool isTransitioning = GameStateManager.Instance.IsTransitioning;
        Debug.Log(
            $"[GameStateTest] {action} -> State={GameStateManager.Instance.CurrentState}, " +
            $"IsPlaying={isPlaying}, IsTransitioning={isTransitioning}, timeScale={Time.timeScale}");
    }
}
