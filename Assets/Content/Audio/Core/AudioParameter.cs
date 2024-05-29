using System;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using USCSL;

public class AudioParameter : MonoBehaviour
{
    public string parameterName;
    public float minValue;
    public float maxValue;

    [SerializeField, Clamp("minValue", "maxValue"), OnValueChanged("CurrentValueChanged"), ReadOnly, AllowNesting]
    private float currentValue;

    [SerializeField, ReadOnly, AllowNesting]
    private float currentNormalizedValue = 0;

    public UnityEvent<AudioParameter> onValueChanged = new();

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