﻿namespace TinyTrade.Indicators;

public class BollingerBands : Indicator
{
    private readonly int stdev;
    private readonly int period;
    private readonly Queue<float> firstValues;
    private readonly Ma ma;

    public (float?, float?, float?) Last { get; private set; } = (null, null, null);

    public BollingerBands(int period = 20, int stdev = 2)
    {
        this.stdev = stdev;
        ma = new Ma(period);
        this.period = period;
        firstValues = new Queue<float>();
    }

    public (float?, float?, float?) ComputeNext(float high, float low, float close)
    {
        var tp = (close + high + low) / 3F;
        var currentMa = ma.ComputeNext(tp);

        firstValues.Enqueue(tp);
        if (firstValues.Count > period)
        {
            firstValues.Dequeue();
        }
        if (firstValues.Count < period)
        {
            return (null, null, null);
        }
        var currentStdev = CalculateStdev(firstValues.Average());

        var bU = currentMa + stdev * currentStdev;
        var bD = currentMa - stdev * currentStdev;
        Last = (currentMa, bU, bD);
        return (currentMa, bU, bD);
    }

    public override void Reset()
    {
        Last = (null, null, null);
        firstValues.Clear();
        ma.Reset();
    }

    private float CalculateStdev(float mean)
    {
        double cumulative = 0;
        foreach (var v in firstValues)
        {
            cumulative += Math.Pow(v - mean, 2);
        }
        cumulative /= firstValues.Count;
        return (float)Math.Sqrt(cumulative);
    }
}