using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private PlayerBody playerA;
    [SerializeField] private PlayerBody playerB;
    [SerializeField] private PlayerBody defaultControlledPlayer;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Header("Control")]
    [SerializeField] private float swingForce = 12f;
    [SerializeField] private KeyCode switchKey = KeyCode.F;
    [SerializeField] private KeyCode releaseBothKey = KeyCode.G;
    [SerializeField] private float reattachCooldown = 0.35f;
    [Header("Debug")]
    [SerializeField] private bool drawSwingTangent = true;
    [SerializeField] private float tangentPreviewLength = 1.5f;
    [SerializeField] private Color tangentColor = Color.cyan;
    [SerializeField] private Color selectedTangentColor = Color.yellow;
    [SerializeField] private Color attachPreviewColor = Color.green;

    private PlayerBody currentPlayer;
    private bool bothReleased;
    private PlayerBody blockedAutoAttachPlayer;
    private float blockedAutoAttachUntil;

    public PlayerBody CurrentPlayer => currentPlayer;

    public bool IsControlling(PlayerBody body)
    {
        return body != null && currentPlayer == body;
    }

    private void Start()
    {
        ResolveVirtualCamera();
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

        TryAutoAttachReleasedPlayers();
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

        AttachPoint attachPoint = FindAvailableAttachPoint(activePlayer, false);
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

        AttachPoint attachPoint = FindAvailableAttachPoint(currentPlayer, false);
        if (attachPoint == null)
        {
            ReleaseBothPlayers();
            return;
        }

        AttachPlayerToPoint(currentPlayer, attachPoint);
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

        PlayerBody movingPlayer = currentPlayer;
        PlayerBody fixedPlayer = GetOtherPlayer(movingPlayer);
        Vector2 movingVelocity = movingPlayer != null ? movingPlayer.Velocity : Vector2.zero;

        bothReleased = true;
        playerA.SetFixed(false);
        playerB.SetFixed(false);

        if (movingPlayer != null)
        {
            movingPlayer.SetVelocity(movingVelocity);
        }

        if (fixedPlayer != null)
        {
            fixedPlayer.SetVelocity(movingVelocity * 0.5f);
            blockedAutoAttachPlayer = fixedPlayer;
            blockedAutoAttachUntil = Time.time + reattachCooldown;
        }
        else
        {
            blockedAutoAttachPlayer = null;
            blockedAutoAttachUntil = 0f;
        }
    }

    public void FixCurrentPlayer()
    {
        if (currentPlayer == null)
        {
            return;
        }

        currentPlayer.SetFixed(true);
        bothReleased = false;
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
        if (currentPlayer != null)
        {
            currentPlayer.SetFixed(false);
        }

        if (otherPlayer != null)
        {
            otherPlayer.SetFixed(true);
        }

        bothReleased = false;
        blockedAutoAttachPlayer = null;
        blockedAutoAttachUntil = 0f;
        UpdateVirtualCameraFollow();
    }

    private void AttachPlayerToPoint(PlayerBody playerToAttach, AttachPoint attachPoint)
    {
        if (playerToAttach == null || attachPoint == null)
        {
            return;
        }

        playerToAttach.AttachToPoint(attachPoint.transform);
        playerToAttach.SetFixed(true);

        PlayerBody otherPlayer = GetOtherPlayer(playerToAttach);
        currentPlayer = otherPlayer;

        if (currentPlayer != null)
        {
            currentPlayer.SetFixed(false);
        }

        bothReleased = false;
        blockedAutoAttachPlayer = null;
        blockedAutoAttachUntil = 0f;
        UpdateVirtualCameraFollow();
    }

    private void TryAutoAttachReleasedPlayers()
    {
        if (!bothReleased)
        {
            return;
        }

        PlayerBody firstCandidate = currentPlayer;
        AttachPoint attachPoint = FindAvailableAttachPoint(firstCandidate, true);
        if (attachPoint != null)
        {
            AttachPlayerToPoint(firstCandidate, attachPoint);
            return;
        }

        PlayerBody secondCandidate = GetOtherPlayer(currentPlayer);
        attachPoint = FindAvailableAttachPoint(secondCandidate, true);
        if (attachPoint != null)
        {
            AttachPlayerToPoint(secondCandidate, attachPoint);
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

    private AttachPoint FindAvailableAttachPoint(PlayerBody playerBody, bool respectBlock)
    {
        if (playerBody == null)
        {
            return null;
        }

        if (respectBlock && playerBody == blockedAutoAttachPlayer && Time.time < blockedAutoAttachUntil)
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

    private void ResolveVirtualCamera()
    {
        if (virtualCamera != null)
        {
            return;
        }

        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera == null)
        {
            Debug.LogWarning("PlayerController could not find a CinemachineVirtualCamera in the scene.");
        }
    }

    private void UpdateVirtualCameraFollow()
    {
        ResolveVirtualCamera();
        if (virtualCamera == null)
        {
            return;
        }

        virtualCamera.Follow = currentPlayer != null ? currentPlayer.transform : null;
    }
}
