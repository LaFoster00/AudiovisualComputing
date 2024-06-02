using System;
using NWaves.Signals;
using NWaves.Signals.Builders;
using NWaves.Signals.Builders.Base;

public enum WaveType
{
    Sin,
    Saw,
    Square,
    PinkNoise
}

public class Oscillator : AudioProvider
{
    public AudioParameter waveType;
    public AudioParameter frequency;
    public AudioParameter portamentoTime;
    
    private float[] _waveTable;
    private PinkNoiseBuilder _pinkNoise;
    
    private double _phase;
    private double _currentPhaseStep;
    private double _targetPhaseStep;
    private double _phaseStepDelta;
    private bool _seekFreq;

    private AudioFormat _waveFormat;


    private void OnEnable()
    {
        _waveFormat = AudioManager.Instance.AudioFormat;

        _seekFreq = true;

        frequency.onValueChanged.AddListener(OnFrequencyChanged);
        waveType.onValueChanged.AddListener(OnWaveTypeChanged);

        _pinkNoise = (PinkNoiseBuilder)new PinkNoiseBuilder()
            .SampledAt(_waveFormat.SampleRate);
        
        OnWaveTypeChanged(null);
        OnFrequencyChanged(null);
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

    private void OnDisable()
    {
        frequency.onValueChanged.RemoveListener(OnFrequencyChanged);
        waveType.onValueChanged.RemoveListener(OnWaveTypeChanged);
    }

    private void OnFrequencyChanged(AudioParameter parameter)
    {
        _seekFreq = true;
    }

    public override void Read(Span<float> buffer)
    {
        if (_seekFreq) // process frequency change only once per call to Read
        {
            _targetPhaseStep = _waveTable.Length * ((frequency.CurrentValue) / _waveFormat.SampleRate);

            _phaseStepDelta = (_targetPhaseStep - _currentPhaseStep) / (_waveFormat.SampleRate * portamentoTime.CurrentValue);
            _seekFreq = false;
        }
        for (int n = 0; n < buffer.Length; n+=_waveFormat.Channels)
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
                int waveTableIndex = (int)_phase % _waveTable.Length;
                var sample = _waveTable[waveTableIndex];
                for (int ch = 0; ch < _waveFormat.Channels; ch++)
                {
                    buffer[n + ch] = sample;
                }
            }

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