using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAudioProvider
{
    public bool CanRead();
    public void Read(Span<float> buffer);
}
