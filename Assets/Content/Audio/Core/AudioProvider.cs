using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AudioProvider : SerializationReferableMonoBehaviour, IAudioProvider
{
    public abstract void Read(Span<float> buffer);
}
