using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidiToFrequencyOffset : AudioProvider
{
    [SerializeField]
    private MidiNoteReceiver noteReceiver;

    public override void Read(Span<float> buffer)
    {
        var currentNote = noteReceiver.GetCurrentNote();
        var noteOffset = currentNote.GetFrequencyOffsetFromMidiNote();
        
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = noteOffset;
        }
    }
}
