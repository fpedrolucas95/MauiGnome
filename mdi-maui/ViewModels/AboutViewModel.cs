using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mdi_maui.Controls;
using System.Runtime.InteropServices;

namespace mdi_maui.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    private const string AppVersion = "0.1";

    // Referência à janela pai para manipular o fechamento
    private readonly MDIWindow? _parentWindow;

    [ObservableProperty]
    private string version;

    [ObservableProperty]
    private string dotNetVersion;

    public AboutViewModel(MDIWindow? parentWindow)
    {
        _parentWindow = parentWindow;

        // Definir a versão do aplicativo
        Version = $"{AppVersion}-{ThisAssembly.Git.Branch}-{ThisAssembly.Git.Commit[..7]}-{ThisAssembly.Git.CommitDate}";

        // Definir a versão do .NET
        DotNetVersion = RuntimeInformation.FrameworkDescription;

        // Inicializar o comando de fechamento usando o método Close da MDIWindow
        CloseThisWindowCommand = new RelayCommand(CloseWindow);
        OpenGitHubCommand = new RelayCommand(async () => await OpenGitHub());
    }

    public IRelayCommand CloseThisWindowCommand { get; }
    public IRelayCommand OpenGitHubCommand { get; }

    private void CloseWindow()
    {
        // Garante que a janela seja fechada se _parentWindow for válida
        _parentWindow?.Close();
    }

    private async Task OpenGitHub()
    {
        var url = "https://github.com/fpedrolucas95/MauiGnome";
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            await Launcher.OpenAsync(uri);
        }
    }
}
