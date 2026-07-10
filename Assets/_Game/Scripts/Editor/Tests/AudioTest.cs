using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour
{
    public string audioName;
    public void Play()
    {
        AudioManager.Instance.PlayOneShot(audioName);
    }
}
