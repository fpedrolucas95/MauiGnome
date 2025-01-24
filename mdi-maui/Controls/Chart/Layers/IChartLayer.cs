using mdi_maui.Models;

namespace mdi_maui.Controls.Chart.Layers;

public interface IChartLayer
{
    void DrawLayer(ICanvas canvas, RectF dirtyRect, RectF chartArea, ChartRenderContext context);
}
