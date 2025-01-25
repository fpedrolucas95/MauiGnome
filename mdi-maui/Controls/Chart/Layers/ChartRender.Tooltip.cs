using mdi_maui.Models;
using System.Globalization;

namespace mdi_maui.Controls.Chart.Layers;

public class TooltipLayer : IChartLayer
{
    #region Fields
    private readonly float _fontSize = 12f;
    #endregion

    #region Public Methods
    public void DrawLayer(ICanvas canvas, RectF dirtyRect, RectF chartArea, ChartRenderContext context)
    {
        if (!context.ShowTooltip || context.SelectedCandle == null)
            return;

        var priceArea = context.ShowVolume ? context.PriceArea : chartArea;

        if (context.PointerX < priceArea.Left || context.PointerX > priceArea.Right) return;
        if (context.PointerY < priceArea.Top || context.PointerY > priceArea.Bottom) return;

        var candle = context.SelectedCandle;
        float pointerX = context.PointerX;
        float pointerY = context.PointerY;
        var culture = CultureInfo.CreateSpecificCulture("pt-BR");

        var lines = new (string Label, string Value)[]
        {
            ("Data",       candle.Timestamp.ToString("dd/MM/yyyy HH:mm", culture)),
            ("Abertura",   candle.Open.ToString("C2", culture)),
            ("Máxima",     candle.High.ToString("C2", culture)),
            ("Mínima",     candle.Low.ToString("C2", culture)),
            ("Fechamento", candle.Close.ToString("C2", culture))
        };

        float lineHeight = _fontSize * 1.2f;
        float maxLabelWidth = 0;
        float maxValueWidth = 0;

        foreach (var line in lines)
        {
            float labelWidth = (float)line.Label.Length * _fontSize * 0.6f;
            float valueWidth = (float)line.Value.Length * _fontSize * 0.5f;
            if (labelWidth > maxLabelWidth) maxLabelWidth = labelWidth;
            if (valueWidth > maxValueWidth) maxValueWidth = valueWidth;
        }

        float padding = 8f;
        float boxWidth = maxLabelWidth + maxValueWidth + padding * 3;
        float boxHeight = lines.Length * lineHeight + padding * 2;
        float tooltipX = pointerX + 12;
        float tooltipY = pointerY + 12;

        if (tooltipX + boxWidth > priceArea.Right)
            tooltipX = priceArea.Right - boxWidth - 5;

        if (tooltipY + boxHeight > priceArea.Bottom)
            tooltipY = priceArea.Bottom - boxHeight - 5;

        canvas.FillColor = Colors.Black.WithAlpha(0.8f);
        canvas.FillRoundedRectangle(tooltipX, tooltipY, boxWidth, boxHeight, 6);

        float textY = tooltipY + padding;
        canvas.FontSize = _fontSize;
        canvas.FontColor = Colors.White;

        foreach (var line in lines)
        {
            canvas.DrawString(line.Label, tooltipX + padding, textY, maxLabelWidth, lineHeight, HorizontalAlignment.Left, VerticalAlignment.Center);
            canvas.DrawString(line.Value, tooltipX + padding + maxLabelWidth + padding, textY, maxValueWidth, lineHeight, HorizontalAlignment.Right, VerticalAlignment.Center);
            textY += lineHeight;
        }
    }
    #endregion
}
