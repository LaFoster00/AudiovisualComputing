using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRRayInteractor), typeof(LineRenderer))]
[ExecuteAlways]
public class CustomRayVisual : MonoBehaviour
{
    private XRRayInteractor rayInteractor;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
    }

    private void Update()
    {
        UpdateLine();
    }

    private void UpdateLine()
    {
        Vector3 startPosition = rayInteractor.transform.position;
        Vector3 endPosition = startPosition + rayInteractor.transform.forward * rayInteractor.maxRaycastDistance;

        if (rayInteractor.TryGetCurrentRaycast(
                out RaycastHit? raycastHit,
                out int raycastHitIndex,
                out RaycastResult? uiRaycastHit,
                out int uiRaycastHitIndex,
                out bool isUIHitClosest))
        {
            if (isUIHitClosest && uiRaycastHit.HasValue)
            {
                endPosition = uiRaycastHit.Value.worldPosition;
                lineRenderer.enabled = true;
            }
            else if (raycastHit.HasValue)
            {
                endPosition = raycastHit.Value.point;
                lineRenderer.enabled = false;
            }
        }

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
    }
}