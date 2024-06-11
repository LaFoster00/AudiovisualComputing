using System;
using System.Collections;
using System.Collections.Generic;
using Content.Audio.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class ChannelSend : MonoBehaviour
{
    [FormerlySerializedAs("_target")] [SerializeField]
    public AudioProvider target;

    [FormerlySerializedAs("_gain")] [SerializeField]
    public double gain = 1.0;

    private WorkingBuffer _workingBuffer;

    private void OnEnable()
    {
        _workingBuffer = new();
    }

    public void Read(Span<float> targetBuffer)
    {
        target.Read(_workingBuffer);
        Send(targetBuffer);
    }

    protected void Send(Span<float> targetBuffer)
    {
        for (var sample = 0; sample < targetBuffer.Length; sample++)
        {
            targetBuffer[sample] += (float)(gain * _workingBuffer[sample]);
        }
    }
}
