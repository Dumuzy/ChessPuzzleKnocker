using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Xml.Linq;
using ChessSharp;
using AwiUtils;
using static System.Windows.Forms.LinkLabel;
using System.Windows.Forms;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using ChessSharp.SquareData;
using ChessKnocker;

namespace PuzzlePecker
{
    public class PuzzleSet
    {
        /// <param name="filters">Format= motifA1 motifA2:percentageA; motifB1:percentageB; motifC1 motifC2: percentageC
        /// This means: percentageA of the puzzles will be of motifA1 and motifA2, percentageB of puzzles will be 
        /// of motifB1, the rest will be of motifC1 and motifC2.
        /// </param>
        public PuzzleSet(string name, int numPuzzles, string filters, int lowerRating, int upperRating,
            int startPuzzleNumForCreation, Func<string> getPuzzleDbFileFunc)
        {
            this.Name = name;
            this.NumPuzzlesWanted = numPuzzles;
            this.StartPuzzleNumForCreation = startPuzzleNumForCreation;
            this.LowerRating = lowerRating;
            this.UpperRating = upperRating;
            this.Filters = PuzzleFilter.CreateFilters(filters);
            this.Puzzles = new Li<Puzzle>();
            this.DbFileName = getPuzzleDbFileFunc();
            SearchPuzzles();
        }

        static public PuzzleSet ReadPuzzleSet(string name) => File.Exists(SFileName(name)) ? new PuzzleSet(name) : null;

        private PuzzleSet(string name)
        {
            this.Name = name;
            this.Puzzles = new Li<Puzzle>();
            this.Filters = new Li<PuzzleFilter>();
            ReadSet();
            ShufflePuzzles();
            this.Puzzles = Puzzles.OrderBy(p => p.NumTriedInRound).ThenBy(p => p.NumCorrect).ToLi();
        }

        public PuzzleGame NextPuzzle()
        {
            PuzzleGame game = null;
            if (HasPuzzles)
            {
                if (currentPuzzleNum >= Puzzles.Count - 1)
                    RebasePuzzles();
                if (Puzzles[currentPuzzleNum + 1].NumCorrect != CurrentRound - 1)
                    RebasePuzzles();
                game = new PuzzleGame(Puzzles[++currentPuzzleNum].SRawPuzzle, true);
            }
            return game;
        }

        /// <summary> NextRound can happen when currentPuzzleNum is anywhere. </summary>
        public void RebasePuzzles()
        {
            currentPuzzleNum = -1;
            ShufflePuzzles();
            this.Puzzles = Puzzles.OrderBy(p => p.NumCorrect).ThenByDescending(p => p.NumTriedInRound).ToLi();
        }

        public string CurrentLichessId => Puzzles[currentPuzzleNum].LichessId;

        public string CurrentRating => Puzzles[currentPuzzleNum].Rating;

        public void CurrentIsCorrect() => ++Puzzles[currentPuzzleNum].NumCorrect;

        public void CurrentIsTried() => Puzzles[currentPuzzleNum].IncNumTried();

        public Li<string> CurrentTags { get { return Puzzles[currentPuzzleNum].Tags; }
            set { Puzzles[currentPuzzleNum].Tags = value; } }

        public bool HasPuzzles => !Puzzles.IsEmpty;

        public int NumTotal => Puzzles.Count;

        public int CurrentPosition => currentPuzzleNum;

        public bool IsRoundFinished(int round) => NumCorrect(round) == NumTotal;

        public int NumCorrect(int round) => Puzzles.Count(p => p.NumCorrect >= round);

        public int NumErrors(int round) => Puzzles.Count(p => p.NumCorrect < round && p.NumTriedInRound > 0);

        public int NumUntried(int round) => Puzzles.Count - NumErrors(round) - NumCorrect(round);

        public int CurrentRound { get; private set; } = 1;

        public void IncCurrentRound()
        {
            ++CurrentRound;
            foreach (var p in Puzzles)
                p.ZeroNumTriedInRound();
        }

        public override string ToString()
        {
            var s = $"PS num = {Puzzles.Count}/{NumPuzzlesWanted}";
            foreach (var fi in Filters)
                s += $"  {fi} num={fi.NumSelected} Enough={fi.HasEnoughOfFilter(NumPuzzlesWanted)}";
            return s;
        }

        public const int StartPuzzleNumRandom = -5;

        public const string FileExt = ".pzl";

        private void ShufflePuzzles() => Puzzles = Puzzles.OrderBy(p => rand.Next()).ToLi();

        #region SearchPuzzles
        void SearchPuzzles()
        {
            var puzzles = new Dictionary<string, Puzzle>();
            var startPuzzleNum = StartPuzzleNumForCreation;
            var fileName = DbFileName;
            if (fileName == null)
                return;
            double startPuzzleRatio = rand.NextDouble(),
                fileSize = new System.IO.FileInfo(fileName).Length, readSize = 0;
            for (int i = 0; puzzles.Count != NumPuzzlesWanted && i < 3; ++i)
            {
                int j = 0;
                double minReadSize = startPuzzleRatio * fileSize;
                foreach (string line in File.ReadLines(fileName))
                {
                    if ((startPuzzleNum != StartPuzzleNumRandom && j++ >= startPuzzleNum) ||
                         (startPuzzleNum == StartPuzzleNumRandom && readSize >= minReadSize))
                        if (IsAllowed(line, puzzles))
                        {
                            var pu = new Puzzle(line);
                            puzzles.Add(pu.LichessId, pu);
                            if (puzzles.Count >= NumPuzzlesWanted)
                                break;
                        }
                    readSize += line.Length;
                }
                // for debugging
                foreach (var f in Filters)
                {
                    var num = puzzles.Values.Where(p => f.IsMatching(p.SRawPuzzle.Split(',').ToLiro())).Count();
                    Debug.WriteLine($"Filter={f} Num={num}");
                }
                if (puzzles.Count < NumPuzzlesWanted)
                {
                    if (startPuzzleNum == StartPuzzleNumRandom)
                    {
                        startPuzzleRatio = i == 0 ? startPuzzleRatio / 4 : rand.NextDouble();
                        readSize = 0;
                    }
                    else
                        startPuzzleNum = 0;

                    if (i > 0)
                    {
                        if (i % 2 == 0 && UpperRating <= 3150)
                            UpperRating += 50;
                        else if (i % 2 == 1 && LowerRating >= 550)
                            LowerRating -= 50;
                    }
                }
            }
            this.Puzzles = puzzles.Values.ToLi();
        }

        bool IsAllowed(string line, Dictionary<string, Puzzle> puzzles)
        {
            if (line.StartsWith("#")) // comment line
                return false;
            var lineParts = line.Split(',').ToLiro();
            if (IsAllowedByRatingEtc(lineParts))
                foreach (var filter in Filters)
                    if (!filter.HasEnoughOfFilter(NumPuzzlesWanted) && filter.IsMatching(lineParts)
                        && !puzzles.ContainsKey(lineParts[0]))
                    {
                        filter.IncNumSelected();
                        return true;
                    }
            return false;
        }

        bool IsAllowedByRatingEtc(IList<string> lineParts)
        {
            var rating = Helper.ToInt(lineParts[3]);
            bool ok = rating >= LowerRating && rating <= UpperRating;
            ok &= IsAllowedByPopularityAndNbPlays(lineParts);
            return ok;
        }

        static public bool IsAllowedByPopularityAndNbPlays(IList<string> lineParts)
        {
            var popularity = Helper.ToInt(lineParts[5]);
            var nbPlays = Helper.ToInt(lineParts[6]);
            bool ok = popularity > 50 && nbPlays > 20;
            if (!ok)
                ok = popularity == 0 && nbPlays == 0 && lineParts[8] == "";   // it's a lichess_part line. 
            return ok;
        }
        #endregion SearchPuzzles

        #region Read and Write
        public void ReadSet()
        {
            var lines = File.ReadAllLines(FileName).ToLiro();
            foreach (var line in lines)
            {
                if (line.StartsWith("TT=") || line.StartsWith("TR="))
                    Puzzles.Add(new Puzzle(line, 0));
                else if (line.StartsWith("FIL="))
                    Filters.Add(new PuzzleFilter(line));
                else
                {
                    var pp = line.Split('=', 2).ToLiro();
                    switch (pp[0])
                    {
                        // case "currentPuzzleNum": currentPuzzleNum = Helper.ToInt(pp[1]); break;
                        case "numPuzzlesWanted": NumPuzzlesWanted = Helper.ToInt(pp[1]); break;
                        case "startPuzzleNumForCreation": StartPuzzleNumForCreation = Helper.ToInt(pp[1]); break;
                        case "lowerRating": LowerRating = Helper.ToInt(pp[1]); break;
                        case "upperRating": UpperRating = Helper.ToInt(pp[1]); break;
                        case "currentRound": CurrentRound = Helper.ToInt(pp[1]); break;
                            // date ={ DateTime.Now}
                    }
                }
            }
        }

        public void WriteSet()
        {
            var lines = HeaderToLines();
            lines.Add("[Filters]");
            lines.AddRange(FiltersToLines());
            lines.Add("[Puzzles]");
            lines.AddRange(Puzzles.Select(p => p.ToDbString()));
            File.WriteAllLines(FileName, lines);
        }

        Li<string> HeaderToLines()
        {
            Li<string> lines = $@"
[Header]
version=1.0
numPuzzlesWanted={NumPuzzlesWanted}
startPuzzleNumForCreation={StartPuzzleNumForCreation}
lowerRating={LowerRating}
upperRating={UpperRating}
date={DateTime.Now}
currentRound={CurrentRound}
".SplitToLines('\n', '\r').ToLi();
            return lines;
        }

        Li<string> FiltersToLines() => Filters.Select(f => f.ToDbString()).ToLi();

        public void ExportSet()
        {
            Puzzles.Clear();
            ReadSet();
            new PvfExporter(FileName, Puzzles).Export();
        }
        #endregion Read and Write
        #region Fields
        Li<Puzzle> Puzzles;

        public readonly Li<PuzzleFilter> Filters;
        public int LowerRating { get; private set; }
        public int UpperRating { get; private set; }
        public int NumPuzzlesWanted { get; private set; }
        public int StartPuzzleNumForCreation { get; private set; }

        int currentPuzzleNum = -1;
        readonly string Name, DbFileName;
        string FileName => SFileName(Name);

        static string SFileName(string name) => Helper.MakeFilenameSafe(name) + FileExt;
        static Random rand = new Random();
        #endregion Fields  
    }
}

internal class Puzzle
{
    public Puzzle(string sLichessPuzzle)
    {
        this.SRawPuzzle = sLichessPuzzle;
    }

    public Puzzle(string dbString, int dummy) => Read(dbString);

    public string SRawPuzzle { get; private set; }

    public int NumCorrect { get; set; }

    public Li<string> Tags
    {
        get { return SPuzzle.Split(',')[7].SplitToWords().ToLi(); }
        set
        {
            var currSTags = SPuzzle.Split(',')[7];
            var sTags = string.Join(" ", value);
            if (SPuzzle == SRawPuzzle)
                SRawPuzzle = SRawPuzzle.Replace(currSTags, sTags);
            else throw new Exception("SPuzzle != SRawPuzzle");
        }
    }

    public void IncNumTried() { ++NumTriedInRound; ++NumTriedTotal; }

    public void ZeroNumTriedInRound() => NumTriedInRound = 0;

    public int NumTriedInRound { get; private set; }

    public int NumTriedTotal { get; private set; }

    public string LichessId => SPuzzle.Split(',', 2)[0];

    public string Rating => SPuzzle.Split(',')[3];

    public override string ToString() => $"TT={NumTriedInRound} C={NumCorrect} E= P={SRawPuzzle}";

    public string ToDbString() => $"TR={NumTriedInRound};C={NumCorrect};TT={NumTriedTotal};P={SRawPuzzle}";

    private string SPuzzle => SRawPuzzle.Split(',').Length >= 4 ?  SRawPuzzle : ",,,,,";

    public void Read(string dbString)
    {
        var parts = dbString.Split(';');
        foreach (var p in parts)
        {
            var parts2 = p.Split('=', 2).ToLiro();
            switch (parts2[0])
            {
                case "C": NumCorrect = Helper.ToInt(parts2[1]); break;
                case "TT": NumTriedTotal = Helper.ToInt(parts2[1]); break;
                case "TR": NumTriedInRound = Helper.ToInt(parts2[1]); break;
                case "P": SRawPuzzle = parts2[1]; break;
            }
        }
    }

    public override int GetHashCode() => SRawPuzzle.GetHashCode();
}

