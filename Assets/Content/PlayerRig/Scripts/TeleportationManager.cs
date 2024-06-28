using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRRayInteractor))]
[RequireComponent(typeof(LineRenderer))]
public class TeleportationManager : MonoBehaviour
{
    [Required, SerializeField] private InputActionReference teleportActivate;
    [Required, SerializeField] private InputActionReference teleportCancel;

    [Required, SerializeField] private TeleportationProvider teleportationProvider;

    private bool canceled;
    
    private XRRayInteractor _rayInteractor;
    private LineRenderer _lineRenderer;
    
    private void OnEnable()
    {
        teleportActivate.action.started += OnTeleportationStarted;
        teleportActivate.action.canceled += OnTeleportationEnded;
        teleportCancel.action.canceled += OnTeleportationCanceled;
    }


    private void Start()
    {
        _rayInteractor = GetComponent<XRRayInteractor>();
        _lineRenderer = GetComponent<LineRenderer>();
        _rayInteractor.enabled = false;
        _lineRenderer.enabled = false;
        teleportActivate.action.Enable();
    }

    private void OnTeleportationStarted(InputAction.CallbackContext callbackContext)
    {
        _rayInteractor.enabled = true;
        _lineRenderer.enabled = true;
        canceled = false;
    }
    
    private void OnTeleportationEnded(InputAction.CallbackContext obj)
    {
        if (!_rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit) || canceled)
        {
            _rayInteractor.enabled = false;
            _lineRenderer.enabled = false;
            return;
        }

        TeleportRequest request = new TeleportRequest()
        {
            destinationPosition = hit.point
        };

        teleportationProvider.QueueTeleportRequest(request);

        _rayInteractor.enabled = false;
        _lineRenderer.enabled = false;
    }

    private void OnTeleportationCanceled(InputAction.CallbackContext obj)
    {
        canceled = true;
    }
}