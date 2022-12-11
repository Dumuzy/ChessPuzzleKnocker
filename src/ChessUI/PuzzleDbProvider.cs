using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using AwiUtils;

namespace ChessUI
{

    /// <summary> This class gets a lichess_db_puzzle.csv or lic_part_puzzle.csv from somewhere. 
    /// It downloads and uncompresses such files if needed. </summary>
    static internal class PuzzleDbProvider
    {
        public const string LichessCsvPartBase = "lic_part_puzzle";
        public const string LichessCsvFileName = "lichess_db_puzzle.csv";
        public static string LichessCsvDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

        static public string GetLichessCsvFile()
        {
            string fileName = null;
            if (File.Exists(LichessCsvFileName))
                fileName = LichessCsvFileName;
            else
                fileName = TryDownloadLichessPart();
            if (fileName == null)
            {
                DownloadLichessCsvIfNeeded();
                if (File.Exists(LichessCsvFileName))
                    fileName = LichessCsvFileName;
            }
            return fileName;
        }

        static private string GetUncompressedCsvGz()
        {
            PuzzleCompressor.DecompressAllCsvGzFiles(LichessCsvPartBase);
            var licFiles = Directory.EnumerateFiles(".", LichessCsvPartBase + "*.csv").Select(f => new FileInfo(f));
            licFiles.OrderByDescending(f => f.Length);
            return licFiles.FirstOrDefault()?.Name;
        }

        static private string TryDownloadLichessPart()
        {
            var licPartFile = "lichess_db_puzzle.csv";
            if (!File.Exists(licPartFile))
            {
                var licPartUrls = @"
                http://99-developer-tools.com/wp-content/uploads/2022/12/lichess_db_puzzle.csv.gz
                http://schachclub-ittersbach.de/wordpress/wp-content/uploads/2022/10/lic_part_puzzle-100000.csv.gz
                "
                        .SplitToLines();
                foreach (var url in licPartUrls)
                {
                    try
                    {
                        var client = new WebClient();
                        string fn = Path.GetFileName(url);
                        client.DownloadFile(url, fn);
                        if (File.Exists(fn))
                            break;
                    }

                    catch (Exception) { }
                }
            }
            return GetUncompressedCsvGz();
        }

        static private void DownloadLichessCsvIfNeeded()
        {
            if (!File.Exists(LichessCsvFileName))
            {
                MessageBox.Show($@"You must download 
    https://database.lichess.org/lichess_db_puzzle.csv.bz2 
now and extract it to 
    {LichessCsvDirectory}. 
Press OK when done.",
                    "Attention", MessageBoxButtons.OKCancel);
            }
        }
    }
}
