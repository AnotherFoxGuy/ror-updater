using System.Collections.Generic;

public class ReleaseInfo
{
    public string Version;
    public List<PFileInfo> Filelist;
}

public class PFileInfo
{
    public string Name;
    public string Hash;
    public string Directory;
}

public class Branch
{
    public string Name;
    public string Hash;
}

public class BranchInfo
{
    public string UpdaterVersion;
    public List<Branch> Branches;
}


