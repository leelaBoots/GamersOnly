using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Entities
{
    public class Group
    {
      // empty ctor needed for EF schema ageneration
      public Group() {}

      public Group(string name)
      {
        Name = name;
      }

      // "Key" will force this property value to be unique
      [Key]
      public string Name { get; set; }

      public ICollection<Connection> Connections { get; set; } = new List<Connection>();
    }
}