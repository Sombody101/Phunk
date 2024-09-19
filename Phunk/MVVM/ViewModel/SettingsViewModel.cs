using Phunk.Core;
using Phunk.MVVM.View.Windows;
using Phunk.Ooki;

namespace Phunk.MVVM.ViewModel;

public class SettingsViewModel : ObservableObject
{
    public GlobalViewModel GlobalViewModel { get; } = GlobalViewModel.Instance;
    public RelayCommand SelectJavaFolderPath { get; init; }

    public SettingsViewModel()
    {
        SelectJavaFolderPath = new RelayCommand((e) =>
        {
            VistaFolderBrowserDialog dialog = new()
            {
                Description = "Please select the jre folder.",
                UseDescriptionForTitle = true // This applies to the Vista style dialog only, not the old dialog.
            };

            if (dialog.ShowDialog(new SettingsWindow()) is { } result && result)
            {
                GlobalViewModel.JavaPathFolderSettingsTxt = dialog.SelectedPath;
            }
        });
    }
}
