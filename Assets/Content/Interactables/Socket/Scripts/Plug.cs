using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class Plug : AudioProvider
{
    public enum PlugMode
    {
        Undefined,
        Source,
        Target
    }

    [SerializeField] private Plug otherPlug;

    [SerializeField] private PlugMode plugMode = PlugMode.Undefined;

    // Do not access directly. Use property instead.
    [SerializeField] private Socket socketTarget;

    private Socket SocketTarget
    {
        get => socketTarget;
        set
        {
            if (plugMode == PlugMode.Target && socketTarget)
            {
                socketTarget.Target = null;
            }
            
            socketTarget = value;
            
            if (plugMode == PlugMode.Target && socketTarget)
            {
                socketTarget.Target = this;
            }
        }
    }

    public UnityEvent<Socket> onPlugInserted;
    public UnityEvent<Socket> onPlugRemoved;

    public void OnPluggedIn(Socket socket)
    {
        onPlugInserted?.Invoke(socket);
        if (socket.Direction == SocketDirection.Input)
        {
            if (otherPlug.plugMode == PlugMode.Target) return;

            SocketTarget = null;
            plugMode = PlugMode.Target;
            SocketTarget = socket;
        }
        else if (socket.Direction == SocketDirection.Output)
        {
            if (otherPlug.plugMode == PlugMode.Source) return;

            SocketTarget = null;
            plugMode = PlugMode.Source;
            SocketTarget = socket;
        }
    }

    public void OnPluggedOut(Socket socket)
    {
        onPlugRemoved?.Invoke(socket);
        SocketTarget = null;

        plugMode = PlugMode.Undefined;
    }


    private void OnEnable()
    {
        if (SocketTarget)
        {
            transform.GetComponent<Rigidbody>().MovePosition(SocketTarget.transform.position);
        }
    }

    public override void Read(Span<float> buffer)
    {
        if (plugMode == PlugMode.Target)
        {
            otherPlug.Read(buffer);
        }
        else if (plugMode == PlugMode.Source)
        {
            SocketTarget.Read(buffer);
        }
    }
}