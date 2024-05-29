using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using USCSL;

public interface IDialUser
{
    void DialChanged(float dialValue);
}

public enum KnobType
{
    Linear = 0,
    Logarithmic = 1,
    Logarithmic2 = 2,
    Logarithmic10 = 3,
    Exponential = 4
}

[SelectionBase]
[DisallowMultipleComponent]
public class Dial : XRBaseInteractable
{
    [SerializeField] private Transform linkedDial;

    [SerializeField] private KnobType knobType = KnobType.Linear;

    [SerializeField, Range(0f, 1f), OnValueChanged("OnRotationBoundChanged")]
    private float minRotation;

    [SerializeField, Range(0f, 1f), OnValueChanged("OnRotationBoundChanged")]
    private float maxRotation;

    [SerializeField, Min(2), OnValueChanged("OnRotationBoundChanged")]
    private int steps = 25;

    [SerializeField, Range(0f, 1f), OnValueChanged("OnRotationBoundChanged")]
    private float startPosition;

    [SerializeField, ReadOnly] private int currentStep;

    [SerializeField, Range(0f, 1f)] private float angleTolerance = 0.5f;

    private IXRSelectInteractor _interactor;
    private Quaternion _initialHandRotation;
    private float _currentDialRotation;
    private float _initialDialRotation;

    protected override void OnEnable()
    {
        base.OnEnable();
        OnRotationBoundChanged();
    }

    private void OnRotationBoundChanged()
    {
        RotateDial(startPosition);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        _interactor = args.interactorObject;
        _interactor.transform.GetComponent<XRDirectInteractor>().hideControllerOnSelect = true;

        _initialHandRotation = GetInteractorRotation();
        _initialDialRotation = _currentDialRotation;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        _interactor.transform.GetComponent<XRDirectInteractor>().hideControllerOnSelect = false;
        _interactor = null;
    }

    private void Update()
    {
        if (_interactor != null)
        {
            RotateDial();
        }
    }

    private Quaternion GetInteractorRotation() => _interactor.transform.rotation;

    private void RotateDial()
    {
        Quaternion currentHandRotation = GetInteractorRotation();
        var deltaRotation = Quaternion.Inverse(_initialHandRotation) * currentHandRotation;
        var deltaAngle = deltaRotation.eulerAngles.z;
        if (deltaAngle > 180f)
        {
            deltaAngle -= 360f;
        }
        var normalizedDelta = deltaAngle / 360f;
        var newRotation = _initialDialRotation - normalizedDelta;

        RotateDial(newRotation);
    }

    private void RotateDial(float normalizedRotation)
    {

        
        _currentDialRotation = Mathf.Clamp01(normalizedRotation);

        float stepValue = 1f / (steps - 1);
        float snappedRotation = Mathf.Round(_currentDialRotation / stepValue) * stepValue;
        float angle = Mathf.Lerp(minRotation * 360f, maxRotation * 360f, snappedRotation);
        angle -= 180f;
        
        var localEulerAngles = linkedDial.localEulerAngles;
        localEulerAngles.y = angle;
        linkedDial.localEulerAngles = localEulerAngles;
        
        var newStep = (int)(normalizedRotation * ActualSteps);
        if (newStep == currentStep)
            return;
        currentStep = newStep;
        if (TryGetComponent(out IDialUser dial))
            dial.DialChanged(MapValueToType(snappedRotation));
    }

    private float StepToAngle(int step)
    {
        return Mathf.Repeat(MinRotationDegrees + step * SnapRotationAmount - 180f, 360f);
    }

    private float MapValueToType(float value)
    {
        return knobType switch
        {
            KnobType.Linear => value,
            KnobType.Logarithmic => MapToLogScale(value, math.E),
            KnobType.Logarithmic2 => MapToLogScale(value, 2),
            KnobType.Logarithmic10 => MapToLogScale(value, 10),
            KnobType.Exponential => math.exp(value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float MapToLogScale(float value, float logBase)
    {
        // Ensure the value is within the expected range
        if (value < 0 || value > 1)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be in the range [0, 1]");

        // Map the linear value to the logarithmic scale
        var logMinValue = Math.Log(0.0001f, logBase);
        var logMaxValue = Math.Log(1.0f, logBase);

        var logValue = logMinValue + value * (logMaxValue - logMinValue);
        return (float)Math.Pow(logBase, logValue);
    }

    private int ActualSteps => Math.Max(steps - 1, 0);
    private float MinRotationDegrees => Mathf.Repeat(minRotation * 359.9f, 360f);
    private float MaxRotationDegrees => Mathf.Repeat(maxRotation * 359.9f, 360f);
    private float SnapRotationAmount => ((maxRotation - minRotation) / ActualSteps) * 360f;
}