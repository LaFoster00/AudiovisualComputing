using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AsioOut _asioOut;

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
        Instance = this;
        var driverNames = AsioOut.GetDriverNames();
        _asioOut = new AsioOut(driverNames.First(s => s.Contains("UMC")));
        _asioOut.Init(SampleProvider);
        _asioOut.Play();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDisable()
    {
        _asioOut.Dispose();
    }
}