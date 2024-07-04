using EasyConsume.Domain.Interfaces;
using EasyConsume.Domain.Services;
using EasyConsume.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Client.Workers
{
    public class SubscriptionWorker : BackgroundService
    {
        private readonly ConfigsService _subscriptionConfigClient;
        private readonly ILogger<SubscriptionClient> _logger;
        private readonly SubscriptionProcessor _subscriptionProcessor;

        public SubscriptionWorker(SubscriptionProcessor subscriptionProcessor, ConfigsService subscriptionConfigClient,  ILogger<SubscriptionClient> logger)
        {
            _subscriptionProcessor = subscriptionProcessor;
            _subscriptionConfigClient = subscriptionConfigClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var subscriptions = await _subscriptionConfigClient.GetStringAsync("SubscriptionApi", "api/subscriptions");
                await _subscriptionProcessor.ProcessSubscriptions(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error : {ex.Message}");
            }
        }
    }
}
