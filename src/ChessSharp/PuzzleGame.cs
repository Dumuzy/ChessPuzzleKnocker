using AwiUtils;
using ChessSharp.Pieces;
using ChessSharp.SquareData;
using System;
using System.Linq;

namespace ChessSharp
{
    public class PuzzleGame : ChessGame
    {
        public PuzzleGame(string fenOrLichessPuzzle) : base(fenOrLichessPuzzle)
        {
            if (puzzle.Moves != null && !puzzle.Moves.IsEmpty && puzzle.PuzzleType != PuzzleType.LucasChessPuzzle)
                MakeMove(CurrMove);
        }

        /// <param name="formMakeMove"></param>
        /// <returns>true, if puzzle is finished.</returns>
        public bool MakeMoveAndAnswer(Action<Square, Square> formMakeMove, Move tryMove)
        {
            if (HasMove)
                MakeMove(formMakeMove, tryMove);
            if (HasMove)
                MakeMove(formMakeMove, null);
            return !HasMove;
        }

        /// <summary> Checks if the move m is the correct move for the puzzle. </summary>
        /// <returns> True, if it is the correct move. False otherwise. </returns>
        public bool TryMove(Move m)
        {
            bool isOk = IsCorrectMove(m.Source, m.Destination, m.PromoteTo);
            if (!isOk && puzzle.Moves.Count == puzzleMoveNum + 1)
            {
                // Might be an alternate end.  Lichess puzzles allow alternate ends - 
                // but only in mating puzzles as last move. 
                if (puzzle.Motifs.Contains("mate", StringComparer.OrdinalIgnoreCase))
                {
                    var clone = this.DeepClone();
                    clone.MakeMove(m, false);
                    if (clone.GameState == GameState.WhiteWinner || clone.GameState == GameState.BlackWinner)
                        isOk = true;
                }
            }

            if (!isOk)
                ++NErrors;
            return isOk;
        }

        public int NErrors { get; private set; }

        public override ChessGame DeepClone() => this.DeepTClone<PuzzleGame>();

        private bool IsCorrectMove(Square from, Square to, PawnPromotion? pp) => HasMove &&
                    from == CurrMove.Source && to == CurrMove.Destination && pp == CurrMove.PromoteTo;

        public void MakeMove(Action<Square, Square> formMakeMove, Move tryMove = null)
        {
            // Lichess allows alternate solutions in mating puzzles as last move. 
            // Therefore, not always i CurrMove the move to make. 
            if (tryMove != null)
                formMakeMove(tryMove.Source, tryMove.Destination);
            else
                formMakeMove(CurrMove.Source, CurrMove.Destination);
            ++puzzleMoveNum;
        }

        private void MakeMove(Move m)
        {
            base.MakeMove(m, true);
            ++puzzleMoveNum;
        }

        public bool HasMove => puzzleMoveNum < puzzle.Moves.Count;

        public Move CurrMove => puzzle.Moves[puzzleMoveNum];

        int puzzleMoveNum = 0;
    }
}
