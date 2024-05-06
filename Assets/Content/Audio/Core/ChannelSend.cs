using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChannelSend
{
    [SerializeReference]
    private IAudioProvider _target;

    [SerializeField]
    private double _gain;

    public ChannelSend(IAudioProvider target, double gain)
    {
        _target = target;
        _gain = gain;
    }

    public void Read(Span<float> targetBuffer, Span<float> workingBuffer, ulong nSample)
    {
        _target.Read(workingBuffer, nSample);
        Send(targetBuffer, workingBuffer, nSample);
    }

    protected void Send(Span<float> targetBuffer, Span<float> workingBuffer, ulong nSample)
    {
        for (var sample = 0; sample < targetBuffer.Length; sample++)
        {
            targetBuffer[sample] += (float)(_gain * workingBuffer[sample]);
        }
    }
}
