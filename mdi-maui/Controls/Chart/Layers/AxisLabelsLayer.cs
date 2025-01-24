using mdi_maui.Models;

namespace mdi_maui.Controls.Chart.Layers;

public class AxisLabelsLayer(ChartRenderContext context) : IChartLayer
{
    #region Consts
    private const float DefaultFontSize = 10f;
    private const int MaxPriceLabels = 10;
    private const int MaxTimeLabels = 10;
    private const float LabelPadding = 5f;
    #endregion

    #region Fields
    private readonly ChartRenderContext _context = context;
    #endregion

    #region Public Methods
    public Color GetOptimalColor()
    {
        float backgroundLuminance = _context?.BackgroundColor?.GetLuminosity() ?? 0.5f;
        return backgroundLuminance > 0.5 ? Colors.Black : Colors.White;
    }

    public void DrawLayer(ICanvas canvas, RectF dirtyRect, RectF chartArea, ChartRenderContext context)
    {
        if (context.DataSeries == null || !context.DataSeries.Any())
            return;

        if (context.VisibleData == null || !context.VisibleData.Any())
            return;

        canvas.FontSize = DefaultFontSize;
        canvas.FontColor = GetOptimalColor();
        DrawPriceLabels(canvas, chartArea, context.MinPrice, context.MaxPrice);
        DrawTimeLabels(canvas, dirtyRect, chartArea, context.VisibleData);
    }
    #endregion

    #region Private Methods
    private static void DrawPriceLabels(ICanvas canvas, RectF chartArea, double minPrice, double maxPrice)
    {
        int labelCount = Math.Min(MaxPriceLabels, Math.Max(2, (int)(chartArea.Height / 50)));
        double priceRange = maxPrice - minPrice;
        for (int i = 0; i < labelCount; i++)
        {
            double val = minPrice + (i / (double)(labelCount - 1)) * priceRange;
            float yPos = chartArea.Bottom - (i / (float)(labelCount - 1)) * chartArea.Height;
            string txt = val.ToString($"F{GetDecimalPlaces(priceRange)}");
            DrawStringLeft(canvas, txt, chartArea.Right + LabelPadding, yPos - (DefaultFontSize * 0.7f), 42, 14);
        }
    }

    private void DrawTimeLabels(ICanvas canvas, RectF dirtyRect, RectF chartArea, IReadOnlyList<ChartData> visibleData)
    {
        if (!visibleData.Any()) return;
        int maxLabels = Math.Min(MaxTimeLabels, Math.Max(2, (int)(chartArea.Width / 64)));
        int step = Math.Max(1, visibleData.Count / (maxLabels - 1));
        float candleWidth = _context.BaseCandleWidth * _context.ZoomLevel;
        float candleSpacing = _context.BaseCandleSpacing * _context.ZoomLevel;
        float candleStep = candleWidth + candleSpacing;
        float xPos = chartArea.Right - visibleData.Count * candleStep;
        float labelY = chartArea.Bottom + 5;
        for (int i = 0; i < visibleData.Count; i += step)
        {
            var d = visibleData[i];
            string timeLabel = FormatTimestamp(d.Timestamp);
            float labelXPos = xPos + (candleWidth / 2);
            DrawStringCenter(canvas, timeLabel, labelXPos, labelY, 60, 14);
            xPos += step * candleStep;
        }
    }

    private static int GetDecimalPlaces(double range)
    {
        if (range > 100) return 2;
        if (range > 10) return 3;
        if (range > 1) return 4;
        return 5;
    }

    private static string FormatTimestamp(DateTime timestamp)
    {
        return timestamp.ToString("HH:mm");
    }

    private static void DrawStringLeft(ICanvas canvas, string text, float x, float y, float w, float h) =>
        canvas.DrawString(text, x, y, w, h, HorizontalAlignment.Left, VerticalAlignment.Center);

    private static void DrawStringCenter(ICanvas canvas, string text, float x, float y, float w, float h) =>
        canvas.DrawString(text, x, y, w, h, HorizontalAlignment.Center, VerticalAlignment.Center);
    #endregion
}