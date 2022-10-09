﻿using GeneticSharp;
using HandierCli.Progress;
using Microsoft.Extensions.Logging;
using TinyTrade.Core.Constructs;
using TinyTrade.Core.DataProviders;
using TinyTrade.Core.Exchanges.Backtest;
using TinyTrade.Core.Models;
using TinyTrade.Services;

namespace TinyTrade.Opt.Modules;

internal class StrategyFitness : IFitness
{
    private readonly ILogger? logger;
    private readonly BacktestService backtestService;
    private readonly OptimizableStrategyModel templateModel;
    private readonly BacktestDataframeProvider provider;
    private readonly LocalTestExchange exchange;

    public StrategyFitness(BacktestService backtestService, Pair pair, TimeInterval interval, OptimizableStrategyModel strategyModel, ILogger? logger = null)
    {
        provider = new BacktestDataframeProvider(interval, pair, Timeframe.FromFlag(strategyModel.Timeframe));
        exchange = new LocalTestExchange(100, logger);
        this.backtestService = backtestService;
        this.templateModel = strategyModel;
        this.logger = logger;
    }

    public async Task Load()
    {
        var bar = ConsoleProgressBar.Factory().Lenght(20).Build();
        var progress = new Progress<IDataframeProvider.LoadProgress>(p => bar.Report(p.Progress, p.Description));
        await provider.Load(progress);
        bar.Dispose();
    }

    public double Evaluate(IChromosome chromosome)
    {
        if (chromosome is not FloatingPointChromosome strategyChromosome) return 0;
        var floats = strategyChromosome.ToFloatingPoints();
        if (floats.Length != templateModel.Genes.Count) return 0;
        var zip = templateModel.Genes.Zip(floats).ToList();
        var evaluationModel = new StrategyModel()
        {
            Name = templateModel.Name,
            Parameters = templateModel.Parameters,
            Timeframe = templateModel.Timeframe,
            Traits = zip.ConvertAll(z => new StrategyTrait(z.First.Key, (float)z.Second))
        };

        var res = backtestService.RunCachedBacktest(provider, exchange, evaluationModel, false).Result;
        return res is null ? 0 : CalculateFitness((BacktestResultModel)res);
    }

    private static double CalculateFitness(BacktestResultModel resultModel)
    {
        var a = 0.65F;
        var g = 4F;
        var d = 1.75F;
        var r = resultModel.FinalBalance / resultModel.InitialBalance;
        var wr = resultModel.WinRate;
        var r_penalize = 1F;
        var wr_penalize = 1F;
        if (r < 1) r_penalize = (float)Math.Pow(r, g);
        if (wr < 0.5F) wr_penalize = (float)Math.Pow(wr + 0.5F, d);
        var fac2 = r_penalize * Math.Pow(r + 1, a);
        var fac3 = wr_penalize * Math.Pow(wr + 1, 1 - a);
        var res = fac2 + fac3;
        return 365 * (res / resultModel.Days);
    }
}