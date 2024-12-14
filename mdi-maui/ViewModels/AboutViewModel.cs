using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mdi_maui.Controls;
using System.Runtime.InteropServices;

namespace mdi_maui.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    #region Fields
    private const string AppVersion = "0.2";
    private readonly MDIWindow? _parentWindow;
    #endregion

    #region Properties
    [ObservableProperty]
    private string version = string.Empty;

    [ObservableProperty]
    private string dotNetVersion = string.Empty;

    public IRelayCommand CloseThisWindowCommand { get; private set; } = null!;
    public IRelayCommand OpenGitHubCommand { get; private set; } = null!;
    #endregion

    #region Constructor
    public AboutViewModel(MDIWindow? parentWindow)
    {
        _parentWindow = parentWindow;
        InitializeProperties();
        InitializeCommands();
    }
    #endregion

    #region Private Methods
    private void InitializeProperties()
    {
        Version = $"{AppVersion}-{ThisAssembly.Git.Branch}-{ThisAssembly.Git.Commit[..7]}-{ThisAssembly.Git.CommitDate}";
        DotNetVersion = RuntimeInformation.FrameworkDescription;
    }

    private void InitializeCommands()
    {
        CloseThisWindowCommand = new RelayCommand(CloseWindow);
        OpenGitHubCommand = new AsyncRelayCommand(OpenGitHub);
    }

    private void CloseWindow()
    {
        _parentWindow?.Close();
    }

    private async Task OpenGitHub()
    {
        const string url = "https://github.com/fpedrolucas95/MauiGnome";
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            await Launcher.OpenAsync(uri);
        }
    }
    #endregion
}
