using EasyConsume.GrainInterfaces;

namespace EasyConsume.Domain.Interfaces
{
    public interface ISubscriptionFactory
    {
        IEventGrain Create(long? id);
            }
}
