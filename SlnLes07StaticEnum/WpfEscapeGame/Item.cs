using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace WpfEscapeGame
{
    internal class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsLocked { get; set; } = false;
        public Item Key { get; set; }
        public Item HiddenItem { get; set; }
        public bool IsPortable { get; set; }

        public Item(string name, string desc) 
        {
            Name = name;
            Description = desc;
            IsPortable = true;
        }

        public override string ToString()
        {
            return Name;
        }

        public Item(string naam, string desc, bool portable)
        {
            Name = naam;
            Description = desc;
            IsPortable = portable;
        }
    }
}
