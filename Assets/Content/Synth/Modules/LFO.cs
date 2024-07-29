using System;
using System.Collections;
using System.Collections.Generic;
using Audio.Core;
using NaughtyAttributes;
using NWaves.Signals.Builders;
using UnityEngine;

public class LFO : AudioProvider
{
    [Header("Wave Settings")] public AudioParameter waveType;
    public AudioParameter frequency;
    public AudioProvider frequencyOffset;

    private float[] _waveTable;
    private PinkNoiseBuilder _pinkNoise;
    private ADSREnvelope _adsrEnvelope;

    private double _phase;
    private double _currentPhaseStep;

    private AudioFormat _waveFormat;

    private WorkingBuffer _frequencyOffsetSamples;
    
    public override bool CanProvideAudio => true;
    
    [Button("Populate AudioParameters")]
    private void PopulateAudioParameters()
    {
        var existingAudioParameters = GetComponents<AudioParameter>();
        this.PopulateAudioParameter(
            existingAudioParameters,
            ref waveType,
            "WaveType",
            0,
            Enum.GetNames(typeof(WaveType)).Length - 1);
        this.PopulateAudioParameter(
            existingAudioParameters,
            ref frequency,
            "Frequency",
            0.1f,
            20f);
    }

    private void OnEnable()
    {
        _waveFormat = AudioManager.Instance.AudioFormat;

        #region SetupBuffersAndHelpers

        _frequencyOffsetSamples = new WorkingBuffer();

        #endregion

        #region InitializeWaveTable

        waveType.onValueChanged.AddListener(OnWaveTypeChanged);

        _pinkNoise = (PinkNoiseBuilder)new PinkNoiseBuilder()
            .SampledAt(_waveFormat.SampleRate);
        OnWaveTypeChanged(null);

        #endregion
    }
    
    private void OnDisable()
    {
        waveType.onValueChanged.RemoveListener(OnWaveTypeChanged);
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

    protected override void Preprocess_Impl(uint numSamples, ulong frame)
    {
        frequencyOffset.Preprocess(numSamples, frame);
    }

    public override void Read(Span<float> buffer)
    {
        frequencyOffset.Read(_frequencyOffsetSamples);
        
        for (int n = 0; n < buffer.Length; n += _waveFormat.Channels)
        {
            ReadWave(buffer, n);
            UpdateWaveTableIndex(_frequencyOffsetSamples, n);
        }
    }
    
    private void UpdateWaveTableIndex(Span<float> frequencyOffsetSamples, int sample)
    {
        _currentPhaseStep = _waveTable.Length *
                            (frequency.CurrentValue.NoteOffsetFrequency(
                                 frequencyOffsetSamples[sample].NoteOffset()) /
                             _waveFormat.SampleRate);
        _phase = (_phase + _currentPhaseStep) % _waveTable.Length;
    }
    
    private void ReadWave(Span<float> buffer, int n)
    {
        switch ((WaveType)waveType.CurrentValue)
        {
            case WaveType.PinkNoise:
            {
                var sample = _pinkNoise.NextSample();
                for (int ch = 0; ch < _waveFormat.Channels; ch++)
                {
                    buffer[n + ch] = sample;
                }

                break;
            }
            case WaveType.Sin:
            case WaveType.Saw:
            case WaveType.Square:
            default:
            {
                var sample = _waveTable[(int)_phase];
                for (int ch = 0; ch < _waveFormat.Channels; ch++)
                {
                    buffer[n + ch] = sample;
                }

                break;
            }
        }
    }
}
