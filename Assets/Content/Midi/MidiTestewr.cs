using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MidiTestewr : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        InputSystem.onDeviceChange += (device, change) =>
        {
            var midiDevice = device as Minis.MidiDevice;
            if (midiDevice == null) return;

            Debug.Log(string.Format("{0} ({1}) {2}",
                device.description.product, midiDevice.channel, change));
        };
    }
}
