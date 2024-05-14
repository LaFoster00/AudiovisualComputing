using System;
using System.Collections.Generic;
using NAudio.Wave;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

// Supplies a maximum of 2048 samples per read, per channel
[System.Serializable]
public class SampleProvider : ISampleProvider
{
    [ReadOnly]
    public int samplesCount;
    
    private float[] _samples;
    // For sends, so that they can let the target generate a signal an then mix it in)
    private float[] _workingBuffer;
    
    private Memory<float> _samplesMemory; 
    private Memory<float> _workingBufferMemory;
    public Span<float> Samples => _samplesMemory.Span;
    public Span<float> WorkingBuffer => _workingBufferMemory.Span;


    // The current starting sample
    [SerializeField]
    private ulong _nSample = 0;

    public WaveFormat WaveFormat { get; private set; }

    [FormerlySerializedAs("mixers")] [SerializeField] private List<ChannelSend> sends = new();
    

    public int NumMixers
    {
        get => sends.Count;
    }

    public void AddMixer(ChannelSend provider)
    {
        if (!sends.Contains(provider))
            sends.Add(provider);
    }

    public void RemoveMixer(ChannelSend provider)
    {
        sends.Remove(provider);
    }

    public void Init(int sampleRate = 44100, int channels = 2)
    {
        samplesCount = sampleRate * AudioManager.Instance.desiredLatency / 1000;
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
        _samples = new float[samplesCount];
        _workingBuffer = new float[samplesCount];
    }

    public int Read(float[] buffer, int offset, int sampleCount)
    {
        int actualSampleCount = math.min(_samples.Length, sampleCount);
        Array.Clear(_samples, 0, actualSampleCount);
        Array.Clear(_workingBuffer, 0, actualSampleCount);

        foreach (var send in sends)
        {
            _samplesMemory = new Memory<float>(_samples, 0, actualSampleCount);
            _workingBufferMemory = new Memory<float>(_workingBuffer, 0, actualSampleCount);
            send.Read(
                Samples,
                WorkingBuffer,
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