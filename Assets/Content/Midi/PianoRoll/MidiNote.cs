using System;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine.Serialization;

namespace Content.Midi.PianoRoll
{
    [Serializable]
    public class MidiNote
    {
        // Length in beats
        public uint duration;
        
        public NoteName note;

        public byte octave;
    }
}