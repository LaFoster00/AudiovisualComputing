using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Effect : IAudioProvider
{
    [SerializeField]
    private IAudioProvider _target;
    
    public Effect(IAudioProvider target)
    {
        _target = target;
    }

    public void Read(Span<float>  buffer, ulong nSample)
    {
        _target.Read(buffer, nSample);
        Process(buffer, nSample);
    }

    public abstract void Process(Span<float> buffer, ulong nSample);
}
