using EasyConsume.Domain.DTO;
using EasyConsume.GrainInterfaces;
using EasyConsume.Infrastructure.Messaging;
using LaunchDarkly.EventSource;
using LaunchDarkly.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans;
using Orleans.Serialization.Invocation;

namespace EasyConsume.Grains
{
    public class SubscriptionGrain : Grain, ISubscription
    {
        private readonly ILogger<SubscriptionGrain> _logger;
        private readonly ProducerPool _producerPool;
        private readonly MessageRateCalculator _messageStatsService;

        public SubscriptionGrain(ILogger<SubscriptionGrain> logger,ProducerPool producerPool, MessageRateCalculator messageStatsService)
        {
            _messageStatsService= messageStatsService;
            _producerPool = producerPool;
            _logger = logger;
        }

        public async Task Process(string message)
        {
            var response = JsonConvert.DeserializeObject<OddsModel>(message);
            _messageStatsService.IncrementMessageCount(response.FixtureId, response.SuperOddsType);
            await _producerPool.SendMessage(message);
        }
    }
}