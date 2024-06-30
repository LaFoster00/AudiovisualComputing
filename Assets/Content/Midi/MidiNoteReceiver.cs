using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using UnityEngine;

public class MidiNoteReceiver : MonoBehaviour, IOutputDevice
{
    public string midiDeviceName = "Keyboard";

    private SevenBitNumber? currentNote;
    private bool newNote;

    private IEnumerator<bool> gate;

    private DevicesConnector currentDeviceConnector;
    private DevicesConnector virtualKeyboardConnector;
    private DevicesConnector midiKeyboardConnector;


    private void OnEnable()
    {
        AudioManager.Instance.SampleProvider.OnDataRead += OnDataRead;
        gate = GateSignal();
        
        if (MidiManager.Instance.InputDevices.TryGetValue("VirtualKeyboard", out var virtualKeyboard))
        {
            virtualKeyboardConnector = virtualKeyboard.Connect(this);
        }

        if (MidiManager.Instance.InputDevices.TryGetValue("MidiKeyboard", out var midiKeyBoard))
        {
            virtualKeyboardConnector = midiKeyBoard.Connect(this);
        }


        ConnectToInputDevice();
    }

    private void OnDisable()
    {
        AudioManager.Instance.SampleProvider.OnDataRead -= OnDataRead;

        Dispose();
    }

    private void ConnectToInputDevice()
    {
        if (MidiManager.Instance.InputDevices.TryGetValue(midiDeviceName, out var currentDevice))
        {
            virtualKeyboardConnector = currentDevice.Connect(this);
        }
    }

    private void OnDataRead(int samples)
    {
        gate.MoveNext();
    }


    // Hold the current gate state, making sure the gate is retriggered when a new note is played even when the other
    // isn't released yet.
    private IEnumerator<bool> GateSignal()
    {
        yield return false;
        while (true)
        {
            if (newNote)
            {
                newNote = false;
                yield return false;
            }
            else
            {
                yield return currentNote != null;
            }
        }
    }

    public bool GetCurrentGate()
    {
        return gate.Current;
    }


    public SevenBitNumber? GetCurrentNote()
    {
        return currentNote;
    }

    public void Dispose()
    {
        currentDeviceConnector?.Disconnect();
        currentDeviceConnector = null;
        virtualKeyboardConnector?.Disconnect();
        virtualKeyboardConnector = null;
        midiKeyboardConnector?.Disconnect();
        midiKeyboardConnector = null;
    }

    public void PrepareForEventsSending()
    {
    }

    public void SendEvent(MidiEvent midiEvent)
    {
        switch (midiEvent.EventType)
        {
            case MidiEventType.NormalSysEx:
                break;
            case MidiEventType.EscapeSysEx:
                break;
            case MidiEventType.SequenceNumber:
                break;
            case MidiEventType.Text:
                break;
            case MidiEventType.CopyrightNotice:
                break;
            case MidiEventType.SequenceTrackName:
                break;
            case MidiEventType.InstrumentName:
                break;
            case MidiEventType.Lyric:
                break;
            case MidiEventType.Marker:
                break;
            case MidiEventType.CuePoint:
                break;
            case MidiEventType.ProgramName:
                break;
            case MidiEventType.DeviceName:
                break;
            case MidiEventType.ChannelPrefix:
                break;
            case MidiEventType.PortPrefix:
                break;
            case MidiEventType.EndOfTrack:
                break;
            case MidiEventType.SetTempo:
                break;
            case MidiEventType.SmpteOffset:
                break;
            case MidiEventType.TimeSignature:
                break;
            case MidiEventType.KeySignature:
                break;
            case MidiEventType.SequencerSpecific:
                break;
            case MidiEventType.UnknownMeta:
                break;
            case MidiEventType.CustomMeta:
                break;
            case MidiEventType.NoteOff:
                currentNote = null;
                break;
            case MidiEventType.NoteOn:
                currentNote = ((NoteOnEvent)midiEvent).NoteNumber;
                newNote = true;
                break;
            case MidiEventType.NoteAftertouch:
                break;
            case MidiEventType.ControlChange:
                break;
            case MidiEventType.ProgramChange:
                break;
            case MidiEventType.ChannelAftertouch:
                break;
            case MidiEventType.PitchBend:
                break;
            case MidiEventType.TimingClock:
                break;
            case MidiEventType.Start:
                break;
            case MidiEventType.Continue:
                break;
            case MidiEventType.Stop:
                break;
            case MidiEventType.ActiveSensing:
                break;
            case MidiEventType.Reset:
                break;
            case MidiEventType.MidiTimeCode:
                break;
            case MidiEventType.SongPositionPointer:
                break;
            case MidiEventType.SongSelect:
                break;
            case MidiEventType.TuneRequest:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public event EventHandler<MidiEventSentEventArgs> EventSent;
}