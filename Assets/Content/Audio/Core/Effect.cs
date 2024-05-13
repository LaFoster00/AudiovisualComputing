using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Effect : AudioProvider
{
    [SerializeField]
    private AudioProvider _target;
    
    public Effect(AudioProvider target)
    {
        _target = target;
    }

    public override void Read(Span<float>  buffer, ulong nSample)
    {
        _target.Read(buffer, nSample);
        Process(buffer, nSample);
    }

    public abstract void Process(Span<float> buffer, ulong nSample);
}
