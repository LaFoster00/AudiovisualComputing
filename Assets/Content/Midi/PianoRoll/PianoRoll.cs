using System;
using Audio.Core;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using AudioSettings = Audio.Core.AudioSettings;

public class PianoRoll : MonoBehaviour, IInputDevice, IOutputDevice
{
    public bool IsListeningForEvents { get; private set; }
    public event EventHandler<MidiEventReceivedEventArgs> EventReceived;
    public event EventHandler<MidiEventSentEventArgs> EventSent;


    [SerializeField, Required] private Image pianoRollScreeen;
    [SerializeField] private int measures = 1;

    private Vector3 grabPoint;
    private Vector3 grabPointInSurfaceSpace;
    private XRRayInteractor currentInteractor;

    private Material pianoRollMat;

    private Vector2 _shaderPosition;

    private float _beat;

    private float _beatIncreasePerSample;

    private Vector2 shaderPosition
    {
        get => _shaderPosition;
        set
        {
            _shaderPosition = value;
            pianoRollMat.SetFloat(PositionX, _shaderPosition.x);
            pianoRollMat.SetFloat(PositionY, _shaderPosition.y);
        }
    }

    private static readonly int PositionX = Shader.PropertyToID("PositionX");
    private static readonly int PositionY = Shader.PropertyToID("PositionY");
    private static readonly int CursorTime = Shader.PropertyToID("CursorTime");
    private static readonly int NumberOfBars = Shader.PropertyToID("NumberOfBars");

    protected void OnEnable()
    {
        AudioManager.Instance.SampleProvider.OnDataRead += SampleProviderOnOnDataRead;

        pianoRollMat = pianoRollScreeen.material;

        // Multiply by 4 since we have 16 beats per 4 / 4 measure
        _beatIncreasePerSample =
            (float)AudioSettings.Instance.Tempo * 4 / 60 / AudioManager.Instance.SampleProvider.AudioFormat.SampleRate;

        shaderPosition = Vector2.zero;
        pianoRollMat.SetInt(NumberOfBars, measures);
    }

    private void SampleProviderOnOnDataRead(int samples)
    {
        var samplesPerChannel = samples / AudioManager.Instance.SampleProvider.AudioFormat.Channels;
        _beat += samplesPerChannel * _beatIncreasePerSample;
        _beat %= measures * AudioSettings.Instance.BeatsPerMeasure;
        //Debug.Log(_beat);
    }

    #region Interactable

    #endregion

    private void Update()
    {
        pianoRollMat.SetFloat(CursorTime, _beat);
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
        if (IsListeningForEvents)
            SendEvent(e.Event);
    }
}