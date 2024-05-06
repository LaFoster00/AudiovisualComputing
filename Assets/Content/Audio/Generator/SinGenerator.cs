using System;
using UnityEngine;

[Serializable]
public class SinGenerator : Generator
{
    [SerializeField] 
    public double gain;

    [SerializeField] public double frequency;
    
    public SinGenerator(double gain, double frequency)
    {
        this.gain = gain;
        this.frequency = frequency;
    }
    
    public override void Read(Span<float> buffer, ulong nSample)
    {
        int totalSample = 0;
        for (int sample = 0; sample < buffer.Length / AudioManager.Instance.WaveFormat.Channels; sample++)
        {
            double value = gain * Math.Sin((double) nSample++ * (2.0 * Math.PI * frequency / (double) AudioManager.Instance.WaveFormat.SampleRate));
            for (int channel = 0; channel < AudioManager.Instance.WaveFormat.Channels; channel++)
            {
                buffer[totalSample++] = (float)value;
            }
        }
    }
}
