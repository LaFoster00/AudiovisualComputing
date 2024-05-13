using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public delegate void PluggedIndEvent(Socket args);

    public event PluggedIndEvent OnPlugInserted;

    public delegate void UnpluggedEvent(Socket args);

    public event UnpluggedEvent OnPlugRemoved;

    public void OnPluggedIn(Socket socket)
    {
        OnPlugInserted?.Invoke(socket);
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

    public void OnPluggedOut(Socket args)
    {
        OnPlugRemoved?.Invoke(args);
        SocketTarget = null;

        plugMode = PlugMode.Undefined;
    }


    public override void Read(Span<float> buffer, ulong nSample)
    {
        if (plugMode == PlugMode.Target)
        {
            otherPlug.Read(buffer, nSample);
        }
        else if (plugMode == PlugMode.Source)
        {
            SocketTarget.Read(buffer, nSample);
        }
    }
}