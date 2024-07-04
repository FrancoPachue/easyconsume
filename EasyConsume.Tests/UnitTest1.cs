using EasyConsume.GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.TestingHost;
using System;
using System.Threading.Tasks;
using Xunit;
using Orleans.TestingHost;
using Orleans.TestKit;
using Microsoft.Extensions.Hosting;
using Orleans.EventSourcing;
using Orleans.Runtime;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orleans.EventSourcing;
using Orleans.Runtime;
using Orleans.Runtime.LogConsistency;
using Orleans.Serialization;
using Microsoft.Extensions.Logging;

public class EventGrainTests : IClassFixture<EventGrainTests.Fixture>
{
    private readonly TestCluster _cluster;

    public EventGrainTests(Fixture fixture)
    {
        _cluster = fixture.Cluster;
    }

    public class Fixture : IDisposable
    {
        public TestCluster Cluster { get; }

        public Fixture()
        {
            var builder = new TestClusterBuilder();
            builder.AddClientBuilderConfigurator<HostConfigurator>();
            builder.AddSiloBuilderConfigurator<SiloConfigurator>();
            this.Cluster = builder.Build();
            this.Cluster.Deploy();
        }

        public void Dispose()
        {
            Cluster.StopAllSilos();
        }
    }

    // Ejemplo de prueba para el grano EventGrain
    [Fact]
    public async Task Process_SingleMessage_CorrectState()
    {
        var grain = _cluster.GrainFactory.GetGrain<IEventGrain>("perpe");

        // Ejecutar lógica de prueba
        await grain.Process("Test Message");

        // Verificar el estado esperado
        var confirmedEvents = await grain.GetConfirmedEvents();
        Assert.Contains("Test Message", confirmedEvents);
    }

    private class SiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder hostBuilder)
        {
            hostBuilder.AddMemoryGrainStorage("MemoryStore");
        }
    }

    private class HostConfigurator : IHostConfigurator
    {
        public void Configure(IHostBuilder host) =>
          host.ConfigureServices(services =>
                      services.AddSingleton<Factory<IGrainContext, ILogConsistencyProtocolServices>>(serviceProvider =>
                      {
                          var factory = ActivatorUtilities.CreateFactory(typeof(ProtocolServices), new[] { typeof(IGrainContext) });
                          return arg1 => (ILogConsistencyProtocolServices)factory(serviceProvider, new object[] { arg1 });
                      })
                       .AddTransient<IEventGrain, EventGrain>());
    }

    /// <summary>
    /// Functionality for use by log view adaptors that run distributed protocols.
    /// This class allows access to these services to providers that cannot see runtime-internals.
    /// It also stores grain-specific information like the grain reference, and caches
    /// </summary>
    internal class ProtocolServices : ILogConsistencyProtocolServices
    {
        private readonly ILogger log;
        private readonly DeepCopier deepCopier;
        private readonly IGrainContext grainContext;   // links to the grain that owns this service object

        public ProtocolServices(
            IGrainContext grainContext,
            ILoggerFactory loggerFactory,
            DeepCopier deepCopier,
            ILocalSiloDetails siloDetails)
        {
            this.grainContext = grainContext;
            this.log = loggerFactory.CreateLogger<ProtocolServices>();
            this.deepCopier = deepCopier;
            this.MyClusterId = siloDetails.ClusterId;
        }

        public GrainId GrainId => grainContext.GrainId;

        public string MyClusterId { get; }

        public T DeepCopy<T>(T value) => this.deepCopier.Copy(value);

        public void ProtocolError(string msg, bool throwexception)
        {
            log.LogError(
                (int)(throwexception ? ErrorCode.LogConsistency_ProtocolFatalError : ErrorCode.LogConsistency_ProtocolError),
                "{GrainId} Protocol Error: {Message}",
                grainContext.GrainId,
                msg);

            if (!throwexception)
                return;

            throw new OrleansException(string.Format("{0} (grain={1}, cluster={2})", msg, grainContext.GrainId, this.MyClusterId));
        }

        public void CaughtException(string where, Exception e)
        {
            log.LogError(
                (int)ErrorCode.LogConsistency_CaughtException,
                e,
               "{GrainId} exception caught at {Location}",
               grainContext.GrainId,
               where);
        }

        public void CaughtUserCodeException(string callback, string where, Exception e)
        {
            log.LogWarning(
                (int)ErrorCode.LogConsistency_UserCodeException,
                e,
                "{GrainId} exception caught in user code for {Callback}, called from {Location}",
                grainContext.GrainId,
                callback,
                where);
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            if (log != null && log.IsEnabled(level))
            {
                var msg = $"{grainContext.GrainId} {string.Format(format, args)}";
                log.Log(level, 0, msg, null, (m, exc) => $"{m}");
            }
        }
    }
}