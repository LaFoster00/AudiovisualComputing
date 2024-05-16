using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using USCSL;

// Abstraction for audio parameters. Value range for setting is always 0 - 1. Will be scaled up later automatically.
[Serializable]
public class AudioParameter
{
    [SerializeField, ReadOnly] public string name;
    [SerializeField] public float minValue;
    [SerializeField] public float maxValue;
    [SerializeField] private float currentValue;

    public float CurrentValue
    {
        get => currentValue;
        set => currentValue = math.clamp(value, 0f, 1f).Map(0f, 1f, minValue, maxValue);
    }
}