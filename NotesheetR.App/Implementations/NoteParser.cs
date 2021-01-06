using Accord.Video.FFMPEG;
using NotesheetR.App.Abstractions;
using NotesheetR.Core.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace NotesheetR.App.Implementations
{
    public class NoteParser : INoteParser
    {
        public IEnumerable<KeyboardTile> ParseToKeyboardTiles(Video video, NoteInstance firstNote)
        {
            return ParseToKeyboardTilesInternal(video, firstNote);
        }

        private IEnumerable<KeyboardTile> ParseToKeyboardTilesInternal(Video video, NoteInstance firstNote)
        {
            var bitmaps = GetFirst500Bitmaps(video.FilePath);

            var startingFrame = GetStartBitmap(bitmaps);
            var keyboardYCoordinate = GetYCoordinateFromKeyboard(bitmaps[startingFrame]);

            List<KeyboardTile> keyboardTiles = GetKeyboardTilesFromKeyboard(bitmaps[startingFrame], keyboardYCoordinate, firstNote);

            foreach (var bitmap in bitmaps)
                bitmap.Dispose();

            List<Color> AllColors = GetAllColors();
            var currentFrame = startingFrame;
            foreach (var bitmap in GetBitmapsFrom(video.FilePath, startingFrame))
            {
                PopulateKeyboardTiles(keyboardTiles, bitmap, keyboardYCoordinate, AllColors, currentFrame);
                currentFrame++;

                bitmap.Dispose();
            }

            video.Framecount = currentFrame;

            return keyboardTiles;
        }

        private void PopulateKeyboardTiles(
            List<KeyboardTile> keyboardTiles,
            Bitmap bitmap, int keyboardYCoordinate,
            List<Color> allColors,
            int currentFrame)
        {

            foreach (var keyboardTile in keyboardTiles)
            {
                var pixel = bitmap.GetPixel(keyboardTile.XMiddle, keyboardYCoordinate);
                var closestColor = allColors[ClosestColor(allColors, pixel)];

                if (!ColorsAreClose(closestColor, keyboardTile.RestingColor, 100))
                {
                    if (keyboardTile.IsPressed)
                        continue;
                    keyboardTile.IsPressed = true;
                    keyboardTile.LastPress = currentFrame;
                }
                else if (keyboardTile.IsPressed)
                {
                    keyboardTile.IsPressed = false;
                    keyboardTile.TilePresses.Add(new Tuple<int, int>(keyboardTile.LastPress, currentFrame - 1));
                }
            }
        }

        private static bool ColorsAreClose(Color a, Color z, int threshold = 50)
        {
            int r = (int)a.R - z.R,
                g = (int)a.G - z.G,
                b = (int)a.B - z.B;
            return (r * r + g * g + b * b) <= threshold * threshold;
        }

        private List<KeyboardTile> GetKeyboardTilesFromKeyboard(Bitmap bitmap, int keyboardYCoordinate, NoteInstance firstNote)
        {
            List<Color> AllColors = GetAllColors();
            var AllTiles = new List<KeyboardTile>();

            List<Color> KnownColors = new List<Color>() { Color.White, Color.Black, Color.Gray };

            KeyboardTile currTile = GetFirstTile(bitmap, keyboardYCoordinate, firstNote);

            for (int x = currTile.XStart; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, keyboardYCoordinate);
                var closestColor = KnownColors[ClosestColor(KnownColors, pixel)];

                var currTileColor = KnownColors[ClosestColor(KnownColors, currTile.RestingColor)];

                if (currTileColor != closestColor)
                {

                    if (currTile.Note == Notes.B || currTile.Note == Notes.E)
                    {
                        if (closestColor == Color.Black)
                            closestColor = Color.Gray;
                    }

                    if (closestColor == Color.Gray)
                    {
                        switch (currTile.Note)
                        {
                            case Notes.B:
                            case Notes.E:
                                closestColor = Color.Black;
                                pixel = Color.White;
                                break;
                            default:
                                continue;
                        }
                    }

                    currTile.XEnd = x - 1;
                    currTile.XMiddle = (currTile.XEnd + currTile.XStart) / 2;
                    var rawColor = bitmap.GetPixel(currTile.XMiddle, keyboardYCoordinate);
                    currTile.RestingColor = AllColors[ClosestColor(AllColors, rawColor)];
                    AllTiles.Add(currTile);
                    currTile = new KeyboardTile()
                    {
                        XStart = x,
                        RestingColor = pixel,
                        Note = NextNoteDict[currTile.Note],
                        Octave = currTile.Octave
                    };

                    if (currTile.Note == Notes.A)
                        currTile.Octave = currTile.Octave + 1;
                }
            }

            var wrongTiles = AllTiles.Where(x => x.XStart + 2 > x.XEnd).ToList();

            if (wrongTiles.Count != 0)
                throw new Exception("Could not read the Keyboard tiles from video");

            return AllTiles;
        }

        private static KeyboardTile GetFirstTile(Bitmap bitmap, int keyboardYCoordinate, NoteInstance firstNote)
        {
            List<Color> KnownColors = new List<Color>() { Color.White, Color.Black };

            for (int x = 2; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, keyboardYCoordinate);
                var closestColor = KnownColors[ClosestColor(KnownColors, pixel)];

                if (closestColor.Name.Contains("White") || closestColor.Name == "Transparent")
                {
                    return new KeyboardTile()
                    {
                        XStart = x,
                        RestingColor = Color.White,
                        Note = firstNote.Note,
                        Octave = firstNote.Octave
                    };
                }
            }

            throw new Exception("First tile could not be found.");
        }

        public Dictionary<Notes, Notes> NextNoteDict = new Dictionary<Notes, Notes>()
        {
            {Notes.A, Notes.AB },
            {Notes.AB, Notes.B },
            {Notes.B, Notes.C },
            {Notes.C, Notes.CD },
            {Notes.CD, Notes.D },
            {Notes.D, Notes.DE },
            {Notes.DE, Notes.E },
            {Notes.E, Notes.F },
            {Notes.F, Notes.FG },
            {Notes.FG, Notes.G },
            {Notes.G, Notes.GA },
            {Notes.GA, Notes.A },
        };

        private static int GetStartBitmap(Bitmap[] bitmaps)
        {
            List<Color> KnownColors = GetAllColors();

            for (int i = 0; i < bitmaps.Length; i++)
            {
                var bitmap = bitmaps[i];

                for (int y = bitmap.Height - 1; y > bitmap.Height / 2; y--)
                {
                    bool isValid = false;

                    Dictionary<Color, int> colors = GetColorsFromLine(KnownColors, bitmap, y);

                    if (colors.Any(x => x.Key.Name.Contains("White") || x.Key.Name == "Transparent"))
                    {
                        var whiteColors = colors.Where(x => x.Key.Name.Contains("White") || x.Key.Name == "Transparent");

                        var sum = whiteColors.Sum(x => x.Value);

                        if (sum > bitmap.Width / 2)
                        {
                            isValid = true;
                        }
                    }

                    if (isValid)
                        return i;
                }
            }

            throw new Exception("Starting Frame not found.");
        }
        private static int GetYCoordinateFromKeyboard(Bitmap bitmap)
        {
            List<Color> KnownColors = new List<Color>() { Color.White, Color.Black };

            for (int y = 0; y < bitmap.Height; y++)
            {
                Dictionary<Color, int> colors = GetColorsFromLine(KnownColors, bitmap, y);

                if (colors.Any(x => x.Key.Name.Contains("White") || x.Key.Name == "Transparent"))
                {
                    var whiteColors = colors.Where(x => x.Key.Name.Contains("White") || x.Key.Name == "Transparent");
                    var sumWhite = whiteColors.Sum(x => x.Value);

                    var blackColors = colors.Where(x => x.Key.Name.Contains("Black"));
                    var sumBlack = blackColors.Sum(x => x.Value);

                    if (sumWhite > bitmap.Width / 3 && sumBlack > bitmap.Width / 3)
                    {
                        return y + 15;
                    }
                }
            }

            throw new Exception("Keyboard not found");
        }

        private static Dictionary<Color, int> GetColorsFromLine(List<Color> KnownColors, Bitmap bitmap, int y)
        {
            var colors = new Dictionary<Color, int>();
            for (int x = 0; x < bitmap.Width;)
            {
                var pixel = bitmap.GetPixel(x, y);

                var closestColor = KnownColors[ClosestColor(KnownColors, pixel)];

                if (!colors.ContainsKey(closestColor))
                    colors[closestColor] = 0;

                while (x < bitmap.Width)
                {
                    if (bitmap.GetPixel(x, y) != pixel)
                        break;

                    colors[closestColor]++;
                    x++;
                }
            }

            return colors;
        }

        private static Bitmap[] GetFirst500Bitmaps(string filePath)
        {
            using (var vFReader = new VideoFileReader())
            {
                vFReader.Open(filePath);

                var bitmapCount = 500 > vFReader.FrameCount ? vFReader.FrameCount : 500;

                Bitmap[] bitmaps = new Bitmap[bitmapCount];
                for (int i = 0; i < bitmapCount; i++)
                {
                    Bitmap bmpBaseOriginal = vFReader.ReadVideoFrame();
                    bitmaps[i] = bmpBaseOriginal;
                }
                vFReader.Close();

                return bitmaps;
            }
        }

        private IEnumerable<Bitmap> GetBitmapsFrom(string filePath, int startingFrame)
        {
            using (var vFReader = new VideoFileReader())
            {
                vFReader.Open(filePath);

                for (int i = 0; i < vFReader.FrameCount; i++)
                {
                    Bitmap bmpBaseOriginal = vFReader.ReadVideoFrame();

                    if (i >= startingFrame)
                        yield return bmpBaseOriginal;
                }
                vFReader.Close();
            }
        }

        public static int ClosestColor(List<Color> colors, Color target)
        {
            var colorDiffs = colors.Select(n => ColorDiff(n, target)).Min(n => n);
            return colors.FindIndex(n => ColorDiff(n, target) == colorDiffs);
        }

        public static int ColorDiff(Color c1, Color c2)
        {
            return (int)Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
                                   + (c1.G - c2.G) * (c1.G - c2.G)
                                   + (c1.B - c2.B) * (c1.B - c2.B));
        }

        private static List<Color> GetAllColors()
        {
            List<Color> allColors = new List<Color>();

            foreach (PropertyInfo property in typeof(Color).GetProperties())
            {
                if (property.PropertyType == typeof(Color))
                {
                    allColors.Add((Color)property.GetValue(null));
                }
            }

            return allColors;
        }
    }
}
