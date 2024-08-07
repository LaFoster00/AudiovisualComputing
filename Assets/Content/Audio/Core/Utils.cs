using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Unity.Mathematics;

namespace Audio.Core
{
    public static class Utils
    {
        public static readonly SevenBitNumber C4NoteNumber = (SevenBitNumber)CalculateNoteNumber(NoteName.C, 4);
        public static readonly float NoteOffsetDefault = 0.5f;
        public static readonly int totalNumberNotes = (int)SevenBitNumber.MaxValue + 1;
        // The value needed to offset a frequency by 1 note using the NoteOffset function
        public static readonly float NoteOffsetFactor = (1.0f / totalNumberNotes);

        public static bool IsNoteNumberValid(int noteNumber)
        {
            return noteNumber >= SevenBitNumber.MinValue && noteNumber <= SevenBitNumber.MaxValue;
        }

        public static int CalculateNoteNumber(NoteName noteName, int octave)
        {
            return (octave + 1) * Octave.OctaveSize + (int)noteName;
        }

        public static int GetNoteOffsetFromC4(this SevenBitNumber midiNoteNumber)
        {
            return midiNoteNumber - C4NoteNumber;
        }

        // Midi note starting at C4(60) to note offset with NoteOffsetFactor scaling
        public static float GetFrequencyOffsetFromMidiNote(this SevenBitNumber midiNoteNumber)
        {
            return NoteOffsetDefault + midiNoteNumber.GetNoteOffsetFromC4() * NoteOffsetFactor;
        }

        // Frequency offset default = 0.5f, 0.05 per octave
        public static float NoteOffset(this float frequencyOffset) =>
            (frequencyOffset - NoteOffsetDefault) * totalNumberNotes;

        // Note offset in 1.0f per note, note C4(0), note A4 (9) is assumed to be  (440hz)
        public static float NoteOffsetFrequency(this float frequency, float noteOffset) =>
            frequency * math.pow(2, (noteOffset - 9f) / 12f);

        public static double HertzToMel(this double hertz)
        {
            return 2595 * math.log10(1 + hertz / 700.0);
        }

        public static double MelToHertz(this double mel)
        {
            return 700 * (math.pow(10, mel / 2595.0) - 1);
        }

        public static float HertzToMel(this float hertz)
        {
            return (float)((double)hertz).HertzToMel();
        }

        public static float MelToHertz(this float mel)
        {
            return (float)((double)mel).MelToHertz();
        }

        // Converts a linear slider value to a frequency using a perceptual scale
        public static double LinearSliderToMelSlider(this double sliderValue, double sliderMin, double sliderMax,
            double freqMin, double freqMax)
        {
            // Normalize slider value to range 0 to 1
            double normalizedSliderValue = (sliderValue - sliderMin) / (sliderMax - sliderMin);

            // Convert the frequency range to Mel scale
            double melMin = HertzToMel(freqMin);
            double melMax = HertzToMel(freqMax);

            // Map normalized slider value to the Mel scale range
            double melValue = melMin + normalizedSliderValue * (melMax - melMin);

            // Convert Mel value back to frequency
            return MelToHertz(melValue);
        }

        // Converts a frequency to a slider value using a perceptual scale
        public static double MelSliderToLinearSlider(this double frequency, double sliderMin, double sliderMax,
            double freqMin, double freqMax)
        {
            // Convert the frequency to Mel scale
            double melValue = HertzToMel(frequency);

            // Convert the frequency range to Mel scale
            double melMin = HertzToMel(freqMin);
            double melMax = HertzToMel(freqMax);

            // Normalize Mel value to range 0 to 1
            double normalizedMelValue = (melValue - melMin) / (melMax - melMin);

            // Map normalized Mel value to the slider range
            return sliderMin + normalizedMelValue * (sliderMax - sliderMin);
        }

        // Converts a linear slider value to a frequency using a perceptual scale
        public static float LinearSliderToMelSlider(this float sliderValue, float sliderMin, float sliderMax,
            float freqMin,
            float freqMax)
        {
            // Normalize slider value to range 0 to 1
            float normalizedSliderValue = (sliderValue - sliderMin) / (sliderMax - sliderMin);

            // Convert the frequency range to Mel scale
            float melMin = HertzToMel(freqMin);
            float melMax = HertzToMel(freqMax);

            // Map normalized slider value to the Mel scale range
            float melValue = melMin + normalizedSliderValue * (melMax - melMin);

            // Convert Mel value back to frequency
            return MelToHertz(melValue);
        }

        // Converts a frequency to a slider value using a perceptual scale
        public static float MelSliderToLinearSlider(this float frequency, float sliderMin, float sliderMax,
            float freqMin,
            float freqMax)
        {
            // Convert the frequency to Mel scale
            float melValue = HertzToMel(frequency);

            // Convert the frequency range to Mel scale
            float melMin = HertzToMel(freqMin);
            float melMax = HertzToMel(freqMax);

            // Normalize Mel value to range 0 to 1
            float normalizedMelValue = (melValue - melMin) / (melMax - melMin);

            // Map normalized Mel value to the slider range
            return sliderMin + normalizedMelValue * (sliderMax - sliderMin);
        }

        public static double LinearToDb(this double linear)
        {
            return 10 * math.log10(linear);
        }

        public static double DBToLinear(this double db)
        {
            return math.pow(10, db / 10);
        }

        public static float LinearToDb(this float linear)
        {
            return 10 * math.log10(linear);
        }

        public static float DBToLinear(this float db)
        {
            return math.pow(10, db / 10);
        }

        // (linear) x0 - x1 to (log) y0 - y1
        public static double LinearToLog(this double linearValue, double logBase = 10)
        {
            if (linearValue is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(linearValue), "Value must be in the range [0, 1]");
            }

            // Scale linear value to a range suitable for logarithmic transformation
            double minLog = System.Math.Log10(1); // log(1) = 0
            double maxLog = System.Math.Log10(logBase); // log(10) = 1 for base 10

            // Apply logarithmic transformation
            double logValue = minLog + (System.Math.Log10(1 + linearValue * (logBase - 1)) - minLog) / (maxLog - minLog);

            // Normalize to the range 0 to 1
            return (logValue - minLog) / (maxLog - minLog);
        }

        // (linear) x0 - x1 to (log) y0 - y1
        public static float LinearToLog(this float linearValue, float logBase = 10)
        {
            return (float)LinearToLog((double)linearValue, (double)logBase);
        }

        // Converts a logarithmic value (0 to 1) back to a linear value (0 to 1)
        public static double LogarithmicToLinear(double logValue, double logBase = 10)
        {
            if (logValue < 0 || logValue > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(logValue), "Value must be in the range [0, 1]");
            }

            // Scale log value to a range suitable for exponential transformation
            double minLog = System.Math.Log10(1); // log(1) = 0
            double maxLog = System.Math.Log10(logBase); // log(10) = 1 for base 10

            // Apply exponential transformation
            double linearValue = (System.Math.Pow(10, logValue * (maxLog - minLog) + minLog) - 1) / (logBase - 1);

            // Normalize to the range 0 to 1
            return linearValue;
        }

        // Converts a logarithmic value (0 to 1) back to a linear value (0 to 1)
        public static float LogarithmicToLinear(this float logValue, float logBase = 10)
        {
            return (float)LogarithmicToLinear((double)logValue, (double)logBase);
        }
    }
}