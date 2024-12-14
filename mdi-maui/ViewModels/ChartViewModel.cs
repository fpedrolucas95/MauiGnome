using mdi_maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace mdi_maui.ViewModels
{
    public class ChartViewModel : BindableObject
    {
        public ObservableCollection<CandlestickData> CandlestickSeries { get; private set; }

        private readonly System.Timers.Timer _timer;
        private readonly Random _randomGenerator;
        private double _lastClosePrice;
        private DateTime _lastCandleTime;
        private CandlestickData? _currentCandle;

        public ICommand ChangeIntervalCommand { get; }

        private int _intervalInSeconds = 60;

        public ChartViewModel()
        {
            CandlestickSeries = new ObservableCollection<CandlestickData>();
            _randomGenerator = new Random();
            _lastClosePrice = 100;
            _lastCandleTime = DateTime.Now;

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += UpdateCurrentCandle;
            _timer.AutoReset = true;

            ChangeIntervalCommand = new Command<string>(ChangeInterval);

            GenerateInitialCandlesticks();
        }

        public void StartRealTimeCandlestickGeneration()
        {
            _timer.Start();
        }

        public void StopRealTimeCandlestickGeneration()
        {
            _timer.Stop();
        }

        private void UpdateCurrentCandle(object? sender, ElapsedEventArgs e)
        {
            if (_currentCandle == null)
            {
                _currentCandle = CreateNewCandlestickTemplate();
            }

            double priceChange = _randomGenerator.NextDouble() * 2 - 1;
            _currentCandle.Close += priceChange;
            _currentCandle.High = Math.Max(_currentCandle.High, _currentCandle.Close);
            _currentCandle.Low = Math.Min(_currentCandle.Low, _currentCandle.Close);

            if ((DateTime.Now - _lastCandleTime).TotalSeconds >= _intervalInSeconds)
            {
                var finalizedCandle = _currentCandle;
                _currentCandle = null;

                Application.Current?.Dispatcher.Dispatch(() =>
                {
                    CandlestickSeries.Add(finalizedCandle);
                    if (CandlestickSeries.Count > 50)
                    {
                        CandlestickSeries.RemoveAt(0);
                    }
                });

                _lastCandleTime = DateTime.Now;
                _lastClosePrice = finalizedCandle.Close;
            }
        }

        private CandlestickData CreateNewCandlestickTemplate()
        {
            return new CandlestickData
            {
                Open = _lastClosePrice,
                High = _lastClosePrice,
                Low = _lastClosePrice,
                Close = _lastClosePrice,
                Timestamp = _lastCandleTime.AddSeconds(_intervalInSeconds)
            };
        }

        private void ChangeInterval(string interval)
        {
            if (int.TryParse(interval, out int newInterval))
            {
                _intervalInSeconds = newInterval;

                Application.Current?.Dispatcher.Dispatch(() =>
                {
                    CandlestickSeries.Clear();
                    GenerateInitialCandlesticks();
                });
            }
        }

        private void GenerateInitialCandlesticks()
        {
            DateTime startTime = DateTime.Now.AddSeconds(-50 * _intervalInSeconds);
            _lastCandleTime = startTime;

            for (int i = 0; i < 50; i++)
            {
                var candle = GenerateNewCandlestick();
                CandlestickSeries.Add(candle);
                _lastClosePrice = candle.Close;
                _lastCandleTime = candle.Timestamp;
            }
        }

        private CandlestickData GenerateNewCandlestick()
        {
            double open = _lastClosePrice;
            double high = open + _randomGenerator.NextDouble() * 10;
            double low = open - _randomGenerator.NextDouble() * 10;
            double close = low + _randomGenerator.NextDouble() * (high - low);

            return new CandlestickData
            {
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Timestamp = _lastCandleTime.AddSeconds(_intervalInSeconds)
            };
        }
    }
}
