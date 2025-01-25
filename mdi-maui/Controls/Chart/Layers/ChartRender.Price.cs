using mdi_maui.Enum;
using mdi_maui.Models;

namespace mdi_maui.Controls.Chart.Layers;

public class PriceLayer : IChartLayer
{
    #region Public Methods
    public void DrawLayer(ICanvas canvas, RectF dirtyRect, RectF chartArea, ChartRenderContext context)
    {
        var finalArea = context.ShowVolume ? context.PriceArea : chartArea;

        if (context.DataSeries == null || !context.DataSeries.Any()) return;

        var visibleData = GetVisibleData(context.DataSeries, context, finalArea.Width);
        CalculateAutoScale(visibleData, context.ChartType, out double minPrice, out double maxPrice);
        context.MinPrice = minPrice;
        context.MaxPrice = maxPrice;
        context.VisibleData = visibleData;

        float cw = context.BaseCandleWidth * context.ZoomLevel;
        float cs = context.BaseCandleSpacing * context.ZoomLevel;
        float step = cw + cs;
        float firstCandleX = finalArea.Right - visibleData.Count * step;

        context.CandleWidthPx = cw;
        context.CandleSpacingPx = cs;
        context.FirstCandleX = firstCandleX;
        context.VisibleCount = visibleData.Count;

        if (!visibleData.Any()) return;

        if (context.ChartType == ChartType.Candle)
            DrawCandles(canvas, finalArea, visibleData, firstCandleX, step, cw, context);
        else if (context.ChartType == ChartType.Line)
            DrawLine(canvas, finalArea, visibleData, firstCandleX, step, cw, context);
        else if (context.ChartType == ChartType.Volume)
            DrawVolume(canvas, finalArea, visibleData, firstCandleX, step, cw, context);
        else
            DrawArea(canvas, finalArea, visibleData, firstCandleX, step, cw, context);

        if (context.ChartType != ChartType.Volume)
            DrawCurrentPriceLine(canvas, finalArea, visibleData, context);
    }
    #endregion

    #region Private Methods
    private static List<ChartData> GetVisibleData(IReadOnlyList<ChartData> data, ChartRenderContext ctx, float availableWidth)
    {
        float cw = ctx.BaseCandleWidth * ctx.ZoomLevel;
        float cs = ctx.BaseCandleSpacing * ctx.ZoomLevel;
        int maxVisible = (int)(availableWidth / (cw + cs));
        return data.Skip(Math.Max(0, data.Count - maxVisible)).ToList();
    }

    private static void CalculateAutoScale(List<ChartData> visible, ChartType chartType, out double minPrice, out double maxPrice)
    {
        if (!visible.Any())
        {
            minPrice = 0;
            maxPrice = 1;
            return;
        }

        if (chartType == ChartType.Volume)
        {
            maxPrice = visible.Max(c => c.Volume);
            minPrice = 0;
            double padding = maxPrice * 0.05;
            maxPrice += padding;
        }
        else
        {
            double vMax = visible.Max(c => c.High);
            double vMin = visible.Min(c => c.Low);
            double range = vMax - vMin;
            double pad = range * 0.05;
            maxPrice = vMax + pad;
            minPrice = vMin - pad;
        }
    }

    private static void DrawCandles(ICanvas canvas, RectF area, List<ChartData> data, float firstCandleX, float step, float candleW, ChartRenderContext ctx)
    {
        for (int i = 0; i < data.Count; i++)
        {
            var d = data[i];
            float xPos = firstCandleX + i * step;
            float oY = TranslatePriceToY(d.Open, area, ctx.MinPrice, ctx.MaxPrice);
            float cY = TranslatePriceToY(d.Close, area, ctx.MinPrice, ctx.MaxPrice);
            float hY = TranslatePriceToY(d.High, area, ctx.MinPrice, ctx.MaxPrice);
            float lY = TranslatePriceToY(d.Low, area, ctx.MinPrice, ctx.MaxPrice);

            bool bull = d.Close >= d.Open;
            canvas.StrokeSize = 1.2f;
            canvas.StrokeColor = bull ? Color.FromArgb("#28B463") : Color.FromArgb("#E74C3C");
            canvas.FillColor = bull ? Color.FromArgb("#2ECC71") : Color.FromArgb("#EC7063");

            float bodyTop = Math.Min(oY, cY);
            float bodyH = Math.Abs(oY - cY);
            canvas.FillRoundedRectangle(xPos, bodyTop, candleW, bodyH, 2);

            float wickX = xPos + candleW / 2;
            canvas.DrawLine(wickX, hY, wickX, lY);
        }
    }

    private static void DrawLine(ICanvas canvas, RectF area, List<ChartData> data, float firstCandleX, float step, float candleW, ChartRenderContext ctx)
    {
        var path = new PathF();
        bool first = true;
        for (int i = 0; i < data.Count; i++)
        {
            var d = data[i];
            float xPos = firstCandleX + i * step;
            float yVal = TranslatePriceToY(d.Close, area, ctx.MinPrice, ctx.MaxPrice);

            if (first)
            {
                path.MoveTo(xPos, yVal);
                first = false;
            }
            else
            {
                path.LineTo(xPos, yVal);
            }
        }
        canvas.StrokeColor = Colors.Green;
        canvas.StrokeSize = 2;
        canvas.DrawPath(path);
    }

    private static void DrawArea(ICanvas canvas, RectF area, List<ChartData> data, float firstCandleX, float step, float candleW, ChartRenderContext ctx)
    {
        var path = new PathF();
        bool first = true;
        float startX = 0;
        float firstY = 0;
        for (int i = 0; i < data.Count; i++)
        {
            var d = data[i];
            float xPos = firstCandleX + i * step;
            float yVal = TranslatePriceToY(d.Close, area, ctx.MinPrice, ctx.MaxPrice);

            if (first)
            {
                path.MoveTo(xPos, yVal);
                firstY = yVal;
                startX = xPos;
                first = false;
            }
            else
            {
                path.LineTo(xPos, yVal);
            }
        }

        float lastX = firstCandleX + (data.Count - 1) * step;
        path.LineTo(lastX, area.Bottom);
        path.LineTo(startX, area.Bottom);
        path.LineTo(startX, firstY);

        canvas.StrokeColor = Color.FromArgb("#1ABC9C");
        canvas.FillColor = Color.FromArgb("#1ABC9C").WithAlpha(0.3f);
        canvas.StrokeSize = 2;
        canvas.DrawPath(path);
    }

    private static void DrawVolume(ICanvas canvas, RectF area, List<ChartData> data, float firstCandleX, float step, float candleW, ChartRenderContext ctx)
    {
        for (int i = 0; i < data.Count; i++)
        {
            var d = data[i];
            float xPos = firstCandleX + i * step;
            float topY = TranslatePriceToY(d.Volume, area, ctx.MinPrice, ctx.MaxPrice);
            float barHeight = area.Bottom - topY;

            bool bull = d.Close >= d.Open;
            var color = bull ? Color.FromArgb("#2ECC71") : Color.FromArgb("#EC7063");

            canvas.FillColor = color.WithAlpha(0.7f);
            canvas.FillRectangle(xPos, topY, candleW, barHeight);
        }
    }

    private static void DrawCurrentPriceLine(ICanvas canvas, RectF area, List<ChartData> visible, ChartRenderContext ctx)
    {
        if (ctx.ChartType == ChartType.Volume) return;

        var lastCandle = visible[^1];
        float y = TranslatePriceToY(lastCandle.Close, area, ctx.MinPrice, ctx.MaxPrice);

        Color color;
        if (ctx.ChartType == ChartType.Candle)
        {
            bool bull = lastCandle.Close >= lastCandle.Open;
            color = bull ? Color.FromArgb("#28B463") : Color.FromArgb("#E74C3C");
        }
        else if (ctx.ChartType == ChartType.Line)
        {
            color = Colors.Green;
        }
        else
        {
            color = Color.FromArgb("#1ABC9C");
        }

        canvas.SaveState();
        canvas.StrokeDashPattern = new float[] { 3, 3 };
        canvas.StrokeColor = color;
        canvas.StrokeSize = 1;
        canvas.DrawLine(area.Left, y, area.Right, y);
        canvas.RestoreState();

        string priceStr = lastCandle.Close.ToString("F2");
        canvas.FontSize = 12f;
        float textWidth = (float)(priceStr.Length * 12f * 0.6);
        float textHeight = 12f * 1.4f;
        float boxWidth = textWidth + 10;
        float boxHeight = textHeight;
        float boxLeft = area.Right;
        float boxTop = y - boxHeight / 2;

        var path = new PathF();
        path.MoveTo(boxLeft, y);
        path.LineTo(boxLeft + 8, boxTop);
        path.LineTo(boxLeft + boxWidth, boxTop);
        path.LineTo(boxLeft + boxWidth, boxTop + boxHeight);
        path.LineTo(boxLeft + 8, boxTop + boxHeight);
        path.Close();

        canvas.FillColor = color.WithAlpha(0.7f);
        canvas.FillPath(path);

        float textX = boxLeft + 10;
        float textY = boxTop;
        canvas.DrawString(priceStr, textX, textY, textWidth, textHeight, HorizontalAlignment.Left, VerticalAlignment.Center);
    }

    private static float TranslatePriceToY(double price, RectF area, double min, double max)
    {
        double rg = max - min;
        if (rg <= 0) return area.Bottom;
        return (float)(area.Bottom - ((price - min) / rg) * area.Height);
    }
    #endregion
}
