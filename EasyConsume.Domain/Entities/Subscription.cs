using EasyConsume.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Domain.Entities
{
    public class Subscription : Entity
    {
        public Subscription(int id) : base(id)
        {
            Parameters = new List<Parameter>();
        }
        public string Endpoint { get; set; }
        public List<Parameter> Parameters { get; set; }

        public override string ToString()
        {
            string host = GetHost();
            string endpoint = Endpoint;
            if (!string.IsNullOrEmpty(host))
            {
                endpoint = $"{host}{Endpoint}";
            }

            string queryString = "";
            if (Parameters != null && Parameters.Any())
            {
                var parameterStrings = Parameters.Select(parameter => $"{parameter.Name}={Uri.EscapeDataString(parameter.Value)}");
                queryString = "?" + string.Join("&", parameterStrings);
            }

            return $"{endpoint}{queryString}";
        }

        private string GetHost()
        {
            return "server_url";
        }
    }
}
