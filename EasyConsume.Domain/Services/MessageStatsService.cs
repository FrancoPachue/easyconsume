using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyConsume.Domain.Services
{
    public class ConsumerStats
    {
        public int MessageCount { get; set; }
        public int MaxMessagesPerSecond { get; set; }
    }

    public class MessageStatsService
    {
        private readonly ConcurrentDictionary<string, ConsumerStats> _messageCounts;
        private readonly Timer _timer;
        private int _lastTotalMessages;
        private DateTime _lastSecondStart;

        public MessageStatsService()
        {
            _messageCounts = new ConcurrentDictionary<string, ConsumerStats>();
            _lastTotalMessages = 0;
            _lastSecondStart = DateTime.UtcNow;
            _timer = new Timer(TimerElapsed, null, 0, 1000); // Verificar cada segundo
        }

        public void RegisterMessageConsumed(string consumerName)
        {
            _messageCounts.AddOrUpdate(
                consumerName,
                new ConsumerStats { MessageCount = 1 },
                (key, value) =>
                {
                    value.MessageCount++;
                    return value;
                });

            UpdateMaxMessagesPerSecond(consumerName);
        }

        private void UpdateMaxMessagesPerSecond(string consumerName)
        {
            var currentTime = DateTime.UtcNow;
            var elapsedTime = currentTime - _lastSecondStart;

            if (elapsedTime.TotalSeconds >= 1)
            {
                var currentTotalMessages = GetTotalMessagesConsumed();
                var messagesConsumedLastSecond = currentTotalMessages - _lastTotalMessages;
                _lastTotalMessages = currentTotalMessages;

                if (_messageCounts.TryGetValue(consumerName, out ConsumerStats stats))
                {
                    stats.MaxMessagesPerSecond = Math.Max(stats.MaxMessagesPerSecond, messagesConsumedLastSecond);
                }

                _lastSecondStart = currentTime;
            }
        }

        private int GetTotalMessagesConsumed()
        {
            return _messageCounts.Values.Sum(entry => entry.MessageCount);
        }

        private void TimerElapsed(object sender)
        {
            int totalMessages = GetTotalMessagesConsumed();

            var logMessage = $"Total messages consumed: {totalMessages}\n";

            // Loguear la mayor cantidad de mensajes por segundo por consumidor
            foreach (var (consumer, stats) in _messageCounts)
            {
                if (stats.MaxMessagesPerSecond > 0)
                {
                    logMessage += $"Consumer: {consumer}, Max messages per second: {stats.MaxMessagesPerSecond}\n";
                    // No es necesario restablecer el valor aquí, ya que se actualizará en el siguiente segundo
                }
            }

            Console.WriteLine(logMessage);
        }
    }
}