using ChessSharp.Pieces;
using ChessSharp.SquareData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AwiUtils;

namespace ChessSharp
{
    /// <summary>Represents the chess game.</summary>
    public class ChessGame : IDeepCloneable<ChessGame>
    {
        /// <summary>Gets <see cref="Piece"/> in a specific square.</summary>
        /// <param name="file">The <see cref="Linie"/> of the square.</param>
        /// <param name="rank">The <see cref="Rank"/> of the square.</param>
        public Piece? this[Linie file, Rank rank] => Board[(int)rank][(int)file];

        public Piece? this[Square sq] => Board[(int)sq.Rank][(int)sq.File];

        public void SetPiece(Square sq, Piece? p) => Board[(int)sq.Rank][(int)sq.File] = p;

        /// <summary>Gets a list of the game moves.</summary>
        public Li<Move> Moves { get; private set; } // TODO: BAD! Investigate why the class consumer would even need this. Make it a private field if appropriate. And make it some kind of interface (`IEnumerable` for example).

        /// <summary>Gets a 2D array of <see cref="Piece"/>s in the board.</summary>
        public Piece?[][] Board { get; private set; } // TODO: It's bad idea to expose this to public.

        /// <summary>Gets the <see cref="Player"/> who has turn.</summary>
        public Player WhoseTurn { get; private set; } = Player.White;

        /// <summary>Gets the current <see cref="ChessSharp.GameState"/>.</summary>
        public GameState GameState { get; private set; }

        public bool IsPromotionMove(Square? source, Square target)
        {
            bool isPromotion = false;
            if (source != null && (target.Rank == Rank.First || target.Rank == Rank.Eighth))
            {
                Piece? piece = this[source.Value];
                isPromotion = piece != null && piece.GetType() == typeof(Pawn);
            }
            return isPromotion;
        }


        internal bool CanWhiteCastleKingSide { get; set; } = true;
        internal bool CanWhiteCastleQueenSide { get; set; } = true;
        internal bool CanBlackCastleKingSide { get; set; } = true;
        internal bool CanBlackCastleQueenSide { get; set; } = true;


        /// <summary>Initializes a new instance of <see cref="ChessGame"/>.</summary>
        public ChessGame(string fenOrLichessPuzzle)
        {
            if (fenOrLichessPuzzle.Contains(','))
                // it's not a fen, it's a Lichess puzzle
                puzzle = LichessPuzzle.Create(fenOrLichessPuzzle, PuzzleType.LichessPuzzle);
            else if (fenOrLichessPuzzle.Contains('|'))
                // it's not a fen, it's a LucasChess puzzle
                puzzle = LichessPuzzle.Create(fenOrLichessPuzzle, PuzzleType.LucasChessPuzzle);
            else
                puzzle = LichessPuzzle.Create(fenOrLichessPuzzle, PuzzleType.FEN);

            Moves = new Li<Move>();

            Board = new Piece?[8][];
            var fenparts = puzzle.Fen.Split(" /".ToCharArray()).ToList();
            for (int i = 0; i < 8; ++i)
            {
                var fenrow = fenparts[i];
                var r = new Piece?[8];
                for (int j = 0, c = 0; j < fenrow.Length; ++j)
                {
                    char curr = fenrow[j];
                    if (int.TryParse(curr.ToString(), out int spaces) && spaces != 0)
                        c += spaces;
                    else
                        r[c++] = fenpieces[curr];
                }
                Board[7 - i] = r;
            }
            WhoseTurn = fenparts[8] == "w" ? Player.White : Player.Black;
            if (fenparts.Count > 9)
            {
                CanWhiteCastleKingSide = fenparts[9].Contains('K');
                CanWhiteCastleQueenSide = fenparts[9].Contains('Q');
                CanBlackCastleKingSide = fenparts[9].Contains('k');
                CanBlackCastleQueenSide = fenparts[9].Contains('q');

                // TODO: Es folgen enpassant, halbzüge seit dem letzten Bauernzug oder Schlagen einer Figur, Zugnummer
                // 
            }

        }

        static readonly Dictionary<char, Piece> fenpieces = new Dictionary<char, Piece>
        {
            { 'r', new Rook(Player.Black) },
            { 'n', new Knight(Player.Black) },
            { 'b', new Bishop(Player.Black) },
            { 'q', new Queen(Player.Black) },
            { 'k', new King(Player.Black) },
            { 'p', new Pawn(Player.Black) },
            { 'R', new Rook(Player.White) },
            { 'N', new Knight(Player.White) },
            { 'B', new Bishop(Player.White) },
            { 'Q', new Queen(Player.White) },
            { 'K', new King(Player.White) },
            { 'P', new Pawn(Player.White) },
        };



        /// <summary>Makes a move in the game.</summary>
        /// <param name="move">The <see cref="Move"/> you want to make.</param>
        /// <param name="isMoveValidated">Only pass true when you've already checked that the move is valid.</param>
        /// <returns>Returns true if the move is made; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        ///     The <c>move</c> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     The <see cref="Move.Source"/> square of the <c>move</c> doesn't contain a piece.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///    The <c>move.PromoteTo</c> is null and the move is a pawn promotion move.
        /// </exception>
        public bool MakeMove(Move move, bool isMoveValidated)
        {
            if (move == null)
                throw new ArgumentNullException(nameof(move));

            Piece? piece = this[move.Source.File, move.Source.Rank];
            if (piece == null)
                throw new InvalidOperationException("Source square has no piece.");

            if (!isMoveValidated && !IsValidMove(move))
                return false;

            SetCastleStatus(move, piece);

            if (piece is King && move.GetAbsDeltaX() == 2)
            {
                // Queen-side castle
                if (move.Destination.File == Linie.C)
                {
                    var rook = this[Linie.A, move.Source.Rank];
                    Board[(int)move.Source.Rank][(int)Linie.A] = null;
                    Board[(int)move.Source.Rank][(int)Linie.D] = rook;
                }

                // King-side castle
                if (move.Destination.File == Linie.G)
                {
                    var rook = this[Linie.H, move.Source.Rank];
                    Board[(int)move.Source.Rank][(int)Linie.H] = null;
                    Board[(int)move.Source.Rank][(int)Linie.F] = rook;
                }
            }

            if (piece is Pawn)
            {
                if ((move.Player == Player.White && move.Destination.Rank == Rank.Eighth) ||
                    (move.Player == Player.Black && move.Destination.Rank == Rank.First))
                {
                    piece = move.PromoteTo switch
                    {
                        PawnPromotion.Knight => new Knight(piece.Owner),
                        PawnPromotion.Bishop => new Bishop(piece.Owner),
                        PawnPromotion.Rook => new Rook(piece.Owner),
                        PawnPromotion.Queen => new Queen(piece.Owner),
                        _ => throw new ArgumentException($"A promotion move should have a valid {move.PromoteTo} property.", nameof(move)),
                    };
                }
                // Enpassant
                if (Pawn.GetPawnMoveType(move) == PawnMoveType.Capture &&
                    this[move.Destination.File, move.Destination.Rank] == null)
                {
                    this.SetPiece(Moves.Last().Destination, null);
                }

            }
            this.SetPiece(move.Source, null);
            this.SetPiece(move.Destination, piece);
            Moves.Add(move);
            WhoseTurn = ChessUtilities.Opponent(move.Player);
            SetGameState();
            return true;
        }

        private void SetCastleStatus(Move move, Piece piece)
        {
            if (piece.Owner == Player.White && piece is King)
            {
                CanWhiteCastleKingSide = false;
                CanWhiteCastleQueenSide = false;
            }

            if (piece.Owner == Player.White && piece is Rook &&
                move.Source.File == Linie.A && move.Source.Rank == Rank.First)
            {
                CanWhiteCastleQueenSide = false;
            }

            if (piece.Owner == Player.White && piece is Rook &&
                move.Source.File == Linie.H && move.Source.Rank == Rank.First)
            {
                CanWhiteCastleKingSide = false;
            }

            if (piece.Owner == Player.Black && piece is King)
            {
                CanBlackCastleKingSide = false;
                CanBlackCastleQueenSide = false;
            }

            if (piece.Owner == Player.Black && piece is Rook &&
                move.Source.File == Linie.A && move.Source.Rank == Rank.Eighth)
            {
                CanBlackCastleQueenSide = false;
            }

            if (piece.Owner == Player.Black && piece is Rook &&
                move.Source.File == Linie.H && move.Source.Rank == Rank.Eighth)
            {
                CanBlackCastleKingSide = false;
            }
        }

        /// <summary>Checks if a given move is valid or not.</summary>
        /// <param name="move">The <see cref="Move"/> to check its validity.</param>
        /// <returns>Returns true if the given <c>move</c> is valid; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        ///     The given <c>move</c> is null.
        /// </exception>
        public bool IsValidMove(Move move)
        {
            if (move == null)
                throw new ArgumentNullException(nameof(move));

            Piece? pieceSource = this[move.Source];
            Piece? pieceDestination = this[move.Destination];
            return (WhoseTurn == move.Player && pieceSource != null && pieceSource.Owner == move.Player &&
                    !Equals(move.Source, move.Destination) &&
                    (pieceDestination == null || pieceDestination.Owner != move.Player) &&
                    pieceSource.IsValidGameMove(move, this) && !PlayerWillBeInCheck(move));
        }

        internal bool PlayerWillBeInCheck(Move move)
        {
            if (move == null)
                throw new ArgumentNullException(nameof(move));

            ChessGame clone = DeepClone(); // Make the move on this board to keep original board as is.
            Piece? piece = clone[move.Source];
            clone.SetPiece(move.Source, null);
            if (piece is Pawn && clone.Moves.Any())
            {
                var lm = clone.Moves.Last();
                // en passant handling
                if (Pawn.GetPawnMoveType(lm) == PawnMoveType.TwoSteps &&
                    lm.Destination.Rank == move.Source.Rank)
                    if (lm.Source.File == move.Destination.File)
                    {
                        var lmPiece = clone[lm.Destination];
                        if (lmPiece is Pawn &&
                                AbsDist(lm.Source.File, move.Source.File) == 1 &&
                                clone[move.Destination] == null)
                            // It is en passant. Remove the taken pawn. 
                            clone.SetPiece(lm.Destination, null);
                    }
            }
            clone.SetPiece(move.Destination, piece);

            return ChessUtilities.IsPlayerInCheck(move.Player, clone);
        }

        public int Dist(Linie l1, Linie l2) => (int)l1 - (int)l2;

        public int AbsDist(Linie l1, Linie l2) => Math.Abs((int)l1 - (int)l2);

        internal void SetGameState()
        {
            Player opponent = WhoseTurn;
            Player lastPlayer = ChessUtilities.Opponent(opponent);
            bool isInCheck = ChessUtilities.IsPlayerInCheck(opponent, this);
            var hasValidMoves = ChessUtilities.GetValidMoves(this).Count > 0;

            if (isInCheck && !hasValidMoves)
            {
                GameState = lastPlayer == Player.White ? GameState.WhiteWinner : GameState.BlackWinner;
                return;
            }

            if (!hasValidMoves)
            {
                GameState = GameState.Stalemate;
                return;
            }

            if (isInCheck)
            {
                GameState = opponent == Player.White ? GameState.WhiteInCheck : GameState.BlackInCheck;
                return;
            }
            GameState = IsInsufficientMaterial() ? GameState.Draw : GameState.NotCompleted;
        }

        internal bool IsInsufficientMaterial() // TODO: Much allocations seem to happen here? (LINQ)
        {
            IEnumerable<Piece?> pieces = Board.SelectMany(x => x); // https://stackoverflow.com/questions/32588070/flatten-jagged-array-in-c-sharp

            var whitePieces = pieces.Select((p, i) => new { Piece = p, SquareColor = (i % 8 + i / 8) % 2 })
                .Where(p => p.Piece?.Owner == Player.White).ToArray();

            var blackPieces = pieces.Select((p, i) => new { Piece = p, SquareColor = (i % 8 + i / 8) % 2 })
                .Where(p => p.Piece?.Owner == Player.Black).ToArray();

            switch (whitePieces.Length)
            {
                // King vs King
                case 1 when blackPieces.Length == 1:
                // White King vs black king and (Bishop|Knight)
                case 1 when blackPieces.Length == 2 && blackPieces.Any(p => p.Piece is Bishop ||
                                                                            p.Piece is Knight):
                // Black King vs white king and (Bishop|Knight)
                case 2 when blackPieces.Length == 1 && whitePieces.Any(p => p.Piece is Bishop ||
                                                                            p.Piece is Knight):
                    return true;
                // King and bishop vs king and bishop
                case 2 when blackPieces.Length == 2:
                    {
                        var whiteBishop = whitePieces.FirstOrDefault(p => p.Piece is Bishop);
                        var blackBishop = blackPieces.FirstOrDefault(p => p.Piece is Bishop);
                        return whiteBishop != null && blackBishop != null &&
                               whiteBishop.SquareColor == blackBishop.SquareColor;
                    }
                default:
                    return false;
            }
        }

        internal static bool IsValidMove(Move move, ChessGame board)
        {
            if (move == null)
                throw new ArgumentNullException(nameof(move));

            Piece? pieceSource = board[move.Source];
            Piece? pieceDestination = board[move.Destination];

            return (pieceSource != null && pieceSource.Owner == move.Player &&
                    !Equals(move.Source, move.Destination) &&
                    (pieceDestination == null || pieceDestination.Owner != move.Player) &&
                    pieceSource.IsValidGameMove(move, board) && !board.PlayerWillBeInCheck(move));
        }

        internal bool IsTherePieceInBetween(Square square1, Square square2)
        {
            int xStep = Math.Sign(square2.File - square1.File);
            int yStep = Math.Sign(square2.Rank - square1.Rank);

            Rank rank = square1.Rank;
            Linie file = square1.File;
            while (true) // TODO: Prevent possible infinite loop (by throwing an exception) when passing un-logical squares (two squares not on same file, rank, or diagonal).
            {
                rank += yStep;
                file += xStep;
                if (rank == square2.Rank && file == square2.File)
                {
                    return false;
                }

                if (Board[(int)rank][(int)file] != null)
                {
                    return true;
                }
            }

        }

        public ChessGame DeepClone()
        {
            var g = this.DeepTClone<ChessGame>();
            return g;
        }

        protected LichessPuzzle puzzle;
    }

    public enum PuzzleType { FEN, LichessPuzzle, LucasChessPuzzle }

    public class LichessPuzzle
    {

        public static LichessPuzzle Create(string line, PuzzleType lineType)
        {
            LichessPuzzle p;
            if (lineType == PuzzleType.LichessPuzzle)
                p = new LichessPuzzle(lineType, line);
            else if (lineType == PuzzleType.FEN)
                p = new LichessPuzzle(lineType, "," + line + ",");
            else if (lineType == PuzzleType.LucasChessPuzzle)
            {
                var parts = line.Split('|');
                p = new LichessPuzzle(lineType, "xxxxx," + parts[0] + "," + parts[2]);
            }
            else
                throw new NotImplementedException();
            return p;
        }

        private LichessPuzzle(PuzzleType lineType, string line)
        {
            this.PuzzleType = lineType;
            var parts = line.Split(',');
            Fen = parts[1];
            var firstPlayer = Fen.Split()[1] == "w" ? Player.White : Player.Black;
            SMoves = parts[2].SplitToWords().Where(sm => !sm.IsContainedIn("-+ +- 1-0 0-1".SplitToWords())).ToLiro();
            Moves = new Li<Move>();

            if (SMoves != null && SMoves.Any())
            {
                ChessGame game = new ChessGame(Fen);
                for (int i = 0; i < SMoves.Count; ++i)
                    Moves.Add(CreateMoveFromSMove(game, SMoves[i], firstPlayer, i));
            }
            if (parts.Length >= 8)
                Motifs = parts[7].SplitToWords().ToLiro();
            else
                Motifs = new Liro<string>();
        }

        static readonly Liro<char> piecesChars = "N B R Q K S L T D".Split().Select(s => s[0]).ToLiro();
        static readonly Liro<char> filesChars = "a b c d e f g h".Split().Select(s => s[0]).ToLiro();

        private static Move CreateMoveFromSMove(ChessGame game, string sMove, Player firstPlayer, int nMove)
        {
            var origMove = sMove;
            sMove = sMove.Replace(" ", "").Replace("x", "").Replace("#", "").Replace("+", "")
                .TrimStart("1234567890.".ToCharArray());
            Square from, to;
            PawnPromotion? promoteTo = (PawnPromotion?)null;
            Move m = null;
            if (sMove[0].IsContainedIn(piecesChars) || sMove.Length <= 3)
            {
                // Default. Die letzten 2 Zeichen sind das Zielfeld. 
                string toString = sMove.Substring(sMove.Length - 2, 2);
                Li<Move> possMoves = null;
                if (sMove.Length == 3 && sMove[0].IsContainedIn(piecesChars))
                {
                    // Normaler Figurenzug oder Schlagzug 
                    possMoves = ChessUtilities.GetValidMovesOfTargetSquare(game, Square.Create(toString),
                        ChessUtilities.PieceTypeFromChar(sMove[0]));
                }
                else if (sMove.StartsWith("-0"))
                {
                    // Rochade, das erste 0 wurde oben getrimmt. 
                    if (sMove == "-0")
                        possMoves = ChessUtilities.GetValidMovesOfTargetSquare(game, 
                            game.WhoseTurn == Player.White ? Square.Create("g1") : Square.Create("g8"), typeof(King));
                    else if (sMove == "-0-0")
                        possMoves = ChessUtilities.GetValidMovesOfTargetSquare(game, 
                            game.WhoseTurn == Player.White ? Square.Create("c1") : Square.Create("c8"), typeof(King));
                }
                else if (sMove.Length == 2 && !sMove[1].IsContainedIn(filesChars))
                {
                    // Bauernzug 1 oder 2 vor
                    possMoves = ChessUtilities.GetValidMovesOfTargetSquare(game, Square.Create(toString), typeof(Pawn));
                }
                else if (sMove.Length == 2 && sMove[1].IsContainedIn(filesChars))
                {
                    // Bauernzugschlagzug wie cd oder fe;
                    throw new NotImplementedException();
                    // var possMoves = ChessUtilities.GetValidMovesOfTargetSquare(game, Square.Create(toString), typeof(Pawn));
                }
                else if (sMove[sMove.Length - 1].IsContainedIn(piecesChars))
                {
                    // Bauernumwandlungszug
                    possMoves = possMoves.Where(m => m.Source.File == Parser.ParseFile(sMove[0])).ToLi();
                }
                else if (sMove.Length == 3 && sMove[0].IsContainedIn(filesChars))
                {
                    // Bauernschlagzug
                    possMoves = ChessUtilities.GetValidMovesOfTargetSquare(game, Square.Create(toString), typeof(Pawn));
                    possMoves = possMoves.Where(m => m.Source.File == Parser.ParseFile(sMove[0])).ToLi();
                }
                else if (sMove.Length == 4 && sMove[0].IsContainedIn(piecesChars))
                {
                    //Figurenzug wie Sbd2 oder The1 o.ä.
                    possMoves = ChessUtilities.GetValidMovesOfTargetSquare(game, Square.Create(toString),
                        ChessUtilities.PieceTypeFromChar(sMove[0]));
                    if (sMove[1].IsContainedIn(filesChars))
                        possMoves = possMoves.Where(m => m.Source.File == Parser.ParseFile(sMove[1])).ToLi();
                    else
                        possMoves = possMoves.Where(m => m.Source.Rank == Parser.ParseRank(sMove[1])).ToLi();
                }
                if (possMoves == null || possMoves.Count != 1)
                    throw new Exception("Cannot parse move:" + origMove);
                m = possMoves[0];
            }
            else
            {
                from = Square.Create(sMove.Substring(0, 2));
                to = Square.Create(sMove.Substring(2, 2));
                promoteTo = sMove.Length > 4 ? PawnPromotionHelper.Get(sMove[4]) :
                    (PawnPromotion?)null;
                var player = nMove % 2 == 0 ? firstPlayer : ChessUtilities.Opponent(firstPlayer);
                m = new Move(from, to, player, promoteTo);
            }
            if (game != null)
                game.MakeMove(m, true);
            return m;
        }

        public readonly string Fen;
        public readonly Liro<string> SMoves;
        public readonly Li<Move> Moves;
        public readonly Liro<string> Motifs;
        public readonly PuzzleType PuzzleType;
    }

    public class PuzzleGame : ChessGame
    {
        public PuzzleGame(string fenOrLichessPuzzle) : base(fenOrLichessPuzzle)
        {
            if (puzzle.Moves != null && !puzzle.Moves.IsEmpty && puzzle.PuzzleType != PuzzleType.LucasChessPuzzle)
                MakeMove(CurrMove);
        }

        /// <summary>
        /// 
        /// </summary>
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

        private bool IsCorrectMove(Square from, Square to, PawnPromotion? pp) => HasMove &&
                    from == CurrMove.Source && to == CurrMove.Destination && pp == CurrMove.PromoteTo;

        private void MakeMove(Action<Square, Square> formMakeMove, Move? tryMove)
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
