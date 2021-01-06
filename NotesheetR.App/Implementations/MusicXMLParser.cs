using MusicXmlSchema;
using NotesheetR.App.Abstractions;
using NotesheetR.Core.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NotesheetR.App.Implementations
{
    public class MusicXMLParser : IMusicXMLParser
    {
        public string KeyboardTilesToMusicXML(List<KeyboardTile> keyboardTiles, int frameCount)
        {
            // Downloaded youtube videos always have a fps of 23.98
            int videoLength = frameCount / 24;

            int measureCount = videoLength / 2;

            ScorePartwisePartMeasure[] measures = new ScorePartwisePartMeasure[measureCount];
            for (int i = 0; i < measureCount; i++)
            {
                measures[i] = new ScorePartwisePartMeasure();
                measures[i].Number = i.ToString();
            }

            Dictionary<Note, int> NoteFrameDict = new Dictionary<Note, int>();

            foreach (var keyboardTile in keyboardTiles)
            {
                foreach (var press in keyboardTile.TilePresses)
                {
                    var startFrame = press.Item1;
                    var endFrame = press.Item2;

                    Pitch keyboardPitch = new Pitch();
                    keyboardPitch.Octave = keyboardTile.Octave.ToString();
                    keyboardPitch.Step = NotesToMusicXMLStep[keyboardTile.Note];
                    keyboardPitch.Alter = NoteToAlter(keyboardTile.Note);
                    double startSec = startFrame / 24;
                    int measureIndex = (int)(startSec / 2);
                    var measure = measures[measureIndex];
                    Note pressNote = new Note();
                    pressNote.Pitch = keyboardPitch;
                    pressNote.Type = DurationToNodeType(press);
                    measure.Note.Add(pressNote);
                    NoteFrameDict.Add(pressNote, startFrame);
                }
            }

            for (int i = 0; i < measures.Length; i++)
            {
                var measure = measures[i];

                Note[] notes = new Note[measure.Note.Count];
                var notesOrdered = notes.OrderBy(x => NoteFrameDict[x]).Select(x => x);

                measure.Note.CopyTo(notes, 0);

                while (measure.Note.Count > 0)
                    measure.Note.RemoveAt(0);

                foreach (var note in notesOrdered)
                    measure.Note.Add(note);

                var lastFrame = 0;

                foreach (var note in measure.Note)
                {
                    var startFrame = NoteFrameDict[note];

                    if (lastFrame == startFrame)
                        note.Chord = new Empty();

                    lastFrame = startFrame;
                }
            }

            ScorePartwise spw = new ScorePartwise();
            PartList partList = new PartList();
            ScorePart scorePart = new ScorePart();
            PartName partName = new PartName();
            partName.Value = "NotesheetR";
            scorePart.PartName = partName;
            scorePart.Id = "1";
            partList.ScorePart = scorePart;
            ScorePartwisePart part = new ScorePartwisePart();
            part.Id = "1";

            foreach (var measure in measures)
                part.Measure.Add(measure);

            spw.Part.Add(part);
            spw.PartList = partList;
            return ParseToXml(spw);
        }

        private Dictionary<Notes, Step> NotesToMusicXMLStep = new Dictionary<Notes, Step>()
        {
            { Notes.A, Step.A },
            { Notes.AB, Step.A },
            { Notes.B, Step.B },
            { Notes.C, Step.C },
            { Notes.CD, Step.C },
            { Notes.D, Step.D },
            { Notes.DE, Step.D },
            { Notes.E, Step.E },
            { Notes.F, Step.F },
            { Notes.FG, Step.F },
            { Notes.G, Step.G },
            { Notes.GA, Step.G },
        };

        private int NoteToAlter(Notes note)
        {
            if (note == Notes.AB
                || note == Notes.CD
                || note == Notes.DE
                || note == Notes.FG
                || note == Notes.GA)
                return 1;
            return 0;
        }

        private NoteType DurationToNodeType(Tuple<int, int> startEnd)
        {
            NoteType noteType = new NoteType();
            var frameDuration = startEnd.Item2 - startEnd.Item1;
            double ms = ((double)frameDuration / (double)24) * 1000;
            if (ms > 1500)
                noteType.Value = NoteTypeValue.Whole;
            else if (ms > 750)
                noteType.Value = NoteTypeValue.Half;
            else if (ms > 375)
                noteType.Value = NoteTypeValue.Quarter;
            else if (ms > 187)
                noteType.Value = NoteTypeValue.Eighth;
            else
                noteType.Value = NoteTypeValue.Item16Th;
            return noteType;
        }

        private string ParseToXml(ScorePartwise spw)
        {
            StringWriter tw = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(MusicXmlSchema.ScorePartwise));
            serializer.Serialize(tw, spw);
            var musicXml = tw.ToString();
            musicXml = ReplaceHeader(musicXml);
            return musicXml;
        }

        private string ReplaceHeader(string oldXml)
        {
            var xmlHeader = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<!DOCTYPE score-partwise PUBLIC
    ""-//Recordare//DTD MusicXML 3.1 Partwise//EN""
    ""http://www.musicxml.org/dtds/partwise.dtd"">
<score-partwise version=""3.1"">";
            var xmlHeaderToReplace = @"<?xml version=""1.0"" encoding=""utf-16""?>
<score-partwise xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">";
            return oldXml.Replace(xmlHeaderToReplace, xmlHeader);
        }
    }
}
