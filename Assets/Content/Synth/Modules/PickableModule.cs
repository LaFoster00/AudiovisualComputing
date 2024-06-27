using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PickableModule : XRGrabInteractable
{
    [Min(1)] public int unitWidth = 1;

    private XRBaseInteractable[] childInteractables;
    private XRBaseInteractor[] childInteractors;
    protected override void Awake()
    {
        base.Awake();
        childInteractables = GetComponentsInChildren<XRBaseInteractable>();
        childInteractors = GetComponentsInChildren<XRBaseInteractor>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRDirectInteractor)
        {
            //SetChildInteractablesActive(false);
        }
        base.OnSelectEntered(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        if (args.interactorObject is XRDirectInteractor)
        {
            //SetChildInteractablesActive(true);
        }
        base.OnSelectExited(args);
    }

    private void SetChildInteractablesActive(bool active)
    {
        foreach (var interactable in childInteractables)
        {
            if (interactable != this)
            {
                interactable.enabled = active;
            }
        }
        foreach (var interactor in childInteractors)
        {
            interactor.enabled = active;
        }
    }
}