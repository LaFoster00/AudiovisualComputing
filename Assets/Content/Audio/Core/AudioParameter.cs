using System;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using USCSL;

// Abstraction for audio parameters. Value range for setting is always 0 - 1. Will be scaled up later automatically.
[Serializable]
public class AudioParameter
{
    [SerializeField]
    public string name;

    [SerializeField] public float minValue;
    [SerializeField] public float maxValue;

    [SerializeField, Clamp("minValue", "maxValue"), OnValueChanged("CurrentValueChanged"), ReadOnly, AllowNesting]
    private float currentValue;

    [SerializeField, ReadOnly, AllowNesting]
    private float currentNormalizedValue = 0;

    public UnityEvent<AudioParameter> onValueChanged;

    private void CurrentValueChanged()
    {
        CurrentValue = currentValue;
    }

    public float CurrentValue
    {
        get => currentValue;
        set
        {
            currentNormalizedValue = math.clamp(value.Map(minValue, maxValue, 0f, 1f), 0f, 1f);
            currentValue = math.clamp(value, minValue, maxValue);
            onValueChanged?.Invoke(this);
        }
    }

    [ShowNativeProperty]
    public float CurrentNormalizedValue
    {
        get => currentNormalizedValue;
        set
        {
            currentNormalizedValue = math.clamp(value, 0f, 1f);
            currentValue = math.clamp(value.Map(0f, 1f, minValue, maxValue), minValue, maxValue);
            onValueChanged?.Invoke(this);
        }
    }
}