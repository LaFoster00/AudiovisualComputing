using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class SerializedReferenceID : MonoBehaviour
{
    [SerializeField, ReadOnly] private string goid;

    public string GOID => goid;
    
    private static Dictionary<string, MonoBehaviour> _goidToReference = new();

    private void Awake()
    {
        // Only generate new GOID if there isn't already on stored for this object
        if (goid == null || goid == Guid.Empty.ToString() || goid.Equals(""))
            goid = Guid.NewGuid().ToString();

        AddReference(this);
    }

    private void OnDestroy()
    {
        RemoveReference(this);
    }

    [Button("Update Reference Manager")]
    private static void AddReferencesToManager()
    {
        foreach (var id in FindObjectsOfType<SerializedReferenceID>())
        {
            AddReference(id);
        }
    }

    private static void AddReference(SerializedReferenceID gameObject)
    {
        _goidToReference.TryAdd(gameObject.GOID, gameObject);
    }

    private static void RemoveReference(SerializedReferenceID gameObject)
    {
        _goidToReference.Remove(gameObject.GOID);
    }

    public static MonoBehaviour GetGameObject(string goid)
    {
        _goidToReference.TryGetValue(goid, out var gameObject);
        return gameObject;
    }
}