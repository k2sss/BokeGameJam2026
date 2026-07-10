using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AudioHelper
{
    
}

public static class AudioAssetLoader
{
    //Resources.load Path
    public const string AudioAssetLoadPath = "Audio";
    private static Dictionary<string, AudioClipSO> audioClipDic;

    public static AudioClip GetAudioAsset(string audioName)
    {
        if (string.IsNullOrEmpty(audioName))
        {
            return null;
        }

        EnsureLoaded();
        if (!audioClipDic.TryGetValue(audioName, out var audioClipSO) || audioClipSO == null)
        {
            return null;
        }

        var clipList = audioClipSO.audioClipList;
        if (clipList == null || clipList.Count == 0)
        {
            return null;
        }

        var validClipList = clipList.Where(clip => clip != null).ToList();
        if (validClipList.Count == 0)
        {
            return null;
        }

        if (validClipList.Count == 1)
        {
            return validClipList[0];
        }

        var randomIndex = Random.Range(0, validClipList.Count);
        return validClipList[randomIndex];
    }

    private static void EnsureLoaded()
    {
        if (audioClipDic != null)
        {
            return;
        }

        audioClipDic = new Dictionary<string, AudioClipSO>();
        var audioClipSOList = Resources.LoadAll<AudioClipSO>(AudioAssetLoadPath);
        foreach (var audioClipSO in audioClipSOList)
        {
            if (audioClipSO == null)
            {
                continue;
            }

            if (audioClipDic.ContainsKey(audioClipSO.name))
            {
                Debug.LogWarning($"Duplicate AudioClipSO name found: {audioClipSO.name}, later asset will be ignored.");
                continue;
            }

            audioClipDic.Add(audioClipSO.name, audioClipSO);
        }
    }
}


[RequireComponent(typeof(AudioSource))]
public class AudioManager : BaseMonoManager<AudioManager>
{
    private AudioSource audioSource;
    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayOneShot(string audioName, float volume = 1f)
    {
        var clip = GetAudioClip(audioName);
        if (clip == null)
        {
            Debug.LogError($"there is no audio named:{audioName}");
            return;
        }

        audioSource.PlayOneShot(clip, volume);
    }
    
    public void PlayOneShot(AudioClip audioClip,float volume = 1f)
    {
        audioSource.PlayOneShot(audioClip,volume);
    }

    private AudioClip GetAudioClip(string audioName)
    {
        return AudioAssetLoader.GetAudioAsset(audioName);
    }
    
}
