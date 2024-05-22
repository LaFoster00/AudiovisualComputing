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

[RequireComponent(typeof(XRGrabInteractable))]
public class Dial : MonoBehaviour
{
    
    [SerializeField] private Transform linkedDial;

    [SerializeField] private KnobType knobType = KnobType.Linear;
    [SerializeField, Range(0f, 1f), OnValueChanged("OnRotationBoundChanged")] private float minRotation;
    [SerializeField, Range(0f, 1f), OnValueChanged("OnRotationBoundChanged")] private float maxRotation;
    [SerializeField, Min(2), OnValueChanged("OnRotationBoundChanged")] private int steps = 25;
    [SerializeField, Range(0f, 1f), OnValueChanged("OnRotationBoundChanged")]
    private float startPosition;
    [SerializeField, ReadOnly] private int currentStep;
    
    [SerializeField, Range(0f, 1f)] private float angleTolerance = 0.5f;

    private IXRSelectInteractor _interactor;
    
    private Quaternion _startRotation;
    private bool _requiresStartAngle = true;
    private bool _shouldGetHandRotation = false;
    private XRGrabInteractable GrabInteractable => GetComponent<XRGrabInteractable>();
    private int ActualSteps => Math.Max(steps - 1, 0);
    private float MinRotationDegrees => Mathf.Repeat(minRotation * 359.9f, 360f);
    private float MaxRotationDegrees => Mathf.Repeat(maxRotation * 359.9f, 360f);

    private float SnapRotationAmount => ((maxRotation - minRotation) / ActualSteps) * 360f;


    private void OnEnable()
    {
        GrabInteractable.selectEntered.AddListener(GrabbedBy);
        GrabInteractable.selectExited.AddListener(GrabEnd);
        if (TryGetComponent(out IDialUser dial))
            dial.DialChanged(MapValueToType(1f/ActualSteps * currentStep));
    }

    private void OnDisable()
    {
        GrabInteractable.selectEntered.RemoveListener(GrabbedBy);
        GrabInteractable.selectExited.RemoveListener(GrabEnd);
    }

    private void OnRotationBoundChanged()
    {
        currentStep = (int)(ActualSteps * startPosition);
        var localEulerAngles = linkedDial.localEulerAngles;
        localEulerAngles.y = StepToAngle(currentStep);
        linkedDial.localEulerAngles = localEulerAngles;
        if (TryGetComponent(out IDialUser dial))
            dial.DialChanged(MapValueToType(1f/ActualSteps * currentStep));
    }

    private void GrabbedBy(SelectEnterEventArgs arg0)
    {
        _interactor = GrabInteractable.GetOldestInteractorSelecting();
        _interactor.transform.GetComponent<XRDirectInteractor>().hideControllerOnSelect = true;

        _shouldGetHandRotation = true;
        _startRotation = Quaternion.identity;
    }

    private void GrabEnd(SelectExitEventArgs arg0)
    {
        _shouldGetHandRotation = false;
        _requiresStartAngle = true;
    }

    private void Update()
    {
        if (_shouldGetHandRotation)
        {
            var rotationAngle = GetInteractorRotation();
            GetRotationDistance(rotationAngle);
        }
    }

    private Quaternion GetInteractorRotation() => _interactor.transform.localRotation;

    private void GetRotationDistance(Quaternion currentRotation)
    {
        if (!_requiresStartAngle)
        {
            var rotationDifference = Quaternion.Inverse(_startRotation) * currentRotation;
            var zAxisRotationDifference = rotationDifference.eulerAngles.z;
            if (!(zAxisRotationDifference > angleTolerance * SnapRotationAmount)) return;
            if (zAxisRotationDifference > 180f)
            {
                zAxisRotationDifference -= 360;
            }

            var nSteps = (int)(zAxisRotationDifference / SnapRotationAmount);
            RotateDial(nSteps);
            _startRotation = currentRotation;
        }
        else
        {
            _requiresStartAngle = false;
            _startRotation = currentRotation;
        }
    }

    private void RotateDial(int nSteps)
    {
        currentStep = math.clamp(currentStep - nSteps, 0, ActualSteps);
        var localEulerAngles = linkedDial.localEulerAngles;
        localEulerAngles.y = StepToAngle(currentStep);
        linkedDial.localEulerAngles = localEulerAngles;
        if (TryGetComponent(out IDialUser dial))
            dial.DialChanged(MapValueToType(1f/ActualSteps * currentStep));
    }

    private float StepToAngle(int step)
    {
        return Mathf.Repeat(MinRotationDegrees + step * SnapRotationAmount - 180f, 360f);
    }

    private float CheckAngle(float currentAngle, float startAngle) => (360f - currentAngle) + startAngle;

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

}