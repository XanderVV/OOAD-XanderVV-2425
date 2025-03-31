using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfEscapeGame
{
    internal class Room
    {
        public string Name { get; }
        public string Description { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();

        public Room(string name, string desc)
        {
            Name = name;
            Description = desc;
        }
    }
}
