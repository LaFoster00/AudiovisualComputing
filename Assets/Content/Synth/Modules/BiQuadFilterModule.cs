using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Filters.BiQuad;
using UnityEngine;
using USCSL;

public enum BiQuadFilterType
{
    LowPass,
    HighPass,
    BandPass,
    Notch,
    AllPass,
    Peak,
    LowShelf,
    HighShelf,
}

public class BiQuadFilterModule : AudioProvider
{
    public AudioProvider target;

    public AudioParameter filterType;
    public AudioParameter frequency;
    public AudioParameter q;
    public AudioParameter gain;

    private IEnumerator<double> _currentFrequency;
    private IEnumerator<double> _currentQ;
    private IEnumerator<double> _currentGain;

    private BiQuadFilter[] _channelFilters;

    private readonly float _nextAfter05 = 0.4999f;

    public override bool CanProvideAudio => true;
    
    private void OnEnable()
    {
        _channelFilters = new BiQuadFilter[AudioManager.Instance.AudioFormat.Channels];

        filterType.onValueChanged.AddListener(OnFilterTypeChanged);
        frequency.onValueChanged.AddListener(OnFilterSettingsChanged);
        q.onValueChanged.AddListener(OnFilterSettingsChanged);
        gain.onValueChanged.AddListener(OnFilterSettingsChanged);

        OnFilterTypeChanged(null);
    }

    private void SetFilter<T>(params object[] args) where T : BiQuadFilter
    {
        var constructor = typeof(T).GetConstructor(args.Select(a => a.GetType()).ToArray());

        if (constructor == null)
        {
            throw new ArgumentException(
                $"No matching constructor found for type {typeof(T).Name} with the specified arguments.");
        }

        for (int channel = 0; channel < _channelFilters.Length; channel++)
        {
            _channelFilters[channel] = (T)constructor.Invoke(args);
        }
    }

    private void OnDisable()
    {
        filterType.onValueChanged.RemoveListener(OnFilterSettingsChanged);
        frequency.onValueChanged.RemoveListener(OnFilterSettingsChanged);
        q.onValueChanged.RemoveListener(OnFilterSettingsChanged);
        gain.onValueChanged.RemoveListener(OnFilterSettingsChanged);
    }

    private void OnFilterTypeChanged(AudioParameter arg0)
    {
        switch ((BiQuadFilterType)System.Math.Round(filterType.CurrentValue))
        {
            case BiQuadFilterType.LowPass:
                SetFilter<LowPassFilter>(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue);
                break;
            case BiQuadFilterType.HighPass:
                SetFilter<HighPassFilter>(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue);
                break;
            case BiQuadFilterType.BandPass:
                SetFilter<BandPassFilter>(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue);
                break;
            case BiQuadFilterType.Notch:
                SetFilter<NotchFilter>(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue);
                break;
            case BiQuadFilterType.AllPass:
                SetFilter<AllPassFilter>(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue);
                break;
            case BiQuadFilterType.Peak:
                SetFilter<PeakFilter>(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue, gain.CurrentValue);
                break;
            case BiQuadFilterType.LowShelf:
                SetFilter<LowShelfFilter>(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue, gain.CurrentValue);
                break;
            case BiQuadFilterType.HighShelf:
                SetFilter<HighShelfFilter>(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue, gain.CurrentValue);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnFilterSettingsChanged(AudioParameter parameter)
    {
        foreach (var channelFilter in _channelFilters)
        {
            switch (channelFilter)
            {
                case LowPassFilter filter:
                    filter.Change(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue);
                    break;
                case HighPassFilter filter:
                    filter.Change(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue);
                    break;
                case BandPassFilter filter:
                    filter.Change(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue);
                    break;
                case NotchFilter filter:
                    filter.Change(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue);
                    break;
                case AllPassFilter filter:
                    filter.Change(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue);
                    break;
                case PeakFilter filter:
                    filter.Change(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue, gain.CurrentValue);
                    break;
                case LowShelfFilter filter:
                    filter.Change(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue, gain.CurrentValue);
                    break;
                case HighShelfFilter filter:
                    filter.Change(frequency.CurrentNormalizedValue * _nextAfter05, q.CurrentValue, gain.CurrentValue);
                    break;
            }
        }
    }

    protected override void Preprocess_Impl(uint numSamples, ulong frame)
    {
        target.Preprocess(numSamples, frame);
    }

    public override void Read(Span<float> buffer)
    {
        target.Read(buffer);
        for (var n = 0; n < buffer.Length; n++)
        {
            var ch = n % AudioManager.Instance.AudioFormat.Channels;
            buffer[n] = _channelFilters[ch].Process(buffer[n]);
        }
    }

    // Here comes the actual filter logic
}