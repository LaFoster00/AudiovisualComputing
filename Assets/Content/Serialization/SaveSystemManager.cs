using System;
using NaughtyAttributes;
using UnityEngine;


public class SaveSystemManager : MonoBehaviour
{
    private static SaveSystemManager _instance;
    public static SaveSystemManager Instance => _instance;
    
    private void Awake()
    {
        if (_instance == null && _instance != this)
            Destroy(this);
        _instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(this);
    }

    [Button("SaveGame")]
    public void SaveGame()
    {
        SaveSystem.SaveGame();
    }
    
    [Button("LoadGame")]
    public void LoadGame()
    {
        SaveSystem.LoadGame();
    }
}