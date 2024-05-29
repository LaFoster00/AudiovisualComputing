using Unity.Mathematics;

public enum BiQuadFilterType
{
    LowPass = 0,
    HighPass = 1,
    LowShelf = 2,
    HighShelf = 3,
    Notch = 4,
    Peaking = 5,
    AllPass = 6,
    BandpassPeakGain = 7,
    BandpassSkirtGain = 8,
    
    LastFilter = BandpassSkirtGain
}

public class BiQuadFilter
{
    // coefficients
    private double a0;
    private double a1;
    private double a2;
    private double a3;
    private double a4;

    // state
    private float x1 = 0;
    private float x2 = 0;
    private float y1 = 0;
    private float y2 = 0;

    /// <summary>
    /// Passes a single sample through the filter
    /// </summary>
    /// <param name="inSample">Input sample</param>
    /// <returns>Output sample</returns>
    public float Transform(float inSample)
    {
        // compute result
        var result = a0 * inSample + a1 * x1 + a2 * x2 - a3 * y1 - a4 * y2;

        // shift x1 to x2, sample to x1 
        x2 = x1;
        x1 = inSample;

        // shift y1 to y2, result to y1 
        y2 = y1;
        y1 = (float)result;

        return y1;
    }

    private void SetCoefficients(double aa0, double aa1, double aa2, double b0, double b1, double b2)
    {
        // precompute the coefficients
        a0 = b0 / aa0;
        a1 = b1 / aa0;
        a2 = b2 / aa0;
        a3 = aa1 / aa0;
        a4 = aa2 / aa0;
    }

    /// <summary>
    /// Set this up as a low pass filter
    /// </summary>
    /// <param name="sampleRate">Sample Rate</param>
    /// <param name="cutoffFrequency">Cut-off Frequency</param>
    /// <param name="q">Bandwidth</param>
    public void SetLowPassFilter(float cutoffFrequency, float q, bool clearSamples)
    {
        // H(s) = 1 / (s^2 + s/Q + 1)
        var w0 = 2 * math.PI * cutoffFrequency / AudioManager.Instance.AudioFormat.SampleRate;
        var cosw0 = math.cos(w0);
        var alpha = math.sin(w0) / (2 * q);

        var b0 = (1 - cosw0) / 2;
        var b1 = 1 - cosw0;
        var b2 = (1 - cosw0) / 2;
        var aa0 = 1 + alpha;
        var aa1 = -2 * cosw0;
        var aa2 = 1 - alpha;
        SetBiQuadFilter(aa0, aa1, aa2, b0, b1, b2, clearSamples);
    }

    /// <summary>
    /// Set this up as a peaking EQ
    /// </summary>
    /// <param name="sampleRate">Sample Rate</param>
    /// <param name="centreFrequency">Centre Frequency</param>
    /// <param name="q">Bandwidth (Q)</param>
    /// <param name="dbGain">Gain in decibels</param>
    public void SetPeakingEq(float centreFrequency, float q, float dbGain, bool clearSamples)
    {
        // H(s) = (s^2 + s*(A/Q) + 1) / (s^2 + s/(A*Q) + 1)
        var w0 = 2 * math.PI * centreFrequency / AudioManager.Instance.AudioFormat.SampleRate;
        var cosw0 = math.cos(w0);
        var sinw0 = math.sin(w0);
        var alpha = sinw0 / (2 * q);
        var a = math.pow(10, dbGain / 40); // TODO: should we square root this value?

        var b0 = 1 + alpha * a;
        var b1 = -2 * cosw0;
        var b2 = 1 - alpha * a;
        var aa0 = 1 + alpha / a;
        var aa1 = -2 * cosw0;
        var aa2 = 1 - alpha / a;
        SetBiQuadFilter(aa0, aa1, aa2, b0, b1, b2, clearSamples);
    }

    /// <summary>
    /// Set this as a high pass filter
    /// </summary>
    public void SetHighPassFilter(float cutoffFrequency, float q, bool clearSamples)
    {
        // H(s) = s^2 / (s^2 + s/Q + 1)
        var w0 = 2 * math.PI * cutoffFrequency / AudioManager.Instance.AudioFormat.SampleRate;
        var cosw0 = math.cos(w0);
        var alpha = math.sin(w0) / (2 * q);

        var b0 = (1 + cosw0) / 2;
        var b1 = -(1 + cosw0);
        var b2 = (1 + cosw0) / 2;
        var aa0 = 1 + alpha;
        var aa1 = -2 * cosw0;
        var aa2 = 1 - alpha;
        SetBiQuadFilter(aa0, aa1, aa2, b0, b1, b2, clearSamples);
    }


    /// <summary>
    /// Create a bandpass filter with constant skirt gain
    /// </summary>
    public void SetBandPassFilterConstantSkirtGain(float centreFrequency, float q, bool clearSamples)
    {
        // H(s) = s / (s^2 + s/Q + 1)  (constant skirt gain, peak gain = Q)
        var w0 = 2 * math.PI * centreFrequency / AudioManager.Instance.AudioFormat.SampleRate;
        var cosw0 = math.cos(w0);
        var sinw0 = math.sin(w0);
        var alpha = sinw0 / (2 * q);

        var b0 = sinw0 / 2; // =   Q*alpha
        var b1 = 0;
        var b2 = -sinw0 / 2; // =  -Q*alpha
        var a0 = 1 + alpha;
        var a1 = -2 * cosw0;
        var a2 = 1 - alpha;
        SetBiQuadFilter(a0, a1, a2, b0, b1, b2, clearSamples);
    }

    /// <summary>
    /// Create a bandpass filter with constant peak gain
    /// </summary>
    public void SetBandPassFilterConstantPeakGain(float centreFrequency, float q, bool clearSamples)
    {
        // H(s) = (s/Q) / (s^2 + s/Q + 1)      (constant 0 dB peak gain)
        var w0 = 2 * math.PI * centreFrequency / AudioManager.Instance.AudioFormat.SampleRate;
        var cosw0 = math.cos(w0);
        var sinw0 = math.sin(w0);
        var alpha = sinw0 / (2 * q);

        var b0 = alpha;
        var b1 = 0;
        var b2 = -alpha;
        var a0 = 1 + alpha;
        var a1 = -2 * cosw0;
        var a2 = 1 - alpha;
        SetBiQuadFilter(a0, a1, a2, b0, b1, b2, clearSamples);
    }

    /// <summary>
    /// Creates a notch filter
    /// </summary>
    public void SetNotchFilter(float centreFrequency, float q, bool clearSamples)
    {
        // H(s) = (s^2 + 1) / (s^2 + s/Q + 1)
        var w0 = 2 * math.PI * centreFrequency / AudioManager.Instance.AudioFormat.SampleRate;
        var cosw0 = math.cos(w0);
        var sinw0 = math.sin(w0);
        var alpha = sinw0 / (2 * q);

        var b0 = 1;
        var b1 = -2 * cosw0;
        var b2 = 1;
        var a0 = 1 + alpha;
        var a1 = -2 * cosw0;
        var a2 = 1 - alpha;
        SetBiQuadFilter(a0, a1, a2, b0, b1, b2, clearSamples);
    }

    /// <summary>
    /// Creaes an all pass filter
    /// </summary>
    public void SetAllPassFilter(float centreFrequency, float q, bool clearSamples)
    {
        //H(s) = (s^2 - s/Q + 1) / (s^2 + s/Q + 1)
        var w0 = 2 * math.PI * centreFrequency / AudioManager.Instance.AudioFormat.SampleRate;
        var cosw0 = math.cos(w0);
        var sinw0 = math.sin(w0);
        var alpha = sinw0 / (2 * q);

        var b0 = 1 - alpha;
        var b1 = -2 * cosw0;
        var b2 = 1 + alpha;
        var a0 = 1 + alpha;
        var a1 = -2 * cosw0;
        var a2 = 1 - alpha;
        SetBiQuadFilter(a0, a1, a2, b0, b1, b2, clearSamples);
    }

    /// <summary>
    /// H(s) = A * (s^2 + (sqrt(A)/Q)*s + A)/(A*s^2 + (sqrt(A)/Q)*s + 1)
    /// </summary>
    /// <param name="sampleRate"></param>
    /// <param name="cutoffFrequency"></param>
    /// <param name="shelfSlope">a "shelf slope" parameter (for shelving EQ only).  
    /// When S = 1, the shelf slope is as steep as it can be and remain monotonically
    /// increasing or decreasing gain with frequency.  The shelf slope, in dB/octave, 
    /// remains proportional to S for all other values for a fixed f0/Fs and dBgain.</param>
    /// <param name="dbGain">Gain in decibels</param>
    public void SetLowShelf(float cutoffFrequency, float shelfSlope, float dbGain, bool clearSamples)
    {
        var w0 = 2 * math.PI * cutoffFrequency / AudioManager.Instance.AudioFormat.SampleRate;
        var cosw0 = math.cos(w0);
        var sinw0 = math.sin(w0);
        var a = math.pow(10, dbGain / 40); // TODO: should we square root this value?
        var alpha = sinw0 / 2 * math.sqrt((a + 1 / a) * (1 / shelfSlope - 1) + 2);
        var temp = 2 * math.sqrt(a) * alpha;

        var b0 = a * ((a + 1) - (a - 1) * cosw0 + temp);
        var b1 = 2 * a * ((a - 1) - (a + 1) * cosw0);
        var b2 = a * ((a + 1) - (a - 1) * cosw0 - temp);
        var a0 = (a + 1) + (a - 1) * cosw0 + temp;
        var a1 = -2 * ((a - 1) + (a + 1) * cosw0);
        var a2 = (a + 1) + (a - 1) * cosw0 - temp;
        SetBiQuadFilter(a0, a1, a2, b0, b1, b2, clearSamples);
    }

    /// <summary>
    /// H(s) = A * (A*s^2 + (sqrt(A)/Q)*s + 1)/(s^2 + (sqrt(A)/Q)*s + A)
    /// </summary>
    /// <param name="sampleRate"></param>
    /// <param name="cutoffFrequency"></param>
    /// <param name="shelfSlope"></param>
    /// <param name="dbGain"></param>
    /// <returns></returns>
    public void SetHighShelf(float cutoffFrequency, float shelfSlope, float dbGain, bool clearSamples)
    {
        var w0 = 2 * math.PI * cutoffFrequency / AudioManager.Instance.AudioFormat.SampleRate;
        var cosw0 = math.cos(w0);
        var sinw0 = math.sin(w0);
        var a = math.pow(10, dbGain / 40); // TODO: should we square root this value?
        var alpha = sinw0 / 2 * math.sqrt((a + 1 / a) * (1 / shelfSlope - 1) + 2);
        var temp = 2 * math.sqrt(a) * alpha;

        var b0 = a * ((a + 1) + (a - 1) * cosw0 + temp);
        var b1 = -2 * a * ((a - 1) + (a + 1) * cosw0);
        var b2 = a * ((a + 1) + (a - 1) * cosw0 - temp);
        var a0 = (a + 1) - (a - 1) * cosw0 + temp;
        var a1 = 2 * ((a - 1) - (a + 1) * cosw0);
        var a2 = (a + 1) - (a - 1) * cosw0 - temp;
        SetBiQuadFilter(a0, a1, a2, b0, b1, b2, clearSamples);
    }

    private void SetBiQuadFilter(double a0, double a1, double a2, double b0, double b1, double b2,
        bool clearSamples)
    {
        SetCoefficients(a0, a1, a2, b0, b1, b2);

        if (clearSamples)
        {
            // zero initial samples
            x1 = x2 = 0;
            y1 = y2 = 0;
        }
    }
}