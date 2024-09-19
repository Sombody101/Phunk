using Phunk.Core;
using System.Text;

namespace Phunk.MVVM.ViewModel;

public class GlobalViewModel : ObservableObject
{
    public static GlobalViewModel Instance { get; } = new GlobalViewModel();

    private bool _isCustomJavaPath;
    public bool IsCustomJavaPath
    {
        get => _isCustomJavaPath;
        set
        {
            _isCustomJavaPath = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Enables/Disables the Settings Window if process has started or not
    /// </summary>
    private bool _isProcessStarting;
    public bool IsProcessStarting
    {
        get => _isProcessStarting;
        set
        {
            _isProcessStarting = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Status text to let the user know what is happening
    /// </summary>
    private readonly StringBuilder _statusText = new();
    public string StatusText
    {
        get => _statusText.ToString();
        set
        {
            _ = _statusText.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// For the progress bar, displays the current progress
    /// for the executed task
    /// </summary>
    private float _progressValue;
    public float ProgressValue
    {
        get => _progressValue;
        set
        {
            _progressValue = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// If all requirements are met for the application
    /// </summary>
    private bool _metRequirements;
    public bool MetRequirements
    {
        get => _metRequirements;
        set
        {
            _metRequirements = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Stores the logs from the Phunk and displays them
    /// </summary>
    private readonly StringBuilder _phunkLogs = new();
    public string PhunkLogs
    {
        get => _phunkLogs.ToString();
        set
        {
            _ = _phunkLogs.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    private readonly StringBuilder _missingRequirements = new();
    public string MissingRequirements
    {
        get => _missingRequirements.ToString();
        set
        {
            _ = _missingRequirements.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    private bool _canStart;
    public bool CanStart
    {
        get => _canStart;
        set
        {
            _canStart = value;
            OnPropertyChanged();
        }
    }

    private readonly StringBuilder _newApkName = new();
    public string NewApkName
    {
        get => _newApkName.ToString();
        set
        {
            _ = _newApkName.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    #region Settings

    /* Main Settings */

    private readonly StringBuilder _javaPathFolderSettingsTxt = new();
    public string JavaPathFolderSettingsTxt
    {
        get => _javaPathFolderSettingsTxt.ToString();
        set
        {
            _ = _javaPathFolderSettingsTxt.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    private readonly StringBuilder _finalOutputNameSettingsTxt = new();
    public string FinalOutputNameSettingsTxt
    {
        get => _finalOutputNameSettingsTxt.ToString();
        set
        {
            _ = _finalOutputNameSettingsTxt.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    private readonly StringBuilder _decompileAdditionalParamsSettingsTxt = new();
    public string DecompileAdditionalParamsSettingsTxt
    {
        get => _decompileAdditionalParamsSettingsTxt.ToString();
        set
        {
            _ = _decompileAdditionalParamsSettingsTxt.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    private readonly StringBuilder _signingZipaligningParamsSettingsTxt = new();
    public string SigningZipaligningParamsSettingsTxt
    {
        get => _signingZipaligningParamsSettingsTxt.ToString();
        set
        {
            _ = _signingZipaligningParamsSettingsTxt.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    private readonly StringBuilder _customPackageNameSettingsTxt = new();
    public string CustomPackageNameSettingsTxt
    {
        get => _customPackageNameSettingsTxt.ToString();
        set
        {
            _ = _customPackageNameSettingsTxt.Clear().Append(value);
            OnPropertyChanged();
        }
    }

    /* Configs */

    private bool _autoCleanSettingsBoolean;
    public bool AutoCleanSettingsBoolean
    {
        get => _autoCleanSettingsBoolean;
        set
        {
            _autoCleanSettingsBoolean = value;
            OnPropertyChanged();
        }
    }

    private bool _useApkToolSettingsBoolean;
    public bool UseApkToolSettingsBoolean
    {
        get => _useApkToolSettingsBoolean;
        set
        {
            _useApkToolSettingsBoolean = value;
            OnPropertyChanged();
        }
    }

    private bool _autoUpdatePhunkSettingsBoolean;
    public bool AutoUpdatePhunkSettingsBoolean
    {
        get => _autoUpdatePhunkSettingsBoolean;
        set
        {
            _autoUpdatePhunkSettingsBoolean = value;
            OnPropertyChanged();
        }
    }

    #endregion
}
