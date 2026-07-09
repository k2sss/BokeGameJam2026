using System;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    private enum CommandType
    {
        MoveToLocalOffset,
        MoveToWorldPosition,
        Wait
    }

    [Serializable]
    private class MoveCommand
    {
        [SerializeField] private CommandType commandType = CommandType.MoveToLocalOffset;
        [SerializeField] private Vector3 target;
        [Min(0f)]
        [SerializeField] private float duration = 1f;

        public CommandType Type => commandType;
        public Vector3 Target => target;
        public float Duration => duration;
    }

    [Header("Playback")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool useUnscaledTime;

    [Header("Commands")]
    [SerializeField] private List<MoveCommand> commands = new List<MoveCommand>();

    private Vector3 startPosition;
    private int currentCommandIndex;
    private float commandElapsedTime;
    private Vector3 commandStartPosition;
    private bool isPlaying;

    private void Start()
    {
        startPosition = transform.position;
        commandStartPosition = startPosition;

        if (playOnStart)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!isPlaying || commands.Count == 0)
        {
            return;
        }

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        MoveCommand command = commands[currentCommandIndex];

        if (command.Duration <= 0f)
        {
            CompleteCurrentCommand(command);
            return;
        }

        commandElapsedTime += deltaTime;
        float progress = Mathf.Clamp01(commandElapsedTime / command.Duration);

        if (command.Type == CommandType.MoveToLocalOffset || command.Type == CommandType.MoveToWorldPosition)
        {
            transform.position = Vector3.Lerp(commandStartPosition, GetCommandTarget(command), progress);
        }

        if (progress >= 1f)
        {
            CompleteCurrentCommand(command);
        }
    }

    public void Play()
    {
        if (commands.Count == 0)
        {
            return;
        }

        isPlaying = true;
        currentCommandIndex = Mathf.Clamp(currentCommandIndex, 0, commands.Count - 1);
        commandElapsedTime = 0f;
        commandStartPosition = transform.position;
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void ResetToStart()
    {
        isPlaying = false;
        currentCommandIndex = 0;
        commandElapsedTime = 0f;
        transform.position = startPosition;
        commandStartPosition = startPosition;
    }

    private void CompleteCurrentCommand(MoveCommand command)
    {
        if (command.Type == CommandType.MoveToLocalOffset || command.Type == CommandType.MoveToWorldPosition)
        {
            transform.position = GetCommandTarget(command);
        }

        AdvanceToNextCommand();
    }

    private void AdvanceToNextCommand()
    {
        commandElapsedTime = 0f;
        currentCommandIndex++;

        if (currentCommandIndex >= commands.Count)
        {
            if (!loop)
            {
                isPlaying = false;
                currentCommandIndex = commands.Count - 1;
                return;
            }

            currentCommandIndex = 0;
        }

        commandStartPosition = transform.position;
    }

    private Vector3 GetCommandTarget(MoveCommand command)
    {
        if (command.Type == CommandType.MoveToLocalOffset)
        {
            return startPosition + command.Target;
        }

        return command.Target;
    }
}
