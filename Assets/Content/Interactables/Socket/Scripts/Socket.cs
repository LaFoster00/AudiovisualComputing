using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public enum SocketDirection
{
    Input,
    Output
}

[RequireComponent(typeof(XRSocketInteractor))]
public class Socket : AudioProvider
{
    [FormerlySerializedAs("_target")] [SerializeField, ShowIf("IsSocketOutput")]
    private AudioProvider target = null;

    [SerializeField]
    private float defaultValue;

    [FormerlySerializedAs("_direction")] [SerializeField] private SocketDirection direction = SocketDirection.Input;

    
    private bool IsSocketOutput()
    {
        return direction == SocketDirection.Output;
    }
    
    public SocketDirection Direction
    {
        get => direction;
    }

    public AudioProvider Target
    {
        get => target; 
        set => target = value;
    }

    private XRSocketInteractor _xrSocketInteractor;
    
    private void OnEnable()
    {
        _xrSocketInteractor = GetComponent<XRSocketInteractor>();
        _xrSocketInteractor.selectEntered.AddListener(OnPlugPluggedIn);
        _xrSocketInteractor.selectExited.AddListener(OnPluggedOut);
    }

    private void OnDisable()
    {
        _xrSocketInteractor.selectEntered.RemoveListener(OnPlugPluggedIn);
        _xrSocketInteractor.selectExited.RemoveListener(OnPluggedOut);
    }

    private void OnPluggedOut(SelectExitEventArgs args)
    {
        var plug = args.interactableObject.transform.GetComponent<Plug>();
        if (plug)
        {
            plug.OnPluggedOut(this);
        }
    }

    private void OnPlugPluggedIn(SelectEnterEventArgs args)
    {
        var plug = args.interactableObject.transform.GetComponent<Plug>();
        if (plug)
        {
            plug.OnPluggedIn(this);
        }
    }

    public override bool CanRead()
    {
        return target && target.CanRead();
    }

    public override void Read(Span<float> buffer)
    {
        if (CanRead())
            Target.Read(buffer);
        else
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = defaultValue;
            }
        }
    }
}
