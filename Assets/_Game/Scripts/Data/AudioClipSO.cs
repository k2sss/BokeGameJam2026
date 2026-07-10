using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "new AuidoClip", menuName = "SO/AuidoClip")]
public class AudioClipSO : ScriptableObject
{
    public List<AudioClip> audioClipList;

    /// <summary>取一个可用 AudioClip；多 clip 时随机。</summary>
    public AudioClip GetClip()
    {
        if (audioClipList == null || audioClipList.Count == 0)
        {
            return null;
        }

        List<AudioClip> validClips = audioClipList.Where(clip => clip != null).ToList();
        if (validClips.Count == 0)
        {
            return null;
        }

        if (validClips.Count == 1)
        {
            return validClips[0];
        }

        return validClips[Random.Range(0, validClips.Count)];
    }
}
