using System.Collections.ObjectModel;
using mdi_maui.Controls.Chart.Layers;
using mdi_maui.Enum;
using mdi_maui.Models;

namespace mdi_maui.Controls.Chart;

public class ChartRenderer : IDrawable
{
    #region Fields
    private readonly List<IChartLayer> _layers;
    private readonly ChartRenderContext _context;
    private readonly float _baseCandleWidth;
    private readonly float _baseCandleSpacing;

    private float _pointerX;
    private float _pointerY;
    private bool _showPointer;
    private ObservableCollection<ChartData>? _chartData;
    private Color _backgroundColor = Colors.Black;
    private ChartType _chartType = ChartType.Candle;
    private float _zoomLevel = 1f;
    private bool _showVolume;
    #endregion

    #region Constructor
    public ChartRenderer(float candleWidth, float candleSpacing)
    {
        _baseCandleWidth = candleWidth;
        _baseCandleSpacing = candleSpacing;

        _context = new ChartRenderContext
        {
            BaseCandleWidth = _baseCandleWidth,
            BaseCandleSpacing = _baseCandleSpacing,
            ZoomLevel = _zoomLevel,
            BackgroundColor = _backgroundColor,
            ChartType = _chartType,
            ShowVolume = false
        };

        _layers = new List<IChartLayer>
        {
            new BaseBackgroundLayer(),
            new PriceLayer(),
            new VolumeLayer(),
            new AxisLabelsLayer(_context),
            new CrosshairLayer(),
            new TooltipLayer()
        };
    }
    #endregion

    #region Public Methods
    public ChartRenderContext GetContext() => _context;

    public void UpdateChartData(ObservableCollection<ChartData>? chartData, Color backgroundColor, int interval, DateTime lastCloseTime, ChartType chartType)
    {
        _chartData = chartData;
        _backgroundColor = backgroundColor;
        _chartType = chartType;

        _context.DataSeries = _chartData;
        _context.BackgroundColor = _backgroundColor;
        _context.ChartType = _chartType;
    }

    public void UpdateShowVolume(bool showVolume)
    {
        _showVolume = showVolume;
        _context.ShowVolume = showVolume;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        _context.ZoomLevel = _zoomLevel;
        _context.BaseCandleWidth = _baseCandleWidth;
        _context.BaseCandleSpacing = _baseCandleSpacing;
        _context.PointerX = _pointerX;
        _context.PointerY = _pointerY;
        _context.ShowPointer = _showPointer;

        canvas.FillColor = _context.BackgroundColor;
        canvas.FillRectangle(dirtyRect);

        if (_context.DataSeries == null || !_context.DataSeries.Any()) return;

        var chartArea = new RectF(dirtyRect.Left, dirtyRect.Top, dirtyRect.Width - 60, dirtyRect.Height - 24);

        if (_context.ShowVolume)
        {
            float margin = chartArea.Height * 0.05f;
            float volumeHeight = chartArea.Height * 0.20f;
            float priceHeight = chartArea.Height - volumeHeight - margin;

            _context.PriceArea = new RectF(chartArea.Left, chartArea.Top, chartArea.Width, priceHeight);

            _context.VolumeArea = new RectF(chartArea.Left, chartArea.Top + priceHeight + margin, chartArea.Width, volumeHeight);
        }
        else
        {
            _context.PriceArea = chartArea;
            _context.VolumeArea = RectF.Zero;
        }

        foreach (var layer in _layers)
        {
            layer.DrawLayer(canvas, dirtyRect, chartArea, _context);
        }
    }

    public void ZoomIn()
    {
        _zoomLevel *= 1.1f;
    }

    public void ZoomOut()
    {
        _zoomLevel *= 0.9f;
    }

    public void UpdatePointer(float x, float y, bool active)
    {
        _pointerX = x;
        _pointerY = y;
        _showPointer = active;
    }
    #endregion
}
