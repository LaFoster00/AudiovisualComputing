using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTester : MonoBehaviour
{
    [SerializeField]
    private SinGenerator _generator;
    
    [SerializeField]
    private ChannelSend _send;
    
    private void Start()
    {
        _send = new ChannelSend(_generator, 1);
        AudioManager.Instance.WaveProvider.AddMixer(_send);
    }
}
