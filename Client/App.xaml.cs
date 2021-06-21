﻿// This file is part of ror-updater
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;
using Sentry;

namespace ror_updater
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ReleaseInfo ReleaseInfoData;
        public Branch SelectedBranch;
        public BranchInfo BranchInfo;
        public string CDNUrl;

        public static UpdateChoice Choice;

        private PageSwitcher _pageSwitcher;

        private WebClient _webClient;

        private StartupForm _sForm;

        public string LocalVersion;

        private string _localUpdaterVersion;

        public Settings Settings;


        public void InitApp(object sender, StartupEventArgs e)
        {
            _sForm = new StartupForm();
            _sForm.Show();
            // Render the form
            _sForm.Update();

            File.WriteAllText(Utils.LogPath, "Updater Started\n");

            SentrySdk.ConfigureScope(scope => { scope.AddAttachment(Utils.LogPath); });

            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            _localUpdaterVersion = fileVersionInfo.ProductVersion;
            Utils.LOG(Utils.LogVerb.DEBUG, $"Updater version: {_localUpdaterVersion}");

            /* TODO: Not working?
             if (File.Exists("RoR.exe") && Utils.FileIsInUse("RoR.exe"))
            {
                Utils.LOG($"game in use");
                MessageBox.Show("Please close the game before starting the updater", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Quit();
            }*/

            Utils.LOG(Utils.LogVerb.INFO, "Creating Web Handler");
            _webClient = new WebClient();
            Utils.LOG(Utils.LogVerb.INFO, "Done.");

            var currentDirectory = Directory.GetCurrentDirectory();

            if (File.Exists($"{currentDirectory}/ror-updater-settings.json"))
            {
                try
                {
                    var set = File.ReadAllText($"{currentDirectory}/ror-updater-settings.json");
                    Settings = JsonConvert.DeserializeObject<Settings>(set);
                }
                catch (Exception ex)
                {
                    Utils.LOG(Utils.LogVerb.ERROR, "Failed to read settings file");
                    Utils.LOG(Utils.LogVerb.ERROR, ex.ToString());
                    SentrySdk.CaptureException(ex);
                    Settings = new Settings();
                    Settings.SetDefaults();
                }
            }
            else
            {
                Settings = new Settings();
                Settings.SetDefaults();
            }

            CDNUrl = Settings.ServerUrl;

            Utils.LOG(Utils.LogVerb.INFO, "Done.");
            Utils.LOG(Utils.LogVerb.INFO, $"Skip_updates: {Settings.SkipUpdates}");

            //Download list
            Utils.LOG(Utils.LogVerb.INFO, $"Downloading main list from server: {Settings.ServerUrl}/branches.json");
            try
            {
                var brjson = _webClient.DownloadString($"{Settings.ServerUrl}/branches.json");
                BranchInfo = JsonConvert.DeserializeObject<BranchInfo>(brjson);
                UpdateBranch(Settings.Branch);
            }
            catch (Exception ex)
            {
                Utils.LOG(Utils.LogVerb.ERROR, ex.ToString());
                SentrySdk.CaptureException(ex);
                var result = MessageBox.Show("Could not connect to the main server.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    Utils.LOG(Utils.LogVerb.ERROR, "Failed to connect to server.");
                    Quit();
                }
            }

            Utils.LOG(Utils.LogVerb.DEBUG, $"Online updater version: {BranchInfo?.UpdaterVersion}");
            if (_localUpdaterVersion != BranchInfo?.UpdaterVersion && !Settings.SkipUpdates)
            {
                _sForm.label1.Text = @"Updating...";
                ProcessSelfUpdate();
            }

            try
            {
                //Use Product version instead of file version because we can use it to separate Dev version from release versions, same for TestBuilds
                var versionInfo = FileVersionInfo.GetVersionInfo("RoR.exe");
                LocalVersion = versionInfo.ProductVersion;

                Utils.LOG(Utils.LogVerb.INFO, "Local RoR version: " + LocalVersion);
            }
            catch
            {
                LocalVersion = "unknown";

                Utils.LOG(Utils.LogVerb.INFO, "Game Not found!");
            }

            Utils.LOG(Utils.LogVerb.INFO, "Done.");
            Utils.LOG(Utils.LogVerb.INFO, "Initialization done!");

            _pageSwitcher = new PageSwitcher();
            _pageSwitcher.Show();
            _pageSwitcher.Activate();
            _sForm.Close();
        }

        void ProcessSelfUpdate()
        {
            var result = MessageBox.Show("There is an update available, do you want to install it now?",
                "Update available ", MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var currdir = Directory.GetCurrentDirectory();
                Utils.LOG(Utils.LogVerb.INFO, $"Downloading {Settings.ServerUrl}/selfupdate.exe");
                _webClient.DownloadFile($"{Settings.ServerUrl}/selfupdate.exe",
                    $"{currdir}/ror-updater-selfupdate.exe");
                Utils.LOG(Utils.LogVerb.INFO, $"Downloading {Settings.ServerUrl}/patch.zip");
                _webClient.DownloadFile($"{Settings.ServerUrl}/patch.zip", $"{Path.GetTempPath()}/patch.zip");

                Thread.Sleep(100); //Wait a bit
                Process.Start($"{currdir}/ror-updater-selfupdate.exe");
            }
            catch (Exception ex)
            {
                Utils.LOG(Utils.LogVerb.ERROR, ex.ToString());
                MessageBox.Show("SelfUpdate error", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                SentrySdk.CaptureException(ex);
            }

            Quit();
        }

        private void Quit()
        {
            Process.GetCurrentProcess().Kill();
        }

        public void SaveSettings()
        {
            var dat = JsonConvert.SerializeObject(Settings);
            File.WriteAllText($"{Directory.GetCurrentDirectory()}/ror-updater-settings.json", dat);
        }

        public void UpdateBranch(string branchname)
        {
            try
            {
                SelectedBranch = BranchInfo.Branches[branchname];
            }
            catch (Exception ex)
            {
                Utils.LOG(Utils.LogVerb.ERROR, $"Failed to switch to branch {branchname}");
                Utils.LOG(Utils.LogVerb.ERROR, ex.ToString());
                SelectedBranch = BranchInfo.Branches.First().Value;
            }

            Settings.Branch = branchname;

            CDNUrl = SelectedBranch.Url.Contains("http")
                ? SelectedBranch.Url
                : $"{Settings.ServerUrl}/{SelectedBranch.Url}";

            try
            {
                var dat = _webClient.DownloadString($"{CDNUrl}/info.json");
                ReleaseInfoData = JsonConvert.DeserializeObject<ReleaseInfo>(dat);
            }
            catch (Exception ex)
            {
                Utils.LOG(Utils.LogVerb.ERROR, ex.ToString());
                MessageBox.Show("Failed to download branch info", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                SentrySdk.CaptureException(ex);
            }

            Utils.LOG(Utils.LogVerb.INFO,
                $"Switched to branch: {SelectedBranch.Name} Version: {ReleaseInfoData.Version}");
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException(e.Exception);

            // If you want to avoid the application from crashing:
            //e.Handled = true;
        }

        #region Singleton

        private static Lazy<App> _lazyApp;

        public static App Instance => _lazyApp.Value;

        private App()
        {
#if !DEBUG
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            SentrySdk.Init("https://c34f44d72cbc461e9787103e1474f04a@o84816.ingest.sentry.io/5625812");
#endif
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