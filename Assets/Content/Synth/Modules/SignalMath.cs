using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

public class SignalMath : AudioProvider
{
    public AudioProvider source;
    public AudioProvider normalizeSource;

    [FormerlySerializedAs("gainOffset")] public AudioProvider mulOffset;
    [FormerlySerializedAs("gain")] public AudioParameter mul;

    public AudioProvider addOffset;
    public AudioParameter add;

    private WorkingBuffer _gainBuffer;
    private WorkingBuffer _addBuffer;
    private WorkingBuffer _normalizeBuffer;
    
    public override bool CanProvideAudio => true;
    
    [Button("Populate AudioParameters")]
    private void PopulateAudioParameters()
    {
        var existingAudioParameters = GetComponents<AudioParameter>();
        this.PopulateAudioParameter(
            existingAudioParameters,
            ref mul,
            "Multiplier",
            0,
            2);
        this.PopulateAudioParameter(
            existingAudioParameters,
            ref add,
            "Add",
            0,
            1);
    }
    
    private void OnEnable()
    {
        _gainBuffer = new WorkingBuffer();
        _addBuffer = new WorkingBuffer();
        _normalizeBuffer = new WorkingBuffer();
    }

    public override void Read(Span<float> buffer)
    {
        source.Read(buffer);
        normalizeSource.Read(_normalizeBuffer);
        
        mulOffset.Read(_gainBuffer);
        addOffset.Read(_addBuffer);
        
        for (int sample = 0; sample < buffer.Length; sample++)
        {
            buffer[sample] += _normalizeBuffer[sample] * 0.5f + 0.5f;
            buffer[sample] *= mul.CurrentValue + _gainBuffer[sample];
            buffer[sample] += add.CurrentValue + _addBuffer[sample];
        }
    }
}
