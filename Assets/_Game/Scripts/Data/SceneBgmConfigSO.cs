using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 场景名与 BGM 音频资源名的映射表。新场景只需在此资产中加一行。
/// </summary>
[CreateAssetMenu(fileName = "SceneBgmConfig", menuName = "SO/SceneBgmConfig")]
public class SceneBgmConfigSO : ScriptableObject
{
    [Serializable]
    public class SceneBgmEntry
    {
        [Tooltip("与 Build Settings 中场景文件名一致")]
        public string sceneName;

        [Tooltip("GameConstants.AudioNames 中的 BGM 名；留空表示该场景不播 BGM")]
        public string bgmAudioName;

        [Tooltip("是否循环播放")]
        public bool loop = true;
    }

    [SerializeField] private List<SceneBgmEntry> entries = new List<SceneBgmEntry>();

    /// <summary>根据场景名解析 BGM 配置；未找到返回 null。</summary>
    public SceneBgmEntry ResolveEntry(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || entries == null)
        {
            return null;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            SceneBgmEntry entry = entries[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.sceneName))
            {
                continue;
            }

            if (string.Equals(entry.sceneName, sceneName, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        return null;
    }
}
