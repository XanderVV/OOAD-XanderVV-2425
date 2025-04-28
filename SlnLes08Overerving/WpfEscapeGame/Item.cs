using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace WpfEscapeGame
{
    internal class Item : Actor
    {
        public bool IsLocked { get; set; } = false;
        public Item Key { get; set; }
        public Item HiddenItem { get; set; }
        public bool IsPortable { get; set; }

        public Item(string name, string desc) : base(name, desc)
        {
            IsPortable = true;
        }

        public Item(string name, string desc, bool portable) : base(name, desc)
        {
            IsPortable = portable;
        }
    }
}
