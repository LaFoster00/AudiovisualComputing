using System;
using System.Linq;

namespace Content.Audio.Core
{
    public class WorkingBuffer : IDisposable
    {
        private readonly float[] _buffer = AudioManager.Instance.SampleProvider.GetFreeWorkingBuffer();

        public float this[int index]
        {
            get => _buffer[index];
            set => _buffer[index] = value;
        }

        public void Clear(float value = 0f)
        {
            if (value == 0f)
                Array.Clear(_buffer, 0, _buffer.Length);
            else
                Array.Fill(_buffer, value);
        }

        public static implicit operator Span<float>(WorkingBuffer b) => b._buffer;

        public void Dispose()
        {
            AudioManager.Instance.SampleProvider.ReturnWorkingBuffer(_buffer);
        }
    }
}