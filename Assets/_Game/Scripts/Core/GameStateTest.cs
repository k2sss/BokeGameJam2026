using UnityEngine;

/// <summary>
/// Editor 测试用：Play 模式下按 T 触发 GameOver，按 R 重置状态。
/// </summary>
public class GameStateTest : MonoBehaviour
{
    [SerializeField] private KeyCode triggerGameOverKey = KeyCode.T;
    [SerializeField] private KeyCode resetKey = KeyCode.R;

    private void Update()
    {
        if (Input.GetKeyDown(triggerGameOverKey))
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.EnterGameOver();
            }
            else
            {
                Debug.LogWarning("[GameStateTest] GameStateManager not found in scene.");
            }
        }

        if (Input.GetKeyDown(resetKey))
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ResetToPlaying();
                Debug.Log("[GameStateTest] Reset to Playing");
            }
        }
    }
}
