using EasyConsume.Domain.DTO;
using EasyConsume.Domain.Interfaces;
using EasyConsume.Domain.Response;
using EasyConsume.Domain.Services;
using EasyConsume.Grains;
using EasyConsume.Infrastructure.Messaging;
using LaunchDarkly.EventSource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Client
{
    public class SubscriptionAgent : ISubscriptionAgent, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ISubscriptionFactory _grainFactory;
        private readonly SseServiceBase _sseService;
        private readonly MessageRateCalculator _messageStatsService;
        private string _uri;
        private bool _disposed = false;
        private HeartbeatService _heartbeatService;

        public SubscriptionAgent(ISubscriptionFactory grainFactory, ILogger logger, SseServiceBase sseService, MessageRateCalculator messageStatsService)
        {
            _grainFactory = grainFactory;
            _logger = logger;
            _sseService = sseService;
            _messageStatsService = messageStatsService;
        }

        public async Task Start(Uri uri)
        {
            _uri = uri.AbsoluteUri;
            _logger.LogInformation($"Starting subscription {uri}");
            _heartbeatService = new HeartbeatService(TimeSpan.FromSeconds(15), RestartSubscription);
            _heartbeatService.Start();

            await _sseService.StartSseAsync(uri, ReceiveMessage, HeartbeatHandler);
        }

        public async void ReceiveMessage(MessageReceivedEventArgs e)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<OddsModel>(e.Message.Data);
                _messageStatsService.IncrementMessageCount(response.FixtureId,response.SuperOddsType);
                var grain = _grainFactory.Create(response.FixtureId);
                await grain.Process(e.Message.Data);
                _heartbeatService?.UpdateHeartbeatTime();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error consuming: {ex.Message}");
            }
        }

        public void HeartbeatHandler(MessageReceivedEventArgs e)
        {
            try
            {
                //_logger.LogInformation($"Heartbeat received");
                _heartbeatService?.UpdateHeartbeatTime();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling heartbeat: {ex.Message}");
            }
        }

        private async void RestartSubscription()
        {
            _logger.LogInformation($"Restarting {_uri}");
            _heartbeatService.Stop();
            _sseService.Dispose();
            await Start(new Uri(_uri));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _heartbeatService?.Stop();
                _sseService.Dispose();
            }
            _disposed = true;
        }
    }

}
