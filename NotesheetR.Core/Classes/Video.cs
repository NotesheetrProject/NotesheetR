using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesheetR.Core.Classes
{
    public class Video
    {
        public Video(string title, string uri, VideoFormat format, AudioFormat audioFormat, byte[] bytes, string filePath)
        {
            Title = title;
            Uri = uri;
            Format = format;
            AudioFormat = audioFormat;
            Bytes = bytes;
            FilePath = filePath;
        }

        public string Title { get; }
        public string Uri { get; }
        public VideoFormat Format { get; }
        public AudioFormat AudioFormat { get; }
        public byte[] Bytes { get; }

        public string FilePath { get; set; }
        public int Framecount { get; set; }
    }

    public enum VideoFormat
    {
        Flash = 0,
        Mobile = 1,
        Mp4 = 2,
        WebM = 3,
        Unknown = 4
    }
    public enum AudioFormat
    {
        Mp3 = 0,
        Aac = 1,
        Vorbis = 2,
        Unknown = 3,
        Opus = 4
    }
}
