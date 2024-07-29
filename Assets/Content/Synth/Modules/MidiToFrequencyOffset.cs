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

    protected override void Preprocess_Impl(uint numSamples, ulong frame)
    {
        var currentNote = noteReceiver.GetCurrentNote();
        var noteOffset = currentNote?.GetFrequencyOffsetFromMidiNote() ?? lastNoteOffset;
        lastNoteOffset = noteOffset;
    }

    public override void Read(Span<float> buffer)
    {
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = lastNoteOffset;
        }
    }
}
