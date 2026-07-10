using System;
using UnityEngine;

/// <summary>
/// 单个关卡的配置数据（场景名与转场素材）。
/// </summary>
[Serializable]
public class LevelEntry
{
    [Tooltip("Build Settings 中注册的场景名")]
    public string sceneName;

    [Tooltip("关卡显示名，可用于 UI 或调试")]
    public string displayName;

    [Tooltip("过关转场时展示的背景图")]
    public Sprite backgroundSprite;

    [Tooltip("过关转场时展示的角色图")]
    public Sprite characterSprite;

    /// <summary>配置是否包含有效场景名。</summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(sceneName);
}
