using System;
using UnityEngine;

public class SinGenerator : AudioProvider
{
    public AudioParameter frequency;
    
    public double portamentoTime = 0.2;
    
    private static float[] _waveTable;
    private double _phase;
    private double _currentPhaseStep;
    private double _targetPhaseStep;
    private double _phaseStepDelta;
    private bool _seekFreq;

    private AudioFormat _waveFormat;
    

    private void OnEnable()
    {
        _waveFormat = AudioManager.Instance.AudioFormat;
        if (_waveTable == null)
        {
            _waveTable = new float[_waveFormat.SampleRate];
            for (int index = 0; index < _waveFormat.SampleRate; ++index)
                _waveTable[index] =
                    (float)Math.Sin(2 * Math.PI * (double)index / _waveFormat.SampleRate);
            // For sawtooth instead of sine: waveTable[index] = (float)index / sampleRate;
        }

        _seekFreq = true;

        frequency.onValueChanged.AddListener(OnFrequencyChanged);
    }

    private void OnDisable()
    {
        frequency.onValueChanged.RemoveListener(OnFrequencyChanged);
    }

    private void OnFrequencyChanged(AudioParameter parameter)
    {
        _seekFreq = true;
    }

    public override void Read(Span<float> buffer)
    {
        if (_seekFreq) // process frequency change only once per call to Read
        {
            _targetPhaseStep = _waveTable.Length * (frequency.CurrentValue / _waveFormat.SampleRate);

            _phaseStepDelta = (_targetPhaseStep - _currentPhaseStep) / (_waveFormat.SampleRate * portamentoTime);
            _seekFreq = false;
        }
        for (int n = 0; n < buffer.Length; ++n)
        {
            int waveTableIndex = (int)_phase % _waveTable.Length;
            buffer[n] = _waveTable[waveTableIndex];
            _phase += _currentPhaseStep;
            if (_phase > _waveTable.Length)
                _phase -= _waveTable.Length;
            if (Math.Abs(_currentPhaseStep - _targetPhaseStep) > 0.0001f)
            {
                _currentPhaseStep += _phaseStepDelta;
                if (_phaseStepDelta > 0.0 && _currentPhaseStep > _targetPhaseStep)
                    _currentPhaseStep = _targetPhaseStep;
                else if (_phaseStepDelta < 0.0 && _currentPhaseStep < _targetPhaseStep)
                    _currentPhaseStep = _targetPhaseStep;
            }
        }
    }
}