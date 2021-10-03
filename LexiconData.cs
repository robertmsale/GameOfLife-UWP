using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife_UWP
{
    /// <summary>
    /// JSON structure of GoL Lexicon HTTP response payload (ended up not using it but kept for clarity)
    /// </summary>
    class LexiconData
    {
        public string name { get; set; }
        public string description { get; set; }
        public string date { get; set; }
        public string pattern { get; set; }
    }
}
