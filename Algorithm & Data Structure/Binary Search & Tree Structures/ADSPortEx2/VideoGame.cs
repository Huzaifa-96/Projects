using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSPortEx2
{
    //Film class implementation for Assessed Exercise 2

    class VideoGame : IComparable
    {
        private string title; // game title
        private string developer;
        private int releaseyear; // year it was released

        public VideoGame()
        {
            title = "Unknown"; // default title
            developer = "Unknown";
            releaseyear = 0; // default year
        }

        public VideoGame(string title, string developer, int releaseyear)
        {
            Title = title; // use property to set title
            Developer = developer;
            Releaseyear = releaseyear; // use property to set year
        }

        public string Title
        {
            get { return title; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) title = "Unknown"; // if empty, use default
                else title = value.Trim();
            }
        }

        public string Developer
        {
            get { return developer; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) developer = "Unknown"; // if empty, use default
                else developer = value.Trim();
            }
        }

        public int Releaseyear
        {
            get { return releaseyear; }
            set
            {
                if (value < 0) releaseyear = 0; // no negative year
                else releaseyear = value;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1; // null is always smaller
            VideoGame other = obj as VideoGame;
            if (other == null) throw new ArgumentException("Not a VideoGame");
            return string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase); // compare titles
        }
        public override string ToString()
        {
            return $"Title: {Title}, Developer: {Developer}, Year: {Releaseyear}"; // simple text format
        }

    }// End of class
}
