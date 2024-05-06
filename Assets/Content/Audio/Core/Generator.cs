using System;

[System.Serializable]
public abstract class Generator : IAudioProvider
{
    public abstract void Read(Span<float> buffer, ulong nSample);
}
