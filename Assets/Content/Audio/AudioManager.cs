using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SampleProvider))]
public class AudioManager : MonoBehaviour
{
    [SerializeField]
    public SampleProvider SampleProvider;

    public static AudioManager Instance { get; private set; }

    public AudioFormat AudioFormat
    {
        get => SampleProvider.audioFormat;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (!Instance)
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}