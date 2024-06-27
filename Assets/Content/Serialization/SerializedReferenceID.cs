using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class SerializedReferenceID : MonoBehaviour
{
    [SerializeField, ReadOnly] private string guid;

    public string GUID => guid;
    
    private static Dictionary<string, MonoBehaviour> _goidToReference = new();

    private void Awake()
    {
        // Only generate new guid if there isn't already on stored for this object
        if (guid == null || guid == Guid.Empty.ToString() || guid.Equals(""))
            GenerateNewGuid();

        AddReference(this);
    }

    private void OnDestroy()
    {
        RemoveReference(this);
    }

    private void GenerateNewGuid()
    {
        guid = Guid.NewGuid().ToString();
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
        if (_goidToReference.ContainsKey(gameObject.guid))
            gameObject.GenerateNewGuid();
        _goidToReference.TryAdd(gameObject.GUID, gameObject);
    }

    private static void RemoveReference(SerializedReferenceID gameObject)
    {
        _goidToReference.Remove(gameObject.GUID);
    }

    public static MonoBehaviour GetGameObject(string goid)
    {
        _goidToReference.TryGetValue(goid, out var gameObject);
        return gameObject;
    }
}