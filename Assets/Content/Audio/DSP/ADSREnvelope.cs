using System;
using Unity.Mathematics;

public class ADSREnvelope
{
    public float Attack { get; set; }
    public float Decay { get; set; }
    public float Sustain { get; set; }
    public float Release { get; set; }

    public enum EnvelopeStage { Idle, Attack, Decay, Sustain, Release }
    public EnvelopeStage CurrentStage = EnvelopeStage.Idle;

    private float _currentLevel = 0.0f;
    private float _attackIncrement;
    private float _decayIncrement;
    private float _releaseIncrement;
    private float _sampleRate;

    private bool _noteOn = false;

    public ADSREnvelope(float sampleRate)
    {
        _sampleRate = sampleRate;
    }

    public void NoteOn()
    {
        _noteOn = true;
        CurrentStage = EnvelopeStage.Attack;
        _currentLevel = 0;
        _attackIncrement = 1.0f / (Attack * _sampleRate);
        _decayIncrement = (1.0f - Sustain) / (Decay * _sampleRate);
    }

    public void NoteOff()
    {
        if (!_noteOn)
            return;
        _noteOn = false;
        CurrentStage = EnvelopeStage.Release;
        
        _releaseIncrement = (_currentLevel + 0.00001f) / (Release * _sampleRate);
    }

    public float NextSample()
    {
        switch (CurrentStage)
        {
            case EnvelopeStage.Idle:
                break;

            case EnvelopeStage.Attack:
                _currentLevel += _attackIncrement;
                if (_currentLevel >= 1.0f)
                {
                    _currentLevel = 1.0f;
                    CurrentStage = EnvelopeStage.Decay;
                }
                break;

            case EnvelopeStage.Decay:
                _currentLevel -= _decayIncrement;
                if (_currentLevel <= Sustain)
                {
                    _currentLevel = Sustain;
                    CurrentStage = EnvelopeStage.Sustain;
                }
                break;

            case EnvelopeStage.Sustain:
                _currentLevel = Sustain;
                break;

            case EnvelopeStage.Release:
                _currentLevel -= _releaseIncrement;
                if (_currentLevel <= 0.0f)
                {
                    _currentLevel = 0.0f;
                    CurrentStage = EnvelopeStage.Idle;
                }
                break;
        }
        return _currentLevel;
    }
}