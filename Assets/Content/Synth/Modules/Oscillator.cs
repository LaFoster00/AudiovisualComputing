using System;
using NaughtyAttributes;
using NWaves.Signals.Builders;
using UnityEngine;

public enum WaveType
{
    Sin,
    Saw,
    Square,
    PinkNoise
}

public class Oscillator : AudioProvider
{
    [Header("Wave Settings")] public AudioParameter waveType;
    public AudioParameter frequency;
    public AudioProvider frequencyOffset;

    [Header("Shaping")] public AudioParameter attack;
    public AudioParameter decay;
    public AudioParameter sustain;
    public AudioParameter release;

    private float[] _waveTable;
    private PinkNoiseBuilder _pinkNoise;
    private ADSREnvelope _adsrEnvelope;

    private double _phase;
    private double _currentPhaseStep;

    private AudioFormat _waveFormat;

    private WorkingBuffer _frequencyOffsetSamples;


    [Button("Populate AudioParameters")]
    private void PopulateAudioParameters()
    {
        var existingAudioParameters = GetComponents<AudioParameter>();
        this.PopulateAudioParameter(
            existingAudioParameters,
            ref waveType,
            "WaveType",
            0,
            Enum.GetNames(typeof(WaveType)).Length);
        this.PopulateAudioParameter(
            existingAudioParameters,
            ref frequency,
            "Frequency",
            20,
            20000);
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
            0,
            10);
        this.PopulateAudioParameter(
            existingAudioParameters,
            ref sustain,
            "Sustain",
            0,
            1);
        this.PopulateAudioParameter(
            existingAudioParameters,
            ref release,
            "Release",
            0,
            10);
    }

    private void OnEnable()
    {
        _waveFormat = AudioManager.Instance.AudioFormat;

        #region InitializeWaveTable

        waveType.onValueChanged.AddListener(OnWaveTypeChanged);

        _pinkNoise = (PinkNoiseBuilder)new PinkNoiseBuilder()
            .SampledAt(_waveFormat.SampleRate);
        OnWaveTypeChanged(null);

        #endregion

        #region InitializeEnvelope

        attack.onValueChanged.AddListener(OnEnvelopeChanged);
        decay.onValueChanged.AddListener(OnEnvelopeChanged);
        sustain.onValueChanged.AddListener(OnEnvelopeChanged);
        release.onValueChanged.AddListener(OnEnvelopeChanged);

        _adsrEnvelope = new ADSREnvelope(
            _waveFormat.SampleRate);
        OnEnvelopeChanged(null);

        #endregion
    }

    private void OnWaveTypeChanged(AudioParameter arg0)
    {
        switch ((WaveType)waveType.CurrentValue)
        {
            case WaveType.Sin:
                _waveTable = new SineBuilder()
                    .SetParameter("frequency", 1)
                    .OfLength(_waveFormat.SampleRate)
                    .SampledAt(_waveFormat.SampleRate)
                    .Build().Samples;
                break;
            case WaveType.Saw:
                _waveTable = new SawtoothBuilder()
                    .SetParameter("frequency", 1)
                    .OfLength(_waveFormat.SampleRate)
                    .SampledAt(_waveFormat.SampleRate)
                    .Build().Samples;
                break;
            case WaveType.Square:
                _waveTable = new SquareWaveBuilder()
                    .SetParameter("frequency", 1)
                    .OfLength(_waveFormat.SampleRate)
                    .SampledAt(_waveFormat.SampleRate)
                    .Build().Samples;
                break;
            case WaveType.PinkNoise:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnEnvelopeChanged(AudioParameter parameter)
    {
        _adsrEnvelope.Attack = attack.CurrentValue;
        _adsrEnvelope.Decay = decay.CurrentValue;
        _adsrEnvelope.Sustain = sustain.CurrentValue;
        _adsrEnvelope.Release = release.CurrentValue;
    }

    private void OnDisable()
    {
        waveType.onValueChanged.RemoveListener(OnWaveTypeChanged);
    }

    public override void Read(Span<float> buffer)
    {
        
        for (int n = 0; n < buffer.Length; n += _waveFormat.Channels)
        {
            ReadWave(buffer, n);
            ApplyEnvelope(buffer, n);

            UpdateWaveTableIndex();
        }
    }
    
    private void UpdateWaveTableIndex()
    {
        _currentPhaseStep = _waveTable.Length * ((frequency.CurrentValue) / _waveFormat.SampleRate);
        _phase = (_phase + _currentPhaseStep) % _waveTable.Length;
        
        
    }

    private void ApplyEnvelope(Span<float> buffer, int n)
    {
        switch (_adsrEnvelope.CurrentStage)
        {
            case ADSREnvelope.EnvelopeStage.Idle:
                _adsrEnvelope.NoteOn();
                break;
            case ADSREnvelope.EnvelopeStage.Sustain:
            case ADSREnvelope.EnvelopeStage.Release:
                _adsrEnvelope.NoteOff();
                break;
        }

        var envelopeSample = _adsrEnvelope.NextSample();
        for (int ch = 0; ch < _waveFormat.Channels; ch++)
        {
            buffer[n + ch] *= envelopeSample;
        }
    }

    

    private void ReadWave(Span<float> buffer, int n)
    {
        if ((WaveType)waveType.CurrentValue == WaveType.PinkNoise)
        {
            var sample = _pinkNoise.NextSample();
            for (int ch = 0; ch < _waveFormat.Channels; ch++)
            {
                buffer[n + ch] = sample;
            }
        }
        else
        {
            var sample = _waveTable[(int)_phase];
            for (int ch = 0; ch < _waveFormat.Channels; ch++)
            {
                buffer[n + ch] = sample;
            }
        }
    }
}