using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;

namespace mdi_maui.Controls
{
    public partial class CandlestickChartView : ContentView
    {
        #region Consts
        private const int RefreshIntervalMs = 1000;
        private const int CandleDurationSec = 60;
        private const double OpenValueMin = 90;
        private const double OpenValueMax = 110;
        private const double PriceFluctuationRange = 10;
        #endregion

        #region Fields
        private readonly GraphicsView _graphicsView;
        private readonly CandlestickRenderer _renderer;
        private readonly System.Timers.Timer _refreshTimer;
        private DateTime _lastCandleTimestamp;
        private readonly Random _randomGenerator = new();
        #endregion

        #region Constructor
        public CandlestickChartView()
        {
            _renderer = new CandlestickRenderer(12f, 4f);

            _graphicsView = new GraphicsView
            {
                Drawable = _renderer,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            Content = new Border
            {
                Stroke = Colors.Gray,
                StrokeThickness = 0.5,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0, 0, 8, 8) },
                Content = _graphicsView,
            };

            CandlestickSeries.CollectionChanged += (_, __) => RefreshRender();

            _refreshTimer = new System.Timers.Timer(RefreshIntervalMs);
            _refreshTimer.Elapsed += (_, __) => MainThread.BeginInvokeOnMainThread(UpdateCandles);
            _refreshTimer.Start();

            _lastCandleTimestamp = DateTime.Now;
        }
        #endregion

        #region Properties
        public static readonly BindableProperty CandlestickSeriesProperty = BindableProperty.Create(nameof(CandlestickSeries), typeof(ObservableCollection<CandlestickData>), typeof(CandlestickChartView), new ObservableCollection<CandlestickData>(), propertyChanged: (bindable, _, __) => ((CandlestickChartView)bindable).RefreshRender());
        public ObservableCollection<CandlestickData> CandlestickSeries
        {
            get => (ObservableCollection<CandlestickData>)GetValue(CandlestickSeriesProperty);
            set => SetValue(CandlestickSeriesProperty, value);
        }

        public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(CandlestickChartView), Colors.Black);
        public Color ChartBackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }
        #endregion

        #region Private Methods
        private void UpdateCandles()
        {
            var currentTime = DateTime.Now;
            if ((currentTime - _lastCandleTimestamp).TotalSeconds >= CandleDurationSec)
            {
                CandlestickSeries.Add(CreateNewCandlestick());
                _lastCandleTimestamp = currentTime;
            }
            else
            {
                ModifyLatestCandlestick();
            }

            RefreshRender();
        }

        private CandlestickData CreateNewCandlestick()
        {
            double openPrice = _randomGenerator.NextDouble() * (OpenValueMax - OpenValueMin) + OpenValueMin;
            double highPrice = openPrice + _randomGenerator.NextDouble() * PriceFluctuationRange;
            double lowPrice = openPrice - _randomGenerator.NextDouble() * PriceFluctuationRange;
            double closePrice = lowPrice + _randomGenerator.NextDouble() * (highPrice - lowPrice);

            return new CandlestickData
            {
                Open = openPrice,
                High = highPrice,
                Low = lowPrice,
                Close = closePrice,
                Timestamp = _lastCandleTimestamp.AddSeconds(CandleDurationSec)
            };
        }

        private void ModifyLatestCandlestick()
        {
            if (!CandlestickSeries.Any()) return;

            var latestCandle = CandlestickSeries.Last();
            double priceChange = (_randomGenerator.NextDouble() * 2) - 1;

            latestCandle.High = Math.Max(latestCandle.High, latestCandle.Close + priceChange);
            latestCandle.Low = Math.Min(latestCandle.Low, latestCandle.Close - priceChange);
            latestCandle.Close += priceChange;
        }

        private void RefreshRender()
        {
            _renderer.UpdateCandlestickData(CandlestickSeries, ChartBackgroundColor, CandleDurationSec, _lastCandleTimestamp);
            _graphicsView.Invalidate();
        }
        #endregion
    }

    public class CandlestickData
    {
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
