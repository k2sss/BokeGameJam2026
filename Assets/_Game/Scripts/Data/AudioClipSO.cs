using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new AuidoClip",menuName = "SO/AuidoClip")]
public class AudioClipSO : ScriptableObject
{
    public List<AudioClip> audioClipList;
}
