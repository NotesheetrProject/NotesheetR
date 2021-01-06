using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesheetR.App.Implementations;
using NotesheetR.Core.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Reflection;

namespace NotesheetR.VideoDownload.Tests
{
    [TestClass()]
    public class VideoDownloaderTests
    {
        [TestMethod()]
        public void PresentationFunctionCauseWebWontWork()
        {
            if (!Directory.Exists("output"))
                Directory.CreateDirectory("output");

            var videoURL = "https://www.youtube.com/watch?v=pN_gruOjM9I";
            var firstNote = new NoteInstance() { Note = Notes.D, Octave = 3 };

            //------------------------------------------------------------------------//
            //var videoURL = "https://www.youtube.com/watch?v=aCJxjDzyAT8";           //
            //var firstNote = new NoteInstance() { Note = Notes.F, Octave = 3 };      //
            //                                                                        //
            //var videoURL = "https://www.youtube.com/watch?v=M4hAK4bTdl4";           //
            //var firstNote = new NoteInstance() { Note = Notes.A, Octave = 3 };      //
            //                                                                        //
            //var videoURL = "https://www.youtube.com/watch?v=p1WCR7vNcIw";           //
            //var firstNote = new NoteInstance() { Note = Notes.E, Octave = 3 };      //
            //------------------------------------------------------------------------//

            var videoDownloader = new VideoDownloader();

            var video = videoDownloader.GetVideoByUrl(new Uri(videoURL));

            var keyboardTiles = new NoteParser().ParseToKeyboardTiles(video, firstNote);

            var musicXmlParser = new MusicXMLParser();

            var musicXML = musicXmlParser.KeyboardTilesToMusicXML(keyboardTiles.ToList(), video.Framecount);

            File.WriteAllText(@"output\musicxml.xml", musicXML);

            // The output file will be inside the /bin/output folder of the NotesheetR.AppTests project
            // Webiste to view the musicxml.xml => https://www.soundslice.com/musicxml-viewer/
        }
    }
}