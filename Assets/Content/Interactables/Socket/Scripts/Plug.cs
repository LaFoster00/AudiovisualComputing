using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    private MeshRenderer _meshRenderer;
    private Material _material;

    private float _currentLevel;

    public override bool CanProvideAudio => socketTarget && otherPlug.socketTarget;

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
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

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
        
        if (plugMode == PlugMode.Source)
            _material.SetColor(BaseColor, new Color(0, 0, 0));

        plugMode = PlugMode.Undefined;
    }

    public Socket GetSocketTarget()
    {
        return socketTarget;
    }

    public void SetSocketTarget([NotNull] Socket value)
    {
        SocketTarget = value;
        transform.GetComponent<Rigidbody>().MovePosition(SocketTarget.transform.position);
    }

    private void OnEnable()
    {
        if (SocketTarget)
        {
            SetSocketTarget(SocketTarget);
        }

        if (otherPlug._material != null)
        {
            _meshRenderer = otherPlug._meshRenderer;
            _material = otherPlug._material;
        }
        else
        {
            _meshRenderer = transform.parent.GetComponent<MeshRenderer>();
            _material = _meshRenderer.material;
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

        _currentLevel = (buffer[^1] + buffer[^2]) / 2.0f;
    }

    private void Update()
    {
        if (plugMode != PlugMode.Source) return;
        
        var color = _currentLevel < 0.0f
            ? new Color(-_currentLevel, 0, -_currentLevel)
            : new Color(0, _currentLevel, 0);
        _material.SetColor(BaseColor, color);
    }
}