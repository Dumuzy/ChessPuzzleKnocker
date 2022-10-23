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

namespace ChessUI
{
    public class PuzzleSet
    {
        /// <param name="filters">Format= motifA1 motifA2:percentageA; motifB1:percentageB; motifC1 motifC2: percentageC
        /// This means: percentageA of the puzzles will be of motifA1 and motifA2, percentageB of puzzles will be 
        /// of motifB1, the rest will be of motifC1 and motifC2.
        /// </param>
        public PuzzleSet(string name, int numPuzzles, string filters, int lowerRating, int upperRating,
            int startPuzzleNumForCreation)
        {
            this.Name = name;
            this.NumPuzzlesWanted = numPuzzles;
            this.StartPuzzleNumForCreation = startPuzzleNumForCreation;
            this.LowerRating = lowerRating;
            this.UpperRating = upperRating;
            this.Filters = PuzzleFilter.CreateFilters(filters);
            this.Puzzles = new Li<Puzzle>();
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
            this.Puzzles = Puzzles.OrderBy(p => p.NumTried).ThenByDescending(p => p.NumError).ToLi();
        }

        public PuzzleGame NextPuzzle()
        {
            PuzzleGame game = null;
            if (HasPuzzles)
            {
                if (currentPuzzleNum >= Puzzles.Count - 1)
                    RebasePuzzles();
                if (Puzzles[currentPuzzleNum + 1].NumTried != CurrentRound - 1)
                    RebasePuzzles();
                game = new PuzzleGame(Puzzles[++currentPuzzleNum].SLichessPuzzle);
            }
            return game;
        }

        /// <summary> NextRound can happen when currentPuzzleNum is anywhere. </summary>
        public void RebasePuzzles()
        {
            currentPuzzleNum = -1;
            ShufflePuzzles();
            this.Puzzles = Puzzles.OrderBy(p => p.NumTried).ThenByDescending(p => p.NumError).ToLi();
        }

        public string CurrentPuzzleLichessId => Puzzles[currentPuzzleNum].LichessId;

        public string CurrentRating() => Puzzles[currentPuzzleNum].Rating;

        public void CurrentIsFinished() => ++Puzzles[currentPuzzleNum].NumTried;

        public void CurrentIsError() => ++Puzzles[currentPuzzleNum].NumError;

        public bool HasPuzzles => !Puzzles.IsEmpty;

        public int NumTotal => Puzzles.Count;
        /// <summary> Number of puzzles done in the current round. </summary>
        public int NumDone
        {
            get
            {
                var currentRoundIndex = Puzzles.Min(p => p.NumTried);
                var num = NumTotal - Puzzles.Count(p => p.NumTried == currentRoundIndex);
                return num;
            }
        }

        public int CurrentRound => Puzzles.Min(p => p.NumTried) + 1;

        public bool IsRoundFinished(int round) => Puzzles.Count(p => p.NumTried == round) == NumTotal;

        public override string ToString()
        {
            var s = $"PS num = {Puzzles.Count}/{NumPuzzlesWanted}";
            foreach (var fi in Filters)
                s += $"  {fi} num={fi.NumSelected} Enough={fi.HasEnoughOfFilter(NumPuzzlesWanted)}";
            return s;
        }

        private void ShufflePuzzles() => Puzzles = Puzzles.OrderBy(p => rand.Next()).ToLi();

        #region SearchPuzzles
        void SearchPuzzles()
        {
            if (!GetLichessCsvFile())
                return;
            for (int i = 0; Puzzles.Count != NumPuzzlesWanted && i < 20; ++i)
            {
                foreach (string line in File.ReadLines(LichessCsvFileName))
                    if (IsAllowed(line))
                    {
                        Puzzles.Add(new Puzzle(line));
                        if (Puzzles.Count >= NumPuzzlesWanted)
                            break;
                    }
                // for debugging
                foreach (var f in Filters)
                {
                    var num = Puzzles.Where(p => f.IsMatching(p.SLichessPuzzle.Split(',').ToLiro())).Count();
                    Debug.WriteLine($"Filter={f} Num={num}");
                }
                if (Puzzles.Count < NumPuzzlesWanted)
                {
                    if (i % 2 == 0)
                        this.LowerRating -= 50;
                    else
                        this.UpperRating += 50;
                }
            }
        }

        bool IsAllowed(string line)
        {
            var lineParts = line.Split(',').ToLiro();
            if (IsAllowedByRatingEtc(lineParts))
                foreach (var filter in Filters)
                    if (!filter.HasEnoughOfFilter(NumPuzzlesWanted) && filter.IsMatching(lineParts))
                    {
                        filter.IncNumSelected();
                        return true;
                    }
            return false;
        }

        bool IsAllowedByRatingEtc(Liro<string> lineParts)
        {
            var rating = Helper.ToInt(lineParts[3]);
            bool ok = rating >= LowerRating && rating <= UpperRating;
            ok &= Helper.ToInt(lineParts[5]) > 50;  // Popularity
            ok &= Helper.ToInt(lineParts[6]) > 20;  // NbPlays
            return ok;
        }

        public const string LichessCsvFileName = "lichess_db_puzzle.csv";
        public static string LichessCsvDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

        bool GetLichessCsvFile()
        {
            DownloadLichessCsvIfNeeded();
            return File.Exists("LichessCsvFileName");
        }

        private void DownloadLichessCsvIfNeeded()
        {
            if (!File.Exists(PuzzleSet.LichessCsvFileName))
            {
                MessageBox.Show($@"
                You must download 
                    https://database.lichess.org/lichess_db_puzzle.csv.bz2 
                now and extract it to 
                    {LichessCsvDirectory}. 
                Press OK when done.",
                    "Attention", MessageBoxButtons.OKCancel);
            }
        }
        #endregion SearchPuzzles

        #region Read and Write
        public void ReadSet()
        {
            var lines = File.ReadAllLines(FileName).ToLiro();
            foreach (var line in lines)
            {
                if (line.StartsWith("TT="))
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
".SplitToLines('\n', '\r').ToLi();
            return lines;
        }

        Li<string> FiltersToLines() => Filters.Select(f => f.ToDbString()).ToLi();

        #endregion Read and Write

        Li<Puzzle> Puzzles;

        public readonly Li<PuzzleFilter> Filters;
        public int NumPuzzlesWanted, StartPuzzleNumForCreation, LowerRating, UpperRating;

        int currentPuzzleNum = -1;
        readonly string Name;
        string FileName => SFileName(Name);

        static string SFileName(string name) => Helper.MakeFilenameSafe(name) + ".pzl";
        static Random rand = new Random();
    }
}


//public string Name { get; private set; }
//int numPuzzlesWanted, startPuzzleNumForCreation, lowerRating, upperRating;
//int currentPuzzleNum;
//Li<PuzzleFilter> filters;
//Li<Puzzle> Puzzles;



internal class Puzzle
{
    public Puzzle(string sLichessPuzzle)
    {
        this.SLichessPuzzle = sLichessPuzzle;
    }

    public Puzzle(string dbString, int dummy) => Read(dbString);

    public string SLichessPuzzle { get; private set; }

    public int NumTried { get; set; }

    public int NumError { get; set; }

    public string LichessId => SLichessPuzzle.Split(',', 2)[0];

    public string Rating => SLichessPuzzle.Split(',')[3];

    public override string ToString() => $"TT={NumTried} E={NumError} P={SLichessPuzzle}";

    public string ToDbString() => $"TT={NumTried};E={NumError};P={SLichessPuzzle}";

    public void Read(string dbString)
    {
        var parts = dbString.Split(';');
        foreach (var p in parts)
        {
            var parts2 = p.Split('=', 2).ToLiro();
            switch (parts2[0])
            {
                case "TT": NumTried = Helper.ToInt(parts2[1]); break;
                case "E": NumError = Helper.ToInt(parts2[1]); break;
                case "P": SLichessPuzzle = parts2[1]; break;
            }
        }
    }
}


public class PuzzleFilter
{
    static public Li<PuzzleFilter> CreateFilters(string multiCreator)
    {
        var creators = multiCreator.SplitToWords(";".ToCharArray()).ToLi();
        var filters = new Li<PuzzleFilter>();
        foreach (var c in creators)
            filters.Add(new PuzzleFilter(c));

        // If all filters have got percentage 0 somebody did not select any percentages. Set all to 10. 
        if (filters.Count(f => f.Percentage == 0) == filters.Count)
            foreach (var f in filters)
                f.Percentage = 10;

        filters.RemoveAll(f => f.Motifs.Count == 0);
        filters.RemoveAll(f => f.Percentage == 0);

        // Set realPercentage so that Sum(realPercentage) == 100.
        var totalPercentages = filters.Sum(f => f.Percentage);
        foreach (var f in filters)
            f.realPercentage = 100.0 * f.Percentage / totalPercentages;

        return filters;
    }

    /// <summary> Format of the creator string= motifA1 motifA2 ...:percentageA </summary>
    public PuzzleFilter(string creator)
    {
        if (creator.StartsWith("FIL="))
            creator = creator.Substring(4);
        var parts = creator.Trim().Split(':').ToLiro();
        Motifs = parts[0].SplitToWords().Where(m => m != "--").ToLiro();
        Percentage = Helper.ToInt(parts[1]);
    }

    public bool IsMatching(Liro<string> lichessLineParts)
    {
        var motifsInLine = lichessLineParts[7].SplitToWords();
        for (int i = 0; i < Motifs.Count; ++i)
            if (!motifsInLine.Contains(Motifs[i]))
                return false;
        return true;
    }

    public void IncNumSelected() => ++NumSelected;

    public int NumSelected { get; private set; }

    public bool HasEnoughOfFilter(int numTotal) => numTotal * realPercentage / 100 <= NumSelected;

    public override string ToString() => $"{Motifs}:{Percentage}";

    public string ToDbString() => $"FIL={string.Join(' ', Motifs)}:{Percentage}";

    public readonly Liro<string> Motifs;
    public int Percentage { get; private set; }
    private double realPercentage;
}

