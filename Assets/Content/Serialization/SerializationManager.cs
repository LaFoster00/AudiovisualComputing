using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoidReference
{
    [ReadOnly, AllowNesting]
    public string Goid;
    public MonoBehaviour GameObject;

    public GoidReference(string goid, MonoBehaviour gameObject)
    {
        Goid = goid;
        GameObject = gameObject;
    }
}

public class SerializationManager
{
    private List<GoidReference> GoidReferences = new();

    private Dictionary<string, MonoBehaviour> goidToReference = new();

    private static SerializationManager _instance;
    public static SerializationManager Instance => _instance ??= new SerializationManager();

    public SerializationManager()
    {
        SceneManager.sceneUnloaded += SceneManagerOnsceneUnloaded;
    }

    ~SerializationManager()
    {
        SceneManager.sceneUnloaded -= SceneManagerOnsceneUnloaded;
    }

    private void SceneManagerOnsceneUnloaded(Scene arg0)
    {
        GoidReferences.Clear();
        goidToReference.Clear();
    }

    public void AddReference(SerializedReferenceID gameObject)
    {
        if (goidToReference.ContainsKey(gameObject.GOID))
            return;
        
        GoidReferences.Add(new GoidReference(gameObject.GOID, gameObject));
        goidToReference[gameObject.GOID] = gameObject;
    }

    public void RemoveReference(SerializedReferenceID gameObject)
    {
        GoidReferences.RemoveAll(reference => reference.Goid.Equals(gameObject.GOID));
        goidToReference.Remove(gameObject.GOID);
    }
}
