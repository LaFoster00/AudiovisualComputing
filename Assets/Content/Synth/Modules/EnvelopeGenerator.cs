using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class EnvelopeGenerator : AudioProvider
{
    [Header("Shaping")] public AudioProvider gate;
    public AudioParameter attack;
    public AudioParameter decay;
    public AudioParameter sustain;
    public AudioParameter release;

    private ADSREnvelope _adsrEnvelope;
    private AudioFormat _waveFormat;

    private WorkingBuffer _gateBuffer;
    private float _previousGate;
    
    public override bool CanProvideAudio => true;
    
    [Button("Populate AudioParameters")]
    private void PopulateAudioParameters()
    {
        var existingAudioParameters = GetComponents<AudioParameter>();
        this.PopulateAudioParameter(
            existingAudioParameters,
            ref attack,
            "Attack",
            0.001f,
            2);
        this.PopulateAudioParameter(
            existingAudioParameters,
            ref decay,
            "Decay",
            0.001f,
            4);
        this.PopulateAudioParameter(
            existingAudioParameters,
            ref sustain,
            "Sustain",
            0.001f,
            1);
        this.PopulateAudioParameter(
            existingAudioParameters,
            ref release,
            "Release",
            0.001f,
            4);
    }

    private void OnEnable()
    {
        _waveFormat = AudioManager.Instance.AudioFormat;
        
        attack.onValueChanged.AddListener(OnEnvelopeChanged);
        decay.onValueChanged.AddListener(OnEnvelopeChanged);
        sustain.onValueChanged.AddListener(OnEnvelopeChanged);
        release.onValueChanged.AddListener(OnEnvelopeChanged);

        _adsrEnvelope = new ADSREnvelope(
            _waveFormat.SampleRate);
        OnEnvelopeChanged(null);
    }
    
    
    
    private void OnEnvelopeChanged(AudioParameter parameter)
    {
        _adsrEnvelope.Attack = attack.CurrentValue;
        _adsrEnvelope.Decay = decay.CurrentValue;
        _adsrEnvelope.Sustain = sustain.CurrentValue;
        _adsrEnvelope.Release = release.CurrentValue;
    }

    public override void Read(Span<float> buffer)
    {
        gate.Read(_gateBuffer);

        for (int n = 0; n < buffer.Length; n+= _waveFormat.Channels)
        {
            switch (_gateBuffer[n])
            {
                // Check if the envelope should trigger
                case >= 0.5f when _previousGate <= 0.5f:
                    _adsrEnvelope.NoteOn();
                    break;
                // Check if the envelope should turn off
                case <= 0.5f when _previousGate >= 0.5f:
                    _adsrEnvelope.NoteOff();
                    break;
            }

            _previousGate = _gateBuffer[n];
            
            var envelopeSample = _adsrEnvelope.NextSample();
            for (int ch = 0; ch < _waveFormat.Channels; ch++)
            {
                buffer[n + ch] *= envelopeSample;
            }
        }
    }
}
