/// <summary>
/// 场景切换时的转场模式。
/// </summary>
public enum TransitionMode
{
    /// <summary>过关转场：展示背景图 + 角色图并渐变。</summary>
    LevelComplete,

    /// <summary>简单转场：纯黑屏淡入淡出（如重试当前关）。</summary>
    SimpleFade
}
