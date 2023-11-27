using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks.Dataflow;
using AwiUtils;
using PuzzlePecker;

namespace ChessKnocker
{


    interface IMssLevel
    {
        void Write(string fullPath);
        void Read(string fullPath);
    }

    abstract class MssLevel : IMssLevel
    {
        internal MssLevel(Diss args)
        {
            this.levelInfo = args;
            if (levelInfo == null)
                levelInfo = new MssDiss();
        }

        public void Write(string fullPath)
        {
            var lines = levelInfo.ToLines(lvliPrefix);
            lines.AddRange(ToLines());
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllLines(fullPath, lines);
        }

        public void Read(string fullPath)
        {
            var lines = File.ReadAllLines(fullPath).ToLi();
            levelInfo.FromLines(lines, lvliPrefix);
            lines.RemoveAll(li => li.StartsWith(lvliPrefix));
            FromLines(lines);
        }

        protected abstract Li<string> ToLines();
        protected abstract void FromLines(Li<string> lines);

        protected Diss levelInfo;
        const string lvliPrefix = "LVLI.";
    }

    class MssPzlLevel : MssLevel
    {
        internal MssPzlLevel(Diss args, PuzzleSet puzzles) : base(args)
        {
            this.Puzzles = puzzles;
        }

        static internal MssPzlLevel FromFile(string fullPath)
        {
            var level = new MssPzlLevel(null, new PuzzleSet());
            level.Read(fullPath);
            return level;
        }

        protected override Li<string> ToLines() => Puzzles.GetLinesForWriting();

        protected override void FromLines(Li<string> lines) => Puzzles.ReadFromLines(lines);

        readonly PuzzleSet Puzzles;
    }
}
