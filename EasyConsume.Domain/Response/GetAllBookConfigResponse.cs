using EasyConsume.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Domain.Response
{
    public class GetAllBookConfigResponse
    {
        public List<Subscription> Subscriptions { get; set; }
    }
}
