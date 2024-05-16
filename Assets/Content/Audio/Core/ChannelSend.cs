using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ChannelSend : MonoBehaviour
{
    [FormerlySerializedAs("_target")] [SerializeField]
    public AudioProvider target;

    [FormerlySerializedAs("_gain")] [SerializeField]
    public double gain = 1.0;

    public void Read(Span<float> targetBuffer, Span<float> workingBuffer, ulong nSample)
    {
        target.Read(workingBuffer, nSample);
        Send(targetBuffer, workingBuffer, nSample);
    }

    protected void Send(Span<float> targetBuffer, Span<float> workingBuffer, ulong nSample)
    {
        for (var sample = 0; sample < targetBuffer.Length; sample++)
        {
            targetBuffer[sample] += (float)(gain * workingBuffer[sample]);
        }
    }
}
