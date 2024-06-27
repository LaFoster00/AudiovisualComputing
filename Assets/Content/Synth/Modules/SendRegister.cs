using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ChannelSend))]
public class SendRegister : MonoBehaviour
{
    private void OnEnable()
    {
        AudioManager.Instance.SampleProvider.AddMixer(GetComponent<ChannelSend>());
    }

    private void OnDisable()
    {
        AudioManager.Instance.SampleProvider.RemoveMixer(GetComponent<ChannelSend>());
    }
}
