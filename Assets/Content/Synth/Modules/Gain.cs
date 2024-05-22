using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gain : AudioProvider
{
    public AudioProvider source;

    [SerializeReference] public AudioParameter gain = new()
    {
        name = "Gain",
        minValue = 0,
        maxValue = 4,
        CurrentValue = 1
    };
    
    public override void Read(Span<float> buffer, ulong nSample)
    {
        source.Read(buffer, nSample);
        for (int sample = 0; sample < buffer.Length; sample++)
        {
            buffer[sample] *= gain.CurrentValue;
        }
    }
}
