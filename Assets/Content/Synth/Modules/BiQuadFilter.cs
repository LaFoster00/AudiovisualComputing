using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BiQuadFilterType
{
    LowPass = 0,
    HighPass = 1,
    LowShelf = 2,
    HighShelf = 3,
    Notch = 4,
    Peaking = 5,
    AllPass = 6,
    Bandpass = 7,
    
    LastFilter = Bandpass
}

public class BiQuadFilter : AudioProvider
{
    public AudioParameter filterType = new()
    {
        name = "FilterType",
        minValue = 0,
        maxValue = (int)BiQuadFilterType.LastFilter,
        CurrentValue = 0
    };
    
    public AudioParameter frequency = new()
    {
        name = "Frequency",
        minValue = 20,
        maxValue = 20000,
        CurrentValue = 200
    };
    
    public AudioParameter q = new()
    {
        name = "Q",
        minValue = 0.1f,
        maxValue = 10f,
        CurrentValue = 1
    };

    private void OnEnable()
    {
        
    }

    public override void Read(Span<float> buffer, ulong nSample)
    {
        throw new NotImplementedException();
    }

    // Here comes the actual filter logic
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
    public void SetLowPassFilter(float sampleRate, float cutoffFrequency, float q)
    {
        // H(s) = 1 / (s^2 + s/Q + 1)
        var w0 = 2 * Math.PI * cutoffFrequency / sampleRate;
        var cosw0 = Math.Cos(w0);
        var alpha = Math.Sin(w0) / (2 * q);

        var b0 = (1 - cosw0) / 2;
        var b1 = 1 - cosw0;
        var b2 = (1 - cosw0) / 2;
        var aa0 = 1 + alpha;
        var aa1 = -2 * cosw0;
        var aa2 = 1 - alpha;
        SetCoefficients(aa0, aa1, aa2, b0, b1, b2);
    }

    /// <summary>
    /// Set this up as a peaking EQ
    /// </summary>
    /// <param name="sampleRate">Sample Rate</param>
    /// <param name="centreFrequency">Centre Frequency</param>
    /// <param name="q">Bandwidth (Q)</param>
    /// <param name="dbGain">Gain in decibels</param>
    public void SetPeakingEq(float sampleRate, float centreFrequency, float q, float dbGain)
    {
        // H(s) = (s^2 + s*(A/Q) + 1) / (s^2 + s/(A*Q) + 1)
        var w0 = 2 * Math.PI * centreFrequency / sampleRate;
        var cosw0 = Math.Cos(w0);
        var sinw0 = Math.Sin(w0);
        var alpha = sinw0 / (2 * q);
        var a = Math.Pow(10, dbGain / 40); // TODO: should we square root this value?

        var b0 = 1 + alpha * a;
        var b1 = -2 * cosw0;
        var b2 = 1 - alpha * a;
        var aa0 = 1 + alpha / a;
        var aa1 = -2 * cosw0;
        var aa2 = 1 - alpha / a;
        SetCoefficients(aa0, aa1, aa2, b0, b1, b2);
    }

    /// <summary>
    /// Set this as a high pass filter
    /// </summary>
    public void SetHighPassFilter(float sampleRate, float cutoffFrequency, float q)
    {
        // H(s) = s^2 / (s^2 + s/Q + 1)
        var w0 = 2 * Math.PI * cutoffFrequency / sampleRate;
        var cosw0 = Math.Cos(w0);
        var alpha = Math.Sin(w0) / (2 * q);

        var b0 = (1 + cosw0) / 2;
        var b1 = -(1 + cosw0);
        var b2 = (1 + cosw0) / 2;
        var aa0 = 1 + alpha;
        var aa1 = -2 * cosw0;
        var aa2 = 1 - alpha;
        SetCoefficients(aa0, aa1, aa2, b0, b1, b2);
    }
    

    /// <summary>
    /// Create a bandpass filter with constant skirt gain
    /// </summary>
    public void SetBandPassFilterConstantSkirtGain(float sampleRate, float centreFrequency, float q)
    {
        // H(s) = s / (s^2 + s/Q + 1)  (constant skirt gain, peak gain = Q)
        var w0 = 2 * Math.PI * centreFrequency / sampleRate;
        var cosw0 = Math.Cos(w0);
        var sinw0 = Math.Sin(w0);
        var alpha = sinw0 / (2 * q);

        var b0 = sinw0 / 2; // =   Q*alpha
        var b1 = 0;
        var b2 = -sinw0 / 2; // =  -Q*alpha
        var a0 = 1 + alpha;
        var a1 = -2 * cosw0;
        var a2 = 1 - alpha;
        SetBiQuadFilter(a0, a1, a2, b0, b1, b2);
    }

    /// <summary>
    /// Create a bandpass filter with constant peak gain
    /// </summary>
    public void SetBandPassFilterConstantPeakGain(float sampleRate, float centreFrequency, float q)
    {
        // H(s) = (s/Q) / (s^2 + s/Q + 1)      (constant 0 dB peak gain)
        var w0 = 2 * Math.PI * centreFrequency / sampleRate;
        var cosw0 = Math.Cos(w0);
        var sinw0 = Math.Sin(w0);
        var alpha = sinw0 / (2 * q);

        var b0 = alpha;
        var b1 = 0;
        var b2 = -alpha;
        var a0 = 1 + alpha;
        var a1 = -2 * cosw0;
        var a2 = 1 - alpha;
        SetBiQuadFilter(a0, a1, a2, b0, b1, b2);
    }

    /// <summary>
    /// Creates a notch filter
    /// </summary>
    public void SetNotchFilter(float sampleRate, float centreFrequency, float q)
    {
        // H(s) = (s^2 + 1) / (s^2 + s/Q + 1)
        var w0 = 2 * Math.PI * centreFrequency / sampleRate;
        var cosw0 = Math.Cos(w0);
        var sinw0 = Math.Sin(w0);
        var alpha = sinw0 / (2 * q);

        var b0 = 1;
        var b1 = -2 * cosw0;
        var b2 = 1;
        var a0 = 1 + alpha;
        var a1 = -2 * cosw0;
        var a2 = 1 - alpha;
        SetBiQuadFilter(a0, a1, a2, b0, b1, b2);
    }

    /// <summary>
    /// Creaes an all pass filter
    /// </summary>
    public void SetAllPassFilter(float sampleRate, float centreFrequency, float q)
    {
        //H(s) = (s^2 - s/Q + 1) / (s^2 + s/Q + 1)
        var w0 = 2 * Math.PI * centreFrequency / sampleRate;
        var cosw0 = Math.Cos(w0);
        var sinw0 = Math.Sin(w0);
        var alpha = sinw0 / (2 * q);

        var b0 = 1 - alpha;
        var b1 = -2 * cosw0;
        var b2 = 1 + alpha;
        var a0 = 1 + alpha;
        var a1 = -2 * cosw0;
        var a2 = 1 - alpha;
        SetBiQuadFilter(a0, a1, a2, b0, b1, b2);
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
    public void SetLowShelf(float sampleRate, float cutoffFrequency, float shelfSlope, float dbGain)
    {
        var w0 = 2 * Math.PI * cutoffFrequency / sampleRate;
        var cosw0 = Math.Cos(w0);
        var sinw0 = Math.Sin(w0);
        var a = Math.Pow(10, dbGain / 40); // TODO: should we square root this value?
        var alpha = sinw0 / 2 * Math.Sqrt((a + 1 / a) * (1 / shelfSlope - 1) + 2);
        var temp = 2 * Math.Sqrt(a) * alpha;

        var b0 = a * ((a + 1) - (a - 1) * cosw0 + temp);
        var b1 = 2 * a * ((a - 1) - (a + 1) * cosw0);
        var b2 = a * ((a + 1) - (a - 1) * cosw0 - temp);
        var a0 = (a + 1) + (a - 1) * cosw0 + temp;
        var a1 = -2 * ((a - 1) + (a + 1) * cosw0);
        var a2 = (a + 1) + (a - 1) * cosw0 - temp;
        SetBiQuadFilter(a0, a1, a2, b0, b1, b2);
    }

    /// <summary>
    /// H(s) = A * (A*s^2 + (sqrt(A)/Q)*s + 1)/(s^2 + (sqrt(A)/Q)*s + A)
    /// </summary>
    /// <param name="sampleRate"></param>
    /// <param name="cutoffFrequency"></param>
    /// <param name="shelfSlope"></param>
    /// <param name="dbGain"></param>
    /// <returns></returns>
    public void SetHighShelf(float sampleRate, float cutoffFrequency, float shelfSlope, float dbGain)
    {
        var w0 = 2 * Math.PI * cutoffFrequency / sampleRate;
        var cosw0 = Math.Cos(w0);
        var sinw0 = Math.Sin(w0);
        var a = Math.Pow(10, dbGain / 40); // TODO: should we square root this value?
        var alpha = sinw0 / 2 * Math.Sqrt((a + 1 / a) * (1 / shelfSlope - 1) + 2);
        var temp = 2 * Math.Sqrt(a) * alpha;

        var b0 = a * ((a + 1) + (a - 1) * cosw0 + temp);
        var b1 = -2 * a * ((a - 1) + (a + 1) * cosw0);
        var b2 = a * ((a + 1) + (a - 1) * cosw0 - temp);
        var a0 = (a + 1) - (a - 1) * cosw0 + temp;
        var a1 = 2 * ((a - 1) - (a + 1) * cosw0);
        var a2 = (a + 1) - (a - 1) * cosw0 - temp;
        SetBiQuadFilter(a0, a1, a2, b0, b1, b2);
    }
    
    private void SetBiQuadFilter(double a0, double a1, double a2, double b0, double b1, double b2)
    {
        SetCoefficients(a0, a1, a2, b0, b1, b2);

        // zero initial samples
        x1 = x2 = 0;
        y1 = y2 = 0;
    }
}