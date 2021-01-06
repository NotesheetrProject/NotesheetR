using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesheetR.Core.Classes
{
    public class NoteInstance
    {
        public int StartTime { get; set; }
        public int Duration { get; set; }
        public Notes Note { get; set; }
        public byte Octave { get; set; }

    }

    public enum Notes
    {
        A,
        AB,
        B,
        C,
        CD,
        D,
        DE,
        E,
        F,
        FG,
        G,
        GA
    }
}
