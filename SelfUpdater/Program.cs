using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

var tmp = $"{Path.GetTempPath()}/ror-updater";
var dest = Directory.GetCurrentDirectory();
var zipPath = $"{Path.GetTempPath()}/patch.zip";

Thread.Sleep(100); //Sleep a bit before doing anything
if (File.Exists(zipPath))
{
    Directory.CreateDirectory(tmp);
    ZipFile.ExtractToDirectory(zipPath, tmp);

    //Now Create all of the directories
    foreach (var dirPath in Directory.GetDirectories(tmp, "*",
        SearchOption.AllDirectories))
        Directory.CreateDirectory(dirPath.Replace(tmp, dest));

    //Copy all the files & Replaces any files with the same name
    foreach (var newPath in Directory.GetFiles(tmp, "*.*",
        SearchOption.AllDirectories))
        File.Copy(newPath, newPath.Replace(tmp, dest), true);

    Directory.Delete(tmp, true);
}

Thread.Sleep(100); //Sleep a bit before doing anything
Process.Start("ror-updater.exe");