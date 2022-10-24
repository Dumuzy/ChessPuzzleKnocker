namespace ChessUI
{
    partial class InputBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.tbNameOfSet = new System.Windows.Forms.TextBox();
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbUpperRating = new System.Windows.Forms.TrackBar();
            this.tbLowerRating = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbFilter1 = new System.Windows.Forms.ComboBox();
            this.cbFilter1Percent = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cbFilter2Percent = new System.Windows.Forms.ComboBox();
            this.cbFilter2 = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.cbFilter3Percent = new System.Windows.Forms.ComboBox();
            this.cbFilter3 = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.cbFilter1_2 = new System.Windows.Forms.ComboBox();
            this.cbFilter2_2 = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.cbFilter3_2 = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.tbNumPuzzles = new System.Windows.Forms.TextBox();
            this.lblStartAtNumber = new System.Windows.Forms.Label();
            this.tbStartAtNumber = new System.Windows.Forms.TextBox();
            this.txLowerRating = new System.Windows.Forms.TextBox();
            this.txUpperRating = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.tbUpperRating)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLowerRating)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(13, 106);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Upper puzzle rating";
            // 
            // tbNameOfSet
            // 
            this.tbNameOfSet.Location = new System.Drawing.Point(165, 12);
            this.tbNameOfSet.Name = "tbNameOfSet";
            this.tbNameOfSet.Size = new System.Drawing.Size(292, 23);
            this.tbNameOfSet.TabIndex = 1;
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(79, 300);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(75, 23);
            this.btOk.TabIndex = 2;
            this.btOk.Text = "OK";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(165, 300);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 3;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(149, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Name of new puzzle set";
            // 
            // tbUpperRating
            // 
            this.tbUpperRating.BackColor = System.Drawing.SystemColors.ControlLight;
            this.tbUpperRating.LargeChange = 100;
            this.tbUpperRating.Location = new System.Drawing.Point(164, 98);
            this.tbUpperRating.Maximum = 3200;
            this.tbUpperRating.Minimum = 500;
            this.tbUpperRating.Name = "tbUpperRating";
            this.tbUpperRating.RightToLeftLayout = true;
            this.tbUpperRating.Size = new System.Drawing.Size(292, 45);
            this.tbUpperRating.SmallChange = 50;
            this.tbUpperRating.TabIndex = 5;
            this.tbUpperRating.TickFrequency = 200;
            this.tbUpperRating.Value = 900;
            this.tbUpperRating.Scroll += new System.EventHandler(this.tbUpperRating_Scroll);
            // 
            // tbLowerRating
            // 
            this.tbLowerRating.BackColor = System.Drawing.SystemColors.ControlLight;
            this.tbLowerRating.LargeChange = 100;
            this.tbLowerRating.Location = new System.Drawing.Point(164, 41);
            this.tbLowerRating.Maximum = 3200;
            this.tbLowerRating.Minimum = 500;
            this.tbLowerRating.Name = "tbLowerRating";
            this.tbLowerRating.RightToLeftLayout = true;
            this.tbLowerRating.Size = new System.Drawing.Size(292, 45);
            this.tbLowerRating.SmallChange = 50;
            this.tbLowerRating.TabIndex = 8;
            this.tbLowerRating.TickFrequency = 200;
            this.tbLowerRating.Value = 900;
            this.tbLowerRating.Scroll += new System.EventHandler(this.tbLowerRating_Scroll);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(12, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(132, 23);
            this.label4.TabIndex = 7;
            this.label4.Text = "Lower puzzle rating";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 152);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 23);
            this.label3.TabIndex = 10;
            this.label3.Text = "Filter for";
            // 
            // cbFilter1
            // 
            this.cbFilter1.FormattingEnabled = true;
            this.cbFilter1.Location = new System.Drawing.Point(70, 149);
            this.cbFilter1.Name = "cbFilter1";
            this.cbFilter1.Size = new System.Drawing.Size(145, 23);
            this.cbFilter1.TabIndex = 11;
            // 
            // cbFilter1Percent
            // 
            this.cbFilter1Percent.FormattingEnabled = true;
            this.cbFilter1Percent.Location = new System.Drawing.Point(391, 149);
            this.cbFilter1Percent.Name = "cbFilter1Percent";
            this.cbFilter1Percent.Size = new System.Drawing.Size(39, 23);
            this.cbFilter1Percent.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(436, 152);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(22, 23);
            this.label5.TabIndex = 13;
            this.label5.Text = "%";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(436, 184);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(22, 23);
            this.label6.TabIndex = 17;
            this.label6.Text = "%";
            // 
            // cbFilter2Percent
            // 
            this.cbFilter2Percent.FormattingEnabled = true;
            this.cbFilter2Percent.Location = new System.Drawing.Point(391, 181);
            this.cbFilter2Percent.Name = "cbFilter2Percent";
            this.cbFilter2Percent.Size = new System.Drawing.Size(39, 23);
            this.cbFilter2Percent.TabIndex = 16;
            // 
            // cbFilter2
            // 
            this.cbFilter2.FormattingEnabled = true;
            this.cbFilter2.Location = new System.Drawing.Point(70, 181);
            this.cbFilter2.Name = "cbFilter2";
            this.cbFilter2.Size = new System.Drawing.Size(145, 23);
            this.cbFilter2.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(12, 184);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(70, 23);
            this.label7.TabIndex = 14;
            this.label7.Text = "Filter for";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(436, 216);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(22, 23);
            this.label8.TabIndex = 21;
            this.label8.Text = "%";
            // 
            // cbFilter3Percent
            // 
            this.cbFilter3Percent.FormattingEnabled = true;
            this.cbFilter3Percent.Location = new System.Drawing.Point(391, 213);
            this.cbFilter3Percent.Name = "cbFilter3Percent";
            this.cbFilter3Percent.Size = new System.Drawing.Size(39, 23);
            this.cbFilter3Percent.TabIndex = 20;
            // 
            // cbFilter3
            // 
            this.cbFilter3.FormattingEnabled = true;
            this.cbFilter3.Location = new System.Drawing.Point(70, 213);
            this.cbFilter3.Name = "cbFilter3";
            this.cbFilter3.Size = new System.Drawing.Size(145, 23);
            this.cbFilter3.TabIndex = 19;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(12, 216);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(70, 23);
            this.label9.TabIndex = 18;
            this.label9.Text = "Filter for";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(223, 152);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(22, 23);
            this.label10.TabIndex = 22;
            this.label10.Text = "&&";
            // 
            // cbFilter1_2
            // 
            this.cbFilter1_2.FormattingEnabled = true;
            this.cbFilter1_2.Location = new System.Drawing.Point(245, 149);
            this.cbFilter1_2.Name = "cbFilter1_2";
            this.cbFilter1_2.Size = new System.Drawing.Size(140, 23);
            this.cbFilter1_2.TabIndex = 23;
            // 
            // cbFilter2_2
            // 
            this.cbFilter2_2.FormattingEnabled = true;
            this.cbFilter2_2.Location = new System.Drawing.Point(245, 181);
            this.cbFilter2_2.Name = "cbFilter2_2";
            this.cbFilter2_2.Size = new System.Drawing.Size(140, 23);
            this.cbFilter2_2.TabIndex = 25;
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(223, 184);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(22, 23);
            this.label11.TabIndex = 24;
            this.label11.Text = "&&";
            // 
            // cbFilter3_2
            // 
            this.cbFilter3_2.FormattingEnabled = true;
            this.cbFilter3_2.Location = new System.Drawing.Point(245, 213);
            this.cbFilter3_2.Name = "cbFilter3_2";
            this.cbFilter3_2.Size = new System.Drawing.Size(140, 23);
            this.cbFilter3_2.TabIndex = 27;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(223, 216);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(22, 23);
            this.label12.TabIndex = 26;
            this.label12.Text = "&&";
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(12, 255);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(122, 20);
            this.label13.TabIndex = 29;
            this.label13.Text = "Number of puzzles";
            // 
            // tbNumPuzzles
            // 
            this.tbNumPuzzles.Location = new System.Drawing.Point(140, 252);
            this.tbNumPuzzles.Name = "tbNumPuzzles";
            this.tbNumPuzzles.Size = new System.Drawing.Size(75, 23);
            this.tbNumPuzzles.TabIndex = 28;
            // 
            // lblStartAtNumber
            // 
            this.lblStartAtNumber.Location = new System.Drawing.Point(245, 255);
            this.lblStartAtNumber.Name = "lblStartAtNumber";
            this.lblStartAtNumber.Size = new System.Drawing.Size(122, 20);
            this.lblStartAtNumber.TabIndex = 31;
            this.lblStartAtNumber.Text = "Start at number";
            // 
            // tbStartAtNumber
            // 
            this.tbStartAtNumber.Location = new System.Drawing.Point(373, 252);
            this.tbStartAtNumber.Name = "tbStartAtNumber";
            this.tbStartAtNumber.Size = new System.Drawing.Size(75, 23);
            this.tbStartAtNumber.TabIndex = 30;
            // 
            // txLowerRating
            // 
            this.txLowerRating.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txLowerRating.Location = new System.Drawing.Point(287, 63);
            this.txLowerRating.Name = "txLowerRating";
            this.txLowerRating.Size = new System.Drawing.Size(58, 23);
            this.txLowerRating.TabIndex = 32;
            this.txLowerRating.TextChanged += new System.EventHandler(this.txLowerRating_TextChanged);
            // 
            // txUpperRating
            // 
            this.txUpperRating.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txUpperRating.Location = new System.Drawing.Point(287, 120);
            this.txUpperRating.Name = "txUpperRating";
            this.txUpperRating.Size = new System.Drawing.Size(58, 23);
            this.txUpperRating.TabIndex = 33;
            this.txUpperRating.TextChanged += new System.EventHandler(this.txUpperRating_TextChanged);
            // 
            // InputBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 342);
            this.Controls.Add(this.txUpperRating);
            this.Controls.Add(this.txLowerRating);
            this.Controls.Add(this.lblStartAtNumber);
            this.Controls.Add(this.tbStartAtNumber);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.tbNumPuzzles);
            this.Controls.Add(this.cbFilter3_2);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.cbFilter2_2);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.cbFilter1_2);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.cbFilter3Percent);
            this.Controls.Add(this.cbFilter3);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cbFilter2Percent);
            this.Controls.Add(this.cbFilter2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbFilter1Percent);
            this.Controls.Add(this.cbFilter1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbLowerRating);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbUpperRating);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbNameOfSet);
            this.Controls.Add(this.btOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "InputBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create a puzzle set";
            ((System.ComponentModel.ISupportInitialize)(this.tbUpperRating)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLowerRating)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox tbNameOfSet;
        private System.Windows.Forms.Button btOk;

        #endregion

        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar tbUpperRating;
        private System.Windows.Forms.TrackBar tbLowerRating;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbFilter1;
        private System.Windows.Forms.ComboBox cbFilter1Percent;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cbFilter2Percent;
        private System.Windows.Forms.ComboBox cbFilter2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbFilter3Percent;
        private System.Windows.Forms.ComboBox cbFilter3;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cbFilter1_2;
        private System.Windows.Forms.ComboBox cbFilter2_2;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cbFilter3_2;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tbNumPuzzles;
        private System.Windows.Forms.Label lblStartAtNumber;
        private System.Windows.Forms.TextBox tbStartAtNumber;
        private System.Windows.Forms.TextBox txLowerRating;
        private System.Windows.Forms.TextBox txUpperRating;
    }
}