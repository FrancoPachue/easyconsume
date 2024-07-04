using EasyConsume.Client;
using EasyConsume.Domain.Interfaces;
using EasyConsume.Domain.Services;
using EasyConsume.GrainInterfaces;
using EasyConsume.Infrastructure.Messaging;
using Orleans;

namespace EasyConsume.Worker
{
    public class SubscriptionClient : ISubscriptionClient
    {
        private readonly List<SubscriptionAgent> _subscriptions;
        private readonly ISubscriptionFactory _grainFactory;
        private readonly ILogger _logger;
        private readonly SseServiceBase _sseService;
        private readonly MessageRateCalculator _messageStatsService;
        public SubscriptionClient(ISubscriptionFactory grainFactory, SseServiceBase sseService, ILogger<SubscriptionClient> logger, MessageRateCalculator messageStatsService)
        {
            _grainFactory = grainFactory;
            _logger = logger;
            _sseService = sseService;
            _messageStatsService = messageStatsService;
            _subscriptions = new List<SubscriptionAgent>();
        }

        public async Task Add(string uri)
        {
            try
            {
                var sub = new SubscriptionAgent(_grainFactory, _logger, _sseService, _messageStatsService);
                _subscriptions.Add(sub);
                Task.Run( async () => await sub.Start(new Uri(uri)));
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error : {ex.Message}");
            }
        }

        public async Task Update(string uri)
        {
            try
            {
                Remove(uri);
                await Add(uri);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error : {ex.Message}");
            }
        }

        public void Remove(string uri)
        {
            try
            {
                var sub = _subscriptions.Find(x => x.Equals(uri));
                if (sub != null)
                {
                    _subscriptions.Remove(sub);
                    sub.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error : {ex.Message}");
            }
        }
    }
}
