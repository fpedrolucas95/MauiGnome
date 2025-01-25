using mdi_maui.Models;

namespace mdi_maui.Controls.Chart.Layers;

public class AxisLabelsLayer(ChartRenderContext context) : IChartLayer
{
    #region Consts
    private const int MaxTimeLabels = 10;
    private const int MaxPriceLabels = 10;
    private const float LabelPadding = 5f;
    private const float DefaultFontSize = 10f;
    #endregion

    #region Fields
    private readonly ChartRenderContext _context = context;
    #endregion

    #region Public Methods
    public void DrawLayer(ICanvas canvas, RectF dirtyRect, RectF chartArea, ChartRenderContext ctx)
    {
        if (ctx.DataSeries == null || !ctx.DataSeries.Any()) return;
        if (ctx.VisibleData == null || !ctx.VisibleData.Any()) return;

        canvas.FontSize = DefaultFontSize;
        canvas.FontColor = GetOptimalColor();

        var priceArea = ctx.ShowVolume ? ctx.PriceArea : chartArea;
        float timeAxisBottom = ctx.ShowVolume ? ctx.VolumeArea.Bottom : chartArea.Bottom;

        DrawPriceLabels(canvas, priceArea, ctx.MinPrice, ctx.MaxPrice);
        DrawTimeLabels(canvas, priceArea, timeAxisBottom, ctx.VisibleData, ctx);
    }
    #endregion

    #region Private Methods
    private Color GetOptimalColor()
    {
        float lum = _context.BackgroundColor?.GetLuminosity() ?? 0.5f;
        return lum > 0.5 ? Colors.Black : Colors.White;
    }

    private static void DrawPriceLabels(ICanvas canvas, RectF priceArea, double minPrice, double maxPrice)
    {
        int labelCount = Math.Min(MaxPriceLabels, Math.Max(2, (int)(priceArea.Height / 50)));
        double priceRange = maxPrice - minPrice;

        for (int i = 0; i < labelCount; i++)
        {
            double val = minPrice + (i / (double)(labelCount - 1)) * priceRange;

            float yPos = priceArea.Bottom - (i / (float)(labelCount - 1)) * priceArea.Height;

            string txt = val.ToString("F2");
            float labelH = DefaultFontSize * 1.4f;

            canvas.DrawString(txt, priceArea.Right + LabelPadding, yPos - (labelH / 2), 42, labelH, HorizontalAlignment.Left, VerticalAlignment.Center);
        }
    }

    private static void DrawTimeLabels(ICanvas canvas, RectF priceArea, float timeAxisBottom, IReadOnlyList<ChartData> visibleData, ChartRenderContext ctx)
    {
        float step = ctx.CandleWidthPx + ctx.CandleSpacingPx;
        float firstX = ctx.FirstCandleX;

        float labelY = timeAxisBottom + 5;

        int maxLabels = Math.Min(MaxTimeLabels, Math.Max(2, (int)(priceArea.Width / 64)));
        int stepIndex = Math.Max(1, visibleData.Count / (maxLabels - 1));

        for (int i = 0; i < visibleData.Count; i += stepIndex)
        {
            var cndl = visibleData[i];
            string timeLabel = cndl.Timestamp.ToString("HH:mm");
            float candleCenterX = firstX + i * step + (ctx.CandleWidthPx / 2f);

            float labelW = 60;
            float labelH = DefaultFontSize * 1.4f;

            canvas.DrawString(timeLabel, candleCenterX - (labelW / 2), labelY, labelW, labelH, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
    #endregion
}
