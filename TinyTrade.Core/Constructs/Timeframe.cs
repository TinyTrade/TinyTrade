﻿namespace TinyTrade.Core.Constructs;
/// <summary>
/// Struct encapsulating the creation and management of timeframe strings
/// </summary>
[Serializable]
public readonly struct Timeframe
{
    public string Flag { get; init; }

    public int Minutes { get; init; }

    private Timeframe(int minutes)
    {
        Minutes = minutes;
        Flag = MinutesToFlag(minutes);
    }

    private Timeframe(string flag)
    {
        Flag = flag;
        Minutes = FlagToMinutes(flag);
    }

    public static Timeframe FromMinutes(int minutes) => new Timeframe(minutes);

    public static Timeframe FromFlag(string flag) => new Timeframe(flag);

    public static implicit operator string(Timeframe timeframe) => timeframe.Flag;

    public static implicit operator int(Timeframe timeframe) => timeframe.Minutes;

    public static string MinutesToFlag(int minutes)
    {
        return minutes switch
        {
            1 => "1m",
            3 => "3m",
            5 => "5m",
            10 => "10m",
            15 => "15m",
            30 => "30m",
            60 => "1h",
            240 => "4h",
            480 => "8h",
            1440 => "1d",
            1080 => "1w",
            43200 => "1M",
            _ => "1m"
        };
    }

    public static int FlagToMinutes(string flag)
    {
        return flag switch
        {
            "1m" => 1,
            "3m" => 3,
            "5m" => 5,
            "10m" => 10,
            "15m" => 15,
            "30m" => 30,
            "1h" => 60,
            "4h" => 240,
            "8h" => 480,
            "1d" => 1440,
            "1w" => 1080,
            "1M" => 43200,
            _ => 1
        };
    }
}