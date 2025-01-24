using mdi_maui.Models;

namespace mdi_maui.Controls.Chart.Layers;

public class BaseBackgroundLayer : IChartLayer
{
    #region Public Methods
    public void DrawLayer(ICanvas canvas, RectF dirtyRect, RectF chartArea, ChartRenderContext context)
    {
        canvas.FillColor = context.BackgroundColor;
        canvas.FillRectangle(dirtyRect);

        canvas.StrokeColor = Colors.Gray.WithAlpha(0.5f);
        canvas.StrokeSize = 1;

        const int gridLines = 10;
        float xInterval = chartArea.Width / gridLines;
        float yInterval = chartArea.Height / gridLines;

        for (int i = 0; i <= gridLines; i++)
        {
            float x = chartArea.Left + (i * xInterval);
            canvas.DrawLine(x, chartArea.Top, x, chartArea.Bottom);

            float y = chartArea.Top + (i * yInterval);
            canvas.DrawLine(chartArea.Left, y, chartArea.Right, y);
        }
    }
    #endregion
}
