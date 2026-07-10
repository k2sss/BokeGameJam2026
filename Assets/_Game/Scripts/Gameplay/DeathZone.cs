using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeathZone : MonoBehaviour
{
    private bool triggered;

    private void Awake()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered)
        {
            return;
        }

        if (!other.CompareTag(GameConstants.Tags.Player))
        {
            return;
        }

        PlayerBody body = other.GetComponent<PlayerBody>();
        if (body == null)
        {
            return;
        }

        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsPlaying)
        {
            return;
        }

        if (SceneFlowManager.Instance == null)
        {
            Debug.LogWarning("[DeathZone] SceneFlowManager not found in scene.");
            return;
        }

        if (SceneFlowManager.Instance.IsLoading)
        {
            return;
        }

        triggered = true;

        int levelIndex = SceneFlowManager.Instance.CurrentLevelIndex;

        // 教学关：无失败面板，黑幕淡入淡出重载当前关。
        if (levelIndex <= 0)
        {
            SceneFlowManager.Instance.ReloadCurrentLevel();
            return;
        }

        GameStateManager.Instance.EnterGameOver();
    }
}
