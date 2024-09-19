using Microsoft.Win32;
using Newtonsoft.Json;
using Phunk.Core;
using Phunk.MVVM.View.Windows;
using Phunk.Utils;
using System;
using System.CodeDom;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Phunk.MVVM.ViewModel;

public class MainViewModel : ObservableObject
{
    private const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.6446.71 Safari/537.36";

    private static readonly HttpClient httpClient = new();

    public GlobalViewModel GlobalViewModel { get; } = GlobalViewModel.Instance;

    #region APK
    private string? _filePath;
    public string? FilePath
    {
        get => _filePath;
        set { _filePath = value; OnPropertyChanged(); }
    }

    private readonly StringBuilder _apkName = new();
    public string ApkName
    {
        get => _apkName.ToString();
        set
        {
            _ = _apkName.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    private readonly StringBuilder _originalPackageName = new();
    public string OriginalPackageName
    {
        get => _originalPackageName.ToString();
        set
        {
            _ = _originalPackageName.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    private readonly StringBuilder _currentPackageName = new();
    public string CurrentPackageName
    {
        get => _currentPackageName.ToString();
        set
        {
            _ = _currentPackageName.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    #endregion


    #region RELAY COMMANDS

    public RelayCommand StartCommand { get; set; }
    public RelayCommand SettingsCommand { get; set; }
    public RelayCommand SelectAPKCommand { get; set; }
    public RelayCommand CreditsCommand { get; set; }
    public RelayCommand ClearLogsCommand { get; set; }

    public RelayCommand OpenTempCommand { get; set; }

    #endregion

    #region DOWNLOADING


    private readonly StringBuilder _currentFilePath = new();
    public string? CurrentFilePath
    {
        get => _currentFilePath.ToString();
        set
        {
            _ = _currentFilePath.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    #endregion

    public MainViewModel()
    {
        Initialize();

        // Setup the shared HttpClient
        httpClient.DefaultRequestHeaders.Add("user-agent", userAgent);

        StartCommand = new RelayCommand(async o =>
        {
            GlobalViewModel.StatusText = string.Empty;

            try
            {
                GlobalViewModel.CanStart = false;
                GlobalViewModel.PhunkLogs = "\n[Phunk] ~ Starting Phunker!";
                await PerformTaskAsync();
            }
            catch (Exception ex)
            {
                GlobalViewModel.StatusText = ex.Message;
            }
        });

        SettingsCommand = new RelayCommand(o =>
        {
            SettingsWindow settingsWindow = new();
            settingsWindow.Show();
        });

        OpenTempCommand = new RelayCommand(o =>
        {
            _ = Process.Start("explorer.exe", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp"));
        });

        SelectAPKCommand = new RelayCommand(o =>
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "APK files (*.apk)|*.apk"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                GlobalViewModel.PhunkLogs = string.Empty;
                string selectedFilePath = openFileDialog.FileName;
                ApkName = Path.GetFileName(selectedFilePath);
                FilePath = selectedFilePath;
                GlobalViewModel.PhunkLogs = $"[Phunk] ~ Selected {ApkName}\n[Phunk] ~ {FilePath} ";
                GlobalViewModel.CanStart = true;
            }
        });

        CreditsCommand = new RelayCommand(o =>
        {
            CreditsWindow window = new();
            window.Show();
        });

        ClearLogsCommand = new RelayCommand(o =>
        {
            GlobalViewModel.PhunkLogs = string.Empty;
        });
    }

    /// <summary>
    /// When the application starts up, we call the Initialize function first
    /// </summary>
    private void Initialize()
    {
        // UI
        GlobalViewModel.StatusText = "(￢з￢) Waiting for User";
        GlobalViewModel.CanStart = false;
        GlobalViewModel.ProgressValue = 0;
        GlobalViewModel.IsProcessStarting = false;

        // Settings
        GlobalViewModel.FinalOutputNameSettingsTxt = string.Empty;
        GlobalViewModel.DecompileAdditionalParamsSettingsTxt = string.Empty;
        GlobalViewModel.SigningZipaligningParamsSettingsTxt = string.Empty;
        GlobalViewModel.CustomPackageNameSettingsTxt = string.Empty;
        GlobalViewModel.IsCustomJavaPath = false;
        GlobalViewModel.JavaPathFolderSettingsTxt = string.Empty;

        GlobalViewModel.AutoCleanSettingsBoolean = false;
        GlobalViewModel.UseApkToolSettingsBoolean = false;
        GlobalViewModel.AutoUpdatePhunkSettingsBoolean = false;

        // Apk
        OriginalPackageName = "n/a";
        FilePath = "n/a";
        ApkName = "n/a";

        _ = Directory.CreateDirectory("bin");
        _ = Directory.CreateDirectory("temp");
    }

    /// <summary>
    /// Before the whole process starts, there will be a clean up first
    /// </summary>
    private async Task CleanUp()
    {
        PhunkLog("Cleaning up Phunk...", "((⇀‸↼)) Cleaning up first!");

        await Task.Run(() =>
        {
            Directory.Delete("temp", true);
            _ = Directory.CreateDirectory("temp");
        });


        PhunkLog("Ready to go!");
        PhunkLog("Extracting APK");
    }

    /// <summary>
    /// Performs the Task Asynchronously
    /// </summary>
    /// <returns></returns>
    private async Task PerformTaskAsync()
    {
        GlobalViewModel.IsProcessStarting = true;
        GlobalViewModel.StatusText = "((⇀‸↼)) Checking Requirements";
        await Task.Delay(500);
        await CheckDependency();

        GlobalViewModel.ProgressValue = 0;
        if (GlobalViewModel.MetRequirements)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string binFolderPath = Path.Combine(appDirectory, "bin");
            if (!File.Exists(Path.Combine(binFolderPath, "apktool.jar"))
                && !File.Exists(Path.Combine(binFolderPath, "apktool.bat"))
                && !File.Exists(Path.Combine(binFolderPath, "uberapksigner.jar")))
            {

                // Continue with the task
                GlobalViewModel.StatusText = "((⇀‸↼)) Downloading Required Files";
                GlobalViewModel.PhunkLogs = "\n[Phunk] ~ Downloading Files";

                // Download the required files
                await DownloadFiles();

                GlobalViewModel.StatusText = "(人´∀`) Success";

                await Task.Delay(500);
            }

            await CleanUp();

            ApkHandler handler = new();

            GlobalViewModel.ProgressValue = 0;
            GlobalViewModel.StatusText = "((⇀‸↼)) Extracting the APK file using apktool";
            await DecompileApk(binFolderPath, handler);

            GlobalViewModel.ProgressValue = 0;
            GlobalViewModel.StatusText = "((⇀‸↼)) Streaming AndroidManifest.xaml";
            GlobalViewModel.PhunkLogs = "\n[Phunk] ~ Streaming AndroidManifest.xaml and apktool.yaml";
            await StreamApk();

            GlobalViewModel.ProgressValue = 50;
            GlobalViewModel.StatusText = "((⇀‸↼)) Replacing all files & folder names, and inside the files aswell";
            GlobalViewModel.PhunkLogs = "\n[Phunk] ~ Replaced all files & folder names";
            _ = await ReplaceNames();

            GlobalViewModel.ProgressValue = 100;
            GlobalViewModel.StatusText = "(人´∀`) Successfully Replaced";
            GlobalViewModel.PhunkLogs = "\n[Phunk] ~ Success";

            await Task.Delay(500);

            GlobalViewModel.ProgressValue = 0;
            GlobalViewModel.StatusText = "((⇀‸↼)) Building APK using apktool";
            GlobalViewModel.PhunkLogs = "\n[Phunk] ~ Building APK using apktool";
            await BuildApk(binFolderPath, handler);

            GlobalViewModel.ProgressValue = 0;
            GlobalViewModel.StatusText = "((⇀‸↼)) Signing and Zipaligning apk";
            GlobalViewModel.PhunkLogs = "\n[Phunk] ~ Signing and Zipaligning Apk";

            await SignApk(binFolderPath, handler);

            GlobalViewModel.StatusText = "((⇀‸↼)) Preparing for final output";
            GlobalViewModel.PhunkLogs = "\n[Phunk] ~ Preparing for final output";
            await FinalTaskOut();

            GlobalViewModel.ProgressValue = 100;
            GlobalViewModel.StatusText = "(人´∀`) Success! -> Saved as " + GlobalViewModel.FinalOutputNameSettingsTxt;
            GlobalViewModel.PhunkLogs += "\n[Phunk] ~ Success! -> Saved as " + GlobalViewModel.FinalOutputNameSettingsTxt;

            GlobalViewModel.CanStart = true;
            _ = Process.Start("explorer.exe", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp"));
            //AdonisUI.Controls.MessageBox.Show("The Package Name has been replaced! You can now sideload the game!", "(ノ^∇^) Success!", AdonisUI.Controls.MessageBoxButton.OK, AdonisUI.Controls.MessageBoxImage.None);
            GlobalViewModel.IsProcessStarting = false;
        }
        else
        {
            GlobalViewModel.StatusText = "( ´-ω-` ) Can't start the task because you having missing requirements: " + GlobalViewModel.MissingRequirements;
            GlobalViewModel.IsProcessStarting = false;
        }
    }

    /// <summary>
    /// Checks if the requirements are met first before performing the actual task
    /// </summary>
    /// <returns></returns>
    private Task CheckDependency()
    {
        ReqChecker reqChecker = new();

        try
        {
            bool javaAvailable = reqChecker.IsJavaVersionValid();

            GlobalViewModel.MissingRequirements = !javaAvailable
                ? " Java 8+ "
                : string.Empty;

            if (string.IsNullOrEmpty(GlobalViewModel.MissingRequirements))
            {
                GlobalViewModel.StatusText = "(人´∀`) All Requirements Met";
                GlobalViewModel.ProgressValue = 100;
                GlobalViewModel.MetRequirements = true;

            }
            else
            {
                GlobalViewModel.StatusText = $"（◞‸◟）Missing Requirements - {GlobalViewModel.MissingRequirements}";
            }
        }
        catch (Exception ex)
        {
            GlobalViewModel.PhunkLogs = $"\n[Phunk] ! An error occured! {ex.Message}";
        }

        return Task.Delay(1000);
    }

    /// <summary>
    /// Downloads the required files for doing the process
    /// </summary>
    /// <returns></returns>
    private async Task DownloadFiles()
    {
        // Download APK Tools
        GlobalViewModel.StatusText = "((⇀‸↼)) Downloading Apktools";
        GlobalViewModel.PhunkLogs = "\n[Phunk] ~ Downloading Apktools";
        await FetchAndDownload("https://api.github.com/repos/iBotPeaches/apktool/releases", 0, "apktool.jar");
        await DownloadAsync("https://raw.githubusercontent.com/iBotPeaches/Apktool/master/scripts/windows/apktool.bat", "apktool.bat");

        // Download Uber Apk Signer
        GlobalViewModel.StatusText = "((⇀‸↼)) Downloading Uber Apk Signer";
        GlobalViewModel.PhunkLogs = "\n[Phunk] ~ Downloading Uber Apk Signer";

        await FetchAndDownload("https://api.github.com/repos/patrickfav/uber-apk-signer/releases", 1, "uberapksigner.jar");

        await Task.Delay(500);
    }

    /// <summary>
    /// Decompile APK to get its content
    /// </summary>
    /// <param name="binFolderPath">The location of the bin folder where the tools were downloaded</param>
    /// <param name="handler">gets the ApkHandler in order to call the function for decompiling the Apk</param>
    /// <returns></returns>
    private async Task DecompileApk(string binFolderPath, ApkHandler handler)
    {
        // Run APK Tool

        int result = await Task.Run(() => handler.DecompileApkTool(Path.Combine(binFolderPath, "apktool.jar"), FilePath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp/extracted"), GlobalViewModel.DecompileAdditionalParamsSettingsTxt));

        if (result == 0)
        {
            GlobalViewModel.ProgressValue = 100;
            GlobalViewModel.StatusText = "(人´∀`) Extraction Success";
            GlobalViewModel.PhunkLogs = "\n[Phunk] ~ Extraction Success";
        }
        else
        {
            GlobalViewModel.StatusText = "（◞‸◟） Extraction Failed";
            GlobalViewModel.PhunkLogs = "[Phunk] ! Extraction Failed";
        }

        await Task.Delay(500);
    }

    /// <summary>
    /// Streams through the decompiled APK and get information such as its package name,
    /// and replace all of its original package name to a custom one.
    /// </summary>
    /// <returns></returns>
    private async Task StreamApk()
    {
        string customPackageName = !string.IsNullOrEmpty(GlobalViewModel.CustomPackageNameSettingsTxt)
            ? GlobalViewModel.CustomPackageNameSettingsTxt
            : "phunk";

        string extracted_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp/extracted");
        string packageValue = Util.GetPackageValue(Path.Combine(extracted_path, "AndroidManifest.xml"), "package=");

        GlobalViewModel.StatusText = "((⇀‸↼)) Getting Original Package Name";
        OriginalPackageName = packageValue;

        CurrentPackageName = OriginalPackageName.Split('.')[0];
        string final_package_name = OriginalPackageName.Replace(CurrentPackageName, customPackageName);

        CurrentPackageName = final_package_name;

        await Task.Run(() =>
        {
            _ = Util.ReplaceTextInFile(Path.Combine(extracted_path, "AndroidManifest.xml"), OriginalPackageName, CurrentPackageName);
            _ = Util.ReplaceTextInFile(Path.Combine(extracted_path, "apktool.yml"), OriginalPackageName, CurrentPackageName);
        });

        GlobalViewModel.ProgressValue = 100;

        await Task.Delay(500);
    }

    private async Task<Task> ReplaceNames()
    {
        await Task.Run(() =>
        {
            string extractedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp/extracted");

            string folderPath = Path.Combine(extractedPath, "smali/com/", OriginalPackageName.Split('.')[1]);

            string newFolderPath = Path.Combine(Path.GetDirectoryName(folderPath), CurrentPackageName.Split('.')[1]);

            _ = Util.RenameFolder(folderPath, newFolderPath);

            string mainDirectory = Path.GetDirectoryName(FilePath);
            string mainPackage = Path.Combine(mainDirectory, OriginalPackageName);

            // If the OBB exists in where the user selected the apk, we will do some certain process to ensure that everything will work.
            if (Directory.Exists(mainPackage))
            {
                string currentPackage = Path.Combine(mainDirectory, CurrentPackageName);

                // Changes the OBB Folder and File Name to the Set Package Name
                _ = Util.RenameFolder(mainPackage, currentPackage);
                _ = Util.ReplaceKeywordInFileNames(currentPackage, OriginalPackageName, CurrentPackageName);
                _ = Util.MoveFolder(Path.Combine(mainDirectory, CurrentPackageName), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp"));
            }

            _ = Util.ReplaceTextInDirectory(Path.Combine(extractedPath, "smali"),
                OriginalPackageName.Replace('.', '/'),
                CurrentPackageName.Replace('.', '/'));
        });


        GlobalViewModel.ProgressValue = 100;

        return Task.Delay(500);
    }

    private async Task BuildApk(string binFolderPath, ApkHandler apkHandler)
    {
        string extracted_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp/extracted");

        await Task.Run(() =>
        {
            apkHandler.BuildApkTool(Path.Combine(binFolderPath, "apktool.jar"), extracted_path,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", "temp.apk"));
        });

        GlobalViewModel.ProgressValue = 100;
        if (string.IsNullOrEmpty(GlobalViewModel.FinalOutputNameSettingsTxt))
        {
            GlobalViewModel.FinalOutputNameSettingsTxt = ApkName;
        }

        await Task.Delay(500);
    }

    private async Task SignApk(string binFolderPath, ApkHandler apkHandler)
    {
        await Task.Run(() =>
        {
            apkHandler.SignApkTool(Path.Combine(binFolderPath, "uberapksigner.jar"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", "temp.apk"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp"),
                GlobalViewModel.SigningZipaligningParamsSettingsTxt);
        });

        _ = Task.Delay(500);
    }

    private async Task FinalTaskOut()
    {
        string output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", "/temp-aligned-debugSigned.apk");

        if (File.Exists(output))
            _ = Util.RenameFile(output, GlobalViewModel.FinalOutputNameSettingsTxt);

        await Task.Delay(500);
    }

    #region Downloading Functions


    private async Task FetchAndDownload(string url, int num, string name)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode(); // Throw an exception if the status code is not successful

            string jsonResult = await response.Content.ReadAsStringAsync();
            dynamic dynObj = JsonConvert.DeserializeObject(jsonResult)!;
            string browser_url = dynObj[0].assets[num].browser_download_url;

            await DownloadAsync(browser_url, name);
        }
        catch (HttpRequestException ex)
        {
            GlobalViewModel.PhunkLogs = $"\n[Phunk] ! An HttpRequestException occurred! {ex.Message}";
        }
        catch (Exception ex)
        {
            GlobalViewModel.PhunkLogs = $"\n[Phunk] ! An error occurred! {ex.Message}";
        }
    }

    private async Task DownloadAsync(string url, string name)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode(); // Throw an exception if the status code is not successful

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string binFolderPath = Path.Combine(appDirectory, "bin");

            using Stream stream = await response.Content.ReadAsStreamAsync();
            using FileStream fileStream = File.Create(Path.Combine(binFolderPath, name));

            long? fileSize = response.Content.Headers.ContentLength;
            Progress<int> progress = new(percentage =>
            {
                GlobalViewModel.ProgressValue = percentage;
                GlobalViewModel.StatusText = $"[{percentage}%] Downloading - {url}";
            });

            byte[] buffer = new byte[81920];
            int bytesRead = 0;
            int totalBytesRead = 0;

            var progressHandle = (IProgress<int>)progress;
            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalBytesRead += bytesRead;

                int percentage = (int)((double)totalBytesRead / (fileSize ?? 0) * 100);
                progressHandle.Report(percentage);
            }
        }
        catch (HttpRequestException ex)
        {
            GlobalViewModel.PhunkLogs = $"\n[Phunk] ! An HttpRequestException occurred! {ex.Message}";
        }
        catch (Exception ex)
        {
            GlobalViewModel.PhunkLogs = $"\n[Phunk] ! An error occurred! {ex.Message}";
        }
    }
    #endregion

    private void PhunkLog(string message, string statusMessage = "")
    {
        GlobalViewModel.PhunkLogs = $"\n[Phunk] ~ {message}";
        if (statusMessage.Length > 0)
        {
            GlobalViewModel.StatusText = string.IsNullOrEmpty(statusMessage)
                ? message
                : statusMessage;
        }
    }
}
