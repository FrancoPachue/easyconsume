using DotPulsar.Abstractions;
using EasyConsume.Domain.Interfaces;
using EasyConsume.Domain.Services;
using EasyConsume.Infrastructure.Messaging;
using System.Buffers;
using System.Text;

namespace EasyConsume.Client.Workers
{
    public class BookSubscriptionConfigWorker : BackgroundService
    {
        private readonly ILogger<BookSubscriptionConfigWorker> _logger;
        private readonly PulsarSubscriptionManager _subscriptionManager;
        private readonly ISubscriptionClient _subscriptionClient;
        private readonly SubscriptionProcessor _queryBuilder;

        public BookSubscriptionConfigWorker(SubscriptionProcessor queryBuilder, ILogger<BookSubscriptionConfigWorker> logger, PulsarSubscriptionManager subscriptionManager)
        {
            _queryBuilder = queryBuilder;
            _logger = logger;
            _subscriptionManager = subscriptionManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _subscriptionManager.SubscribeAsync(HandleMessage, stoppingToken);
            _logger.LogInformation("BookConfigWorker is stopping.");
        }

        private async Task HandleMessage(IMessage<ReadOnlySequence<byte>> message)
        {
            try
            {
                var json = Encoding.UTF8.GetString(message.Data.ToArray());
                _queryBuilder.ProcessSubscription(json);
                _logger.LogInformation("Processed message and added subscription.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error : {ex.Message}");
            }
        }
    }
}