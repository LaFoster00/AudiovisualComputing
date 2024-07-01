using System;
using System.Collections.Generic;
using Audio.Core;
using Melanchall.DryWetMidi.Common;
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

    private bool deHighlight;

    // The notes that should not be highlighted anymore
    private List<PianoRoll_Slot> deHightlightNotes = new();

    private Dictionary<uint, List<PianoRoll_Slot>> noteStream = new();

    private ulong sample;

    // Main thread beat
    private uint beat;

    // Audio thread beat
    private uint audioStep;

    private bool playing;

    public event EventHandler<MidiEventSentEventArgs> EventSent;
    public event EventHandler<MidiEventReceivedEventArgs> EventReceived;

    public bool IsListeningForEvents { get; private set; }

    private void Start()
    {
        transform.localScale = new Vector3(beatWidth, beatHeight, 1);
        for (int i = 0; i < AudioSettings.Instance.BeatsPerMeasure; i++)
        {
            AddBeatSlots();
        }

        beat = audioStep = 0;

        AddNote(0, NoteName.A, 4, 5);
        AddNote(4, NoteName.C, 3, 1);
        AddNote(10, NoteName.CSharp, 3, 10);
        
        StartPlayback();
    }

    private void OnEnable()
    {
        EventReceived += OnEventReceived;
        AudioManager.Instance.SampleProvider.OnDataRead += SampleProviderOnOnDataRead;
    }


    private void OnDisable()
    {
        EventReceived -= OnEventReceived;
        AudioManager.Instance.SampleProvider.OnDataRead -= SampleProviderOnOnDataRead;
    }

    private void Update()
    {
        foreach (var hightlightNote in hightlightNotes)
        {
            hightlightNote.NotePlayingAnimation();
        }

        hightlightNotes.Clear();

        foreach (var deHightlightNote in deHightlightNotes)
        {
            deHightlightNote.NoteStoppingAnimation();
        }

        deHightlightNotes.Clear();
    }

    public void AddNote(int beatIndex, NoteName note, byte octave, uint duration)
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
        slot.MidiNote.duration = duration;
        slot.MidiNote.note = note;
        slot.MidiNote.octave = octave;
        slot.baseSlot = true;
        for (int i = beatIndex; i < beatIndex + duration; i++)
        {
            var followingSlot = slots[beatIndex][(int)note + 12 * octave];
            followingSlot.containsNote = true;
            followingSlot.MidiNote.duration = duration;
            followingSlot.MidiNote.note = note;
            followingSlot.MidiNote.octave = octave;
        }
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
        }
        else if (slots[beatIndex][(int)note + 12 * octave].containsNote)
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
        if (!playing)
            return;
        sample += (ulong)(samples / AudioManager.Instance.SampleProvider.AudioFormat.Channels);

        var newStep = AudioSettings.SampleToBeat(sample);
        if (newStep != beat)
        {
            if (newStep < slots.Count)
            {
                for (int i = 0; i < Utils.totalNumberNotes; i++)
                {
                    if (slots[(int)newStep][i].baseSlot)
                    {
                        AddNoteToStream(slots[(int)newStep][i]);
                    }
                }
            }

            ReleaseNotesFromStream(newStep);
        }

        beat = newStep;

        if (beat >= slots.Count)
        {
            beat = 0;
        }
    }

    private void ReleaseNotesFromStream(uint endSample)
    {
        if (noteStream.ContainsKey(endSample))
        {
            foreach (var n in noteStream[endSample])
            {
                SendEvent(new NoteOffEvent(
                    (SevenBitNumber)Utils.CalculateNoteNumber(n.MidiNote.note, n.MidiNote.octave),
                    (SevenBitNumber)1));

                //Main thread animations
                deHightlightNotes.Add(n);
            }

            noteStream.Remove(endSample);
        }
    }

    private void AddNoteToStream(PianoRoll_Slot n)
    {
        Debug.Log("Added note to stream");
        SendEvent(new NoteOnEvent(
            (SevenBitNumber)Utils.CalculateNoteNumber(n.MidiNote.note, n.MidiNote.octave),
            (SevenBitNumber)1));

        //get sample at which we should call noteOff on this note
        uint endBeat = beat + n.MidiNote.duration;

        //if not already part of the dictionary, add it
        if (!noteStream.ContainsKey(endBeat))
            noteStream[endBeat] = new List<PianoRoll_Slot>();

        // Main thread animations
        hightlightNotes.Add(n);

        noteStream[endBeat].Add(n);
    }

    public void StartPlayback(uint beat = 0)
    {
        sample = AudioSettings.BeatToSample(beat);
        this.beat = beat;
        playing = true;
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
        EventSent?.Invoke(this, new MidiEventSentEventArgs(midiEvent));
    }


    private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
    {
        SendEvent(e.Event);
    }
}