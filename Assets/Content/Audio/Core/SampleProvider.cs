using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class AudioFormat
{
    /// <summary>number of channels</summary>
    public short Channels;

    /// <summary>sample rate</summary>
    public int SampleRate;
}

// Supplies a maximum of 2048 samples per read, per channel
[RequireComponent(typeof(AudioSource))]
public class SampleProvider : MonoBehaviour
{
    private float[] _samples;
    // For sends, so that they can let the target generate a signal an then mix it in)
    private float[] _workingBuffer;
    
    private Memory<float> _samplesMemory; 
    private Memory<float> _workingBufferMemory;
    public Span<float> Samples => _samplesMemory.Span;
    public Span<float> WorkingBuffer => _workingBufferMemory.Span;


    [NonSerialized]
    public AudioFormat audioFormat;

    [SerializeField] private List<ChannelSend> sends = new();


    private void Awake()
    {
        audioFormat = new AudioFormat
        {
            SampleRate = AudioSettings.outputSampleRate,
            Channels = AudioSettings.speakerMode switch
            {
                AudioSpeakerMode.Mono => 1,
                AudioSpeakerMode.Stereo => 2,
                AudioSpeakerMode.Quad => 4,
                AudioSpeakerMode.Surround => 5,
                AudioSpeakerMode.Mode5point1 => 6,
                AudioSpeakerMode.Mode7point1 => 7,
                AudioSpeakerMode.Prologic => 2,
                _ => throw new ArgumentOutOfRangeException()
            }
        };

        _samples = new float[audioFormat.SampleRate];
        _workingBuffer = new float[audioFormat.SampleRate];
    }

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

    private void OnAudioFilterRead(float[] data, int channels)
    {
        int actualSampleCount = math.min(_samples.Length, data.Length);
        Array.Clear(_samples, 0, actualSampleCount);
        Array.Clear(_workingBuffer, 0, actualSampleCount);

        foreach (var send in sends)
        {
            _samplesMemory = new Memory<float>(_samples, 0, actualSampleCount);
            _workingBufferMemory = new Memory<float>(_workingBuffer, 0, actualSampleCount);
            send.Read(
                Samples,
                WorkingBuffer);
        }

        for (var sample = 0; sample < actualSampleCount; sample++)
        {
            data[sample] = _samples[sample];
        }
    }
}