﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using ror_updater;

namespace list_generator
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var cmd = new RootCommand
            {
                new Argument<string>("path", "Path to files"),
                new Option<string?>(new[] {"--branch", "-b"}, "The Branch name"),
            };

            cmd.Handler = CommandHandler.Create<string, string?, IConsole>(GenerateJsonInfo);

            return cmd.Invoke(args);
        }

        static void GenerateJsonInfo(string path, string? branchname, IConsole console)
        {
            if (string.IsNullOrEmpty(branchname))
                branchname = "Release";

            Console.WriteLine(path);

            var fullPath = Path.GetFullPath(path);

            List<PFileInfo> _filelist = new List<PFileInfo>();
            var filePaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            var i = 0;
            foreach (var fileName in filePaths)
            {
                Thread.Sleep(10);

                var fileInfo = new FileInfo(fileName);
                var fileD = fileInfo.Directory?.ToString();
                var dir = fileD.Replace(fullPath, ".");

                _filelist.Add(new PFileInfo
                {
                    Name = fileInfo.Name,
                    Directory = dir.Replace("\\", "/"),
                    Hash = Utils.GetFileHash(fileInfo.FullName)
                });

                Console.Write($"\r{i}/{filePaths.Length}\r");

                i++;
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(path + "/RoR.exe");
            var strLocalVersion = versionInfo.ProductVersion;


            File.WriteAllText(path + "/info.json", JsonConvert.SerializeObject(
                new ReleaseInfo
                {
                    Version = strLocalVersion,
                    Filelist = _filelist
                }
            ));
            

            File.WriteAllText("./branches.json.example", JsonConvert.SerializeObject(
                new BranchInfo
                {
                    UpdaterVersion = "1.10",
                    Branches = new Dictionary<string, Branch>
                    {
                        {branchname, new Branch {Name = branchname, Url = $"/{branchname.ToLower()}/"}}
                    }
                }
            ));

            Console.WriteLine("\nDone");
        }
    }
}