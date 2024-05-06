using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Mixer
{
    [SerializeReference]
    private IAudioProvider _target;

    public Mixer(IAudioProvider target)
    {
        _target = target;
    }

    public void Read(Span<float> targetBuffer, Span<float> workingBuffer, ulong nSample)
    {
        _target.Read(workingBuffer, nSample);
        Mix(targetBuffer, workingBuffer, nSample);
    }

    protected abstract void Mix(Span<float> targetBuffer, Span<float> workingBuffer, ulong nSample);
}
