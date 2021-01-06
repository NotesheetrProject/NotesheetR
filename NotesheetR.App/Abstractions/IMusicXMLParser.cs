using NotesheetR.Core.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesheetR.App.Abstractions
{
    public interface IMusicXMLParser
    {
        string KeyboardTilesToMusicXML(List<KeyboardTile> keyboardTiles, int frameCount);
    }
}
