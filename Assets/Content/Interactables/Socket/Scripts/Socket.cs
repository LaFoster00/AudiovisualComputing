using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum SocketDirection
{
    Input,
    Output
}

[RequireComponent(typeof(XRSocketInteractor))]
public class Socket : AudioProvider
{
    [SerializeField, ShowIf("IsOutputSocket")]
    private AudioProvider _target = null;

    [SerializeField, HideIf("HasTarget")] private float defaultValue = 0;

    [SerializeField] private SocketDirection _direction = SocketDirection.Input;
    
    public override bool CanProvideAudio => true;
    
    private bool IsOutputSocket()
    {
        return _direction == SocketDirection.Output;
    }

    private bool HasTarget()
    {
        return _target != null;
    }
    
    public SocketDirection Direction
    {
        get => _direction;
    }

    public AudioProvider Target
    {
        get => _target; 
        set => _target = value;
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
    

    protected override void Preprocess_Impl(uint numSamples, ulong frame){
        if (Target && Target.CanProvideAudio)
            Target.Preprocess(numSamples, frame);
    }
    
    public override void Read(Span<float> buffer)
    {
        
        if (Target && Target.CanProvideAudio) 
            Target.Read(buffer);
        else
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = defaultValue;
            }
    }
}
