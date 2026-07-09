using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private PlayerBody playerA;
    [SerializeField] private PlayerBody playerB;
    [SerializeField] private PlayerBody defaultControlledPlayer;

    [Header("Control")]
    [SerializeField] private float swingForce = 12f;
    [SerializeField] private KeyCode switchKey = KeyCode.F;
    [SerializeField] private KeyCode releaseBothKey = KeyCode.G;

    [Header("Debug")]
    [SerializeField] private bool drawSwingTangent = true;
    [SerializeField] private float tangentPreviewLength = 1.5f;
    [SerializeField] private Color tangentColor = Color.cyan;
    [SerializeField] private Color selectedTangentColor = Color.yellow;
    [SerializeField] private Color attachPreviewColor = Color.green;

    private PlayerBody currentPlayer;

    public PlayerBody CurrentPlayer => currentPlayer;

    public bool IsControlling(PlayerBody body)
    {
        return body != null && currentPlayer == body;
    }

    private void Start()
    {
        InitializePlayers();
    }

    private void Update()
    {
        if (!CanAcceptInput())
        {
            return;
        }

        if (Input.GetKeyDown(releaseBothKey))
        {
            ReleaseBothPlayers();
            return;
        }

        if (Input.GetKeyDown(switchKey))
        {
            SwitchPlayer();
        }
    }

    private void FixedUpdate()
    {
        if (!CanAcceptInput())
        {
            return;
        }

        if (currentPlayer == null)
        {
            return;
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Mathf.Approximately(horizontalInput, 0f))
        {
            return;
        }

        PlayerBody pivotPlayer = GetOtherPlayer(currentPlayer);
        if (pivotPlayer == null)
        {
            return;
        }

        currentPlayer.ApplySwingInput(horizontalInput, swingForce, pivotPlayer.transform.position);
    }

    private void OnDrawGizmos()
    {
        if (!drawSwingTangent)
        {
            return;
        }

        PlayerBody activePlayer = GetDebugCurrentPlayer();
        PlayerBody pivotPlayer = GetOtherPlayer(activePlayer);
        if (activePlayer == null || pivotPlayer == null)
        {
            return;
        }

        Vector2 radius = (Vector2)activePlayer.transform.position - (Vector2)pivotPlayer.transform.position;
        if (radius.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Vector2 tangentA = new Vector2(-radius.y, radius.x).normalized;
        Vector2 tangentB = -tangentA;

        Vector3 origin = activePlayer.transform.position;
        Gizmos.color = tangentColor;
        Gizmos.DrawLine(origin, origin + (Vector3)(tangentA * tangentPreviewLength));
        Gizmos.DrawLine(origin, origin + (Vector3)(tangentB * tangentPreviewLength));

        AttachPoint attachPoint = FindAvailableAttachPoint(activePlayer);
        if (attachPoint != null)
        {
            Gizmos.color = attachPreviewColor;
            Gizmos.DrawLine(origin, attachPoint.transform.position);
            Gizmos.DrawSphere(attachPoint.transform.position, 0.12f);
        }

        float horizontalInput = Application.isPlaying ? Input.GetAxisRaw("Horizontal") : 0f;
        if (Mathf.Approximately(horizontalInput, 0f) || currentPlayer == null)
        {
            return;
        }

        Vector2 selectedTangent = horizontalInput > 0f
            ? (tangentA.x >= tangentB.x ? tangentA : tangentB)
            : (tangentA.x <= tangentB.x ? tangentA : tangentB);

        Gizmos.color = selectedTangentColor;
        Gizmos.DrawLine(origin, origin + (Vector3)(selectedTangent * tangentPreviewLength));
    }

    public void SwitchPlayer()
    {
        if (playerA == null || playerB == null || currentPlayer == null)
        {
            return;
        }

        AttachPoint attachPoint = FindAvailableAttachPoint(currentPlayer);
        if (attachPoint == null)
        {
            return;
        }

        currentPlayer.SnapToPosition(attachPoint.Position);
        currentPlayer.SetFixed(true);

        PlayerBody nextPlayer = GetOtherPlayer(currentPlayer);
        currentPlayer = nextPlayer;

        if (currentPlayer != null)
        {
            currentPlayer.SetFixed(false);
        }
    }

    public void ReleaseBothPlayers()
    {
        if (playerA == null || playerB == null)
        {
            return;
        }

        if (currentPlayer == null)
        {
            currentPlayer = defaultControlledPlayer != null ? defaultControlledPlayer : playerA;
        }

        playerA.SetFixed(false);
        playerB.SetFixed(false);
    }

    public void FixCurrentPlayer()
    {
        if (currentPlayer == null)
        {
            return;
        }

        currentPlayer.SetFixed(true);
    }

    private void InitializePlayers()
    {
        if (playerA == null || playerB == null)
        {
            Debug.LogWarning("PlayerController requires Player A and Player B.");
            return;
        }

        PlayerBody initialPlayer = defaultControlledPlayer;
        if (initialPlayer == null || (initialPlayer != playerA && initialPlayer != playerB))
        {
            initialPlayer = playerA;
        }

        SetCurrentPlayer(initialPlayer);
    }

    private void SetCurrentPlayer(PlayerBody nextPlayer)
    {
        currentPlayer = nextPlayer;

        PlayerBody otherPlayer = GetOtherPlayer(currentPlayer);
        currentPlayer.SetFixed(false);

        if (otherPlayer != null)
        {
            otherPlayer.SetFixed(true);
        }
    }

    private PlayerBody GetOtherPlayer(PlayerBody player)
    {
        if (player == null)
        {
            return null;
        }

        return player == playerA ? playerB : playerA;
    }

    private PlayerBody GetDebugCurrentPlayer()
    {
        if (currentPlayer != null)
        {
            return currentPlayer;
        }

        if (defaultControlledPlayer != null)
        {
            return defaultControlledPlayer;
        }

        return playerA;
    }

    private AttachPoint FindAvailableAttachPoint(PlayerBody playerBody)
    {
        if (playerBody == null)
        {
            return null;
        }

        AttachPoint[] attachPoints = FindObjectsOfType<AttachPoint>();
        AttachPoint bestPoint = null;
        float bestDistanceSqr = float.MaxValue;

        for (int i = 0; i < attachPoints.Length; i++)
        {
            AttachPoint point = attachPoints[i];
            if (!point.CanAttach(playerBody))
            {
                continue;
            }

            float distanceSqr = point.GetDistanceSqr(playerBody);
            if (distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                bestPoint = point;
            }
        }

        return bestPoint;
    }

    private bool CanAcceptInput()
    {
        return GameStateManager.Instance == null || GameStateManager.Instance.IsPlaying;
    }
}
