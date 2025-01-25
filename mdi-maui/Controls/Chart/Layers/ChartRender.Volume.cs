using mdi_maui.Models;

namespace mdi_maui.Controls.Chart.Layers;

public class VolumeLayer : IChartLayer
{
    #region Public Methods
    public void DrawLayer(ICanvas canvas, RectF dirtyRect, RectF chartArea, ChartRenderContext context)
    {
        if (!context.ShowVolume) return;

        var volumeArea = context.VolumeArea;

        if (context.VisibleData == null || context.VisibleData.Count == 0) return;

        double maxVol = context.VisibleData.Max(d => d.Volume);
        double minVol = 0;
        double range = maxVol - minVol;
        if (range <= 0) range = 1;

        float cw = context.BaseCandleWidth * context.ZoomLevel;
        float cs = context.BaseCandleSpacing * context.ZoomLevel;
        float step = cw + cs;
        float firstCandleX = volumeArea.Right - context.VisibleData.Count * step;

        for (int i = 0; i < context.VisibleData.Count; i++)
        {
            var d = context.VisibleData[i];
            float xPos = firstCandleX + i * step;

            float topY = (float)(volumeArea.Bottom - ((d.Volume - minVol) / range) * volumeArea.Height);
            float barHeight = volumeArea.Bottom - topY;

            bool bull = d.Close >= d.Open;
            var color = bull ? Color.FromArgb("#2ECC71") : Color.FromArgb("#EC7063");

            canvas.FillColor = color.WithAlpha(0.7f);
            canvas.FillRectangle(xPos, topY, cw, barHeight);
        }
    }
    #endregion
}
