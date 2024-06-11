using System;
using System.Linq;

public class WorkingBuffer : IDisposable
{
    private float[] _buffer = AudioManager.Instance.SampleProvider.GetFreeWorkingBuffer();

    // Fallback
    ~WorkingBuffer()
    {
        Dispose();
    }
    
    public float this[int index]
    {
        get => _buffer[index];
        set => _buffer[index] = value;
    }

    public void Clear(float value = 0f)
    {
        for (int i = 0; i < _buffer.Length; i++)
        {
            _buffer[i] = value;
        }
    }

    public static implicit operator Span<float>(WorkingBuffer b) =>
        b._buffer.AsSpan(0, AudioManager.Instance.SampleProvider.CurrentDataLength);

    public void Dispose()
    {
        if (_buffer == null)
            return;
        Clear();
        AudioManager.Instance.SampleProvider.ReturnWorkingBuffer(_buffer);
        _buffer = null;
    }
}