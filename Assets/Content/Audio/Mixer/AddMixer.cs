using System;
using NAudio.Utils;

public class AddMixer : Mixer
{
    public AddMixer(IAudioProvider target) : base(target)
    {
    }

    protected override void Mix(Span<float> targetBuffer, Span<float> workingBuffer, ulong nSample)
    {
        for (var sample = 0; sample < targetBuffer.Length; sample++)
        {
            targetBuffer[sample] += workingBuffer[sample];
        }
    }
}