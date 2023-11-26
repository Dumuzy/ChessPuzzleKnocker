using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AwiUtils;
using ChessSharp;

namespace ChessKnocker
{
    internal class PvfExporter
    {
        public PvfExporter(string pzlFileName, Li<Puzzle> puzzles)
        {
            FileName = pzlFileName;
            Puzzles = puzzles;
        }

        public void Export()
        {
            var lines = new Li<string>("pvffvp104\r\nVersion: 1".SplitToLines());
            lines.Add("");
            int n = 1;
            foreach (var p in Puzzles)
                AddPuzzle(lines, p, ref n);
            var fn = FileName.Replace(".pzl", ".pvf");
            File.WriteAllLines(fn, lines);
        }

        void AddPuzzle(Li<string> lines, Puzzle p, ref int n)
        {
            bool isWithMoveInQuestion = ShallBeWithMoveInQuestion(p);
            PuzzleGame game = new PuzzleGame(p.SRawPuzzle, !isWithMoveInQuestion);

            lines.Add("Begin Fen");
            lines.Add("fen: " + game.ShortFen);
            lines.Add("b1number: " + n++);
            var q = game.WhoseTurn == Player.White ? "1." : "1...";
            if (isWithMoveInQuestion)
                q += ChessUtilities.NotationFromPuzzleMoves(game, "de", 0, 1) + "?";
            lines.Add("question: " + q);
            var rating = GetRatingEx(p, game);
            if (isWithMoveInQuestion)
                rating += GetRatingPlusForMoveInQuestion(rating);
            lines.Add("lichessRating: " + rating);

            if (isWithMoveInQuestion)
                game.MakeMove(game.PuzzleMoves[0], true);
            var mvs = ChessUtilities.NotationFromPuzzleMoves(game, "de", 1, 100);
            lines.Add("answer: " + mvs); ;

            lines.Add("b1v2aktiv: T");
            lines.Add("b1v2done: T");
            lines.Add("type: " + GetPvfType(p.Tags));
            lines.Add("End Fen");
            lines.Add("");
        }

        static bool ShallBeWithMoveInQuestion(Puzzle p) => p.GetHashCode() % 3 == 0;

        static int GetRatingEx(Puzzle p, ChessGame game)
        {
            var iRating = Helper.ToInt(p.Rating);
            if (iRating < 800)
            {
                var mc = ChessUtilities.MaterialCount(game);
                var mcScaled = ((800.0 - iRating) / 200) * mc;
                iRating += (int)mcScaled;
            }
            return iRating;
        }

        static int GetRatingPlusForMoveInQuestion(int iRating)
        {
            var rp = ((2500.0 - iRating) / 13) * 0.5 + 50;
            return (int)rp;
        }

        static string GetPvfType(Li<string> pzlTags)
        {
            string typ = "";
            foreach (var tag in pzlTags)
                if (pzl2pvfTypes.TryGetValue(tag, out typ))
                    break;
            return typ;
        }

        static readonly Dictionary<string, string> pzl2pvfTypes =
            Helper.ToDictionary("kf Kg  qf Dg  rf Tg  bf Lg  nf Sg  pf Bg");


        string FileName;
        Li<Puzzle> Puzzles;
    }
}
