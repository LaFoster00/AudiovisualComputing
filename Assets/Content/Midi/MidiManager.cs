using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Multimedia;
using InputDevice = Melanchall.DryWetMidi.Multimedia.InputDevice;

public class MidiManager : Singleton<MidiManager>
{
    // Midi devices mapped to their product name
    public readonly Dictionary<string, IInputDevice> InputDevices = new();

    private void OnEnable()
    {
        var midiDevices = InputDevice.GetAll();
        foreach (var device in midiDevices)
        {
            InputDevices[device.Name] = device;
            device.StartEventsListening();
        }

        InputDevices["VirtualKeyboard"] = KeyboardInputDevice.Instance;
        InputDevices["VirtualKeyboard"].StartEventsListening();

        InputDevices["MidiKeyboard"] = MidiKeyboard.Instance;
        InputDevices["MidiKeyboard"].StartEventsListening();
    }

    private void OnDisable()
    {
        foreach (var device in InputDevices)
        {
            (device.Value as IDisposable)?.Dispose();
        }
        InputDevices.Clear();
    }
}