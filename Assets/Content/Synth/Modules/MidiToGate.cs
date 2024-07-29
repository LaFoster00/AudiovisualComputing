using System;
using UnityEngine;

public class MidiToGate : AudioProvider
{
    [SerializeField]
    private MidiNoteReceiver noteReceiver;
    
    public override bool CanProvideAudio => true;

    private bool gateCurrent;

    protected override void Preprocess_Impl(uint numSamples, ulong frame)
    {
        gateCurrent = noteReceiver.GetCurrentGate();
    }

    public override void Read(Span<float> buffer)
    {
        

        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = gateCurrent ? 1 : 0;
        }
    }
}