using System;
using UnityEngine;

[System.Serializable]
public abstract class Generator : MonoBehaviour, IAudioProvider
{
    public abstract void Read(Span<float> buffer, ulong nSample);
}
