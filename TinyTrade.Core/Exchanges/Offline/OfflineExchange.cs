﻿using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Exchanges.Offline;

/// <summary>
///   An exchange that keeps a dummy balance and utomatically check for <see cref="OfflinePosition"/> status in the <see
///   cref="Tick(DataFrame)"/> method. It overrides the <see cref="IExchange"/> async methods in order to provide a faster processing:
///   methods are treated as synchronous since there is no need for any endpoint call
/// </summary>
public class OfflineExchange : IExchange
{
    private readonly ILogger? logger;
    private readonly Dictionary<Guid, OfflinePosition> openPositions;
    private float balance;
    private float availableBalance;

    public float InitialBalance { get; private set; }

    public List<OfflinePosition> ClosedPositions { get; private set; }

    public OfflineExchange(float balance = 100, ILogger? logger = null)
    {
        openPositions = new Dictionary<Guid, OfflinePosition>();
        this.logger = logger;
        this.balance = balance;
        availableBalance = balance;
        InitialBalance = balance;
        ClosedPositions = new List<OfflinePosition>();
    }

    public void Reset()
    {
        balance = InitialBalance;
        availableBalance = InitialBalance;
        openPositions.Clear();
    }

    public void OpenPosition(OrderSide side, float openPrice, float stopLoss, float takeProfit, float stake)
    {
        if (availableBalance < stake) return;
        availableBalance -= stake;
        var pos = new OfflinePosition(side, openPrice, takeProfit, stopLoss, stake);
        openPositions.Add(Guid.NewGuid(), pos);
    }

    async Task<float> IExchange.GetTotalBalanceAsync()
    {
        await Task.CompletedTask;
        return GetTotalBalance();
    }

    async Task<float> IExchange.GetAvailableBalanceAsync()
    {
        await Task.CompletedTask;
        return GetAvailableBalance();
    }

    async Task<int> IExchange.GetOpenPositionsNumberAsync()
    {
        await Task.CompletedTask;
        return GetOpenPositionsNumber();
    }

    async Task IExchange.OpenPositionAsync(OrderSide side, float openPrice, float stopLoss, float takeProfit, float stake)
    {
        await Task.CompletedTask;
        OpenPosition(side, openPrice, stopLoss, takeProfit, stake);
    }

    public void Tick(DataFrame dataFrame)
    {
        var remove = new List<Guid>();
        for (var i = 0; i < openPositions.Count; i++)
        {
            var p = openPositions.ElementAt(i);
            if (p.Value.TryClose(dataFrame.Close))
            {
                balance += p.Value.Profit;
                availableBalance += p.Value.Stake + p.Value.Profit;
                remove.Add(p.Key);
                ClosedPositions.Add(p.Value);
            }
        }
        foreach (var i in remove)
        {
            openPositions.Remove(i);
        }
    }

    public float GetAvailableBalance() => availableBalance;

    public int GetOpenPositionsNumber() => openPositions.Count;

    public float GetTotalBalance() => balance;
}