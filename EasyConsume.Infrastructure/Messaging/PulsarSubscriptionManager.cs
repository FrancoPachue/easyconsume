using System;
using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using DotPulsar.Internal;
using EasyConsume.Infrastructure.Configs;

namespace EasyConsume.Infrastructure.Messaging;
public class PulsarSubscriptionManager
{
    private readonly PulsarConfig _config;

    public PulsarSubscriptionManager(PulsarConfig config)
    {
        _config = config;
    }

    public async Task SubscribeAsync(Func<IMessage<ReadOnlySequence<byte>>, Task> messageHandler, CancellationToken cancellationToken = default)
    {
        await using var client = PulsarClient.Builder().ServiceUrl(new Uri(_config.ServiceUrl)).Build();
        await using var consumer = client.NewConsumer()
                .StateChangedHandler(Monitor)
                .SubscriptionName("configs")
                .SubscriptionType(SubscriptionType.Exclusive)
                .Topic("persistent://public/default/topic-configs")
                .Create();

        await foreach (var message in consumer.Messages(cancellationToken))
        {
            Task.Run(()=> messageHandler(message));
            await consumer.Acknowledge(message, cancellationToken: cancellationToken);
        }
    }

    private static void Monitor(ConsumerStateChanged stateChanged)
    {
        var topic = stateChanged.Consumer.Topic;
        var state = stateChanged.ConsumerState;
        Console.WriteLine($"The consumer for topic '{topic}' changed state to '{state}'");
    }
}