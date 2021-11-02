using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using NUnit.Framework;

namespace URL_tester
{
    [TestFixture]
    public class TestUrLs
    {
        private WebClient _webClient;
        private string _serverUrl = "https://update.rigsofrods.org";
        //private string _serverUrl = "http://127.0.0.1:8080";

        [SetUp]
        public void SetUp()
        {
            _webClient = new WebClient();
        }

        [Test]
        public void ValidateBranches()
        {
            var brjson = _webClient.DownloadString($"{_serverUrl}/branches.json");
            var branchInfo = JsonConvert.DeserializeObject<BranchInfo>(brjson);
            Assert.IsNotNull(branchInfo.UpdaterVersion);
            Assert.IsNotEmpty(branchInfo.Branches);
        }

        [Test]
        public void CheckIfInfoExists()
        {
            var brjson = _webClient.DownloadString($"{_serverUrl}/branches.json");
            var branchInfo = JsonConvert.DeserializeObject<BranchInfo>(brjson);
            foreach (var branch in branchInfo.Branches)
            {
                var infoURL = $"{_serverUrl}/{branch.Value.Url}";
                Console.WriteLine($"Checking \"{infoURL}/info.json\"");
                var dat = _webClient.DownloadString($"{infoURL}/info.json");
                var releaseInfoData = JsonConvert.DeserializeObject<ReleaseInfo>(dat);
                Assert.IsNotNull(releaseInfoData.Version);
                Assert.IsNotEmpty(releaseInfoData.Filelist);
                Assert.IsNotNull(releaseInfoData.Filelist.First(i => i.Name == "RoR.exe"));
            }
        }

        [Test]
        public void CheckIfFilesExists()
        {
            var brjson = _webClient.DownloadString($"{_serverUrl}/branches.json");
            var branchInfo = JsonConvert.DeserializeObject<BranchInfo>(brjson);
            foreach (var branch in branchInfo.Branches)
            {
                var branchUrl = $"{_serverUrl}/{branch.Value.Url}";
                Console.WriteLine($"Checking \"{branchUrl}/info.json\"");
                var dat = _webClient.DownloadString($"{branchUrl}/info.json");
                var releaseInfoData = JsonConvert.DeserializeObject<ReleaseInfo>(dat);
                foreach (var fileInfo in releaseInfoData.Filelist)
                {
                    var dlLink = $"{branchUrl}/{fileInfo.Directory.Replace(".", "")}/{fileInfo.Name}";
                    Console.Write($"Checking \"{dlLink}\" ");
                    var http = (HttpWebRequest) WebRequest.Create(dlLink);
                    using var response = (HttpWebResponse) http.GetResponse();
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    Console.Write($"Status: \"{response.StatusCode}\"\n");
                }
            }
        }
    }
}