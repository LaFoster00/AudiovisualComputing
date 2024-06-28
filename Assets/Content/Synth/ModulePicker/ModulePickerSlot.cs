using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class ModulePickerSlot : XRSocketInteractor
{
    private GameObject currentModule;
    private GameObject currentPrefab;

    private bool settingModule;

    protected override void OnDisable()
    {
        base.OnDisable();
        SetModule(null);
        settingModule = true;
    }

    public void SetModule(GameObject prefab)
    {
        settingModule = true;
        if (currentModule)
        {
            var tmp = currentModule;
            currentModule = null;
            Destroy(tmp);
        }

        currentPrefab = prefab;
        if (!prefab) return;

        currentModule = Instantiate(prefab, transform.position, Quaternion.identity, null);
        //StartManualInteraction((IXRSelectInteractable)newModule.transform.GetComponent<XRBaseInteractable>());
        settingModule = false;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        currentModule = null;
        if (!settingModule)
            SetModule(currentPrefab);
    }
}