namespace Audio.Core
{
    public class AudioSettings : Singleton<AudioSettings>
    {
        public readonly int BeatsPerMeasure = 16;
        public int Tempo = 120;

        public static uint SamplesPerBeat =>
            (uint)
            (((uint)AudioManager.Instance.SampleProvider.AudioFormat.SampleRate * 60) /
                (Instance.Tempo * Instance.BeatsPerMeasure)
                * 4); // Beats per measure is in 4/4 measure

        public static uint SampleToBeat(ulong sample)
        {
            return (uint)(sample / SamplesPerBeat);
        }

        public static ulong BeatToSample(uint beat)
        {
            return beat * SamplesPerBeat;
        }
    }
}