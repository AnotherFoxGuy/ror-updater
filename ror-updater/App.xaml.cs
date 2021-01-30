// This file is part of ror-updater
// 
// Copyright (c) 2016 AnotherFoxGuy
// 
// ror-updater is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 3, as
// published by the Free Software Foundation.
// 
// ror-updater is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ror-updater. If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IniParser;
using IniParser.Model;
using Newtonsoft.Json;

namespace ror_updater
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string ServerUrl = "https://vps.anotherfoxguy.com";
        public ReleaseInfo ReleaseInfoData;
        public Branch SelectedBranch;

        public static UpdateChoice Choice;

        private bool _bInit;
        private bool _bSkipUpdates;
        private bool _bSelfUpdating;

        private BackgroundWorker _initDialog = new BackgroundWorker();

        private PageSwitcher _pageSwitcher;

        private StartupForm _sForm;

        private FileIniDataParser _iniDataParser;
        private IniData _iniSettingsData;

        public string LocalVersion;

        private string _localUpdaterVersion;

        private WebClient _webClient;


        public void InitApp(object sender, StartupEventArgs e)
        {
            File.WriteAllText(@"./Updater_log.txt", "");

            //Show something so users don't get confused
            _initDialog.DoWork += InitDialog_DoWork;
            _initDialog.RunWorkerAsync();
            
            Utils.LOG("Info| Creating IpfsEngine");
            SIpfsEngine.Instance.Init();

            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            _localUpdaterVersion = fileVersionInfo.ProductVersion;
            Utils.LOG($"Info| Updater version: {_localUpdaterVersion}");

            Utils.LOG("Info| Creating Web Handler");
            _webClient = new WebClient();
            Utils.LOG("Info| Done.");

            Utils.LOG("Info| Creating INI handler");

            //Proceed
            _iniDataParser = new FileIniDataParser();
            _iniDataParser.Parser.Configuration.CommentString = "#";

            //Dirty code incoming!
            try
            {
                _iniSettingsData = _iniDataParser.ReadFile("./updater.ini", Encoding.ASCII);
            }
            catch (Exception ex)
            {
                Utils.ProcessBadConfig(ex);
            }

            Thread.Sleep(100); //Wait a bit

            try
            {
                _bSkipUpdates = bool.Parse(_iniSettingsData["Dev"]["SkipUpdates"]);
            }
            catch (Exception ex)
            {
                Utils.ProcessBadConfig(ex);
            }

            Utils.LOG("Info| Done.");
            Utils.LOG($"Info| Skip_updates: {_bSkipUpdates}");

            //Get app version
            MessageBoxResult result;

            //Download list
            Utils.LOG($"Info| Downloading main list from server: {ServerUrl}/branches.json");

            BranchInfo branchInfo = null;

            try
            {
                var brjson = _webClient.DownloadString($"{ServerUrl}/branches.json");
                branchInfo = JsonConvert.DeserializeObject<BranchInfo>(brjson);
                SelectedBranch = branchInfo.Branches[0];

                var t = SIpfsEngine.Instance.Engine.FileSystem.ReadAllTextAsync(SelectedBranch.Hash);
                t.Wait();

                var dat = t.Result;
                
                Utils.LOG($"DATA: {dat}");

                ReleaseInfoData = JsonConvert.DeserializeObject<ReleaseInfo>(dat);

                Utils.LOG($"Info| Updater: {branchInfo.UpdaterVersion}");
                Utils.LOG($"Info| Rigs-of-Rods: {ReleaseInfoData.Version}");
                Utils.LOG("Info| Done.");
            }
            catch (Exception ex)
            {
                Utils.LOG(ex.ToString());
                result = MessageBox.Show("Could not connect to the main server.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    Utils.LOG("Error| Failed to connect to server.");
                    Quit();
                }
            }

            if (_localUpdaterVersion != branchInfo?.UpdaterVersion && !_bSkipUpdates)
                ProcessSelfUpdate();

            Thread.Sleep(10); //Wait a bit
            
            try
            {
                //Use Product version instead of file version because we can use it to separate Dev version from release versions, same for TestBuilds
                var versionInfo = FileVersionInfo.GetVersionInfo("RoR.exe");
                LocalVersion = versionInfo.ProductVersion;

                Utils.LOG("Info| local RoR ver: " + LocalVersion);
            }
            catch
            {
                LocalVersion = "unknown";

                Utils.LOG("Info| Game Not found!");
            }

            Utils.LOG("Info| Done.");
            Utils.LOG("Succes| Initialization done!");

            _bInit = true;

            _initDialog = null; //We don't need it anymore.. :3

            _pageSwitcher = new PageSwitcher();
            _pageSwitcher.Show();
            _pageSwitcher.Activate();
        }

        void ProcessSelfUpdate()
        {
            _bSelfUpdating = true;

            _webClient.DownloadFile(ServerUrl + "ror-updater_new.exe", @"./ror-updater_new.exe");
            _webClient.DownloadFile(ServerUrl + "ror-updater_selfupdate.exe", @"./ror-updater_selfupdate.exe");
            
            Thread.Sleep(100); //Wait a bit
            Process.Start(@"./ror-updater_selfupdate.exe");

            Quit();
        }

        public static void Quit()
        {
            Process.GetCurrentProcess().Kill();
        }

        private void InitDialog_DoWork(object sender, DoWorkEventArgs e)
        {
            // Very dirty way to do this. :/
            _sForm = new StartupForm();
            _sForm.Show();


            while (!_bInit)
            {
                //meh?
                Thread.Sleep(500);
                if(_bSelfUpdating)
                    _sForm.label1.Text = "Updating...";
                _sForm.Update();
            }

            _sForm.Hide();
            _sForm = null;
        }

        #region Singleton

        private static Lazy<App> _lazyApp;

        public static App Instance => _lazyApp.Value;

        private App()
        {
            _lazyApp = new Lazy<App>(() => this);
        }

        #endregion
    }

    public enum UpdateChoice
    {
        INSTALL,
        UPDATE,
        REPAIR
    }
}