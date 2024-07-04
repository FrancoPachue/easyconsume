using EasyConsume.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Domain.Entities
{
    public class PulsarSubscription
    {
        public PulsarSubscription(OperationType operationType, Subscription subscription)
        {
            OperationType = operationType;
            Subscription = subscription;
        }

        public OperationType OperationType { get; set; }
        public Subscription Subscription { get; set; }
    }
}
