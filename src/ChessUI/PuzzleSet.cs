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

namespace ChessUI
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
            this.Puzzles = Puzzles.OrderBy(p => p.NumTried).ThenBy(p => p.NumCorrect).ToLi();
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
                game = new PuzzleGame(Puzzles[++currentPuzzleNum].SLichessPuzzle);
            }
            return game;
        }

        /// <summary> NextRound can happen when currentPuzzleNum is anywhere. </summary>
        public void RebasePuzzles()
        {
            currentPuzzleNum = -1;
            ShufflePuzzles();
            this.Puzzles = Puzzles.OrderBy(p => p.NumCorrect).ThenByDescending(p => p.NumTried).ToLi();
        }

        public string CurrentLichessId => Puzzles[currentPuzzleNum].LichessId;

        public string CurrentRating => Puzzles[currentPuzzleNum].Rating;

        public void CurrentIsCorrect() => ++Puzzles[currentPuzzleNum].NumCorrect;

        public void CurrentIsTried() => ++Puzzles[currentPuzzleNum].NumTried;

        public bool HasPuzzles => !Puzzles.IsEmpty;

        public int NumTotal => Puzzles.Count;

        public int CurrentPosition => currentPuzzleNum;

        public bool IsRoundFinished(int round) => NumCorrect(round) == NumTotal;

        public int NumCorrect(int round) => Puzzles.Count(p => p.NumCorrect >= round);

        public int CurrentRound { get; private set; } = 1;

        public void IncCurrentRound() => ++CurrentRound;

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
            var puzzles = new Dictionary<string, Puzzle>();
            var startPuzzleNum = StartPuzzleNumForCreation;
            var fileName = DbFileName;
            if (fileName == null)
                return;
            for (int i = 0; puzzles.Count != NumPuzzlesWanted && i < 3; ++i)
            {
                int j = 0;
                foreach (string line in File.ReadLines(fileName))
                    if (j++ >= startPuzzleNum)
                        if (IsAllowed(line, puzzles))
                        {
                            var pu = new Puzzle(line);
                            puzzles.Add(pu.LichessId, pu);
                            if (puzzles.Count >= NumPuzzlesWanted)
                                break;
                        }
                // for debugging
                foreach (var f in Filters)
                {
                    var num = puzzles.Values.Where(p => f.IsMatching(p.SLichessPuzzle.Split(',').ToLiro())).Count();
                    Debug.WriteLine($"Filter={f} Num={num}");
                }
                if (puzzles.Count < NumPuzzlesWanted)
                {
                    if (i % 2 == 0 && LowerRating >= 550)
                        LowerRating -= 50;
                    else if (i % 2 == 1 && UpperRating <= 3150)
                        UpperRating += 50;
                    startPuzzleNum = 0;
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

        static string SFileName(string name) => Helper.MakeFilenameSafe(name) + ".pzl";
        static Random rand = new Random();
        #endregion Fields  
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

    public int NumCorrect { get; set; }

    public int NumTried { get; set; }

    public string LichessId => SLichessPuzzle.Split(',', 2)[0];

    public string Rating => SLichessPuzzle.Split(',')[3];

    public override string ToString() => $"TT={NumTried} C={NumCorrect} E= P={SLichessPuzzle}";

    public string ToDbString() => $"TT={NumTried};C={NumCorrect};P={SLichessPuzzle}";

    public void Read(string dbString)
    {
        var parts = dbString.Split(';');
        foreach (var p in parts)
        {
            var parts2 = p.Split('=', 2).ToLiro();
            switch (parts2[0])
            {
                case "C": NumCorrect = Helper.ToInt(parts2[1]); break;
                case "TT": NumTried = Helper.ToInt(parts2[1]); break;
                case "P": SLichessPuzzle = parts2[1]; break;
            }
        }
    }

    public override int GetHashCode() => (LichessId + Rating).GetHashCode();
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

    public override string ToString() => $"{Motifs}:{Percentage}%:{NumSelected}#";

    public string ToDbString() => $"FIL={string.Join(' ', Motifs)}:{Percentage}";

    public readonly Liro<string> Motifs;
    public int Percentage { get; private set; }
    private double realPercentage;
}

