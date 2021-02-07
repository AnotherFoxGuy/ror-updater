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
using System.Diagnostics;
using System.IO;
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

        private Settings _settings;


        public void InitApp(object sender, StartupEventArgs e)
        {
            var currdir = Directory.GetCurrentDirectory();
            File.WriteAllText($"{currdir}/Updater_log.txt", "");

            SentrySdk.ConfigureScope(scope => { scope.AddAttachment($"{currdir}/Updater_log.txt"); });

            _sForm = new StartupForm();
            _sForm.Show();

            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            _localUpdaterVersion = fileVersionInfo.ProductVersion;
            Utils.LOG($"Info| Updater version: {_localUpdaterVersion}");

            Utils.LOG("Info| Creating Web Handler");
            _webClient = new WebClient();
            Utils.LOG("Info| Done.");

            if (File.Exists($"{currdir}/ror-updater-settings.json"))
            {
                try
                {
                    var set = File.ReadAllText($"{currdir}/ror-updater-settings.json");
                    _settings = JsonConvert.DeserializeObject<Settings>(set);
                }
                catch (Exception ex)
                {
                    Utils.LOG("Error| Failed to read settings file");
                    Utils.LOG(ex.ToString());
                    SentrySdk.CaptureException(ex);
                    _settings = new Settings();
                    _settings.SetDefaults();
                }
            }
            else
            {
                _settings = new Settings();
                _settings.SetDefaults();
            }

            CDNUrl = _settings.ServerUrl;
            Thread.Sleep(100);

            Utils.LOG("Info| Done.");
            Utils.LOG($"Info| Skip_updates: {_settings.SkipUpdates}");

            //Download list
            Utils.LOG($"Info| Downloading main list from server: {_settings.ServerUrl}/branches.json");
            try
            {
                var brjson = _webClient.DownloadString($"{_settings.ServerUrl}/branches.json");
                BranchInfo = JsonConvert.DeserializeObject<BranchInfo>(brjson);
                UpdateBranch(
                    BranchInfo.Branches.Find(
                        b => b.Name.Equals(_settings.Branch, StringComparison.OrdinalIgnoreCase)) ??
                    BranchInfo.Branches[0]);
            }
            catch (Exception ex)
            {
                Utils.LOG(ex.ToString());
                SentrySdk.CaptureException(ex);
                var result = MessageBox.Show("Could not connect to the main server.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    Utils.LOG("Error| Failed to connect to server.");
                    Quit();
                }
            }

            if (_localUpdaterVersion != BranchInfo?.UpdaterVersion && !_settings.SkipUpdates)
            {
                _sForm.label1.Text = @"Updating...";
                ProcessSelfUpdate();
            }

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
            Utils.LOG("Success| Initialization done!");

            _sForm.Close();

            _pageSwitcher = new PageSwitcher();
            _pageSwitcher.Show();
            _pageSwitcher.Activate();
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
                Utils.LOG($"Downloading {_settings.ServerUrl}/selfupdate.exe");
                _webClient.DownloadFile($"{_settings.ServerUrl}/selfupdate.exe",
                    $"{currdir}/ror-updater-selfupdate.exe");
                Utils.LOG($"Downloading {_settings.ServerUrl}/patch.zip");
                _webClient.DownloadFile($"{_settings.ServerUrl}/patch.zip", $"{Path.GetTempPath()}/patch.zip");

                Thread.Sleep(100); //Wait a bit
                Process.Start($"{currdir}/ror-updater-selfupdate.exe");
            }
            catch (Exception ex)
            {
                Utils.LOG(ex.ToString());
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
            var dat = JsonConvert.SerializeObject(_settings);
            File.WriteAllText($"{Directory.GetCurrentDirectory()}/ror-updater-settings.json", dat);
        }

        public void UpdateBranch(Branch br)
        {
            SelectedBranch = br;
            _settings.Branch = SelectedBranch.Name;

            CDNUrl = br.Url.Contains("http") ? br.Url : $"{_settings.ServerUrl}/{br.Url}";

            var dat = _webClient.DownloadString($"{CDNUrl}/info.json");
            ReleaseInfoData = JsonConvert.DeserializeObject<ReleaseInfo>(dat);

            Utils.LOG($"Info| Switched to branch: {SelectedBranch.Name} Version: {ReleaseInfoData.Version}");
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