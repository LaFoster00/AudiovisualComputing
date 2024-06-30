using System;
using System.Collections.Generic;
using Audio.Core;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine;
using AudioSettings = Audio.Core.AudioSettings;

public class PianoRoll : MonoBehaviour, IInputDevice, IOutputDevice
{
    [Header("Config")] [SerializeField, Tooltip("The PianoRoll slot object to instance")]
    private GameObject slotObject;

    [SerializeField, Tooltip("The parent object for all the slots, with which the scrolling is facilitated.")]
    private Transform slotParent;

    [SerializeField] private Transform timelineMarker;

    [SerializeField] private float beatHeight = 0.1f;
    [SerializeField] private float beatWidth = 0.1f;

    // The matrix of notes in this piano roll
    private List<PianoRoll_Slot[]> slots = new();

    // The notes that should be highlighted
    private List<PianoRoll_Slot> hightlightNotes = new();

    // The notes that should not be highlighted anymore
    private List<PianoRoll_Slot> deHightlightNotes = new();

    private Dictionary<uint, List<PianoRoll_Slot>> noteStream = new();

    // Main thread step
    private int step;

    // Audio thread step
    private int audioStep;

    public event EventHandler<MidiEventSentEventArgs> EventSent;
    public event EventHandler<MidiEventReceivedEventArgs> EventReceived;

    public bool IsListeningForEvents { get; private set; }

    private void Start()
    {
        transform.localScale = new Vector3(beatWidth, beatHeight, 1);
        AudioManager.Instance.SampleProvider.OnDataRead += SampleProviderOnOnDataRead;
        for (int i = 0; i < AudioSettings.Instance.BeatsPerMeasure; i++)
        {
            AddBeatSlots();
        }

        step = audioStep = 0;
    }

    private void Update()
    {
    }

    public void AddNote(int beatIndex, NoteName note, byte octave)
    {
        // Check if our note list can accomodate the desired note beat index
        if (beatIndex > slots.Count)
        {
            for (int i = slots.Count; i < beatIndex + 1; i++)
            {
                AddBeatSlots();
            }
        }

        var slot = slots[beatIndex][(int)note + 12 * octave];
    }

    public void RemoveNote(int beatIndex, NoteName note, byte octave)
    {
        if (slots[beatIndex][(int)note + 12 * octave].baseSlot)
        {
            slots[beatIndex][(int)note + 12 * octave].baseSlot = false;
            Destroy(slots[beatIndex][(int)note + 12 * octave].noteObject.gameObject);

            for (int i = 0; i < beatIndex + slots[beatIndex][(int)note + 12 * octave].MidiNote.duration; i++)
            {
                if (i >= slots.Count)
                    break;

                slots[i][(int)note + 12 * octave].containsNote = false;
            }
        }else if (slots[beatIndex][(int)note + 12 * octave].containsNote)
        {
            if (slots[beatIndex][(int)note + 12 * octave].noteObject)
                Destroy(slots[beatIndex][(int)note + 12 * octave].noteObject.gameObject);
        }
    }

    private void AddBeatSlots()
    {
        slots.Add(new PianoRoll_Slot[Utils.totalNumberNotes]);
        var newSlots = slots[^1];
        var newSlotIndex = slots.Count - 1;
        for (var i = 0; i < Utils.totalNumberNotes; i++)
        {
            var newSlot = Instantiate(slotObject, slotParent, false);
            newSlot.transform.localPosition = new Vector3(newSlotIndex, -i, 0);

            var slot = newSlot.GetComponent<PianoRoll_Slot>();
            newSlots[i] = slot;
            slot.MidiNote.note = (NoteName)(i % 12);
            slot.MidiNote.octave = (byte)(i / 12);
            slot.beatIndex = newSlotIndex;
            slot.pitchIndex = (byte)i;
            slot.Owner = this;
        }
    }

    private void SampleProviderOnOnDataRead(int samples)
    {
        ++audioStep;
    }

    public void Dispose()
    {
    }

    public void StartEventsListening()
    {
        IsListeningForEvents = true;
    }

    public void StopEventsListening()
    {
        IsListeningForEvents = false;
    }

    public void PrepareForEventsSending()
    {
    }

    public void SendEvent(MidiEvent midiEvent)
    {
    }
}