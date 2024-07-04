using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using DotPulsar.Internal;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LaunchDarkly.Logging.LogCapture;

namespace EasyConsume.Infrastructure.Messaging
{
    public class ProducerPool
    {
        private readonly IPulsarClient _pulsarClient;
        private readonly List<IProducer<ReadOnlySequence<byte>>> _producers = new List<IProducer<ReadOnlySequence<byte>>>();
        private int _currentIndex = -1;
        private readonly Timer _monitorTimer;
        private int _totalMessagesPushed;
        private readonly MessageRateCalculator _messageRateCalculator;
        private int _messageRatePushed;
        private readonly object _lock = new object();

        public ProducerPool(MessageRateCalculator messageRateCalculator)
        {
            _messageRateCalculator = messageRateCalculator;
            _pulsarClient = PulsarClient.Builder().Build();
            InitializeProducers();
            _monitorTimer = new Timer(UpdateMessageRate, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // Actualiza cada 5 segundos
        }

        public void InitializeProducers()
        {
            for (int i = 0; i <= 1; i++)
            {
                _producers.Add(_pulsarClient.NewProducer().Topic("pushtest1").Create());
            }
        }

        public void AddProducer()
        {
            lock (_lock)
            {
                _producers.Add(_pulsarClient.NewProducer().Topic("pushtest1").Create());
            }
        }

        public void RemoveProducer()
        {
            lock (_lock)
            {
                _producers.RemoveAt(0);
            }
        }

        public async Task SendMessage(string message)
        {
                var producer = GetNextProducer();
                await producer.Send(Encoding.UTF8.GetBytes(message));
                _messageRatePushed++;
        }

        private IProducer<ReadOnlySequence<byte>> GetNextProducer()
        {
            if (_producers.Count == 0)
            {
                throw new InvalidOperationException("No hay productores disponibles.");
            }

            _currentIndex = (_currentIndex + 1) % _producers.Count; // Avanza al siguiente productor circularmente
            return _producers[_currentIndex];
        }

        private void UpdateMessageRate(object state)
        {
            lock (_lock)
            {
                //ScaleProducers();
                Console.WriteLine($"Total Consumed: {_messageRateCalculator.GetRatesAndTotal()} messages/s |Total Pushed: {_messageRatePushed} messages/s");
            }
        }

        private void ScaleProducers()
        {
            if (_messageRatePushed > 0 && _messageRateCalculator.GetRatesAndTotal() > 0 && _messageRatePushed < _messageRateCalculator.GetRatesAndTotal())
            {
                //AddProducer(); 
                Console.WriteLine("Se agregó un nuevo productor.");
            }
            // Puedes agregar más lógica de escalado/desescalado según tus requisitos específicos
        }
    }
}
