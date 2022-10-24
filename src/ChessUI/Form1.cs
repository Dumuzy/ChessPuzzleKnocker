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
using AwiUtils;
using System.Net;

namespace ChessUI
{

    public partial class Form1 : Form
    {
        private readonly Label[] _squareLabels;
        private readonly Liro<Label> _sideLabels;
        private Square? _selectedSourceSquare;
        private PuzzleGame _gameBoard = new PuzzleGame("00143,r2q1rk1/5ppp/1np5/p1b5/2p1B3/P7/1P3PPP/R1BQ1RK1 b - - 1 17,d8f6 d1h5 h7h6 h5c5,1871,75,93,790,advantage middlegame short,https://lichess.org/jcuxlI63/black#34");
        private PuzzleSet _puzzleSet;
        string _currPuzzleSetName;
        bool _isCurrPuzzleFinishedOk;
        private Dictionary<string, DateTime> _puzzlesWithError = new Dictionary<string, DateTime>();

        class SquareTag
        {
            public SquareTag(string labelName)
            {
                Square = Square.Create(labelName.Substring(4));
            }

            public Square Square;
            public Player? PieceCol;
            public override string ToString() => "" + Square + $" {PieceCol}";
        }

        public Form1()
        {
            InitializeComponent();
            this.Text = "ChessPuzzlePecker";
            cbFlipBoard.Text = "Flip board";
            _squareLabels = Controls.OfType<Label>().Where(m => Regex.IsMatch(m.Name, "lbl_[A-H][1-8]")).ToArray();
            _sideLabels = Controls.OfType<Label>().Where(m => Regex.IsMatch(m.Name, "^label[A-H1-8]$")).ToLiro();

            foreach (var lbl in _squareLabels)
            {
                lbl.Tag = new SquareTag(lbl.Name);
                lbl.BackgroundImageLayout = ImageLayout.Zoom;
                lbl.Click += SquaresLabels_Click;
            }
            this.ResizeForm(0.7);

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
                    MessageBox.Show($"Cannot find PuzzleSet {_currPuzzleSetName}.");
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

            var fileLabels = _sideLabels.Where(m => Regex.IsMatch(m.Name, "^label[A-H]$")).ToLi();
            foreach (var lbl in fileLabels)
            {
                var dx = (int)((lbl.Text[0] - 'A' + 0.3) * delta);
                var dy = (int)((8) * delta);
                lbl.Location = AddDxDy(lbl.Location, dx, dy);
            }

            var rowLabels = _sideLabels.Where(m => Regex.IsMatch(m.Name, "^label[1-8]$")).ToLi();
            foreach (var lbl in rowLabels)
            {
                var dx = 8 * delta;
                var dy = (int)((8 - (lbl.Text[0] - '1') - 0.6) * delta);
                lbl.Location = AddDxDy(lbl.Location, dx, dy);
            }

            foreach (var c in new Control[] { cbFlipBoard, btLichess, btNext, lblWhoseTurn, lblPuzzleState,
                cbPuzzleSets, cbPromoteTo, lblPromoteTo, btCreatePuzleSet, lblPuzzleId, btAbout })
                c.Location = AddDxDy(c.Location, (int)(9.5 * delta), 0);
        }

        static Point AddDxDy(Point p, int dx, int dy)
        {
            var v = new VAPointI(p.X, p.Y);
            v += new VAPointI(dx, dy);
            return new Point(v.X, v.Y);
        }

        #region Board flipping
        private Player PlayerAtBottom =>
             ((SquareTag)lbl_A8.Tag).Square.ToString() == "A8" ? Player.White : Player.Black;

        private void ShowUIFromSideOf(Player player)
        {
            if (PlayerAtBottom != player)
            {
                foreach (var lbl in _squareLabels)
                {
                    var sq = Square.Create(lbl.Name.Substring(4));
                    if (sq.Rank > Rank.Forth)
                        continue;
                    var sqInv = InvertSquare(lbl.Name);
                    var lblInv = _squareLabels.First(l => l.Name == sqInv);
                    SwapTags(lbl, lblInv);
                }

                var lbls = "ABCD1234".ToCharArray();
                foreach (var lbl in _sideLabels)
                {
                    if (lbl.Name[5].IsContainedIn(lbls))
                        continue;
                    var lblInv = _sideLabels.First(l => l.Name == InvertLabel(lbl.Name));
                    SwapTexts(lbl, lblInv);
                }
            }
            // else nothing to do. 
        }

        private void SwapTexts(Label l1, Label l2)
        {
            var th = l1.Text;
            l1.Text = l2.Text;
            l2.Text = th;
        }

        private void SwapTags(Label l1, Label l2)
        {
            var th = l1.Tag;
            l1.Tag = l2.Tag;
            l2.Tag = th;
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
                return Player.Black;
            if (_gameBoard.GameState == GameState.WhiteInCheck || _gameBoard.GameState == GameState.BlackWinner)
                return Player.White;
            return null;
        }

        private void SquaresLabels_Click(object sender, EventArgs e)
        {
            if (_isCurrPuzzleFinishedOk)
                return;
            Label selectedLabel = (Label)sender;
            // First click
            if (_selectedSourceSquare == null)
            {
                // Re-draw to remove previously colored labels.
                DrawBoard(GetPlayerInCheck());

                if ((selectedLabel.Tag as SquareTag).PieceCol != _gameBoard.WhoseTurn)
                    return;
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
                var targetSquare = (selectedLabel.Tag as SquareTag).Square;
                var validDestinations = ChessUtilities.GetValidMovesOfSourceSquare(_selectedSourceSquare.Value, _gameBoard).
                    Select(m => m.Destination).ToLi();
                if (validDestinations.Contains(targetSquare))
                {
                    var tryMove = new Move(_selectedSourceSquare.Value, targetSquare, _gameBoard.WhoseTurn,
                        _gameBoard.IsPromotionMove(_selectedSourceSquare, targetSquare) ?
                            (PawnPromotion)cbPromoteTo.SelectedItem : (PawnPromotion?)null);
                    if (_gameBoard.TryMove(tryMove))
                        _isCurrPuzzleFinishedOk = _gameBoard.MakeMoveAndAnswer(MakeMove, tryMove);
                    else
                        _puzzlesWithError.Add(_puzzleSet.CurrentLichessId, DateTime.Now);
                    if (_isCurrPuzzleFinishedOk)
                    {
                        var currentRound = _puzzleSet.CurrentRound;
                        lblPuzzleState.Text = PuzzleSetInfoDone;
                        if (DoesCurrentPuzzleCountAsCorrect())
                            _puzzleSet.CurrentIsCorrect();
                        lblWhoseTurn.Text = _puzzleSet.CurrentRating;
                        if (_puzzleSet.IsRoundFinished(currentRound))
                        {
                            MessageBox.Show($"Round #{currentRound} is finished. Fanfare.");
                            _puzzleSet.IncCurrentRound();
                            _puzzleSet.RebasePuzzles();
                        }
                    }
                    _puzzleSet.WriteSet();
                }
                else
                    DrawBoard(GetPlayerInCheck()); // to remove selection color. 
                _selectedSourceSquare = null;
            }
        }

        string PuzzleSetInfo => $"{_puzzleSet.CurrentPosition + 1}/{_puzzleSet.NumTotal}  Round {_puzzleSet.CurrentRound}";
        string PuzzleSetInfoDone => $"Done {PuzzleSetInfo}";

        private bool DoesCurrentPuzzleCountAsCorrect()
        {
            bool ok = true;
            // It doesn't count as correct, if an error has been made or help has been ordered a short time ago. 
            if (_puzzlesWithError.TryGetValue(_puzzleSet.CurrentLichessId, out DateTime timestamp))
            {
                if (DateTime.Now - timestamp < TimeSpan.FromMinutes(10))
                    ok = false;
                _puzzlesWithError.Remove(_puzzleSet.CurrentLichessId);
            }
            return ok;
        }

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

                Player whoseTurn = _gameBoard.WhoseTurn;
                lblWhoseTurn.Text = whoseTurn.ToString();
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error\n{exception.Message}\n\n{exception.StackTrace}", "Chess", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btNext_Click(object sender, EventArgs e)
        {
            if (_puzzleSet != null && _puzzleSet.HasPuzzles)
            {
                _isCurrPuzzleFinishedOk = false;
                _gameBoard = _puzzleSet.NextPuzzle();
                if (_gameBoard != null)
                {
                    _puzzleSet.CurrentIsTried();
                    SetSideOf();
                    DrawBoard();
                    lblPuzzleState.Text = PuzzleSetInfo;
                    lblWhoseTurn.Text = _gameBoard.WhoseTurn.ToString();
                    lblPuzzleId.Text = _puzzleSet.CurrentLichessId;
                }
                else
                    SystemSounds.Beep.Play();
            }
        }

        private void btLichess_Click(object sender, EventArgs e)
        {
            _puzzlesWithError.Add(_puzzleSet.CurrentLichessId, DateTime.Now);
            OpenWithDefaultApp("https://lichess.org/training/" + _puzzleSet.CurrentLichessId);
        }

        private void FillPuzzleSetsComboBox()
        {
            var pzls = Directory.EnumerateFiles(".", "*.pzl");
            foreach (var pzl in pzls)
                cbPuzzleSets.Items.Add(Path.GetFileNameWithoutExtension(pzl));
        }

        private void cbPuzzleSets_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currPuzzleSetName = (string)cbPuzzleSets.SelectedItem;

            ReadPuzzles();
            _isCurrPuzzleFinishedOk = true;
            SaveCurrentPzlName();
            btNext_Click(this, null);
        }

        private void SaveCurrentPzlName() => File.WriteAllText(IniFileName, _currPuzzleSetName);

        private void ReadCurrentPzlName()
        {
            if (File.Exists(IniFileName))
                cbPuzzleSets.SelectedItem = File.ReadAllText(IniFileName);
        }

        const string IniFileName = "ChessPuzzlePecker.ini";

        private void btCreatePuzzleSet_Click(object sender, EventArgs e)
        {
            if (_puzzleSet == null)
                _puzzleSet = new PuzzleSet("MyPuzzles-1", 100,
                    "fork master:70;masterVsMaster:10;hangingPiece:20;", 1800, 2150, 1000);

            var ib = new InputBox(_puzzleSet);
            var res = ib.ShowDialog();
            if (res == DialogResult.OK)
            {
                _puzzleSet = null;
                cbPuzzleSets.Items.Add(ib.tbNameOfSet.Text);
                cbPuzzleSets.SelectedItem = ib.tbNameOfSet.Text;
            }
        }

        private void btAbout_Click(object sender, EventArgs e)
        {
            var t = $@"
ChessPuzzlePecker
A chess puzzle training program inspired by the Woodpecker method.
See on GitHub  https://github.com/Dumuzy/ChessPuzzlePecker.
Special thanks to https://lichess.org/, from where all the puzzle data is coming.
You should download the puzzle data from there: 
https://database.lichess.org/lichess_db_puzzle.csv.bz2 
                    
Greetings to http://schachclub-ittersbach.de/.
                    ";
            MessageBox.Show(t, "ChessPuzzlePecker");

        }
    }
}