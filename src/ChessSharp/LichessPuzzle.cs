using ChessSharp.Pieces;
using ChessSharp.SquareData;
using System;
using System.Linq;
using AwiUtils;

namespace ChessSharp
{
    public enum PuzzleType { FEN, LichessPuzzle, LucasChessPuzzle }

    public class LichessPuzzle
    {
        public static LichessPuzzle Create(string line)
        {
            LichessPuzzle puzzle = null;
            if (line.Contains(','))
                // it's not a fen, it's a Lichess puzzle
                puzzle = LichessPuzzle.Create(line, PuzzleType.LichessPuzzle);
            else if (line.Contains('|'))
                // it's not a fen, it's a LucasChess puzzle
                puzzle = LichessPuzzle.Create(line, PuzzleType.LucasChessPuzzle);
            else
                puzzle = LichessPuzzle.Create(line, PuzzleType.FEN);
            return puzzle;
        }

        private static LichessPuzzle Create(string line, PuzzleType lineType)
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
                else if (sMove[^1].IsContainedIn(piecesChars)) // 1 from end == last char. 
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
}
