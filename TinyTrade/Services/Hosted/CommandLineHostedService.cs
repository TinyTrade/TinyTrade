﻿using HandierCli.CLI;
using HandierCli.Progress;
using HandierCli.Statics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TinyTrade.Services.Hosted;

internal class CommandLineHostedService : IHostedService
{
    private readonly ILogger logger;
    private readonly CommandLine cli;
    private readonly IServiceProvider services;

    public CommandLineHostedService(IServiceProvider services, ILoggerProvider provider)
    {
        cli = services.GetRequiredService<CommandLine>();
        logger = provider.CreateLogger(string.Empty);
        RegisterCommands();
        this.services = services;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => cli.RunAsync(), cancellationToken);
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        ConsoleExtensions.ClearConsoleLine();
        var spinner = ConsoleSpinner.Factory().Info("See ya ").Frames(6, ":)", ":|", ":(", ":(", ":(", ":(").Completed("").Build();
        await spinner.Await(Task.Delay(750, cancellationToken));
    }

    private void RegisterCommands()
    {
        cli.Register(Command.Factory("backtest")
            .Description("run a simulation on historical market data")
            .WithArguments(ArgumentsHandler.Factory()
                .Mandatory("strategy file", @".json$")
                .Mandatory("interval pattern", @"20[1-2][0-9]-[0-1][0-9]|20[1-2][0-9]-[0-1][0-9]")
                .Mandatory("pair symbol", @"USDT$"))
            .AddAsync(async handler =>
            {
                var service = services.GetRequiredService<BacktestService>();
                var intervalPattern = handler.GetPositional(1);
                var pair = handler.GetPositional(2);
                var strategyFile = handler.GetPositional(0);
                await service.RunBacktest(pair, intervalPattern, strategyFile);
            }));

        cli.Register(Command.Factory("run")
            .Description("run a foretest simulation or a live trading session")
            .WithArguments(ArgumentsHandler.Factory()
                .Mandatory("mode", new string[] { "foretest", "live" })
                .Mandatory("strategy file", @".json$")
                .Mandatory("pair symbol", @"USDT$"))
            .AddAsync(async handler =>
            {
                logger.LogDebug("Simulating running");
                var service = services.GetRequiredService<LiveService>();
                await service.RunLive(handler.GetPositional(0), handler.GetPositional(1), handler.GetPositional(2));
            }));

        cli.Register(Command.Factory("snap")
            .Description("look for active foretest simulations or live sessions currently running")
            .WithArguments(ArgumentsHandler.Factory())
            .Add(handler =>
            {
                logger.LogDebug("Simulating snapping");
                var service = services.GetRequiredService<SnapService>();
                service.Snapshot();
            }));
    }
}