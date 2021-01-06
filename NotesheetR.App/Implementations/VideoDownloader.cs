using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NotesheetR.App.Abstractions;
using VideoLibrary;

namespace NotesheetR.App.Implementations
{
    public class VideoDownloader : IVideoDownloader
    {
        public Core.Classes.Video GetVideoByUrl(Uri uri)
        {
            var youtube = YouTube.Default;
            YouTubeVideo ytVideo = youtube.GetVideo(uri.AbsoluteUri);
            var filePath = $@"{GetHashString(uri.ToString())}.mp4";
            byte[] bytes = null;
            if (!File.Exists(filePath))
            {
                bytes = ytVideo.GetBytes();
                File.WriteAllBytes(filePath, bytes);
            }
            else
            {
                bytes = File.ReadAllBytes(filePath);
            }

            return new Core.Classes.Video(
                ytVideo.Title,
                ytVideo.Uri,
                (Core.Classes.VideoFormat)ytVideo.Format,
                (Core.Classes.AudioFormat)ytVideo.AudioFormat,
                bytes,
                filePath
                );
        }

        private static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
