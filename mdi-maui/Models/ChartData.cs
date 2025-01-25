using mdi_maui.Enum;

namespace mdi_maui.Models;

public class ChartData
{
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public double Volume { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ChartRenderContext
{
    public IReadOnlyList<ChartData>? DataSeries { get; set; }
    public ChartType ChartType { get; set; }
    public Color? BackgroundColor { get; set; }
    public float BaseCandleWidth { get; set; }
    public float BaseCandleSpacing { get; set; }
    public float ZoomLevel { get; set; } = 1f;
    public RectF PriceArea { get; set; }
    public RectF VolumeArea { get; set; }
    public double MinPrice { get; set; }
    public double MaxPrice { get; set; }
    public bool ShowVolume { get; set; }
    public IReadOnlyList<ChartData>? VisibleData { get; set; }
    public float PointerX { get; set; }
    public float PointerY { get; set; }
    public bool ShowPointer { get; set; }
    public float FirstCandleX { get; set; }
    public float CandleWidthPx { get; set; }
    public float CandleSpacingPx { get; set; }
    public int VisibleCount { get; set; }
    public ChartData? SelectedCandle { get; set; }
    public bool ShowTooltip { get; set; }
}