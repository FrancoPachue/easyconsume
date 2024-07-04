using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.GrainInterfaces
{
    public interface IEventGrain: IGrainWithStringKey
    {
        Task<List<string>> GetConfirmedEvents();
        Task Process(string message);
    }
}
