using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Audio.Core;

public class MidiToFrequencyOffset : AudioProvider
{
    [SerializeField]
    private MidiNoteReceiver noteReceiver;

    private float lastNoteOffset;
    
    public override bool CanProvideAudio => true;
    
    public override void Read(Span<float> buffer)
    {
        var currentNote = noteReceiver.GetCurrentNote();
        var noteOffset = currentNote?.GetFrequencyOffsetFromMidiNote() ?? lastNoteOffset;
        lastNoteOffset = noteOffset;
        
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = noteOffset;
        }
    }
}
