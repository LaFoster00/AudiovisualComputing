using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiQuadFilterModule : AudioProvider
{
    public AudioProvider target;

    [SerializeReference] public AudioParameter filterType = new()
    {
        name = "FilterType",
        minValue = 0,
        maxValue = (int)BiQuadFilterType.LastFilter,
        CurrentValue = 0
    };

    [SerializeReference] public AudioParameter frequency = new()
    {
        name = "Frequency",
        minValue = 20,
        maxValue = 20000,
        CurrentValue = 200
    };

    [SerializeReference] public AudioParameter q = new()
    {
        name = "Q",
        minValue = 0.1f,
        maxValue = 10f,
        CurrentValue = 1
    };

    [SerializeReference] public AudioParameter gain = new()
    {
        name = "Gain",
        minValue = -12,
        maxValue = 12,
        CurrentValue = 0
    };

    private BiQuadFilter[] _channelFilters;

    private bool initialized = false;

    private void OnEnable()
    {
        _channelFilters = new BiQuadFilter[AudioManager.Instance.WaveFormat.Channels];
        for (int channel = 0; channel < _channelFilters.Length; channel++)
        {
            _channelFilters[channel] = new BiQuadFilter();
        }

        filterType.onValueChanged.AddListener(OnFilterSettingsChanged);
        frequency.onValueChanged.AddListener(OnFilterSettingsChanged);
        q.onValueChanged.AddListener(OnFilterSettingsChanged);
        gain.onValueChanged.AddListener(OnFilterSettingsChanged);

        OnFilterSettingsChanged(null);
    }

    private void OnDisable()
    {
        filterType.onValueChanged.RemoveListener(OnFilterSettingsChanged);
        frequency.onValueChanged.RemoveListener(OnFilterSettingsChanged);
        q.onValueChanged.RemoveListener(OnFilterSettingsChanged);
        gain.onValueChanged.RemoveListener(OnFilterSettingsChanged);
    }

    private void OnFilterSettingsChanged(AudioParameter parameter)
    {
        var clearSamples = !initialized;
        foreach (var filter in _channelFilters)
        {
            switch ((BiQuadFilterType)Math.Round(filterType.CurrentValue))
            {
                case BiQuadFilterType.LowPass:
                    filter.SetLowPassFilter(frequency.CurrentValue, q.CurrentValue, clearSamples);
                    break;
                case BiQuadFilterType.HighPass:
                    filter.SetHighPassFilter(frequency.CurrentValue, q.CurrentValue, clearSamples);
                    break;
                case BiQuadFilterType.LowShelf:
                    filter.SetLowShelf(frequency.CurrentValue, q.CurrentValue, gain.CurrentValue, clearSamples);
                    break;
                case BiQuadFilterType.HighShelf:
                    filter.SetHighShelf(frequency.CurrentValue, q.CurrentValue, gain.CurrentValue, clearSamples);
                    break;
                case BiQuadFilterType.Notch:
                    filter.SetNotchFilter(frequency.CurrentValue, q.CurrentValue, clearSamples);
                    break;
                case BiQuadFilterType.Peaking:
                    filter.SetPeakingEq(frequency.CurrentValue, q.CurrentValue, gain.CurrentValue, clearSamples);
                    break;
                case BiQuadFilterType.AllPass:
                    filter.SetAllPassFilter(frequency.CurrentValue, q.CurrentValue, clearSamples);
                    break;
                case BiQuadFilterType.BandpassPeakGain:
                    filter.SetBandPassFilterConstantPeakGain(frequency.CurrentValue, q.CurrentValue, clearSamples);
                    break;
                case BiQuadFilterType.BandpassSkirtGain:
                    filter.SetBandPassFilterConstantSkirtGain(frequency.CurrentValue, q.CurrentValue, clearSamples);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        initialized = true;
    }

    public override void Read(Span<float> buffer, ulong nSample)
    {
        target.Read(buffer, nSample);
        for (int n = 0; n < buffer.Length; n++)
        {
            int ch = n % AudioManager.Instance.WaveFormat.Channels;

            buffer[n] = _channelFilters[ch].Transform(buffer[n]);
        }
    }

    // Here comes the actual filter logic
}