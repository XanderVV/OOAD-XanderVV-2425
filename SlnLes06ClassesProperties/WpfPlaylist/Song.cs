using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPlaylist
{
    class Song
    {
        public string Name { get; }
        public Artist Artist { get; }
        public int Year { get; }
        public TimeSpan Duration { get; }
        public Uri Uri { get; }
        public string ImagePath { get; }

        public Song(string name, Artist artist, int year, TimeSpan duration, Uri uri, string imagePath)
        {
            Name = name;
            Artist = artist;
            Year = year;
            Duration = duration;
            Uri = uri;
            ImagePath = imagePath;
        }

        public override string ToString()
        {
            return $"{Name} - {Artist.Name} ({Year}, {Duration})";
        }
    }
}