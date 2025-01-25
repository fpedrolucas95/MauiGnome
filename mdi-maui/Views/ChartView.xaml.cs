using mdi_maui.ViewModels;
using mdi_maui.Enum;

namespace mdi_maui.Views;

public partial class ChartView : ContentView
{
    #region Fields
    private readonly ChartViewModel _viewModel;
    #endregion

    #region Constructor
    public ChartView()
    {
        InitializeComponent();

        _viewModel = new ChartViewModel();
        BindingContext = _viewModel;

        _viewModel.StartRealTimeCandlestickGeneration();

#if WINDOWS
        ConfigureWindowsKeyHandlers();
#endif
    }
    #endregion

    #region Private Methods
#if WINDOWS
    private void ConfigureWindowsKeyHandlers()
    {
        Microsoft.Maui.Handlers.WindowHandler? windowHandler =
            Application.Current?.Windows?.FirstOrDefault()?.Handler as Microsoft.Maui.Handlers.WindowHandler;

        if (windowHandler?.PlatformView is Microsoft.UI.Xaml.Window platformWindow)
        {
            platformWindow.Activated += (s, e) =>
            {
                if (platformWindow.Content is Microsoft.UI.Xaml.FrameworkElement rootElement)
                {
                    rootElement.KeyDown += RootElement_KeyDown;
                }
            };
        }
    }

    private void RootElement_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Add)
        {
            CandleChart.ZoomIn();
        }
        else if (e.Key == Windows.System.VirtualKey.Subtract)
        {
            CandleChart.ZoomOut();
        }
    }
#endif

    private void OnZoomInClicked(object sender, EventArgs e)
    {
        CandleChart.ZoomIn();
    }

    private void OnZoomOutClicked(object sender, EventArgs e)
    {
        CandleChart.ZoomOut();
    }

    private void OnVolumeEnabledClicked(object sender, EventArgs e)
    {
        CandleChart.ShowVolume = !CandleChart.ShowVolume;

        if (CandleChart.ShowVolume)
            VolumeEnableButton.BackgroundColor = Color.FromArgb("#2196F3");
        else
            VolumeEnableButton.ClearValue(Button.BackgroundColorProperty);
    }


    private void OnToggleChartTypeClicked(object sender, EventArgs e)
    {
        if (CandleChart.ChartType == ChartType.Candle)
        {
            CandleChart.ChartType = ChartType.Line;
            ChartTypeButton.Text = "L";
        }
        else
        {
            CandleChart.ChartType = ChartType.Candle;
            ChartTypeButton.Text = "C";
        }
    }
    #endregion
}