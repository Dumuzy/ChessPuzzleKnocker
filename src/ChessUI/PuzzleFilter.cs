using System;
using System.Linq;
using ChessSharp;
using AwiUtils;

namespace PuzzlePecker
{
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

}