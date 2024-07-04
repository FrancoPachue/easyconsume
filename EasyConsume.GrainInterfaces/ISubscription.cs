using LaunchDarkly.EventSource;
using Orleans;

namespace EasyConsume.GrainInterfaces
{
    public interface ISubscription : IGrainWithStringKey
    {
        Task Process(string message);
    }
}