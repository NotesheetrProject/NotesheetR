using NotesheetR.App.Abstractions;
using NotesheetR.App.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NotesheetR.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IVideoDownloader videoDownloader;
        private readonly INoteParser noteParser;
        private readonly IMusicXMLParser musicXMLParser;

        public HomeController()
        {
            videoDownloader = new VideoDownloader();
            noteParser = new NoteParser();
            musicXMLParser = new MusicXMLParser();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public void ConvertVideoByUrl(string uri)
        {
            //var video = videoDownloader.GetVideoByUrl(new Uri(uri));

            //var keyboardTiles = new NoteParser().ParseToKeyboardTiles(video);

            //var musicXmlParser = new MusicXMLParser();

            //var musicXML = musicXmlParser.KeyboardTilesToMusicXML(keyboardTiles.ToList(), video.Framecount);
        }
    }
}