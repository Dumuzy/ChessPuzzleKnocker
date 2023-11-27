using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using AwiUtils;
using ChessSharp.SquareData;
using PuzzlePecker;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.AxHost;

namespace ChessKnocker
{
    struct LevelCoords
    {
        public LevelCoords(int brett, string square)
        {
            this.Brett = brett;
            this.Square = Square.Create(square);
        }
        public int Brett { get; private set; }
        public Square Square { get; private set; }
        public override string ToString() => "" + Brett + Square;
    }

    internal class MssBrett
    {
        public MssBrett(int brettNum, string textToCreate)
        {
            this.brettNum = brettNum;
            levels = new Dictionary<LevelCoords, IMssLevel>();
            CreateLevels(textToCreate);
        }


        /// <summary>
        /// Hier steckt eine Sprache zur Levelerzeugung dahinter. Defaults in [].
        /// createLevel typ=simplestPzl source=quellfile tags=nf   numaufg=10     startp=1   lr=500      dr=100       
        /// Befehl      Arg1=Typ                  tags      #Aufgaben[10]  startp[1]  lowerRating upper=lr+dr[100]   
        ///          pkt=10          square=a1   name=Springergabel$square     target=YXZ   ; 
        ///          Punkte/Aufg.    Startfeld   Square wird automat. addiert    Ende-Marker
        ///          
        /// createLevelRow  ...wie bei createLevel...  
        ///     ddlr=50           ddpkt=2                    numlevelsinrow=8         bonus=100        ; 
        ///     DeltaLowerRating  DeltaPunkte/Aufg. [pkt/5]  Anzahl Levels[8]    Punkte/Reihe  Ende-Marker 
        /// 
        /// </summary>
        /// <param name="linesToCreate"></param>
        private void CreateLevels(string textToCreate)
        {
            var cmds = SplitToCommands(textToCreate);
            foreach (var cmd in cmds)
                RunCommand(cmd);
        }

        private Li<string> SplitToCommands(string text)
        {
            var lines = text.SplitToLines().Where(li => !li.StartsWith("//"));
            text = string.Join('\n', lines);
            text = text.Replace('\n', ' ');
            var ll = text.SplitToLines(';').ToLi();
            return ll;
        }

        private void RunCommand(string cmd)
        {
            var parts = cmd.Split(' ', 2).Select(s => s.Trim()).ToLi();
            cmd = parts[0];
            if (parts.Count == 2)
            {
                var args = new MssDiss(parts[1]);
                switch (cmd)
                {
                    case "createLevel": CreateLevel(args); break;
                    case "createLevelRow": CreateLevelRow(args); break;
                }
            }
            else
                LogError("parts != 2");
        }

        private void CreateLevel(Diss args)
        {
            IMssLevel level = null;
            if (args["typ"] == "simplestPzl")
                level = CreateLevelSimplestPzl(args);
            else
                LogNotImplemented(args["typ"]);
            if (level != null)
            {
                var c = new LevelCoords(brettNum, args["square"]);
                levels[c] = level;
                level.Write(GetFullTargetPathForLevel(args));
            }
            else LogError("Couldn't create level.");
        }

        private IMssLevel CreateLevelSimplestPzl(Diss args)
        {
            IMssLevel level = null;
            var fn = args["source"];
            var filter = new MssPuzzleFilter(args);
            var startp = args.Int("startp");
            if (Path.GetExtension(fn) == PuzzleSet.FileExt)
            {
                fn = GetFullPath(fn);
                var ps = PuzzleSet.ReadFromPath(fn, false);
                var myPuzzles = new Li<Puzzle>();
                for (int i = startp; i < ps.Puzzles.Count; ++i)
                {
                    var p = ps.Puzzles[i];
                    if (filter.IsAllowed(p))
                        myPuzzles.Add(p);
                    if (myPuzzles.Count >= filter.MaxNumPuzzles)
                        break;
                }
                if (myPuzzles.Count < filter.MaxNumPuzzles)
                {
                    // Fang von vorn an. 
                    for (int i = 0; i < ps.Puzzles.Count; ++i)
                    {
                        var p = ps.Puzzles[i];
                        if (filter.IsAllowed(p))
                            myPuzzles.Add(p);
                        if (myPuzzles.Count >= filter.MaxNumPuzzles)
                            break;
                    }
                }

                var nps = new PuzzleSet(args["name"], filter.MaxNumPuzzles,
                    string.Join(' ', filter.AllowedTags.Select(f => f + ":10")),
                    filter.LowerRating, filter.UpperRating, startp, fn, myPuzzles);

                level = new MssPzlLevel(args, nps);
            }
            else if (fn == "licsv")
            {
                var ps = new PuzzleSet(args["name"], filter.MaxNumPuzzles,
                        string.Join(' ', filter.AllowedTags.Select(f => f + ":10")),
                        filter.LowerRating, filter.UpperRating, startp,
                        PuzzleDbProvider.GetLichessCsvFile);
                level = new MssPzlLevel(args, ps);
            }
            else
                LogNotImplemented(fn);
            return level;
        }

        const string targetBaseDir = ".";

        private string GetFullPath(string fileName) => Path.Combine(targetBaseDir, fileName);

        private string GetFullTargetPathForLevel(Diss args)
            => Path.Combine(targetBaseDir, "B" + brettNum, args["target"] + ".lvl");



        private void CreateLevelRow(MssDiss args)
        {
            if (args["typ"] == "simplestPzl")
                CreateLevelRowSimplestPzl(args);
            else
                LogNotImplemented(args["typ"]);
        }

        private void CreateLevelRowSimplestPzl(MssDiss args)
        {
            var sq = args["square"];
            int startRank = Helper.ToInt(sq[1].ToString());
            int endRank = startRank + args.Int("numlevelsinrow") - 1;
            for (int i = startRank; i <= endRank; i++)
            {
                args.Add("square", sq[0].ToString() + i.ToString());
                CreateLevel(args);
                args.IncVal("pkt");
                args.IncVal("startp");
                args.IncVal("lr");
            }
        }

        static public void TestCreate()
        {
            var s = @"
            //createLevelRow    typ=simplestPzl square=B1 tags=mateIn1   numaufg=10   
            //                startp=1 ddstartp=1000  lr=500 ddlr=100     dr=100    version=1.0   
            //                pkt=10  ddpkt=4     name=Matt§in§1§$square target=$square; 

            //createLevelRow typ=simplestPzl square=C1 tags=bf   source=fork-500-1500.pzl numaufg=10   
            //                startp=1 ddstartp=100   lr=500 ddlr=50    dr=100    version=1.0   
            //                pkt=10 ddpkt=2
            //                name=Läufergabel§$square target=$square; 

            //createLevelRow typ=simplestPzl square=D1 tags=rf  source=fork-500-1500.pzl numaufg=10   
            //                startp=1 ddstartp=100   lr=500 ddlr=50    dr=100    version=1.0   
            //                pkt=10 ddpkt=2 name=Turmgabel§$square; 

            //createLevelRow  typ=simplestPzl square=E1 tags=pin   numaufg=10   
            //                startp=1 ddstartp=1000  lr=500 ddlr=50     dr=100    version=1.0   
            //                pkt=15  ddpkt=5   name=Fesselung§$square; 

            //createLevelRow  typ=simplestPzl square=F1 tags=skewer   numaufg=10   
            //                startp=1 ddstartp=1000  lr=500 ddlr=50     dr=100    version=1.0   
            //                pkt=15  ddpkt=5   name=Spieß§$square; 

            //createLevelRow typ=simplestPzl square=G1 tags=qf  source=fork-500-1500.pzl numaufg=5   
            //               startp=1 ddstartp=100   lr=700 ddlr=100    dr=200    version=1.0   
            //               pkt=25 ddpkt=5 name=Damengabel§$square; 

            //createLevelRow  typ=simplestPzl square=H1 tags=fork   numaufg=10   
            //                startp=5000 ddstartp=200  lr=500 ddlr=50     dr=100    version=1.0   
            //                pkt=25  ddpkt=10   name=Gabel§$square; 
                    ";
            var m = new MssBrett(1, s);

        }

        private void LogNotImplemented(string arg) => LogError("NotImplemented: " + arg);

        private void LogError(string msg)
        {
            Debug.WriteLine("MssBrett Error:" + msg);
        }

        int brettNum;
        readonly Dictionary<LevelCoords, IMssLevel> levels;
    }

    class MssPuzzleFilter
    {
        public MssPuzzleFilter(Diss args)
        {
            AllowedTags = args["tags"].Split(',').ToLi();
            LowerRating = args.Int("lr");
            UpperRating = LowerRating + args.Int("dr");
            MaxNumPuzzles = args.Int("numaufg");
        }

        public bool IsAllowed(Puzzle p)
        {
            var r = Helper.ToInt(p.Rating);
            bool ok = r <= UpperRating && r >= LowerRating;
            if (ok)
                ok = p.Tags.Intersect(AllowedTags).Any();
            return ok;
        }

        public override string ToString() => $"{MaxNumPuzzles}of{AllowedTags}@{LowerRating}-{UpperRating}";

        readonly public Li<string> AllowedTags;
        readonly public int LowerRating, UpperRating, MaxNumPuzzles;
    }

}
