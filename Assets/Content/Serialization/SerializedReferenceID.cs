using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class SerializedReferenceID : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private string goid;

    public string GOID => goid;
    
    private void Awake()
    {
        // Only generate new GOID if there isn't already on stored for this object
        if (goid == null || goid == Guid.Empty.ToString() || goid.Equals(""))
            goid = Guid.NewGuid().ToString();
        
        SerializationManager.Instance.AddReference(this);
    }

    private void OnDestroy()
    {
        SerializationManager.Instance.RemoveReference(this);
    }
}
