using System;
using System.Collections.Generic;
using Audio.Core;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using UnityEngine;

public class MidiKeyboard : Singleton<MidiKeyboard>, IInputDevice
{
    public List<MidiKey> keys = new();

    private void Start()
    {
        foreach (var key in keys)
        {
            key.onClick.AddListener(() => OnKeyPress(key));
            key.onRelease.AddListener(() => OnKeyRelease(key));
        }
    }

    public bool IsListeningForEvents { get; private set; }
    public event EventHandler<MidiEventReceivedEventArgs> EventReceived;

    private void OnKeyRelease(MidiKey key)
    {
        if (IsListeningForEvents)
            SendMidiNoteOff(Utils.CalculateNoteNumber(key.noteName, key.octave));
    }

    private void OnKeyPress(MidiKey key)
    {
        if (IsListeningForEvents)
            SendMidiNoteOn(Utils.CalculateNoteNumber(key.noteName, key.octave));
    }

    private void SendMidiNoteOn(int note)
    {
        if (!Utils.IsNoteNumberValid(note))
            return;
        Debug.Log("MIDI Note On: " + note);
        EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(
            new NoteOnEvent((SevenBitNumber)note, SevenBitNumber.MaxValue)));
    }

    private void SendMidiNoteOff(int note)
    {
        if (!Utils.IsNoteNumberValid(note))
            return;
        Debug.Log("MIDI Note Off: " + note);
        EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(
            new NoteOffEvent((SevenBitNumber)note, SevenBitNumber.MaxValue)));
    }

    public void Dispose()
    {
        StopEventsListening();
    }

    public void StartEventsListening()
    {
        IsListeningForEvents = true;
    }

    public void StopEventsListening()
    {
        IsListeningForEvents = false;
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].Active)
                SendMidiNoteOff(i);
        }
    }
}