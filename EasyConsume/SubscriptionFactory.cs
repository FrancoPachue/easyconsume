using EasyConsume.Domain.Interfaces;
using EasyConsume.GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Client
{
    public class SubscriptionGrainFactory:ISubscriptionFactory
    {
        private readonly IClusterClient _clusterClient;
        public SubscriptionGrainFactory(IClusterClient clusterClient) 
        { 
            _clusterClient= clusterClient;
        }

        public IEventGrain Create(long? id)
        {
            return _clusterClient.GetGrain<IEventGrain>(id.ToString());
        }
    }
}
