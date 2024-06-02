using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTester : MonoBehaviour
{
    [SerializeField]
    private Oscillator _generator;
    
    [SerializeField]
    private ChannelSend _send;
    
    private void Start()
    {
        AudioManager.Instance.SampleProvider.AddMixer(_send);
    }
}
