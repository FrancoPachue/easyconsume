using EasyConsume.Domain.Entities;
using EasyConsume.Domain.Enums;
using EasyConsume.Domain.Interfaces;
using EasyConsume.Domain.Response;
using Newtonsoft.Json;

namespace EasyConsume.Domain.Services
{
    public class SubscriptionProcessor
    {
        private readonly ISubscriptionClient _subscriptionClient;

        public SubscriptionProcessor(ISubscriptionClient subscriptionClient)
        {
            _subscriptionClient = subscriptionClient;
        }

        public async Task ProcessSubscriptions(string json)
        {
            var response = JsonConvert.DeserializeObject<GetAllBookConfigResponse>(json);
            var subscriptions = response.Subscriptions;
            if (subscriptions != null && subscriptions.Count > 0)
            {
                foreach (var subscription in subscriptions)
                {
                    await _subscriptionClient.Add(subscription.ToString());
                }
            }
        }

        public void ProcessSubscription(string json)
        {
            var response = JsonConvert.DeserializeObject<PulsarSubscription>(json);
            var subscription = response.Subscription;
            switch (response.OperationType)
            {
                case OperationType.Insert:
                    _subscriptionClient.Add(subscription.ToString());
                    break;
                case OperationType.Update: 
                    _subscriptionClient.Update(subscription.ToString());
                    break;
                case OperationType.Delete:
                    _subscriptionClient.Remove(subscription.ToString());
                    break;
                default:
                    break;
            }
        }
    }
}
