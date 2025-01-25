using mdi_maui.Enum;
using mdi_maui.Models;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;

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
    private bool _isPointerDown;
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

    public static readonly BindableProperty ShowVolumeProperty = BindableProperty.Create(nameof(ShowVolume), typeof(bool), typeof(ChartComponent), false, propertyChanged: (bindable, oldValue, newValue) => { var component = (ChartComponent)bindable; component._renderer.UpdateShowVolume((bool)newValue); component.RefreshRender(); });
    public bool ShowVolume
    {
        get => (bool)GetValue(ShowVolumeProperty);
        set => SetValue(ShowVolumeProperty, value);
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
            Volume = _randomGenerator.NextDouble() * 2000,
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
        latest.Volume += _randomGenerator.NextDouble() * 100;
    }

    private void RefreshRender()
    {
        _renderer.UpdateChartData(DataSeries, ChartBackgroundColor, CandleDurationSec, _lastCandleTimestamp, ChartType);
        _graphicsView.Invalidate();
    }

    private void OnPointerDown(object? sender, TouchEventArgs e)
    {
        if (e.Touches.Length == 0) return;
        var point = e.Touches[0];
        _isPointerDown = true;
        _renderer.UpdatePointer(point.X, point.Y, true);

        var ctx = _renderer.GetContext();
        if (ctx.VisibleData != null && ctx.VisibleData.Count > 0)
        {
            float w = (float)_graphicsView.Width;
            float h = (float)_graphicsView.Height;
            int idx = ChartComponent.GetNearestIndex(point.X, w, ctx);
            if (idx >= 0 && idx < ctx.VisibleData.Count)
            {
                var cndl = ctx.VisibleData[idx];
                float cw = ctx.BaseCandleWidth * ctx.ZoomLevel;
                float cs = ctx.BaseCandleSpacing * ctx.ZoomLevel;
                float step = cw + cs;
                float firstCandleX = (w - 60f) - ctx.VisibleData.Count * step;
                float candleX = firstCandleX + idx * step;
                float candleRight = candleX + cw;
                if (point.X >= candleX && point.X <= candleRight)
                {
                    RectF dirtyRect = new(0, 0, w, h);
                    var chartArea = new RectF(dirtyRect.Left, dirtyRect.Top, dirtyRect.Width - 60, dirtyRect.Height - 24);

                    float oY = TranslatePriceToY(cndl.Open, chartArea, ctx.MinPrice, ctx.MaxPrice);
                    float cY = TranslatePriceToY(cndl.Close, chartArea, ctx.MinPrice, ctx.MaxPrice);
                    float hY = TranslatePriceToY(cndl.High, chartArea, ctx.MinPrice, ctx.MaxPrice);
                    float lY = TranslatePriceToY(cndl.Low, chartArea, ctx.MinPrice, ctx.MaxPrice);

                    float bodyTop = Math.Min(oY, cY);
                    float bodyBottom = Math.Max(oY, cY);
                    float top = Math.Min(bodyTop, hY);
                    float bottom = Math.Max(bodyBottom, lY);

                    if (point.Y >= top && point.Y <= bottom)
                    {
                        ctx.SelectedCandle = cndl;
                        ctx.ShowTooltip = true;
                        _graphicsView.Invalidate();
                        return;
                    }
                }
            }
        }

        ctx.SelectedCandle = null;
        ctx.ShowTooltip = false;
        _graphicsView.Invalidate();
    }

    private void OnPointerMoved(object? sender, TouchEventArgs e)
    {
        if (e.Touches.Length == 0) return;
        var point = e.Touches[0];
        _renderer.UpdatePointer(point.X, point.Y, true);

        var ctx = _renderer.GetContext();
        if (!_isPointerDown)
        {
            _graphicsView.Invalidate();
            return;
        }

        float w = (float)_graphicsView.Width;
        float h = (float)_graphicsView.Height;
        int idx = ChartComponent.GetNearestIndex(point.X, w, ctx);
        if (ctx.VisibleData == null || ctx.VisibleData.Count == 0 || idx < 0 || idx >= ctx.VisibleData.Count)
        {
            ctx.ShowTooltip = false;
            ctx.SelectedCandle = null;
            _graphicsView.Invalidate();
            return;
        }

        var cndl = ctx.VisibleData[idx];
        float cw = ctx.BaseCandleWidth * ctx.ZoomLevel;
        float cs = ctx.BaseCandleSpacing * ctx.ZoomLevel;
        float step = cw + cs;
        float firstCandleX = (w - 60f) - ctx.VisibleData.Count * step;
        float candleX = firstCandleX + idx * step;
        float candleRight = candleX + cw;
        if (point.X < candleX || point.X > candleRight)
        {
            ctx.ShowTooltip = false;
            ctx.SelectedCandle = null;
            _graphicsView.Invalidate();
            return;
        }

        RectF dirtyRect = new(0, 0, w, h);
        var chartArea = new RectF(dirtyRect.Left, dirtyRect.Top, dirtyRect.Width - 60, dirtyRect.Height - 24);

        float oY = TranslatePriceToY(cndl.Open, chartArea, ctx.MinPrice, ctx.MaxPrice);
        float cY = TranslatePriceToY(cndl.Close, chartArea, ctx.MinPrice, ctx.MaxPrice);
        float hY = TranslatePriceToY(cndl.High, chartArea, ctx.MinPrice, ctx.MaxPrice);
        float lY = TranslatePriceToY(cndl.Low, chartArea, ctx.MinPrice, ctx.MaxPrice);

        float bodyTop = Math.Min(oY, cY);
        float bodyBottom = Math.Max(oY, cY);
        float top = Math.Min(bodyTop, hY);
        float bottom = Math.Max(bodyBottom, lY);

        if (point.Y >= top && point.Y <= bottom)
        {
            ctx.SelectedCandle = cndl;
            ctx.ShowTooltip = true;
        }
        else
        {
            ctx.SelectedCandle = null;
            ctx.ShowTooltip = false;
        }

        _graphicsView.Invalidate();
    }

    private void OnPointerUp(object? sender, TouchEventArgs e)
    {
        _isPointerDown = false;
        var ctx = _renderer.GetContext();
        ctx.ShowTooltip = false;
        ctx.SelectedCandle = null;
        _renderer.UpdatePointer(0, 0, false);
        _graphicsView.Invalidate();
    }

    private static float TranslatePriceToY(double price, RectF area, double min, double max)
    {
        double rg = max - min;
        if (rg <= 0) return area.Bottom;
        return (float)(area.Bottom - ((price - min) / rg) * area.Height);
    }

    private static int GetNearestIndex(float pointerX, float viewWidth, ChartRenderContext ctx)
    {
        float cw = ctx.BaseCandleWidth * ctx.ZoomLevel;
        float cs = ctx.BaseCandleSpacing * ctx.ZoomLevel;
        float step = cw + cs;
        float chartWidth = viewWidth - 60f;
        float firstCandleX = chartWidth - ((ctx.VisibleData?.Count ?? 0) * step);
        float rel = pointerX - firstCandleX;
        int idx = (int)Math.Floor(rel / step);
        idx = Math.Clamp(idx, 0, (ctx.VisibleData?.Count ?? 1) - 1);
        return idx;
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