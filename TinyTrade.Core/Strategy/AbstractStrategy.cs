﻿using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.Exchanges;
using TinyTrade.Indicators;

namespace TinyTrade.Core.Strategy;

public abstract class AbstractStrategy : IStrategy
{
    private readonly List<Condition> shortConditions;

    private readonly List<Condition> longConditions;
    private IEnumerable<Indicator>? indicators = null;

    public int MaxConcurrentPositions { get; init; }

    protected ILogger? Logger { get; private set; }

    protected double? CachedTotalBalance { get; private set; }

    protected IExchange Exchange { get; private set; }

    protected int Leverage { get; private set; }

    protected AbstractStrategy(StrategyConstructorParameters parameters)
    {
        Logger = parameters.Logger;
        Exchange = parameters.Exchange;
        MaxConcurrentPositions = Convert.ToInt32(parameters.Parameters.GetValueOrDefault("maxConcurrentPositions", 1));
        Leverage = Convert.ToInt32(parameters.Parameters.GetValueOrDefault("leverage", 1));
        shortConditions = new List<Condition>();
        longConditions = new List<Condition>();
    }

    /// <summary>
    ///   Reset the status of all conditions of the strategy
    /// </summary>
    public void Reset()
    {
        ResetState();
        indicators ??= GetIndicators();
        foreach (var i in indicators)
        {
            i.Reset();
        }
        foreach (var c in longConditions)
        {
            c.Reset();
        }
        foreach (var c in shortConditions)
        {
            c.Reset();
        }
    }

    /// <summary>
    ///   Update the internal state of the strategy
    /// </summary>
    /// <returns> </returns>
    public async Task UpdateState(DataFrame frame)
    {
        Exchange.Tick(frame);
        if (frame.IsClosed)
        {
            CachedTotalBalance = await Exchange.GetTotalBalanceAsync();
            await Tick(frame);
            foreach (var c in shortConditions)
            {
                c.Tick(frame);
            }
            foreach (var c in longConditions)
            {
                c.Tick(frame);
            }

            var posTask = Exchange.GetOpenPositionsNumberAsync();
            var balanceTask = Exchange.GetAvailableBalanceAsync();
            await Task.WhenAll(posTask, balanceTask);
            var openPositions = posTask.Result;
            var balance = balanceTask.Result;
            if (openPositions < MaxConcurrentPositions && balance > 0)
            {
                var stake = GetMargin(frame);

                if (longConditions.Count > 0 && longConditions.All(c => c.IsSatisfied))
                {
                    var side = OrderSide.Buy;

                    await Exchange.OpenPositionAsync(side, frame.Close, GetStopLoss(side, frame), GetTakeProfit(side, frame), (float)stake, Leverage);
                    longConditions.ForEach(c => c.Reset());
                }

                if (shortConditions.Count > 0 && shortConditions.All(c => c.IsSatisfied))
                {
                    var side = OrderSide.Sell;

                    await Exchange.OpenPositionAsync(side, frame.Close, GetStopLoss(side, frame), GetTakeProfit(side, frame), (float)stake, Leverage);
                    shortConditions.ForEach(c => c.Reset());
                }
            }
        }
    }

    /// <summary>
    ///   Add short <see cref="Condition"/>
    /// </summary>
    protected void InjectShortConditions(params Condition[] conditions)
    {
        foreach (var c in conditions)
        {
            if (c is not null && !shortConditions.Contains(c))
            {
                shortConditions.Add(c);
            }
        }
    }

    /// <summary>
    ///   Reset the internal state of the strategy <i> (nullables, cached values ...) </i>
    /// </summary>
    protected abstract void ResetState();

    /// <summary>
    ///   Return the indicators of the strategy, so that they can be automatically reset in <see cref="Reset"/>
    /// </summary>
    /// <returns> </returns>
    protected abstract IEnumerable<Indicator> GetIndicators();

    /// <summary>
    ///   How much to invest in each trade
    /// </summary>
    /// <returns> </returns>
    protected abstract float GetMargin(DataFrame frame);

    /// <summary>
    ///   Get the value for the stop loss for a given side order
    /// </summary>
    /// <returns> </returns>
    protected abstract float GetStopLoss(OrderSide side, DataFrame frame);

    /// <summary>
    ///   Get the value for the take profit for a given side order
    /// </summary>
    /// <returns> </returns>
    protected abstract float GetTakeProfit(OrderSide side, DataFrame frame);

    /// <summary>
    ///   Add long <see cref="Condition"/>
    /// </summary>
    protected void InjectLongConditions(params Condition[] conditions)
    {
        foreach (var c in conditions)
        {
            if (c is not null && !longConditions.Contains(c))
            {
                longConditions.Add(c);
            }
        }
    }

    /// <summary>
    ///   Called each time a closed candle is received
    /// </summary>
    /// <param name="frame"> </param>
    protected virtual Task Tick(DataFrame frame) => Task.CompletedTask;
}