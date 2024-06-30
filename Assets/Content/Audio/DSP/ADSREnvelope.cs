using System;
using Unity.Mathematics;

public class ADSREnvelope
{
    private float _attack;

    public float Attack
    {
        get => _attack;
        set
        {
            _attack = value;
            RecalculateIncrements();
        }
    }

    private float _decay;

    public float Decay
    {
        get => _decay;
        set
        {
            _decay = value;
            RecalculateIncrements();
        }
    }

    private float _sustain;

    public float Sustain
    {
        get => _sustain;
        set
        {
            _sustain = value;
            RecalculateIncrements();
        }
    }

    private float _release;

    public float Release
    {
        get => _release;
        set
        {
            _release = value;
            RecalculateIncrements();
        }
    }

    public enum EnvelopeStage
    {
        Idle,
        Attack,
        Decay,
        Sustain,
        Release
    }

    public EnvelopeStage CurrentStage { get; private set; } = EnvelopeStage.Idle;

    private readonly float _sampleRate;

    private float _currentLevel = 0.0f;

    private float _attackIncrement;
    private float _decayIncrement;
    private float _releaseIncrement;

    public ADSREnvelope(float sampleRate)
    {
        _sampleRate = sampleRate;
    }

    public void NoteOn()
    {
        CurrentStage = EnvelopeStage.Attack;
        _currentLevel = 0;
    }

    public void NoteOff()
    {
        CurrentStage = EnvelopeStage.Release;
    }

    private void RecalculateIncrements()
    {
        _releaseIncrement = (Sustain + 0.00001f) / (Release * _sampleRate);
        _decayIncrement = (1.0f - Sustain) / (Decay * _sampleRate);
        _attackIncrement = 1.0f / (Attack * _sampleRate);
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