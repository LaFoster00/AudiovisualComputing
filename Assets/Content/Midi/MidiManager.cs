using System;
using System.Collections.Generic;
using Minis;
using UnityEngine;
using UnityEngine.InputSystem;

public class MidiManager : MonoBehaviour
{
    // Midi devices mapped to their product name
    public Dictionary<string, MidiDevice> devices = new();

    public delegate void MidiDeviceChanged(MidiDevice device);
    public event MidiDeviceChanged OnDeviceAdded;
    public event MidiDeviceChanged OnDeviceRemoved;

    private static MidiManager _instance;
    public static MidiManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = new GameObject("MidiManager").AddComponent<MidiManager>();
            }
            return _instance;
        }
    }

    private void OnEnable()
    {
        if (_instance != this && _instance != null)
            Destroy(this);
            
        _instance = this;
        InputSystem.onDeviceChange += OnInputSystemOnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnInputSystemOnDeviceChange;
    }

    private void OnApplicationQuit()
    {
        Destroy(this);
    }

    private void OnInputSystemOnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not MidiDevice midiDevice) return;

        switch (change)
        {
            case InputDeviceChange.Added:
                devices.Add(device.description.product, midiDevice);
                Debug.Log($"Midi device {device.description.product} connected.");
                OnDeviceAdded?.Invoke(midiDevice);
                break;
            case InputDeviceChange.Disconnected:
                devices.Remove(device.description.product);
                Debug.Log($"Midi device {device.description.product} disconnected.");
                OnDeviceRemoved?.Invoke(midiDevice);
                break;
            case InputDeviceChange.Removed:
                break;
            case InputDeviceChange.Reconnected:
                break;
            case InputDeviceChange.Enabled:
                break;
            case InputDeviceChange.Disabled:
                break;
            case InputDeviceChange.UsageChanged:
                break;
            case InputDeviceChange.ConfigurationChanged:
                break;
            case InputDeviceChange.SoftReset:
                break;
            case InputDeviceChange.HardReset:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, null);
        }
    }
}