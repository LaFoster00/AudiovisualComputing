using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

public class NAudio_Test : MonoBehaviour
{
    private AsioOut asioOut;

    private SignalGenerator samples;

    // Start is called before the first frame update
    void Start()
    {
        var driverNames = AsioOut.GetDriverNames();
        asioOut = new AsioOut(driverNames.First(s => s.Contains("UMC")));
        samples = (SignalGenerator)new SignalGenerator()
        {
            Gain = 0.2f,
            Frequency = 500,
            Type = SignalGeneratorType.Sin
        };
        asioOut.Init(samples);
    }

    // Update is called once per frame
    void Update()
    {
        Thread.Sleep(TimeSpan.FromSeconds(0.1f));
        samples.Type = samples.Type == SignalGeneratorType.Sin
            ? SignalGeneratorType.Square
            : SignalGeneratorType.Sin;
        samples.Gain = Math.Abs(samples.Gain - 0.2) < 0.00001f ? 0.05 : 0.2;
        samples.Frequency = Math.Abs(samples.Frequency - 500) < 0.001f ? 200 : 500;
        asioOut.Play();
    }

    private void OnDisable()
    {
        asioOut.Dispose();
    }
}