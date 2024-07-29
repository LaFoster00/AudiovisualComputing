using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AudioProvider : MonoBehaviourGuid, IAudioProvider
{
    private ulong lastPreprocessFrame = 0;

    public void Preprocess(uint numSamples, ulong frame){
        if (lastPreprocessFrame == frame)
            return;
        Preprocess_Impl(numSamples, frame);
        lastPreprocessFrame = frame;
    }

    // Will be executed only once no matter how many times per frame it is called
    // Do not forget to call Preprocess on all children
    protected abstract void Preprocess_Impl(uint numSamples, ulong frame);
    public abstract void Read(Span<float> buffer);
    public abstract bool CanProvideAudio { get; }
}