using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioParameterDial : MonoBehaviour, IDial
{
    public AudioProvider targetProvider;
    [SerializeReference] public AudioParameter targetParameter;
    [SerializeField] public int targetParameterIndex;

    public void DialChanged(float dialValue)
    {
        targetParameter.CurrentValue = dialValue;
    }
}
