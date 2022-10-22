using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ChessSharp;
using ChessSharp.Pieces;
using ChessSharp.SquareData;
using Wolter.VA.Utils;

namespace ChessUI
{
    public partial class Form1 : Form
    {
        private readonly Label[] _squareLabels;
        private readonly Label[] _allLabelsToFlip;
        private readonly Dictionary<string, Point> _whiteLocations;
        private readonly Dictionary<string, Point> _blackLocations;
        private Dictionary<string, Point> _currentLocations = null;
        private Square? _selectedSourceSquare;
        private PuzzleGame _gameBoard = new PuzzleGame("00143,r2q1rk1/5ppp/1np5/p1b5/2p1B3/P7/1P3PPP/R1BQ1RK1 b - - 1 17,d8f6 d1h5 h7h6 h5c5,1871,75,93,790,advantage middlegame short,https://lichess.org/jcuxlI63/black#34");
        private PuzzleSet _puzzleSet;
        string _currPuzzleSetName;

        public Form1()
        {
            InitializeComponent();
            this.Text = "ChessPuzzlePecker";
            cbFlipBoard.Text = "Flip board";
            _squareLabels = Controls.OfType<Label>()
                                 .Where(m => Regex.IsMatch(m.Name, "lbl_[A-H][1-8]")).ToArray();
            var fileLabels = Controls.OfType<Label>().Where(m => Regex.IsMatch(m.Name, "^label[A-H]$")).ToLi();
            var rowLabels = Controls.OfType<Label>().Where(m => Regex.IsMatch(m.Name, "^label[1-8]$")).ToLi();

            foreach (var squareLabel in _squareLabels)
                squareLabel.Tag = new SquareTag(squareLabel.Name);

            this.ResizeForm(0.7);

            Array.ForEach(_squareLabels, lbl =>
            {
                lbl.BackgroundImageLayout = ImageLayout.Zoom;
                lbl.Click += SquaresLabels_Click;
            });

            var allLabels = new Li<Label>(_squareLabels);
            allLabels.AddRange(fileLabels);
            allLabels.AddRange(rowLabels);
            _allLabelsToFlip = allLabels.ToArray();

            _whiteLocations = _allLabelsToFlip.ToDictionary(lbl => lbl.Name, lbl => lbl.Location);
            _blackLocations = _allLabelsToFlip.ToDictionary(lbl => InvertLabel(lbl.Name), lbl => lbl.Location);

            foreach (PawnPromotion p in Enum.GetValues(typeof(PawnPromotion)))
                cbPromoteTo.Items.Add(p);
            cbPromoteTo.SelectedItem = PawnPromotion.Queen;

            FillPuzzleSetsComboBox();
            ReadCurrentPzlName();
        }

        void ReadPuzzles()
        {
            if (_currPuzzleSetName != null)
            {
                _puzzleSet = PuzzleSet.ReadPuzzleSet(_currPuzzleSetName);
                if (_puzzleSet == null)
                {
                    MessageBox.Show($"Cannot find PuzzleSet {_currPuzzleSetName}. Creating new one.");
                    _puzzleSet = new PuzzleSet(_currPuzzleSetName, 100,
                        "fork masterVsMaster:70;masterVsMaster:15;hangingPiece:15;", 1800, 2150, 0);
                    _puzzleSet.WriteSet();
                }
            }
        }

        private void ResizeForm(double faktor)
        {
            var currLblSize = _squareLabels.First().Size;
            var newLblSize = new Size((int)(currLblSize.Width * faktor), (int)(currLblSize.Height * faktor));
            var delta = newLblSize.Width - currLblSize.Width;
            this.Size = new Size((int)(Size.Width * faktor), (int)(Size.Height * faktor));
            foreach (var sl in _squareLabels)
            {
                sl.Size = newLblSize;
                var dx = (sl.Name[4] - 'A') * delta;
                var dy = (-(sl.Name[5] - '8')) * delta;
                sl.Location = AddDxDy(sl.Location, dx, dy);
            }

            var fileLabels = Controls.OfType<Label>().Where(m => Regex.IsMatch(m.Name, "^label[A-H]$")).ToLi();
            foreach (var lbl in fileLabels)
            {
                var dx = (int)((lbl.Text[0] - 'A' + 0.3) * delta);
                var dy = (int)((8) * delta);
                lbl.Location = AddDxDy(lbl.Location, dx, dy);
            }

            var rowLabels = Controls.OfType<Label>().Where(m => Regex.IsMatch(m.Name, "^label[1-8]$")).ToLi();
            foreach (var lbl in rowLabels)
            {
                var dx = 8 * delta;
                var dy = (int)((8 - (lbl.Text[0] - '1') - 0.6) * delta);
                lbl.Location = AddDxDy(lbl.Location, dx, dy);
            }

            foreach (var c in new Control[] { cbFlipBoard, btLichess, btNext, lblWhoseTurn, lblPuzzleState, 
                cbPuzzleSets, cbPromoteTo, lblPromoteTo })
                c.Location = AddDxDy(c.Location, (int)(9.5 * delta), 0);
        }

        static Point AddDxDy(Point p, int dx, int dy)
        {
            var v = new VAPointI(p.X, p.Y);
            v += new VAPointI(dx, dy);
            return new Point(v.X, v.Y);
        }

        #region Board flipping
        private void ShowUIFromSideOf(Player player)
        {
            var locationsDictionary = player == Player.White ? _whiteLocations : _blackLocations;
            if (_currentLocations != locationsDictionary)
            {
                Array.ForEach(_allLabelsToFlip, lbl => lbl.Location = locationsDictionary[lbl.Name]);
                _currentLocations = locationsDictionary;
            }
        }

        private static string InvertSquare(string sq)
        {
            // sq is like lbl_A7 for example.
            // file char at index 4,
            // rank char at index 5.
            var f = (char)('A' + 'H' - sq[4]);
            var r = '9' - sq[5];
            return "lbl_" + f + r;
        }

        private static string InvertLabel(string lbl)
        {
            string inv;
            if (lbl.StartsWith("lbl_"))
                inv = InvertSquare(lbl);
            else
            {
                var m = Regex.Match(lbl, "^label([A-H])");
                if (m.Success)
                    inv = "label" + (char)('A' + 'H' - m.Groups[1].Value[0]);
                else
                    inv = "label" + (char)('1' + '8' - lbl[5]);
            }
            return inv;
        }

        private void cbFlipBoard_CheckedChanged(object sender, EventArgs e)
        {
            SetSideOf();
            DrawBoard();
        }

        private void SetSideOf()
        {
            if ((_gameBoard.WhoseTurn == Player.Black && !cbFlipBoard.Checked) ||
                (_gameBoard.WhoseTurn == Player.White && cbFlipBoard.Checked))
                ShowUIFromSideOf(Player.Black);
            else
                ShowUIFromSideOf(Player.White);
        }
        #endregion Board flipping


        public static void OpenWithDefaultApp(string path)
        {
            using Process fileopener = new Process();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }

        private Player? GetPlayerInCheck()
        {
            if (_gameBoard.GameState == GameState.BlackInCheck || _gameBoard.GameState == GameState.WhiteWinner)
            {
                return Player.Black;
            }
            if (_gameBoard.GameState == GameState.WhiteInCheck || _gameBoard.GameState == GameState.BlackWinner)
            {
                return Player.White;
            }
            return null;
        }

        private void SquaresLabels_Click(object sender, EventArgs e)
        {
            if (_isCurrentFinished)
                return;
            Label selectedLabel = (Label)sender;
            // First click
            if (_selectedSourceSquare == null)
            {
                // Re-draw to remove previously colored labels.
                DrawBoard(GetPlayerInCheck());

                // This part highlights possible Squares for the selected Piece. Strange but true, 
                // this color is used also to determine, if a move is to be made. 
                if ((selectedLabel.Tag as SquareTag).PieceCol != _gameBoard.WhoseTurn) return;
                _selectedSourceSquare = (selectedLabel.Tag as SquareTag).Square;
                var validDestinations = ChessUtilities.GetValidMovesOfSourceSquare(_selectedSourceSquare.Value, _gameBoard).Select(m => m.Destination).ToArray();
                if (validDestinations.Length == 0)
                {
                    _selectedSourceSquare = null;
                    return;
                }
                selectedLabel.BackColor = Color.LightGreen;
            }
            else
            {
                // Second click
                var targetSquare = Square.Create(selectedLabel.Name.AsSpan().Slice("lbl_".Length));
                var validDestinations = ChessUtilities.GetValidMovesOfSourceSquare(_selectedSourceSquare.Value, _gameBoard).
                    Select(m => m.Destination).ToLi();
                if (validDestinations.Contains(targetSquare))
                {
                    if (_gameBoard.TryMove(_selectedSourceSquare.Value, targetSquare, 
                            _gameBoard.IsPromotionMove(_selectedSourceSquare.Value, targetSquare) ? 
                            (PawnPromotion)cbPromoteTo.SelectedItem : (PawnPromotion?)null))
                        _isCurrentFinished = _gameBoard.MakeMoveAndAnswer(MakeMove);
                    else
                        _puzzleSet.CurrentIsError();
                    if (_isCurrentFinished)
                    {
                        _puzzleSet.CurrentIsFinished();
                        lblPuzzleState.Text = $"Done {_puzzleSet.NumDone}/{_puzzleSet.NumTotal}  R {_puzzleSet.CurrentRound}" ;
                        lblWhoseTurn.Text = _puzzleSet.CurrentRating();
                    }
                    _puzzleSet.WriteSet();
                }
                else
                    DrawBoard(GetPlayerInCheck()); // to remove selection color. 
                _selectedSourceSquare = null;
            }
        }

        bool _isCurrentFinished;

        private void DrawBoard(Player? playerInCheck = null)
        {
            var lightColor = Color.FromArgb(240, 217, 181);
            var darkColor = Color.FromArgb(181, 136, 99);
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    var file = (Linie)i;
                    var rank = (Rank)j;
                    var square = new Square(file, rank);
                    Label lbl = _squareLabels.First(m => (m.Tag as SquareTag).Square == square);
                    Piece piece = _gameBoard[file, rank];
                    var newColor = ((i + j) % 2 == 0) ? darkColor : lightColor;
                    if (newColor != lbl.BackColor)
                        lbl.BackColor = newColor;
                    if (piece == null)
                    {
                        if (lbl.BackgroundImage != null)
                        {
                            lbl.BackgroundImage = null;
                            (lbl.Tag as SquareTag).PieceCol = null;
                        }
                    }
                    else
                    {
                        lbl.BackgroundImage = (Image)Properties.Resources.ResourceManager.
                            GetObject($"{piece.Owner}{piece.GetType().Name}");
                        (lbl.Tag as SquareTag).PieceCol = piece.Owner;
                    }
                }
            }

            if (_gameBoard.Moves != null && !_gameBoard.Moves.IsEmpty)
            {
                // draw special colors for last move.
                var last = _gameBoard.Moves.Last();
                foreach (var sq in new Square[] { last.Source, last.Destination })
                {
                    var sqLabel = _squareLabels.First(lbl => (lbl.Tag as SquareTag).Square == sq);
                    AddToBackColor(sqLabel, 0, 35, 0);
                }
            }

            if (playerInCheck != null)
            {
                // Division => Rank             Modulus => File
                Square checkedKingSquare = _gameBoard.Board.SelectMany(x => x)
                    .Select((p, i) => new { Piece = p, Square = new Square((ChessSharp.SquareData.Linie)(i % 8), (Rank)(i / 8)) })
                    .First(m => m.Piece is King && m.Piece.Owner == playerInCheck).Square;
                var checkLabel = _squareLabels.First(lbl => (lbl.Tag as SquareTag).Square == checkedKingSquare);
                AddToBackColor(checkLabel, 20, -40, -40);
            }
        }

        void AddToBackColor(System.Windows.Forms.Label lbl, int r, int g, int b)
        {
            var c = lbl.BackColor;
            var nc = Color.FromArgb(Math.Min(c.R + r, 255), Math.Min(c.G + g, 255), Math.Min(c.B + b, 255));
            lbl.BackColor = nc;
        }

        private void MakeMove(Square source, Square destination)
        {
            try
            {
                Player player = _gameBoard.WhoseTurn;
                PawnPromotion? pawnPromotion = _gameBoard.IsPromotionMove(source, destination) ? 
                    (PawnPromotion)cbPromoteTo.SelectedItem : (PawnPromotion?)null;

                var move = new Move(source, destination, player, pawnPromotion);
                if (!_gameBoard.IsValidMove(move))
                {
                    MessageBox.Show("Invalid Move!", "Chess", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                _gameBoard.MakeMove(move, isMoveValidated: true);

                DrawBoard(GetPlayerInCheck());

                if (_gameBoard.GameState == GameState.Draw || _gameBoard.GameState == GameState.Stalemate ||
                    _gameBoard.GameState == GameState.BlackWinner || _gameBoard.GameState == GameState.WhiteWinner)
                {
                    MessageBox.Show(_gameBoard.GameState.ToString());
                    return;
                }

                Player whoseTurn = _gameBoard.WhoseTurn;
                lblWhoseTurn.Text = whoseTurn.ToString();
                // FlipUi(whoseTurn);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error\n{exception.Message}\n\n{exception.StackTrace}", "Chess", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btNext_Click(object sender, EventArgs e)
        {
            if (_puzzleSet != null)
            {
                if (!_isCurrentFinished)
                    _puzzleSet.CurrentIsError();
                _isCurrentFinished = false;
                if (_puzzleSet.IsCurrentRoundFinished)
                {
                    MessageBox.Show("Current round is finished. Fanfare.");
                    _puzzleSet.RebasePuzzles();
                }
                _gameBoard = _puzzleSet.NextPuzzle();
                if (_gameBoard != null)
                {
                    SetSideOf();
                    DrawBoard();
                    lblPuzzleState.Text = "";
                    lblWhoseTurn.Text = _gameBoard.WhoseTurn.ToString();
                }
                else
                    SystemSounds.Beep.Play();
            }
        }

        private void btLichess_Click(object sender, EventArgs e)
        {
            if (!_isCurrentFinished)
                _puzzleSet.CurrentIsError();
            OpenWithDefaultApp("https://lichess.org/training/" + _puzzleSet.CurrentPuzzleLichessId);
        }

        void FillPuzzleSetsComboBox()
        {
            var pzls = Directory.EnumerateFiles(".", "*.pzl");
            foreach (var pzl in pzls)
                cbPuzzleSets.Items.Add(Path.GetFileNameWithoutExtension(pzl));
        }

        private void cbPuzzleSets_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currPuzzleSetName = (string)cbPuzzleSets.SelectedItem;

            ReadPuzzles();
            _isCurrentFinished = true;
            SaveCurrentPzlName();
            btNext_Click(this, null);
        }

        private void SaveCurrentPzlName() => File.WriteAllText("ChessPuzzlePecker.ini", _currPuzzleSetName);

        private void ReadCurrentPzlName() => cbPuzzleSets.SelectedItem = File.ReadAllText("ChessPuzzlePecker.ini");

        private void label9_Click(object sender, EventArgs e)
        {

        }
    }

    class SquareTag
    {
        public SquareTag(string labelName)
        {
            Square = Square.Create(labelName.Substring(4));
        }

        public Square Square;
        public Player? PieceCol;
    }
}