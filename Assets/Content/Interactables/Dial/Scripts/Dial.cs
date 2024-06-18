using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio.Core;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using USCSL;

public interface IDialUser
{
    void DialChanged(float dialValue);
}

public enum KnobType
{
    Linear = 0,
    Perceptual = 1
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

    private IXRSelectInteractor _interactor;
    private Quaternion _currentHandRotation;
    private float _currentDialRotation;

    protected override void OnEnable()
    {
        base.OnEnable();
        OnRotationBoundChanged();
    }

    private void OnRotationBoundChanged()
    {
        SetDialRotation(startPosition);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        _interactor = args.interactorObject;
        _interactor.transform.GetComponent<XRDirectInteractor>().hideControllerOnSelect = true;

        _currentHandRotation = GetInteractorRotation();
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
            UpdateDialRotation();
        }
    }

    private Quaternion GetInteractorRotation() => _interactor.transform.rotation;

    private void UpdateDialRotation()
    {
        var deltaRotation = Quaternion.Inverse(_currentHandRotation) * GetInteractorRotation();
        _currentHandRotation = GetInteractorRotation();
        var deltaAngle = deltaRotation.eulerAngles.z;
        if (deltaAngle > 180f)
        {
            deltaAngle -= 360f;
        }
        var normalizedDelta = deltaAngle / 360f;
        var newRotation = _currentDialRotation - normalizedDelta;

        SetDialRotation(newRotation);
    }

    private void SetDialRotation(float normalizedRotation)
    {
        _currentDialRotation = Mathf.Clamp01(normalizedRotation);

        float stepValue = 1f / (steps - 1);
        float snappedRotation = Mathf.Round(_currentDialRotation / stepValue) * stepValue;
        float angle = Mathf.Lerp(minRotation * 360f, maxRotation * 360f, snappedRotation);
        angle -= 180f;
        
        var localEulerAngles = linkedDial.localEulerAngles;
        localEulerAngles.y = angle;
        linkedDial.localEulerAngles = localEulerAngles;
        
        var newStep = (int)(snappedRotation * ActualSteps);
        if (newStep == currentStep)
            return;
        currentStep = newStep;
        if (TryGetComponent(out IDialUser dial))
            dial.DialChanged(MapValueToType(snappedRotation));
    }

    private float MapValueToType(float value)
    {
        return knobType switch
        {
            KnobType.Linear => value,
            KnobType.Perceptual => value
                .LinearSliderToMelSlider(0, 1, 20, 20000)
                .Map(20, 20000, 0, 1),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int ActualSteps => Math.Max(steps - 1, 0);
}