using System;
using System.Collections.Generic;
using NAudio.Wave;
using Unity.Mathematics;
using UnityEngine;

// Supplies a maximum of 2048 samples per read, per channel
[System.Serializable]
public class SampleProvider : ISampleProvider
{
    [SerializeField]
    private float[] _samples;
    // For mixers, so that they can let the target generate a signal an then mix it in
    [SerializeField]
    private float[] _workingBuffer;
    
    // The current starting sample
    [SerializeField]
    private ulong _nSample = 0;

    public WaveFormat WaveFormat { get; private set; }

    [SerializeReference] private List<ChannelSend> mixers = new();

    public int NumMixers
    {
        get => mixers.Count;
    }

    public void AddMixer(ChannelSend provider)
    {
        if (!mixers.Contains(provider))
            mixers.Add(provider);
    }

    public void RemoveMixer(ChannelSend provider)
    {
        mixers.Remove(provider);
    }

    public SampleProvider() : this(44100, 2)
    {
    }

    public SampleProvider(int sampleRate, int channels)
    {
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
        _samples = new float[channels * 2048];
        _workingBuffer = new float[channels * 2048];
    }

    public int Read(float[] buffer, int offset, int sampleCount)
    {
        int actualSampleCount = math.min(_samples.Length, sampleCount);
        Array.Clear(_samples, 0, actualSampleCount);
        Array.Clear(_workingBuffer, 0, actualSampleCount);

        foreach (var mixer in mixers)
        {
            mixer.Read(
                new Span<float>(_samples, 0, actualSampleCount),
                new Span<float>(_workingBuffer, 0, actualSampleCount),
                _nSample);
        }

        for (int sample = 0; sample < actualSampleCount; sample++)
        {
            buffer[offset++] = _samples[sample];
        }

        _nSample += (ulong)actualSampleCount / (ulong)WaveFormat.Channels;

        return actualSampleCount;
    }
}