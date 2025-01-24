using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mdi_maui.Controls;
using mdi_maui.Views;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;

namespace mdi_maui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    #region Fields
    private readonly IDispatcherTimer _timer;
    private readonly MDIContainer _container;
    #endregion

    #region Properties
    private bool _isMenuVisible;
    public bool IsMenuVisible
    {
        get => _isMenuVisible;
        set => SetProperty(ref _isMenuVisible, value);
    }

    [ObservableProperty]
    private ObservableCollection<MDIWindow> windows = new();

    [ObservableProperty]
    private MDIWindow? activeWindow;

    [ObservableProperty]
    private string currentTime = DateTime.Now.ToString("HH:mm");

    [ObservableProperty]
    private string currentDate = DateTime.Now.ToString("d 'de' MMMM 'de' yyyy", CultureInfo.CurrentCulture);

    public bool IsCloseVisible => Windows.Count > 0;

    public string WindowOpen => Windows.Count.ToString();
    #endregion

    #region Commands
    public IRelayCommand? ToggleMenuCommand { get; private set; }
    public IRelayCommand? OpenCalculatorCommand { get; private set; }
    public IRelayCommand? OpenPaintCommand { get; private set; }
    public IRelayCommand? OpenChartCommand { get; private set; }
    public IRelayCommand? OpenChatbotCommand { get; private set; }
    public IRelayCommand? OpenAboutCommand { get; private set; }
    public IRelayCommand? HideMenuCommand { get; private set; }
    public IRelayCommand? CascadeCommand { get; private set; }
    public IRelayCommand? CloseAllWindowsCommand { get; private set; }
    public IRelayCommand? SwitchThemeCommand { get; private set; }
    #endregion

    #region Constructor
    public MainViewModel(MDIContainer container)
    {
        _container = container;

        InitializeCommands();

        Windows.CollectionChanged += OnWindowsCollectionChanged;

        _timer = Application.Current?.Dispatcher?.CreateTimer() ?? throw new InvalidOperationException("Dispatcher not available");
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e) => UpdateDateTime();
        _timer.Start();

        ObserveThemeChanges();
    }
    #endregion

    #region Private Methods
    private void InitializeCommands()
    {
        ToggleMenuCommand = new RelayCommand(ToggleMenu);
        OpenCalculatorCommand = new RelayCommand(OpenCalculatorWindow);
        OpenPaintCommand = new RelayCommand(OpenPaintWindow);
        OpenChartCommand = new RelayCommand(OpenChartWindow);
        OpenChatbotCommand = new RelayCommand(OpenChatbotWindow);
        OpenAboutCommand = new RelayCommand(OpenAboutWindow);
        HideMenuCommand = new RelayCommand(HideMenu);
        CascadeCommand = new RelayCommand(CascadeWindows);
        CloseAllWindowsCommand = new RelayCommand(CloseAllWindows);
        SwitchThemeCommand = new RelayCommand(ToggleTheme);
    }

    private void OnWindowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ActiveWindow = e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null ? Windows.LastOrDefault() : ActiveWindow;

        OnPropertyChanged(nameof(WindowOpen));
        OnPropertyChanged(nameof(IsCloseVisible));
    }

    private void ToggleMenu()
    {
        IsMenuVisible = !IsMenuVisible;
    }

    private void HideMenu()
    {
        IsMenuVisible = false;
    }

    private void OpenCalculatorWindow()
    {
        HideMenu();
        var calcView = new CalcView();
        var mdiWindow = CreateMDIWindow("Calculadora", calcView, 320, 480, GetIcon("calculator"));
        mdiWindow.Closed += (_, _) => Windows.Remove(mdiWindow);
        Windows.Add(mdiWindow);
        ActiveWindow = mdiWindow;
    }

    private void OpenPaintWindow()
    {
        HideMenu();
        var paintView = new PaintView();
        var mdiWindow = CreateMDIWindow("Paint", paintView, 480, 380, GetIcon("paint"));
        mdiWindow.Closed += (_, _) => Windows.Remove(mdiWindow);
        Windows.Add(mdiWindow);
        ActiveWindow = mdiWindow;
    }

    private void OpenChartWindow()
    {
        HideMenu();
        var chartView = new ChartView();
        var mdiWindow = CreateMDIWindow("Gráfico", chartView, 720, 480, GetIcon("chart"));
        mdiWindow.Closed += (_, _) => Windows.Remove(mdiWindow);
        Windows.Add(mdiWindow);
        ActiveWindow = mdiWindow;
    }

    private void OpenChatbotWindow()
    {
        HideMenu();
        var chatbotView = new ChatbotView();
        var mdiWindow = CreateMDIWindow("Chatbot", chatbotView, 320, 512, GetIcon("chatbot"), false);
        mdiWindow.Closed += (_, _) => Windows.Remove(mdiWindow);
        Windows.Add(mdiWindow);
        ActiveWindow = mdiWindow;
    }

    private void OpenAboutWindow()
    {
        HideMenu();
        var aboutView = new AboutView();
        var mdiWindow = CreateMDIWindow("Sobre", aboutView, 280, 300, GetIcon("about"), false);

        aboutView.BindingContext = new AboutViewModel(mdiWindow);

        mdiWindow.Closed += (_, _) => Windows.Remove(mdiWindow);
        Windows.Add(mdiWindow);
        ActiveWindow = mdiWindow;
    }

    private MDIWindow CreateMDIWindow(string title, View content, double width = 400, double height = 300, string icon = "", bool resize = true)
    {
        return new MDIWindow
        {
            ParentContainer = _container,
            WindowWidth = width,
            WindowHeight = height,
            Title = title,
            WindowContent = content,
            Icon = icon,
            Resize = resize,
            BackgroundColor = Colors.Transparent,
            BindingContext = content.BindingContext
        };
    }

    private void CascadeWindows()
    {
        const int offset = 20;
        for (int i = 0; i < Windows.Count; i++)
        {
            Windows[i].X = i * offset;
            Windows[i].Y = i * offset;
        }
    }

    private void CloseAllWindows()
    {
        foreach (var window in Windows.ToList())
        {
            window.Close();
        }
        Windows.Clear();
        OnPropertyChanged(nameof(IsCloseVisible));
        OnPropertyChanged(nameof(WindowOpen));
    }

    private void UpdateDateTime()
    {
        CurrentTime = DateTime.Now.ToString("HH:mm");
        CurrentDate = DateTime.Now.ToString("d 'de' MMMM 'de' yyyy", CultureInfo.CurrentCulture);
    }

    private static string GetIcon(string windowType)
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? $"dark_{windowType}.png"
            : $"light_{windowType}.png";
    }

    private void UpdateWindowIcons()
    {
        foreach (var window in Windows)
        {
            window.Icon = GetIcon(window.Title.ToLower() switch
            {
                "calculadora" => "calculator",
                "sobre" => "about",
                "paint" => "paint",
                "gráfico" => "chart",
                "chatbot" => "chatbot",
                _ => "default"
            });
        }
    }

    private void ToggleTheme()
    {
        if (Application.Current == null) return;

        Application.Current.UserAppTheme = Application.Current.UserAppTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
    }

    private void ObserveThemeChanges()
    {
        Application.Current?.Dispatcher?.Dispatch(() =>
        {
            Application.Current.RequestedThemeChanged += (_, _) => UpdateWindowIcons();
        });
    }
    #endregion
}
