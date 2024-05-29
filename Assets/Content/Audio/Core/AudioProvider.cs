using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AudioProvider : MonoBehaviour, IAudioProvider
{
    public abstract void Read(Span<float> buffer);
}
