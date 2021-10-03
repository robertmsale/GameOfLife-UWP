using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife_UWP
{
    public class ViewModel
    {
        private static double _CellSize = 48.0f;
        public double Zoom = 1.0f;
        public int CellSize { get => (int)(_CellSize * Zoom); }
        public string StatusBar { get => "Current: "+(universe.Current+1)+" | Total: "+universe.TotalGenerations+" | Living: "+universe.TotalLiving; }
        public Universe universe = new Universe(20, 20, true);
    }
}
