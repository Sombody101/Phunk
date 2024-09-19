using Phunk.MVVM.ViewModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Phunk.Core;

public class ReqChecker
{
    private static readonly Regex versionMatch = new(@"version ""(\d+(\.\d+(_\d+)?)?)");

    public GlobalViewModel GlobalViewModel { get; } = GlobalViewModel.Instance;

    // public bool Check(string value, bool checkJava = false)
    // {
    //     return !checkJava
    //         ? IsCommandAvailable(value)
    //         : IsJavaVersionValid();
    // }

    private static bool IsCommandAvailable(string command)
    {
        try
        {
            using Process process = new();

            process.StartInfo = new()
            {
                FileName = "where",
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return !string.IsNullOrWhiteSpace(output);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool IsJavaVersionValid()
    {
        string versionstr = string.Empty;

        try
        {
            using Process process = new();

            if (GlobalViewModel.JavaPathFolderSettingsTxt.Length != 0)
            {
                GlobalViewModel.PhunkLogs = $"\n[Phunk] using JAVA from {GlobalViewModel.JavaPathFolderSettingsTxt}";
            }

            process.StartInfo = new()
            {
                Arguments = "-version",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = !GlobalViewModel.IsCustomJavaPath && GlobalViewModel.JavaPathFolderSettingsTxt.Length == 0
                    ? "java"
                    : Path.Combine(GlobalViewModel.JavaPathFolderSettingsTxt, "/bin/java.exe"),
            };

            process.Start();
            string output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Match match = versionMatch.Match(output);

                if (match.Success)
                {
                    string versionString = match.Groups[1].Value.Trim();
                    versionstr = versionString;

                    if (Version.TryParse(versionString, out Version version) && version.Major >= 18)
                    {
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GlobalViewModel.PhunkLogs += "\n[Phunk] There was an error checking for the requirements. " + ex.ToString();
            GlobalViewModel.CanStart = true;
        }

        GlobalViewModel.PhunkLogs += "\n[Phunk] Version is not up to date: " + versionstr;
        return false;
    }
}
