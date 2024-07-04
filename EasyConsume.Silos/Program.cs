using EasyConsume.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtoBuf.Meta;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering()
        .AddStateStorageBasedLogConsistencyProvider("StateStorage")
        .AddMemoryGrainStorageAsDefault()
        .AddMemoryGrainStorage("MemoryStore")
        .UseDashboard(options => options.Port = 31000)
            .ConfigureLogging(logging => logging.AddConsole())
            .ConfigureServices(services =>
            services.AddTransient<SseServiceBase>()
            .AddSingleton<MessageRateCalculator>()
            .AddSingleton<ProducerPool>());
    })
    .UseConsoleLifetime();

using IHost host = builder.Build();

await host.RunAsync();