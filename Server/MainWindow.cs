using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using Newtonsoft.Json;

namespace ror_updater_list_maker
{
    public partial class MainWindow : Form
    {
        private static string basepath = @"./redist/";
        private List<PFileInfo> _filelist = new List<PFileInfo>();

        //Get the file's hash in SHA512
        public string GetFileHash(string file)
        {
            var sha = new SHA512Managed();
            var stream = File.OpenRead(file);
            var hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var filePaths = Directory.GetFiles(basepath, "*.*", SearchOption.AllDirectories);
            label1.Text = "0/" + filePaths.Length;
            progressBar1.Maximum = filePaths.Length;
            var i = 0;
            foreach (var fileName in filePaths)
            {
                Thread.Sleep(10);

                var fileInfo = new FileInfo(fileName);
                var fileD = fileInfo.Directory?.ToString();
                var s = fileD.Substring(fileD.LastIndexOf((char) 92 + "redist") + 1);
                s = s.Replace("redist", ".");
                s = s.Replace("" + (char) 92, "/");

                s = s == "." ? s.Replace(".", "./") : s + "/";

                _filelist.Add(new PFileInfo
                {
                    Name = fileInfo.Name,
                    Directory = s,
                    Hash = GetFileHash(fileInfo.FullName)
                });

                i++;
                label1.Text = i + "/" + filePaths.Length;

                progressBar1.Value = i;
                Application.DoEvents();
            }

            var versionInfo = FileVersionInfo.GetVersionInfo("redist/RoR.exe");
            var strLocalVersion = versionInfo.ProductVersion;


            File.WriteAllText("./redist/info.json", JsonConvert.SerializeObject(
                new ReleaseInfo
                {
                    Version = strLocalVersion,
                    Filelist = _filelist
                }
            ));

            File.WriteAllText("./branches.json", JsonConvert.SerializeObject(
                new BranchInfo
                {
                    UpdaterVersion = "1.10",
                    Branches = new List<Branch>
                    {
                        new Branch {Name = "hash", Hash = "5ds5s1f5sf"},
                        new Branch {Name = "sdsds", Hash = "5ds5sdsfsd1f5sf"}
                    }
                }
            ));

            MessageBox.Show("Done!");
        }
    }
}