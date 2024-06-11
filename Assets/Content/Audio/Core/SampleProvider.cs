using System;
using System.Collections.Generic;
using System.Linq;
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
    private int _bufferLength;

    public int CurrentDataLength { get; private set; }
    
    private float[] _samples;
    
    private Memory<float> _samplesMemory; 
    // Should be used when generating data independent from the main audio buffer 
    private List<float[]> _freeWorkingBuffers;
    
    public Span<float> Samples => _samplesMemory.Span;


    [NonSerialized]
    public AudioFormat AudioFormat;

    [SerializeField] private List<ChannelSend> sends = new();

    public float[] GetFreeWorkingBuffer()
    {
        var buffer = _freeWorkingBuffers.LastOrDefault();
        return buffer ?? new float[_bufferLength];
    }

    public void ReturnWorkingBuffer(float[] buffer)
    {
        if (buffer == null)
        {
            return;
        }
        _freeWorkingBuffers.Add(buffer);
    } 
    
    private void Awake()
    {
        AudioFormat = new AudioFormat
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

        AudioSettings.GetDSPBufferSize(out _bufferLength, out int numBuffers);
        _bufferLength *= numBuffers;
        _samples = new float[_bufferLength];
        _freeWorkingBuffers = new List<float[]>(32);
        for (int i = 0; i < _freeWorkingBuffers.Count; i++)
        {
            _freeWorkingBuffers[i] = new float[_bufferLength];
        }
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
        if (!AudioManager.Instance)
            return;
        
        CurrentDataLength = data.Length;
        for (int i = 0; i < CurrentDataLength; i++)
        {
            _samples[i] = 0;
        }

        foreach (var send in sends)
        {
            _samplesMemory = new Memory<float>(_samples, 0, CurrentDataLength);
            send.Read(Samples);
        }

        for (var sample = 0; sample < CurrentDataLength; sample++)
        {
            data[sample] = _samples[sample];
        }
    }
}