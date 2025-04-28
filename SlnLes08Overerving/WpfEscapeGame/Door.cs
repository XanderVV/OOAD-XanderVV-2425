using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfEscapeGame
{
    internal class Door : Actor
    {
        public bool IsLocked { get; set; } = false;
        public Item Key { get; set; }
        public Item HiddenItem { get; set; }
        public Room ToRoom { get; set; }

        public Door(string name, string desc) : base(name, desc)
        {
        }
    }
} 