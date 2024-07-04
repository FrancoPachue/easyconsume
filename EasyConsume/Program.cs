using EasyConsume.Client;
using EasyConsume.Client.Workers;
using EasyConsume.Domain.Interfaces;
using EasyConsume.Domain.Services;
using EasyConsume.Infrastructure.Configs;
using EasyConsume.Infrastructure.Messaging;
using EasyConsume.Worker;
using Microsoft.Extensions.Options;
using ProtoBuf.Meta;

var serviceProvider = new ServiceCollection()
    .AddTransient<PulsarSubscriptionManager>()
    .AddTransient<SubscriptionClient>()
    .AddTransient<SseServiceBase>()
    .BuildServiceProvider();



using var host = Host.CreateDefaultBuilder(args)
    .UseOrleansClient(clientBuilder =>
        clientBuilder.UseLocalhostClustering())
        .ConfigureAppConfiguration((hostContext, configBuilder) =>
        {
            configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                         .AddEnvironmentVariables();
        })
    .ConfigureServices((hostContext, s) =>
        s.Configure<PulsarConfig>(hostContext.Configuration.GetSection("PulsarConfig"))
        .AddLogging(x => {
            x.ClearProviders();
            x.AddJsonConsole(option => option.IncludeScopes = true);
        })
            .AddSingleton(cfg => cfg.GetService<IOptions<PulsarConfig>>().Value)
            .AddTransient<MessageRateCalculator>()
            .AddTransient<ISubscriptionFactory, SubscriptionGrainFactory>()
            .AddTransient<ISubscriptionClient, SubscriptionClient>()
            .AddTransient<SubscriptionProcessor>()
            .AddSingleton<PulsarSubscriptionManager>()
            .AddTransient<ConfigsService>()
            .AddTransient<SseServiceBase>()
            .AddHostedService<BookSubscriptionConfigWorker>()
            .AddHostedService<SubscriptionWorker>()
            .AddHttpClient("SubscriptionApi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7295");
            })
            )
    .Build();

await host.StartAsync();

Console.ReadLine();