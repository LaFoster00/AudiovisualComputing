using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SocketArrayInteractor : XRSocketInteractor
{
    public SocketArrayInteractor left;
    public SocketArrayInteractor right;
    
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        var module = (PickableModule)args.interactableObject;

        SocketArrayInteractor current = this;
        for (int i = 1; i < module.unitWidth; i++)
        {
            current.right.allowHover = false;
            current.right.allowSelect = false;
            current = current.right;
        }

        base.OnSelectEntered(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        var module = (PickableModule)args.interactableObject;

        SocketArrayInteractor current = this;
        for (int i = 1; i < module.unitWidth; i++)
        {
            current.right.allowHover = true;
            current.right.allowSelect = true;
            current = current.right;
        }

        base.OnSelectExited(args);
    }

    protected override bool CanHoverSnap(IXRInteractable interactable)
    {
        return ModuleFits(interactable) && base.CanHoverSnap(interactable);
    }

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        return ModuleFits(interactable) && base.CanHover(interactable);
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return ModuleFits(interactable) && base.CanSelect(interactable);
    }

    private bool ModuleFits(IXRInteractable interactable)
    {
        if (interactable.transform.TryGetComponent(out PickableModule module))
        {
            // Check if there's enough space for the object
            SocketArrayInteractor current = this;
            for (int i = 1; i < module.unitWidth; i++)
            {
                if (!current.right || current.right.hasSelection)
                    return false;
                current = current.right;
            }
        }
        else
            return false;

        return true;
    }
}