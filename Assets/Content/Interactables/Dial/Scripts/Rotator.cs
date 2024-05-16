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

internal interface IDial
{
    void DialChanged(float dialValue);
}

[RequireComponent(typeof(XRGrabInteractable))]
public class Rotator : MonoBehaviour
{
    [SerializeField] private Transform linkedDial;

    [SerializeField, Range(0f, 1f), OnValueChanged("OnRotationBoundChanged")] private float minRotation;
    [SerializeField, Range(0f, 1f), OnValueChanged("OnRotationBoundChanged")] private float maxRotation;
    [SerializeField, Min(2), OnValueChanged("OnRotationBoundChanged")] private int steps = 25;
    [SerializeField, Range(0f, 1f), OnValueChanged("OnRotationBoundChanged")]
    private float startPosition;
    [SerializeField, ReadOnly] private int currentStep;
    
    [SerializeField, Range(0f, 1f)] private float angleTolerance = 0.5f;

    private IXRSelectInteractor _interactor;
    
    private float _startAngle;
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
    }

    private void GrabbedBy(SelectEnterEventArgs arg0)
    {
        _interactor = GrabInteractable.GetOldestInteractorSelecting();
        _interactor.transform.GetComponent<XRDirectInteractor>().hideControllerOnSelect = true;

        _shouldGetHandRotation = true;
        _startAngle = 0f;
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

    private float GetInteractorRotation() => _interactor.transform.eulerAngles.z;

    private void GetRotationDistance(float currentAngle)
    {
        if (!_requiresStartAngle)
        {
            var angleDifference = math.abs(_startAngle - currentAngle);
            if (!(angleDifference > angleTolerance * SnapRotationAmount)) return;

            var angleCheck = CheckAngle(currentAngle, _startAngle);
            if (angleCheck > angleTolerance * SnapRotationAmount)
            {
                RotateDial(_startAngle < currentAngle);
                _startAngle = currentAngle;
            }
        }
        else
        {
            _requiresStartAngle = false;
            _startAngle = currentAngle;
        }
    }

    private void RotateDial(bool clockWise)
    {
        currentStep = math.clamp(currentStep + (clockWise ? -1 : 1), 0, ActualSteps);
        var localEulerAngles = linkedDial.localEulerAngles;
        localEulerAngles.y = StepToAngle(currentStep);
        linkedDial.localEulerAngles = localEulerAngles;
        if (TryGetComponent(out IDial dial))
            dial.DialChanged(1f/ActualSteps * currentStep);
    }

    private float StepToAngle(int step)
    {
        return Mathf.Repeat(MinRotationDegrees + step * SnapRotationAmount - 180f, 360f);
    }

    private float CheckAngle(float currentAngle, float startAngle) => (360f - currentAngle) + startAngle;
}