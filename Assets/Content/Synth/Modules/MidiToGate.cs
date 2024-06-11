using System;
using UnityEngine;

public class MidiToGate : AudioProvider
{
    [SerializeField]
    private MidiNoteReceiver noteReceiver;

    public override void Read(Span<float> buffer)
    {
        var gateCurrent = noteReceiver.GetCurrentGate();

        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = gateCurrent ? 1 : 0;
        }
    }
}