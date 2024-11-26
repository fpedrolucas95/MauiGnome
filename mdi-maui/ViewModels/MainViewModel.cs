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

    [ObservableProperty]
    private ObservableCollection<MDIWindow> windows;

    [ObservableProperty]
    private MDIWindow? activeWindow;

    [ObservableProperty]
    private string currentTime = DateTime.Now.ToString("HH:mm");

    [ObservableProperty]
    private string currentDate = DateTime.Now.ToString("d 'de' MMMM 'de' yyyy", CultureInfo.CurrentCulture);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCloseVisible))]
    [NotifyPropertyChangedFor(nameof(WindowOpen))]
    private int windowCounter;
    #endregion

    #region Properties
    public bool IsCloseVisible => WindowCounter > 0;
    public string WindowOpen => WindowCounter.ToString();
    #endregion

    #region Commands
    public IRelayCommand OpenCalculatorCommand { get; }
    public IRelayCommand CascadeCommand { get; }
    public IRelayCommand CloseAllWindowsCommand { get; }
    #endregion

    #region Constructor
    public MainViewModel()
    {
        Windows = [];
        Windows.CollectionChanged += Windows_CollectionChanged;

        OpenCalculatorCommand = new RelayCommand(OpenCalculatorWindow);
        CascadeCommand = new RelayCommand(CascadeWindows);
        CloseAllWindowsCommand = new RelayCommand(CloseAllWindows);

        _timer = Application.Current?.Dispatcher?.CreateTimer() ?? throw new InvalidOperationException("Application.Current or Dispatcher is null");
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e) => UpdateTime();
        _timer.Start();
    }
    #endregion

    #region Private Methods
    private void Windows_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        WindowCounter = Windows.Count;

        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (MDIWindow window in e.OldItems)
            {
                if (ActiveWindow == window)
                {
                    ActiveWindow = Windows.LastOrDefault();
                }
            }
        }
    }

    private void MdiWindow_Closed(object? sender, EventArgs e)
    {
        if (sender is MDIWindow window)
        {
            Windows.Remove(window);
        }
    }

    private void OpenCalculatorWindow()
    {
        var calcView = new CalcView();

        var mdiWindow = new MDIWindow
        {
            X = WindowCounter * 20,
            Y = WindowCounter * 20,
            WindowWidth = 320,
            WindowHeight = 480,
            Icon = "calculator.png",
            Title = "Calculadora",
            WindowContent = calcView,
            BackgroundColor = Colors.Transparent
        };

        mdiWindow.BindingContext = calcView.BindingContext;

        if (calcView.BindingContext is ICloseWindow vm)
        {
            vm.CloseThisWindowCommand = new RelayCommand(() => mdiWindow.Close());
        }

        mdiWindow.Closed += MdiWindow_Closed;
        Windows.Add(mdiWindow);
        ActiveWindow = mdiWindow;
    }

    private void CascadeWindows()
    {
        const int offset = 20;
        int index = 0;

        foreach (var window in Windows)
        {
            window.X = index * offset;
            window.Y = index * offset;
            index++;
        }
    }

    private void CloseAllWindows()
    {
        foreach (var window in Windows.ToList())
        {
            window.Close();
        }

        Windows.Clear();
        WindowCounter = 0;
        ActiveWindow = null;
    }

    private void UpdateTime()
    {
        CurrentTime = DateTime.Now.ToString("HH:mm");
        CurrentDate = DateTime.Now.ToString("d 'de' MMMM 'de' yyyy", CultureInfo.CurrentCulture);
    }
    #endregion
}

public interface ICloseWindow
{
    IRelayCommand CloseThisWindowCommand { get; set; }
}