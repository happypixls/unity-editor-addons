using System;
using System.IO;

//From here https://forum.unity.com/threads/dllnotfoundexception-when-depend-on-another-dll.31083/

public static class DLLResolver
{
    static DLLResolver()
    {
        var CurrentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
        var DLLPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + "Plugins";
        if(CurrentPath.Contains(DLLPath) == false)
        {
            Environment.SetEnvironmentVariable("PATH", CurrentPath + Path.PathSeparator + DLLPath, EnvironmentVariableTarget.Process);
        }
    }
}