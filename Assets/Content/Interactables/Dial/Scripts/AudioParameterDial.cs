using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioParameterDial : MonoBehaviour, IDialUser
{
    public AudioProvider targetProvider;
    public AudioParameter targetParameter;
    public string targetParameterName;

    public void DialChanged(float dialValue)
    {
        targetParameter.CurrentNormalizedValue = dialValue;
    }
}
