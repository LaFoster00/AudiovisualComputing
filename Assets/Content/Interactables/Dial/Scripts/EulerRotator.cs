using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class EulerRotator : MonoBehaviour
{
    [SerializeField, OnValueChanged("OnLocalRotationChanged")]
    private Vector3 localRotation;
    
    public Vector3 LocalRotation
    {
        get => localRotation;
        set
        {
            localRotation = value;
            transform.localEulerAngles = localRotation;
        }
    }

    private void OnLocalRotationChanged()
    {
        transform.localEulerAngles = localRotation;
    }
}
