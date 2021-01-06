using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesheetR.Core.Classes
{
    public class KeyboardTile
    {
        public bool IsPressed { get; set; }
        public int LastPress { get; set; }
        public int XStart { get; set; }
        public int XEnd { get; set; }
        public int XMiddle { get; set; }
        public List<Tuple<int, int>> TilePresses { get; set; } = new List<Tuple<int, int>>();
        public Notes Note { get; set; }
        public int Octave { get; set; }
        public Color RestingColor { get; set; }
    }
}
