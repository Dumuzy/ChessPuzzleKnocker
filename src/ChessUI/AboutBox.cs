using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ChessPuzzlePecker
{
    partial class AboutBox : Form
    {
        public AboutBox(bool isDonate = false)
        {
            InitializeComponent();
            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct + String.Format(" v{0}", AssemblyVersion);
            this.linkLabel1.Text = TrimTextForLinkLabel(GetRawText(isDonate));

            if (!isDonate)
            {
                AddLink2Label(linkLabel1, "Lichess", "https://lichess.org/");
                AddLink2Label(linkLabel1, "Schachclub Ittersbach", "http://schachclub-ittersbach.de/");
                AddLink2Label(linkLabel1, "Dumuzy@Github", "https://github.com/Dumuzy/ChessPuzzlePecker");
            }
            AddLink2Label(linkLabel1, "Alakaluf at Lichess", "https://lichess.org/@/Alakaluf");
            AddLink2Label(linkLabel1, "Paypal", "https://www.paypal.com");

            if (isDonate)
                labelProductName.Font =  linkLabel1.Font = new System.Drawing.Font("Segoe UI", 12F, 
                    System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte)(0));
        }

        string GetRawText(bool isDonate)
        {
            string t;
            if (isDonate)
                t = @"

If you like this software... I would be delighted if you'd buy me a coffee! :-)
I am   keizer@atlantis44.de   at Paypal.

Also, I'm giving chess lessons for players with a Lichess rapid rating below 1750. 
Only 5 Eu per hour in deutsch or english. Contact Alakaluf at Lichess.";
            else
                t = @"A chess puzzle training program inspired by 
the Woodpecker method.

Special thanks to Lichess, from where all the puzzle data is coming. 
Greetings to Schachclub Ittersbach.

For bug reports or feature requests please contact me: Dumuzy@Github or Alakaluf at Lichess. 
Also, I'm giving chess lessons for players with a Lichess rapid rating below 1750. 
Only 5 Eu per hour in deutsch or english. 

If you like this software... I would be delighted if you'd buy me a coffee! :-)
I am   keizer@atlantis44.de   at Paypal.";
            return t;
        }

        void AddLink2Label(LinkLabel ll, string linkText, string link, int delta = 0)
        {
            var idx = ll.Text.IndexOf(linkText);
            if (idx != -1)
                ll.Links.Add(idx - delta, linkText.Length, link);
        }

        string TrimTextForLinkLabel(string text)
        {
            // Replace single linebreaks by space, but not double line breaks. 
            // A double line breaks represents the end of a paragraph, this shall be kept.
            // A single line break shall be removed, so that the label's word wrap function does all 
            // the line breaking. 
            var t = Regex.Replace(text, @"(?<!\r\n) *\r\n\b(?!\r\n)", " ");
            // Replace \r\ by \n for that counting of characters in AddLink2Label works. 
            t = t.Replace("\r\n", "\n");
            return t;
        }

        void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Determine which link was clicked within the LinkLabel.
            this.linkLabel1.Links[linkLabel1.Links.IndexOf(e.Link)].Visited = true;

            // Display the appropriate link based on the value of the 
            // LinkData property of the Link object.
            string target = e.Link.LinkData as string;

            if (null != target && (target.StartsWith("www") || target.StartsWith("http")))
                OpenWithDefaultApp(target);
        }


        public static void OpenWithDefaultApp(string path)
        {
            using Process fileopener = new Process();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

    }
}
