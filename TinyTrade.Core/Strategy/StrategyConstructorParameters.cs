﻿using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;

namespace TinyTrade.Core.Strategy;

/// <summary>
/// Struct encapsulating, for comodity, all the parameters required to instantiate an <see cref="AbstractStrategy"/>
/// </summary>
public readonly struct StrategyConstructorParameters
{
    public Dictionary<string, object> Parameters { get; init; }

    public List<Trait> Traits { get; init; }

    public ILogger? Logger { get; init; }

    public IExchange Exchange { get; init; }

    public StrategyConstructorParameters(Dictionary<string, object> parameters, List<Trait> genotype, ILogger? logger, IExchange exchange)
    {
        Parameters = parameters;
        Traits = genotype;
        Logger = logger;
        Exchange = exchange;
    }
}