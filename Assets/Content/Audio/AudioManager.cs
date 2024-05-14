using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NaughtyAttributes;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private WaveOutEvent _audioOut;

    public int deviceNumber = -1;
    public int desiredLatency = 100;

    [SerializeField]
    public SampleProvider SampleProvider;

    public static AudioManager Instance { get; private set; }

    public WaveFormat WaveFormat
    {
        get => SampleProvider.WaveFormat;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (!Instance)
        {
            Instance = this;
        }
        
        SampleProvider.Init();

        //var driverNames = AsioOut.GetDriverNames();
        
        _audioOut = new WaveOutEvent();
        _audioOut.DeviceNumber = deviceNumber;
        _audioOut.DesiredLatency = desiredLatency;
        _audioOut.Init(SampleProvider);
        _audioOut.Play();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDisable()
    {
        _audioOut.Dispose();
    }
}