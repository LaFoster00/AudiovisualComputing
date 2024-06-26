using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Content.Serialization;
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
    public string prefab;
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

    private static Dictionary<int, GameObject> _gameObjectReferences = new();

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
            var persistentPrefabs = o.GetComponents<MonoBehaviour>().OfType<IPersistentPrefab>().ToArray();
            List<SerializableComponent> serializedComponents;
            string prefab = null;
            if (persistentPrefabs.Length == 0)
            {
                var persistentComponents = o.GetComponents<MonoBehaviour>().OfType<IPersistentData>();

                 serializedComponents = persistentComponents.Select(persistentComponent =>
                    new SerializableComponent()
                    {
                        type = persistentComponent.GetType().AssemblyQualifiedName!,
                        data = persistentComponent.Serialize()
                    }).ToList();
            }
            else
            {
                serializedComponents = new List<SerializableComponent>();
                if (persistentPrefabs.Length > 1)
                    throw new ArgumentException($"There can only be one persistent prefab on an object({o.name}).");

                prefab = persistentPrefabs[0].GetPrefab();
            }

            SerializableGameObject serializableGameObject = new SerializableGameObject()
            {
                name = o.name,
                transform = new SerializableTransform(o.transform),
                id = o.transform.GetInstanceID(),
                parentId = o.transform.GetComponentInParent<Transform>()?.GetInstanceID() ?? -1,
                prefab = prefab,
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

        Type componentType = Type.GetType(saveData.GameObjects[0].components[0].type);
    }

    public static GameObject Dereference(int saveDataTargetProvider)
    {
        _gameObjectReferences.TryGetValue(saveDataTargetProvider, out var gameObject);
        return gameObject;
    }
}