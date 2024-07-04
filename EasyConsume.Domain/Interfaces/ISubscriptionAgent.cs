using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Domain.Interfaces
{
    public interface ISubscriptionAgent
    {
        Task Start(Uri uri);
    }
}
