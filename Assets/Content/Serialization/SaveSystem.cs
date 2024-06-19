using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToolBox.Serialization.OdinSerializer;
using Unity.Serialization.Json;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[Serializable]
public struct SerializableTransform
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;

    public SerializableTransform(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
        localScale = transform.localScale;
    }

    public void ApplyToTransform(ref Transform transform)
    {
        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = localScale;
    }
}

[Serializable]
public struct SerializableGameObject
{
    public string name;
    public int id;
    public int parentId;
    public SerializableTransform transform;
    public List<SerializableComponent> components;
}

[Serializable]
public struct SerializableComponent
{
    public string type;
    public object data;
}

[Serializable]
public struct SaveData
{
    public List<SerializableGameObject> GameObjects;
}

public static class SaveSystem
{
    public static string CurrentSave;
    
    public static void NewGame()
    {
    }

    public static void SaveGame()
    {
        var serializableGameObjects =
            Object.FindObjectsOfType<GameObject>().Where(o => o.GetComponent<IPersistentData>() != null);

        List<SerializableGameObject> serializedGameObjects = new();
        foreach (var o in serializableGameObjects)
        {
            var persistentComponents = o.GetComponents<MonoBehaviour>().OfType<IPersistentData>();

            List<SerializableComponent> serializedComponents = persistentComponents.Select(persistentComponent =>
                new SerializableComponent()
                {
                    type = persistentComponent.GetType().AssemblyQualifiedName!,
                    data = persistentComponent.Serialize()
                }).ToList();

            SerializableGameObject serializableGameObject = new SerializableGameObject()
            {
                name = o.name,
                transform = new SerializableTransform(o.transform),
                id = o.transform.GetInstanceID(),
                parentId = o.transform.GetComponentInParent<Transform>()?.GetInstanceID() ?? -1,
                components = serializedComponents
            };

            serializedGameObjects.Add(serializableGameObject);
        }

        CurrentSave = JsonSerialization.ToJson(new SaveData()
        {
            GameObjects = serializedGameObjects
        });
        Debug.Log(CurrentSave);
    }

    public static void LoadGame()
    {
        var saveData = JsonSerialization.FromJson<SaveData>(CurrentSave);
    }
}