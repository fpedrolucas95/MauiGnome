using mdi_maui.ViewModels;

namespace mdi_maui.Views;

public partial class ChartView : ContentView
{
    private readonly ChartViewModel _viewModel;

    public ChartView()
    {
        InitializeComponent();
        _viewModel = new ChartViewModel();
        BindingContext = _viewModel;

        _viewModel.StartRealTimeCandlestickGeneration();
    }
}
