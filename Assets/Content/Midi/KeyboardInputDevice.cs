using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine;
using UnityEngine.InputSystem;
using Audio.Core;

public class KeyboardInputDevice : Singleton<KeyboardInputDevice>, IInputDevice
{
    public event EventHandler<MidiEventReceivedEventArgs> EventReceived;

    private static readonly Dictionary<KeyCode, NoteName> NotesNames = new Dictionary<KeyCode, NoteName>
    {
        [KeyCode.A] = NoteName.C,
        [KeyCode.W] = NoteName.CSharp,
        [KeyCode.S] = NoteName.D,
        [KeyCode.E] = NoteName.DSharp,
        [KeyCode.D] = NoteName.E,
        [KeyCode.F] = NoteName.F,
        [KeyCode.T] = NoteName.FSharp,
        [KeyCode.G] = NoteName.G,
        [KeyCode.Y] = NoteName.GSharp,
        [KeyCode.H] = NoteName.A,
        [KeyCode.U] = NoteName.ASharp,
        [KeyCode.J] = NoteName.B
    };

    private Dictionary<KeyCode, InputAction> NoteActions = new Dictionary<KeyCode, InputAction>
    {
        [KeyCode.A] = new(binding: "<Keyboard>/a"),
        [KeyCode.W] = new(binding: "<Keyboard>/w"),
        [KeyCode.S] = new(binding: "<Keyboard>/s"),
        [KeyCode.E] = new(binding: "<Keyboard>/e"),
        [KeyCode.D] = new(binding: "<Keyboard>/d"),
        [KeyCode.F] = new(binding: "<Keyboard>/f"),
        [KeyCode.T] = new(binding: "<Keyboard>/t"),
        [KeyCode.G] = new(binding: "<Keyboard>/g"),
        [KeyCode.Y] = new(binding: "<Keyboard>/y"),
        [KeyCode.H] = new(binding: "<Keyboard>/h"),
        [KeyCode.U] = new(binding: "<Keyboard>/u"),
        [KeyCode.J] = new(binding: "<Keyboard>/j"),
    };
    
    private Dictionary<InputAction, KeyCode> _actionNotes;

    private int _octaveNumber = 4;
    private readonly bool[] _notesActive = new bool[SevenBitNumber.MaxValue];
    public bool IsListeningForEvents { get; private set; }

    private KeyboardInputDevice()
    {
    }

    public void OnEnable()
    {
        // Reverse lookup
        _actionNotes = NoteActions.ToDictionary(x => x.Value, x => x.Key);

        foreach (var noteAction in NoteActions)
        {
            noteAction.Value.started += ListenEvents;
            noteAction.Value.canceled += ListenEvents;
            noteAction.Value.Enable();
        }
    }

    public void OnDisable()
    {
        foreach (var noteAction in NoteActions)
        {
            noteAction.Value.Disable();
            noteAction.Value.started -= ListenEvents;
            noteAction.Value.canceled -= ListenEvents;
        }
    }


    public void StartEventsListening()
    {
        IsListeningForEvents = true;
    }

    public void StopEventsListening()
    {
        for (int i = 0; i < SevenBitNumber.MaxValue; i++)
        {
            if (_notesActive[i])
            {
                EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(
                    new NoteOffEvent((SevenBitNumber)i, SevenBitNumber.MinValue)));
            }
        }

        IsListeningForEvents = false;
    }

    public void Dispose()
    {
    }

    private void ListenEvents(InputAction.CallbackContext keyAction)
    {
        if (!IsListeningForEvents) return;
        if (!_actionNotes.TryGetValue(keyAction.action, out var key)) return;
        if (!NotesNames.TryGetValue(key, out var noteName))
        {
            switch (key)
            {
                case KeyCode.UpArrow:
                    _octaveNumber++;
                    Console.WriteLine($"Octave is {_octaveNumber} now");
                    break;
                case KeyCode.DownArrow:
                    _octaveNumber--;
                    Console.WriteLine($"Octave is {_octaveNumber} now");
                    break;
            }

            return;
        }


        var noteNumber = Utils.CalculateNoteNumber(noteName, _octaveNumber);
        if (!Utils.IsNoteNumberValid(noteNumber))
            return;

        if (keyAction.started)
        {
            EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(
                new NoteOnEvent((SevenBitNumber)noteNumber, SevenBitNumber.MaxValue)));
            _notesActive[noteNumber] = true;
        }

        else if (keyAction.canceled)
        {
            EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(
                new NoteOffEvent((SevenBitNumber)noteNumber, SevenBitNumber.MinValue)));
            _notesActive[noteNumber] = false;
        }
    }
}