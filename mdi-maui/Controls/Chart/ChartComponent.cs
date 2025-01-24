using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;
using mdi_maui.Enum;
using mdi_maui.Models;

namespace mdi_maui.Controls.Chart;

public partial class ChartComponent : ContentView
{
    #region Consts
    private const int RefreshIntervalMs = 1000;
    private const int CandleDurationSec = 60;
    private const double OpenValueMin = 90;
    private const double OpenValueMax = 110;
    private const double PriceFluctuationRange = 10;
    #endregion

    #region Fields
    private readonly GraphicsView _graphicsView;
    private readonly ChartRenderer _renderer;
    private readonly System.Timers.Timer _refreshTimer;
    private readonly Random _randomGenerator = new();
    private DateTime _lastCandleTimestamp;
    #endregion

    #region Constructor
    public ChartComponent()
    {
        _renderer = new ChartRenderer(12f, 4f);

        _graphicsView = new GraphicsView
        {
            Drawable = _renderer,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        Content = new Border
        {
            Stroke = Colors.Gray,
            StrokeThickness = 0.5,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0, 0, 8, 8) },
            Content = _graphicsView
        };

        DataSeries.CollectionChanged += (_, __) => RefreshRender();

        _refreshTimer = new System.Timers.Timer(RefreshIntervalMs);
        _refreshTimer.Elapsed += (_, __) => MainThread.BeginInvokeOnMainThread(UpdateData);
        _refreshTimer.Start();

        _lastCandleTimestamp = DateTime.Now;

        _graphicsView.StartInteraction += OnPointerDown;
        _graphicsView.DragInteraction += OnPointerMoved;
        _graphicsView.EndInteraction += OnPointerUp;
    }
    #endregion

    #region Properties
    public static readonly BindableProperty DataSeriesProperty = BindableProperty.Create(nameof(DataSeries), typeof(ObservableCollection<ChartData>), typeof(ChartComponent), new ObservableCollection<ChartData>(), propertyChanged: (bindable, _, __) => ((ChartComponent)bindable).RefreshRender());
    public ObservableCollection<ChartData> DataSeries
    {
        get => (ObservableCollection<ChartData>)GetValue(DataSeriesProperty);
        set => SetValue(DataSeriesProperty, value);
    }

    public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(ChartComponent), Colors.Black);
    public Color ChartBackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public static readonly BindableProperty ChartTypeProperty = BindableProperty.Create(nameof(ChartType), typeof(ChartType), typeof(ChartComponent), ChartType.Candle, propertyChanged: (bindable, _, __) => ((ChartComponent)bindable).RefreshRender());
    public ChartType ChartType
    {
        get => (ChartType)GetValue(ChartTypeProperty);
        set => SetValue(ChartTypeProperty, value);
    }
    #endregion

    #region Private Methods
    private void UpdateData()
    {
        var currentTime = DateTime.Now;
        if ((currentTime - _lastCandleTimestamp).TotalSeconds >= CandleDurationSec)
        {
            DataSeries.Add(CreateNewDataPoint());
            _lastCandleTimestamp = currentTime;
        }
        else
        {
            ModifyLatestDataPoint();
        }
        RefreshRender();
    }

    private ChartData CreateNewDataPoint()
    {
        double openPrice = _randomGenerator.NextDouble() * (OpenValueMax - OpenValueMin) + OpenValueMin;
        double highPrice = openPrice + _randomGenerator.NextDouble() * PriceFluctuationRange;
        double lowPrice = openPrice - _randomGenerator.NextDouble() * PriceFluctuationRange;
        double closePrice = lowPrice + _randomGenerator.NextDouble() * (highPrice - lowPrice);
        return new ChartData
        {
            Open = openPrice,
            High = highPrice,
            Low = lowPrice,
            Close = closePrice,
            Timestamp = _lastCandleTimestamp.AddSeconds(CandleDurationSec)
        };
    }

    private void ModifyLatestDataPoint()
    {
        if (!DataSeries.Any()) return;
        var latest = DataSeries.Last();
        double priceChange = (_randomGenerator.NextDouble() * 2) - 1;
        latest.High = Math.Max(latest.High, latest.Close + priceChange);
        latest.Low = Math.Min(latest.Low, latest.Close - priceChange);
        latest.Close += priceChange;
    }

    private void RefreshRender()
    {
        _renderer.UpdateChartData(DataSeries, ChartBackgroundColor, CandleDurationSec, _lastCandleTimestamp, ChartType);
        _graphicsView.Invalidate();
    }

    private void OnPointerDown(object? sender, TouchEventArgs e)
    {
        var point = e.Touches.FirstOrDefault();
        _renderer.UpdatePointer(point.X, point.Y, true);
        _graphicsView.Invalidate();
    }

    private void OnPointerMoved(object? sender, TouchEventArgs e)
    {
        var point = e.Touches.FirstOrDefault();
        _renderer.UpdatePointer(point.X, point.Y, true);
        _graphicsView.Invalidate();
    }

    private void OnPointerUp(object? sender, TouchEventArgs e)
    {
        _renderer.UpdatePointer(0, 0, false);
        _graphicsView.Invalidate();
    }
    #endregion

    #region Public Methods
    public void ZoomIn()
    {
        _renderer.ZoomIn();
        RefreshRender();
    }

    public void ZoomOut()
    {
        _renderer.ZoomOut();
        RefreshRender();
    }
    #endregion
}