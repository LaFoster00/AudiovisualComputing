using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Unity.Mathematics;
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
        
    }

    public void Preprocess(uint numSamples, ulong frame)
    {
        target.Preprocess(numSamples, frame);
    }
    
    public void Read(Span<float> targetBuffer)
    {
        if (_workingBuffer == null)
        {
            _workingBuffer = new WorkingBuffer();
            return;
        }

        _workingBuffer.Clear();
        target.Read(_workingBuffer);
        Send(targetBuffer);
    }

    private void Send(Span<float> targetBuffer)
    {
        for (var sample = 0; sample < targetBuffer.Length; sample++)
        {
            targetBuffer[sample] += math.min((float)(gain * _workingBuffer[sample]), 1.0f);
        }
    }
}
