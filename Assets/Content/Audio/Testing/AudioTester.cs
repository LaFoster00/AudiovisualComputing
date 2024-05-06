using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTester : MonoBehaviour
{
    private void Start()
    {
        var sinGenerator = new SinGenerator(0.5f, 100);
        var mixer = new AddMixer(sinGenerator);
        AudioManager.Instance.WaveProvider.AddMixer(mixer);
    }
}
