using System;
using UnityEngine;

public class SinGenerator : AudioProvider
{
    public AudioParameter frequency = new()
    {
        name = "Frequency",
        minValue = 40,
        maxValue = 200,
        CurrentValue = 0.3f
    };

    public override void Read(Span<float> buffer, ulong nSample)
    {
        int totalSample = 0;
        for (int sample = 0; sample < buffer.Length / AudioManager.Instance.WaveFormat.Channels; sample++)
        {
            double value = Math.Sin(nSample++ *
                                    (2.0 * Math.PI * frequency.CurrentValue /
                                     AudioManager.Instance.WaveFormat.SampleRate));
            for (int channel = 0; channel < AudioManager.Instance.WaveFormat.Channels; channel++)
            {
                buffer[totalSample++] = (float)value;
            }
        }
    }
}