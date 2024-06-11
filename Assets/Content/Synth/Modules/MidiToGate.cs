using System;
using System.Collections;
using System.Collections.Generic;
using Minis;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

public class MidiToGate : AudioProvider
{
    public string midiDeviceName = "dummy";
    private MidiDevice midiDevice;

    public bool debug;

    private MidiNoteControl currentNote;

    private IEnumerator<bool> gate;

    private Coroutine debugNotePlay;
    private int debugCurrentNote = -1;

    private void OnEnable()
    {
        MidiManager.Instance.OnDeviceAdded += OnDeviceAdded;
        MidiManager.Instance.OnDeviceRemoved += OnDeviceRemoved;

        if (debug)
        {
            StartCoroutine(DebugNoteGenerator());
            gate = DebugGateSignal();
        }
        else
            gate = GateSignal();
    }
    
    private void OnDisable()
    {
        MidiManager.Instance.OnDeviceAdded -= OnDeviceAdded;
        MidiManager.Instance.OnDeviceRemoved += OnDeviceRemoved;
    }


    // Hold the current gate state, making sure the gate is retriggered when a new note is played even when the other
    // isn't released yet.
    private IEnumerator<bool> GateSignal()
    {
        var lastNote = currentNote;
        yield return false;
        while (true)
        {
            if (lastNote == currentNote)
            {
                yield return currentNote != null;
            }
            else
            {
                lastNote = currentNote;
                yield return false;
            }
        }
    }

    // Hold the current gate state, making sure the gate is retriggered when a new note is played even when the other
    // isn't released yet.
    private IEnumerator<bool> DebugGateSignal()
    {
        var lastNote = debugCurrentNote;
        yield return false;
        while (true)
        {
            if (lastNote == debugCurrentNote)
            {
                yield return debugCurrentNote != -1;
            }
            else
            {
                lastNote = debugCurrentNote;
                yield return false;
            }
        }
    }

    private IEnumerator DebugNoteGenerator()
    {
        var random = new Random();
        while (true)
        {
            debugCurrentNote = random.Next(60, 70);
            yield return new WaitForSeconds(random.Next(1, 2));
        }
    }
    
    private void OnDeviceAdded(MidiDevice device)
    {
        if (midiDevice != null)
        {
            midiDevice.onWillNoteOn -= OnWillNoteOn;
            midiDevice.onWillNoteOff += OnWillNoteOff;
        }

        midiDevice = device.description.product.Equals(midiDeviceName) ? device : midiDevice;
        if (midiDevice != null)
        {
            midiDevice.onWillNoteOn += OnWillNoteOn;
        }
    }

    private void OnWillNoteOn(MidiNoteControl midiNote, float velocity)
    {
        currentNote = midiNote;
    }

    private void OnWillNoteOff(MidiNoteControl midiNote)
    {
        if (currentNote.noteNumber == midiNote.noteNumber)
            currentNote = null;
    }

    private void OnDeviceRemoved(MidiDevice device)
    {
        if (midiDevice != null)
            midiDevice.onWillNoteOn -= OnWillNoteOn;
        midiDevice = device.description.product.Equals(midiDeviceName) ? null : midiDevice;
    }

    public override void Read(Span<float> buffer)
    {
        gate.MoveNext();
        var gateCurrent = gate.Current;

        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = gateCurrent ? 1 : 0;
        }
    }
}