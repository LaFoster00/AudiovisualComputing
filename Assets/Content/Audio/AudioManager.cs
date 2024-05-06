using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AsioOut _asioOut;

    [SerializeField]
    public SampleProvider WaveProvider;

    public static AudioManager Instance { get; private set; }

    public WaveFormat WaveFormat
    {
        get => WaveProvider.WaveFormat;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        Instance = this;
        var driverNames = AsioOut.GetDriverNames();
        _asioOut = new AsioOut(driverNames.First(s => s.Contains("UMC")));
        WaveProvider = new SampleProvider();
        _asioOut.Init(WaveProvider);
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