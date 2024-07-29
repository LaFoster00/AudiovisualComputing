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
    public event Action<int> OnDataRead;

    [NonSerialized] public AudioFormat AudioFormat;

    [SerializeField] private List<ChannelSend> sends = new();

    [ShowNonSerializedField] private int _bufferLength;

    [ShowNativeProperty] public int CurrentDataLength { get; private set; }

    private ulong _frame;
    private float[] _samples;

    // Should be used when generating data independent from the main audio buffer 
    private Queue<float[]> _freeWorkingBuffers;

    public float[] GetFreeWorkingBuffer()
    {
        return _freeWorkingBuffers.Count > 0
            ? _freeWorkingBuffers.Dequeue()
            : new float[_bufferLength];
    }

    public void ReturnWorkingBuffer(float[] buffer)
    {
        if (buffer == null)
        {
            return;
        }

        _freeWorkingBuffers.Enqueue(buffer);
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
        _freeWorkingBuffers = new Queue<float[]>(32);
        for (int i = 0; i < 32; i++)
        {
            _freeWorkingBuffers.Enqueue(new float[_bufferLength]);
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

        OnDataRead?.Invoke(data.Length);

        for (int i = 0; i < CurrentDataLength; i++)
        {
            _samples[i] = 0;
        }

        // First of all run the preprocessing step used to cache data that can be reused
        ++_frame;
        foreach (var send in sends)
        {
            send.Preprocess((uint)CurrentDataLength, _frame);
        }

        // Read back the actual samples
        foreach (var send in sends)
        {
            send.Read(_samples.AsSpan(0, CurrentDataLength));
        }

        
        for (var sample = 0; sample < CurrentDataLength; sample++)
        {
            data[sample] = _samples[sample];
        }
    }
}