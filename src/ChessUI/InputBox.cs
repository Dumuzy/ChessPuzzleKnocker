using System;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using AwiUtils;
using ComboBox = System.Windows.Forms.ComboBox;
using static System.Windows.Forms.DataFormats;
using System.Linq;

namespace ChessUI
{
    public partial class InputBox : Form
    {
        public string UserInput { get; private set; }

        public InputBox(PuzzleSet p)
        {
            InitializeComponent();
            if (p != null)
            {
                tbUpperRating.Value = p.UpperRating;
                txUpperRating.Text = "" + p.UpperRating;
                tbLowerRating.Value = p.LowerRating;
                txLowerRating.Text = "" + p.LowerRating;
                tbNumPuzzles.Text = "" + p.NumPuzzlesWanted;
                tbStartAtNumber.Text = "" + p.StartPuzzleNumForCreation;
                initFilterControls(0, cbFilter1, cbFilter1_2, cbFilter1Percent, p.Filters);
                initFilterControls(1, cbFilter2, cbFilter2_2, cbFilter2Percent, p.Filters);
                initFilterControls(2, cbFilter3, cbFilter3_2, cbFilter3Percent, p.Filters);
            }
        }

        private void initFilterControls(int n, ComboBox cbFilter1, ComboBox cbFilter2, ComboBox cbPercent, 
            Li<PuzzleFilter> filters)
        {
            cbFilter1.Items.AddRange(motifs.ToArray());
            cbFilter2.Items.AddRange(motifs.ToArray());
            cbPercent.Items.AddRange(percentages);
            if (filters.Count > n)
            {
                var fi = filters[n];
                if(fi.Motifs.Count > 0)
                    cbFilter1.SelectedItem = fi.Motifs[0];
                if (fi.Motifs.Count > 1)
                    cbFilter2.SelectedItem = fi.Motifs[1];
                cbPercent.SelectedItem = fi.Percentage.ToString();
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            // filters format = motifA1 motifA2: percentageA; motifB1: percentageB; motifC1 motifC2
            string filters = "";
            for (int i = 1; i <= 3; ++i)
                filters += GetFiltersString(i);
            if (tbNameOfSet.Text.Length <= 1)
                tbNameOfSet.Text += $"-{tbLowerRating.Value}-{tbUpperRating.Value}";
            var ps = new PuzzleSet(tbNameOfSet.Text, Helper.ToInt(tbNumPuzzles.Text), filters, 
                tbLowerRating.Value, tbUpperRating.Value, Helper.ToInt(tbStartAtNumber.Text));
            ps.WriteSet();
            DialogResult = DialogResult.OK;
            Close();
        }

        private string GetFiltersString(int num)
        {
            string fi = "";
            var cb1 = Controls.OfType<ComboBox>().Where(m => m.Name == $"cbFilter{num}").First();
            fi += (string)cb1.SelectedItem;
            var cb2 = Controls.OfType<ComboBox>().Where(m => m.Name == $"cbFilter{num}_2").First();
            fi += " " + (string)cb2.SelectedItem + ":";
            var cbp = Controls.OfType<ComboBox>().Where(m => m.Name == $"cbFilter{num}Percent").First();
            fi += (string)cbp.SelectedItem + ";";
            return fi;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tbUpperRating_Scroll(object sender, EventArgs e)
        {
            txUpperRating.Text = "" + ((System.Windows.Forms.TrackBar)sender).Value;
        }

        private void txUpperRating_TextChanged(object sender, EventArgs e)
        {
            var val = Helper.ToInt(txUpperRating.Text);
            if (val >= 900 && val <= 2700)
                tbUpperRating.Value = val;
        }

        private void tbLowerRating_Scroll(object sender, EventArgs e)
        {
            txLowerRating.Text = "" + ((System.Windows.Forms.TrackBar)sender).Value;
        }

        private void txLowerRating_TextChanged(object sender, EventArgs e)
        {
            var val = Helper.ToInt(txLowerRating.Text);
            if (val >= 900 && val <= 2700)
                tbLowerRating.Value = val;
        }

        static readonly string[] percentages = "10 20 30 40 50 60 70 80 90 100".Split();
        static readonly Li<string> motifs = @"
--
advancedPawn
advantage
anastasiaMate
arabianMate
attackingF2F7
attraction
backRankMate
bishopEndgame
bodenMate
capturingDefender
castling
clearance
crushing
defensiveMove
deflection
discoveredAttack
doubleBishopMate
doubleCheck
dovetailMate
endgame
enPassant
equality
exposedKing
fork
hangingPiece
hookMate
interference
intermezzo
kingsideAttack
knightEndgame
long
master
masterVsMaster
mate
mateIn1
mateIn2
mateIn3
mateIn4
mateIn5
middlegame
oneMove
opening
pawnEndgame
pin
promotion
queenEndgame
queenRookEndgame
queensideAttack
quietMove
rookEndgame
sacrifice
short
skewer
smotheredMate
superGM
trappedPiece
underPromotion
veryLong
xRayAttack
zugzwang
".SplitToWords().ToLi();


    }
}
