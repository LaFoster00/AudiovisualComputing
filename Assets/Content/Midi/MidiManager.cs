using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Multimedia;
#if !UNITY_ANDROID
using InputDevice = Melanchall.DryWetMidi.Multimedia.InputDevice;
#endif

public class MidiManager : Singleton<MidiManager>
{
    // Midi devices mapped to their product name
    public readonly Dictionary<string, IInputDevice> InputDevices = new();

    private void OnEnable()
    {
#if !UNITY_ANDROID
        var midiDevices = InputDevice.GetAll();
        foreach (var device in midiDevices)
        {
            InputDevices[device.Name] = device;
            device.StartEventsListening();
        }
#endif

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