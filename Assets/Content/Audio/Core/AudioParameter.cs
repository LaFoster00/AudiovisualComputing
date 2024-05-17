using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using USCSL;

// Abstraction for audio parameters. Value range for setting is always 0 - 1. Will be scaled up later automatically.
[Serializable]
public class AudioParameter
{
    [SerializeField, ReadOnly] public string name;
    [SerializeField] public float minValue;
    [SerializeField] public float maxValue;

    [SerializeField, Clamp("minValue", "maxValue"), OnValueChanged("CurrentValueChanged")]
    private float currentValue;

    [SerializeField, ReadOnly] private float normalizedValue = 0;

    private void CurrentValueChanged()
    {
        normalizedValue = CurrentNormalizedValue;
    }

    public float CurrentValue
    {
        get => currentValue;
        set => currentValue = math.clamp(value, minValue, maxValue);
    }

    public float CurrentNormalizedValue
    {
        get => currentValue.Map(minValue, maxValue, 0f, 1f);
        set => currentValue = math.clamp(value.Map(0f, 1f, minValue, maxValue), 0f, 1f);
    }
}