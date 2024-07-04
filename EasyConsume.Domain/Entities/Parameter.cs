using EasyConsume.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Domain.Entities
{
    public class Parameter : Entity
    {
        public Parameter(int id, string name, string value) : base(id)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
