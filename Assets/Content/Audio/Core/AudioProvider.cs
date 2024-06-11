using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public abstract class AudioProvider : MonoBehaviour, IAudioProvider
{
    public abstract bool CanRead();
    
    public abstract void Read(Span<float> buffer);
}
