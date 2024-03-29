﻿using TinyTrade.Core.Constructs;

namespace TinyTrade.Core.Strategy;

public abstract class AbstractCondition
{
    public bool IsSatisfied { get; protected set; }

    protected AbstractCondition()
    {
    }

    /// <summary>
    ///   Called every closed candle to update the state of the condition
    /// </summary>
    /// <param name="frame"> </param>
    public abstract void Tick(DataFrame frame);

    public virtual void Reset() => IsSatisfied = false;
}