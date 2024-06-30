using System;
using System.Collections;
using System.Collections.Generic;
using Content.Midi.PianoRoll;
using NaughtyAttributes;
using UnityEngine;

public class PianoRoll_Slot : MonoBehaviour
{
    [Header("Info")] public bool containsNote;

    // True if this slot is not the root slot for the contained note
    public bool baseSlot;

    [Header("Location")] public int beatIndex;
    public byte pitchIndex;

    [Header("Midi")]
    // The midi note this slot represents
    public MidiNote MidiNote;
    
    [ReadOnly]
    public Transform noteObject;

    internal PianoRoll Owner;
}