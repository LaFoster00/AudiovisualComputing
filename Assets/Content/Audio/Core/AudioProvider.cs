using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AudioProvider : MonoBehaviourGuid, IAudioProvider
{
    public abstract void Read(Span<float> buffer);
}
