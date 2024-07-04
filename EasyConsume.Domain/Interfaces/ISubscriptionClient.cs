using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Domain.Interfaces
{
    public interface ISubscriptionClient
    {
        Task Add(string uri);
        Task Update(string uri);
        void Remove(string uri);
    }
}
