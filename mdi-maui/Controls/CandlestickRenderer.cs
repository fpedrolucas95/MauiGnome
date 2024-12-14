using System.Collections.ObjectModel;

namespace mdi_maui.Controls
{
    public class CandlestickRenderer : IDrawable
    {
        #region Fields
        private readonly float _candleWidth;
        private readonly float _candleSpacing;
        private ObservableCollection<CandlestickData>? _candlesticks;
        private Color _backgroundColor = Colors.Black;
        #endregion

        #region Constructor
        public CandlestickRenderer(float candleWidth, float candleSpacing)
        {
            _candleWidth = candleWidth;
            _candleSpacing = candleSpacing;
        }
        #endregion

        #region Public Methods
        public void UpdateCandlestickData(ObservableCollection<CandlestickData>? candlesticks, Color backgroundColor, int interval, DateTime lastCloseTime)
        {
            _candlesticks = candlesticks;
            _backgroundColor = backgroundColor;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            ClearCanvas(canvas, dirtyRect);

            if (_candlesticks == null || !_candlesticks.Any()) return;

            var drawableArea = new RectF(dirtyRect.Left, dirtyRect.Top, dirtyRect.Width - 60, dirtyRect.Height - 24);
            DrawGrid(canvas, drawableArea);
            DrawPriceLabels(canvas, drawableArea);
            DrawTimeLabels(canvas, dirtyRect, drawableArea);

            int maxVisibleCandles = (int)(drawableArea.Width / (_candleWidth + _candleSpacing));
            var visibleCandles = _candlesticks.TakeLast(maxVisibleCandles).ToList();
            float xPosition = drawableArea.Right - visibleCandles.Count * (_candleWidth + _candleSpacing);

            foreach (var candle in visibleCandles)
            {
                RenderCandlestick(canvas, candle, drawableArea, xPosition, _candleWidth);
                xPosition += _candleWidth + _candleSpacing;
            }
        }
        #endregion

        #region Private Methods
        private void ClearCanvas(ICanvas canvas, RectF bounds)
        {
            canvas.FillColor = _backgroundColor;
            canvas.FillRectangle(bounds);
        }

        private void DrawGrid(ICanvas canvas, RectF area)
        {
            canvas.StrokeColor = Colors.Gray.WithAlpha(0.5f);
            canvas.StrokeSize = 1;

            int gridLines = 10;
            float xInterval = area.Width / gridLines;
            float yInterval = area.Height / gridLines;

            for (int i = 0; i <= gridLines; i++)
            {
                float x = area.Left + (i * xInterval);
                canvas.DrawLine(x, area.Top, x, area.Bottom);

                float y = area.Top + (i * yInterval);
                canvas.DrawLine(area.Left, y, area.Right, y);
            }
        }

        private void DrawPriceLabels(ICanvas canvas, RectF area)
        {
            if (_candlesticks == null || !_candlesticks.Any()) return;

            int labelCount = Math.Max(2, (int)(area.Height / 50));
            double maxPrice = _candlesticks.Max(c => c.High);
            double minPrice = _candlesticks.Min(c => c.Low);
            double priceRange = maxPrice - minPrice;
            double padding = priceRange * 0.05;

            maxPrice += padding;
            minPrice -= padding;
            priceRange = maxPrice - minPrice;

            canvas.FontSize = 10;
            canvas.FontColor = Colors.White;

            for (int i = 0; i < labelCount; i++)
            {
                double priceValue = minPrice + (i / (double)(labelCount - 1)) * priceRange;
                float yPosition = area.Bottom - (i / (float)(labelCount - 1)) * area.Height;
                int decimals = GetDecimalPlaces(priceRange);
                string priceText = priceValue.ToString($"F{decimals}");

                canvas.DrawString(
                    priceText,
                    area.Right + 5,
                    yPosition,
                    HorizontalAlignment.Left
                );
            }
        }

        private void DrawTimeLabels(ICanvas canvas, RectF dirtyRect, RectF area)
        {
            if (_candlesticks == null || !_candlesticks.Any()) return;

            int maxLabels = Math.Max(2, (int)(area.Width / 64));
            var visibleCandles = _candlesticks.TakeLast((int)(area.Width / (_candleWidth + _candleSpacing))).ToList();
            int step = Math.Max(1, visibleCandles.Count / (maxLabels - 1));
            float labelY = dirtyRect.Bottom - 15;
            float xPosition = area.Right - visibleCandles.Count * (_candleWidth + _candleSpacing);

            for (int i = 0; i < visibleCandles.Count; i += step)
            {
                var candle = visibleCandles[i];
                string timeLabel = FormatTimestamp(candle.Timestamp);

                canvas.DrawString(
                    timeLabel,
                    xPosition + _candleWidth / 2,
                    labelY,
                    HorizontalAlignment.Center
                );

                xPosition += step * (_candleWidth + _candleSpacing);
            }
        }

        private int GetDecimalPlaces(double range)
        {
            if (range > 100) return 2;
            if (range > 10) return 3;
            if (range > 1) return 4;
            return 5;
        }

        private string FormatTimestamp(DateTime timestamp)
        {
            if (_candlesticks != null && _candlesticks.Count > 1440)
                return timestamp.ToString("dd/MM HH:mm");
            if (_candlesticks != null && _candlesticks.Count > 60)
                return timestamp.ToString("HH:mm");
            return timestamp.ToString("HH:mm:ss");
        }

        private void RenderCandlestick(ICanvas canvas, CandlestickData candle, RectF area, float x, float width)
        {
            float openY = TranslatePriceToY(candle.Open, area);
            float closeY = TranslatePriceToY(candle.Close, area);
            float highY = TranslatePriceToY(candle.High, area);
            float lowY = TranslatePriceToY(candle.Low, area);

            bool isBullish = candle.Close >= candle.Open;
            canvas.FillColor = isBullish ? Colors.Green : Colors.Red;

            float bodyTop = Math.Min(openY, closeY);
            float bodyHeight = Math.Abs(openY - closeY);

            canvas.FillRectangle(x, bodyTop, width, bodyHeight);
            canvas.StrokeColor = Colors.White;

            float wickX = x + width / 2;
            canvas.DrawLine(wickX, highY, wickX, lowY);
        }

        private float TranslatePriceToY(double price, RectF area)
        {
            if (_candlesticks == null || !_candlesticks.Any())
                return area.Bottom;

            double minPrice = _candlesticks.Min(c => c.Low);
            double maxPrice = _candlesticks.Max(c => c.High);
            double priceRange = maxPrice - minPrice;

            if (priceRange == 0)
                return area.Bottom;

            return (float)(area.Bottom - ((price - minPrice) / priceRange) * area.Height);
        }
        #endregion
    }
}
