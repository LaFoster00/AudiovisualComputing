using System;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[SelectionBase]
public class MidiKey : XRBaseInteractable
{
    [SerializeField] public NoteName noteName;
    [SerializeField] public int octave;
    
    [NonSerialized]
    public bool Active;
    
    public UnityEvent onClick;
    public UnityEvent onRelease;


    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        SendMidiNoteOn();
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        SendMidiNoteOff();
    }

    private void SendMidiNoteOn()
    {
        Active = true;
        onClick?.Invoke();
    }

    private void SendMidiNoteOff()
    {
        Active = false;
        onRelease?.Invoke();
    }
}
