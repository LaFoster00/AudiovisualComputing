using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gain : AudioProvider
{
    public AudioProvider source;

    public AudioParameter gain;

    public override bool CanRead()
    {
        return true;
    }

    public override void Read(Span<float> buffer)
    {
        source.Read(buffer);
        for (int sample = 0; sample < buffer.Length; sample++)
        {
            buffer[sample] *= gain.CurrentValue;
        }
    }
}
