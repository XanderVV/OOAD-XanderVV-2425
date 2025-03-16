using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPlaylist
{
    class Artist
    {
        public string Name { get; }
        public DateTime BornDate { get; }
        public string Info { get; }

        public Artist(string name, DateTime bornDate, string info)
        {
            Name = name;
            BornDate = bornDate;
            Info = info;
        }
    }
}