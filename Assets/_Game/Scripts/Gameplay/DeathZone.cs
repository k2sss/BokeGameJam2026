using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeathZone : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private void Awake()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(GameConstants.Tags.Player))
        {
            return;
        }

        PlayerBody body = other.GetComponent<PlayerBody>();
        if (body == null)
        {
            return;
        }

        if (playerController == null || !playerController.IsControlling(body))
        {
            return;
        }

        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("[DeathZone] GameStateManager not found in scene.");
            return;
        }

        GameStateManager.Instance.EnterGameOver();
    }
}
