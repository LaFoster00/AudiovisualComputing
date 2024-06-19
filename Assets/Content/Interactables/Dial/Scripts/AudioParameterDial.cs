using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Serialization.Json;
using UnityEngine;



public class AudioParameterDial : MonoBehaviour, IDialUser, IPersistentData
{
    public AudioProvider targetProvider;
    public AudioParameter targetParameter;
    public string targetParameterName;

    public void DialChanged(float dialValue)
    {
        targetParameter.CurrentNormalizedValue = dialValue;
    }

    [Serializable]
    private struct SaveData
    {
        public int TargetProvider;
        public string TargetParameterName;
    }
    
    public object Serialize()
    {
        var data = new SaveData
        {
            TargetProvider = targetProvider.transform.GetInstanceID(),
            TargetParameterName = targetParameterName
        };
        return data;
    }
}
