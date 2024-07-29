using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAudioProvider
{
    // Will be executed only once no matter how many times per frame it is called
    // Do not forget to call Preprocess on all children
    public void Preprocess(uint numSamples, ulong frame);
    
    public void Read(Span<float> buffer);
    
    public bool CanProvideAudio { get; }
}
