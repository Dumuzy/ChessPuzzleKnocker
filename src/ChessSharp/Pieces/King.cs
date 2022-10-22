using System;
using ChessSharp.SquareData;

namespace ChessSharp.Pieces
{
    /// <summary>Represents a king <see cref="Piece"/>.</summary>
    public class King : Piece
    {
        internal King(Player player) : base(player) { }
        
        internal override bool IsValidGameMove(Move move, ChessGame board)
        {
            // No need to do null checks here, this method isn't public and isn't annotated with nullable.
            // If the caller try to pass a possible null reference, the compiler should issue a warning.
            // TODO: Should I add [NotNull] attribute to the arguments? What's the benefit?
            // The arguments are already non-nullable.

            int absDeltaX = move.GetAbsDeltaX();
            int absDeltaY = move.GetAbsDeltaY();

            // Regular king move.
            if (move.GetAbsDeltaX() <= 1 && move.GetAbsDeltaY() <= 1)
            {
                return true;
            }
            // Not castle move.
            if (absDeltaX != 2 || absDeltaY != 0 || move.Source.File != Linie.E ||
                (move.Player == Player.White && move.Source.Rank != Rank.First) ||
                (move.Player == Player.Black && move.Source.Rank != Rank.Eighth) ||
                (board.GameState == GameState.BlackInCheck || board.GameState == GameState.WhiteInCheck))
            {
                return false;
            }

            // White king-side castle move.
            if (move.Player == Player.White && move.Destination.File == Linie.G && board.CanWhiteCastleKingSide &&
                !board.IsTherePieceInBetween(move.Source, new Square(Linie.H, Rank.First)) &&
                new Rook(Player.White).Equals(board[Linie.H, Rank.First]))
            {
                return !board.PlayerWillBeInCheck(
                    new Move(move.Source, new Square(Linie.F, Rank.First), move.Player));
            }

            // Black king-side castle move.
            if (move.Player == Player.Black && move.Destination.File == Linie.G && board.CanBlackCastleKingSide &&
                !board.IsTherePieceInBetween(move.Source, new Square(Linie.H, Rank.Eighth)) &&
                new Rook(Player.Black).Equals(board[Linie.H, Rank.Eighth]))
            {
                return !board.PlayerWillBeInCheck(
                    new Move(move.Source, new Square(Linie.F, Rank.Eighth), move.Player));
            }

            // White queen-side castle move.
            if (move.Player == Player.White && move.Destination.File == Linie.C && board.CanWhiteCastleQueenSide &&
                !board.IsTherePieceInBetween(move.Source, new Square(Linie.A, Rank.First)) &&
                new Rook(Player.White).Equals(board[Linie.A, Rank.First]))
            {
                return !board.PlayerWillBeInCheck(
                    new Move(move.Source, new Square(Linie.D, Rank.First), move.Player));
            }

            // Black queen-side castle move.
            if (move.Player == Player.Black && move.Destination.File == Linie.C && board.CanBlackCastleQueenSide &&
                !board.IsTherePieceInBetween(move.Source, new Square(Linie.A, Rank.Eighth)) &&
                new Rook(Player.Black).Equals(board[Linie.A, Rank.Eighth]))
            {
                return !board.PlayerWillBeInCheck(
                    new Move(move.Source, new Square(Linie.D, Rank.Eighth), move.Player));
            }

            return false;

        }
    }
}
