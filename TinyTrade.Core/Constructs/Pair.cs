﻿namespace TinyTrade.Core.Constructs;

/// <summary>
/// Struct encapsulating the creation and management of pair strings
/// </summary>
[Serializable]
public struct Pair
{
    public string Asset { get; private set; }

    public string Collateral { get; private set; }

    public Pair(string asset, string collateral)
    {
        Asset = asset;
        Collateral = collateral;
    }

    public static Pair Parse(string pair, char separator = '-')
    {
        int index = pair.IndexOf(separator);
        string asset = pair[..index];
        string collateral = pair[(index + 1)..];
        return new Pair(asset, collateral);
    }

    public override string? ToString() => ForKucoin();

    public string ForKucoin() => Asset + "-" + Collateral;

    public string ForBinance() => Asset + Collateral;
}