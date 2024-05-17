using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gain : AudioProvider
{
    public AudioProvider source;
    
    public AudioParameter gain = new AudioParameter()
    {
        name = "Gain",
        minValue = 0,
        maxValue = 1,
        CurrentNormalizedValue = 0.2f
    };
    
    public override void Read(Span<float> buffer, ulong nSample)
    {
        source.Read(buffer, nSample);
        for (int sample = 0; sample < buffer.Length; sample++)
        {
            buffer[sample] *= gain.CurrentNormalizedValue;
        }
    }
}
