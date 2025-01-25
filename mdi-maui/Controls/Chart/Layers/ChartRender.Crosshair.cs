using mdi_maui.Models;

namespace mdi_maui.Controls.Chart.Layers;

public class CrosshairLayer : IChartLayer
{
    #region Fields
    private readonly float _fontSize = 12f;
    #endregion

    #region Public Methods
    public void DrawLayer(ICanvas canvas, RectF dirtyRect, RectF chartArea, ChartRenderContext context)
    {
        if (!context.ShowPointer)
            return;

        var priceArea = context.ShowVolume ? context.PriceArea : chartArea;

        if (context.PointerX < chartArea.Left || context.PointerX > chartArea.Right) return;

        if (context.PointerY < priceArea.Top || context.PointerY > priceArea.Bottom) return;

        canvas.SaveState();
        canvas.StrokeDashPattern = new float[] { 3, 3 };
        canvas.StrokeColor = Colors.White.WithAlpha(0.7f);
        canvas.StrokeSize = 1;
        canvas.DrawLine(context.PointerX, chartArea.Top, context.PointerX, chartArea.Bottom);

        canvas.DrawLine(priceArea.Left, context.PointerY, priceArea.Right, context.PointerY);
        canvas.RestoreState();

        double priceValue = TranslateYToPrice(context.PointerY, priceArea, context.MinPrice, context.MaxPrice);
        string priceTxt = priceValue.ToString("F2");

        canvas.FontSize = _fontSize;
        canvas.FontColor = Colors.White;

        float labelW = (float)(priceTxt.Length * _fontSize * 0.6);
        float labelH = _fontSize * 1.4f;

        float boxLeft = priceArea.Right;
        float boxTop = context.PointerY - (labelH / 2);
        float boxWidth = labelW + 10;
        float boxHeight = labelH;

        var pricePath = new PathF();
        pricePath.MoveTo(boxLeft, context.PointerY);
        pricePath.LineTo(boxLeft + 8, boxTop);
        pricePath.LineTo(boxLeft + boxWidth, boxTop);
        pricePath.LineTo(boxLeft + boxWidth, boxTop + boxHeight);
        pricePath.LineTo(boxLeft + 8, boxTop + boxHeight);
        pricePath.Close();

        canvas.FillColor = Colors.Gray.WithAlpha(0.6f);
        canvas.FillPath(pricePath);

        float textX = boxLeft + 10;
        float textY = boxTop;
        DrawStringLeft(canvas, priceTxt, textX, textY, labelW, labelH);

        if (context.VisibleData == null || !context.VisibleData.Any())
            return;

        int idx = GetNearestIndex(context.PointerX, priceArea, context.VisibleData.Count, context);
        if (idx < 0 || idx >= context.VisibleData.Count)
            return;

        var candle = context.VisibleData[idx];
        string timeTxt = candle.Timestamp.ToString("HH:mm");

        float timeW = (float)(timeTxt.Length * _fontSize * 0.6);
        float timeH = _fontSize * 1.4f;

        float timeRectLeft = context.PointerX - (timeW / 2) - 5;
        float timeRectTop = chartArea.Bottom + 2;
        float timeRectWidth = timeW + 10;
        float timeRectHeight = timeH;

        canvas.FillColor = Colors.Gray.WithAlpha(0.6f);
        canvas.FillRoundedRectangle(timeRectLeft, timeRectTop, timeRectWidth, timeRectHeight, 4);

        canvas.DrawString(timeTxt, timeRectLeft, timeRectTop, timeRectWidth, timeRectHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
    #endregion

    #region Private Methods
    private static double TranslateYToPrice(float y, RectF area, double min, double max)
    {
        double rg = max - min;
        if (rg <= 0) return min;

        double pct = (area.Bottom - y) / area.Height;
        return min + rg * pct;
    }

    private static int GetNearestIndex(float pointerX, RectF area, int count, ChartRenderContext ctx)
    {
        float cw = ctx.BaseCandleWidth * ctx.ZoomLevel;
        float cs = ctx.BaseCandleSpacing * ctx.ZoomLevel;
        float step = cw + cs;

        float xPos = area.Right - count * step;
        float rel = pointerX - xPos;

        int idx = (int)Math.Floor(rel / step);
        return Math.Clamp(idx, 0, count - 1);
    }

    private static void DrawStringLeft(ICanvas canvas, string text, float x, float y, float w, float h)
    {
        canvas.DrawString(text, x, y, w, h, HorizontalAlignment.Left, VerticalAlignment.Center);
    }
    #endregion
}
