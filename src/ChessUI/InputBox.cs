using System;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using AwiUtils;
using ComboBox = System.Windows.Forms.ComboBox;
using static System.Windows.Forms.DataFormats;
using System.Linq;
using System.IO;

namespace ChessUI
{
    public partial class InputBox : Form
    {
        static InputBox()
        {
            motifs = new Li<MotifEx>();
            motifs.AddRange(motifsRaw.Select(m => new MotifEx(m)));
        }

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
            Form1.TranslateLabels(this, null);
        }

        private void initFilterControls(int n, ComboBox cbFilter1, ComboBox cbFilter2, ComboBox cbPercent,
            Li<PuzzleFilter> filters)
        {
            var orderdMotifs = motifs.OrderBy(m => m.ToString()).ToArray();
            cbFilter1.Items.AddRange(orderdMotifs);
            cbFilter2.Items.AddRange(orderdMotifs);
            cbPercent.Items.AddRange(percentages);
            if (filters.Count > n)
            {
                var fi = filters[n];
                if (fi.Motifs.Count > 0)
                    cbFilter1.SelectedItem = motifs.First(m => m.Motif == fi.Motifs[0]);
                if (fi.Motifs.Count > 1)
                    cbFilter2.SelectedItem = motifs.First(m => m.Motif == fi.Motifs[1]);
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
            {
                if (tbNameOfSet.Text.Length == 0)
                {
                    var motifs = filters.Replace("-", "").Split(';').SelectMany(p => p.Split(':', 2)[0].SplitToWords()).ToLiro();
                    if (motifs.Any())
                        tbNameOfSet.Text = motifs[0];
                }
                tbNameOfSet.Text += $"-{tbLowerRating.Value}-{tbUpperRating.Value}";
            }
            var ps = new PuzzleSet(tbNameOfSet.Text, Helper.ToInt(tbNumPuzzles.Text), filters,
                tbLowerRating.Value, tbUpperRating.Value, Helper.ToInt(tbStartAtNumber.Text),
                PuzzleDbProvider.GetLichessCsvFile);
            if (ps.HasPuzzles)
                ps.WriteSet();

            DialogResult = ps.HasPuzzles ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        private string GetFiltersString(int num)
        {
            string fi = "";
            var cb1 = Controls.OfType<ComboBox>().Where(m => m.Name == $"cbFilter{num}").First();
            fi += (((MotifEx)cb1.SelectedItem)?.Motif ?? "");
            var cb2 = Controls.OfType<ComboBox>().Where(m => m.Name == $"cbFilter{num}_2").First();
            fi += " " + (((MotifEx)cb2.SelectedItem)?.Motif ?? "") + ":";
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
            if (val >= tbUpperRating.Minimum && val <= tbUpperRating.Maximum)
                tbUpperRating.Value = val;
        }

        private void tbLowerRating_Scroll(object sender, EventArgs e)
        {
            txLowerRating.Text = "" + ((System.Windows.Forms.TrackBar)sender).Value;
        }

        private void txLowerRating_TextChanged(object sender, EventArgs e)
        {
            var val = Helper.ToInt(txLowerRating.Text);
            if (val >= tbLowerRating.Minimum && val <= tbLowerRating.Maximum)
                tbLowerRating.Value = val;
        }

        static readonly Li<MotifEx> motifs;
        static readonly string[] percentages = "0 10 20 30 40 50 60 70 80 90 100".Split();
        static readonly Li<string> motifsRaw = @"
--
advancedPawn, , Vorgerückter Bauer
advantage,, Vorteil
anastasiaMate,, Anastasias Matt
arabianMate,, arabisches Matt
attackingF2F7,, Angriff auf f2/f7
attraction,, Hinlenkung
backRankMate, , Grundreihenmatt
bishopEndgame, , Läuferendspiel 
bodenMate,, Bodens Matt
capturingDefender,, schlage den Verteidiger
castling,, Rochade
clearance,, Öffnung
crushing,, vernichtend
defensiveMove,, Verteidigungszug
deflection,, Ablenkung
discoveredAttack,, Abzug
doubleBishopMate,, Zwei-Läufer-Matt
doubleCheck,, Doppelschach
dovetailMate,, Taubenschwanzmatt
endgame,, Endspiel
enPassant,, en passant
equality,, Gleichheit
exposedKing,,exponierter König
fork,, Gabel
hangingPiece,, hängende Figur
hookMate,, Hakenmatt
interference, , Unterbrechung
intermezzo,, Zwischenzug
kingsideAttack,, Angriff am Königsflügel
knightEndgame,, Springerendspiel
long,, lang
master,, Meister
masterVsMaster,, Meister vs Meister
mate,, Matt
mateIn1,, Matt in 1
mateIn2,, Matt in 2
mateIn3,, Matt in 3
mateIn4,, Matt in 4
mateIn5,, Matt in 5
middlegame,, Mittelspiel
oneMove,, Einzüger
opening,, Eröffnung
pawnEndgame,, Bauernendspiel
pin,, Fesselung
promotion,, Umwandlung
queenEndgame,, Damenendspiel
queenRookEndgame,, Damen-Turm-Endspiel
queensideAttack,, Angriff am Damenflügel
quietMove,, stiller Zug
rookEndgame,, Turmendspiel
sacrifice,, Opfer
short,, kurz
skewer,, Spieß
smotheredMate,, ersticktes Matt
superGM,, SuperGM
trappedPiece,, gefangene Figur
underPromotion,, Unterverwandlung
veryLong,, sehr lang
xRayAttack,, Röntgenangriff
zugzwang,, Zugzwang
".SplitToLines().ToLi();


    }

    class MotifEx
    {
        public MotifEx(string line)
        {
            var p = line.Split(',').Select(pp => pp.Trim()).ToLi();
            NameEn = NameDe = Motif = p[0];
            if (p.Count > 1)
            {
                NameEn = p[1] != "" ? p[1] : p[0];
                if (p.Count > 2)
                    NameDe = p[2] != "" ? p[2] : p[0];
            }
        }

        public readonly string Motif, NameEn, NameDe;
        public override string ToString()
        {
            return Form1.Language == "DE" ? NameDe : NameEn;
        }
    }
}
