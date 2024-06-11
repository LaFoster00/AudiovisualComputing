using System.Collections;
using System.Collections.Generic;
using Minis;
using UnityEngine;
using UnityEngine.Rendering;
using USCSL;
using Random = Unity.Mathematics.Random;

public class MidiNoteReceiver : MonoBehaviour
{
    public string midiDeviceName = "dummy";
    private MidiDevice midiDevice;

    public bool debug;

    private MidiNoteControl currentNote;
    private int lastNote = -1;

    private IEnumerator<bool> gate;

    private Coroutine debugNotePlay;


    private void OnEnable()
    {
        AudioManager.Instance.SampleProvider.OnDataRead += OnDataRead;
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
        AudioManager.Instance.SampleProvider.OnDataRead -= OnDataRead;
        MidiManager.Instance.OnDeviceAdded -= OnDeviceAdded;
        MidiManager.Instance.OnDeviceRemoved += OnDeviceRemoved;
    }

    private void OnDataRead()
    {
        gate.MoveNext();
        
        if (debug)
            return;

        if (currentNote != null)
            lastNote = currentNote.noteNumber;
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
        var lastNote = this.lastNote;
        yield return false;
        while (true)
        {
            if (lastNote == this.lastNote)
            {
                yield return this.lastNote != -1;
            }
            else
            {
                lastNote = this.lastNote;
                yield return false;
            }
        }
    }

    private IEnumerator DebugNoteGenerator()
    {
        var random = new Random();
        random.InitState();
        while (true)
        {
            lastNote = random.NextInt(60, 70);
            yield return new WaitForSeconds(random.NextFloat(0.1f, 2.0f));
            lastNote = -1;
            yield return new WaitForSeconds(random.NextFloat(0.1f, 2.0f));
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

    public bool GetCurrentGate()
    {
        return gate.Current;
    }


    public int GetCurrentNote()
    {
        return debug ? ( lastNote == -1 ? 69 : lastNote ) : lastNote;
    }
}