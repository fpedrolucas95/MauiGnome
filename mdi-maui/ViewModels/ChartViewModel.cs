using mdi_maui.Models;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows.Input;

namespace mdi_maui.ViewModels;

public partial class ChartViewModel : BindableObject, IDisposable
{
    #region Fields
    private readonly System.Timers.Timer _timer;
    private readonly Random _randomGenerator;
    private double _lastClosePrice;
    private DateTime _lastCandleTime;
    private ChartData? _currentCandle;
    private int _intervalInSeconds = 60;
    private const int MaxStoredCandles = 1000;
    #endregion

    #region Properties
    public ObservableCollection<ChartData> CandlestickSeries { get; }

    private string? _chartType;
    public string? ChartType
    {
        get => _chartType;
        set
        {
            if (_chartType == value) return;
            _chartType = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Commands
    public ICommand ChangeIntervalCommand { get; }
    public ICommand ToggleChartTypeCommand { get; }
    #endregion

    #region Constructor
    public ChartViewModel()
    {
        CandlestickSeries = new ObservableCollection<ChartData>();
        _randomGenerator = new Random();
        _lastClosePrice = 100;
        _lastCandleTime = DateTime.Now;

        _timer = new System.Timers.Timer(8);
        _timer.Elapsed += UpdateCurrentCandle;
        _timer.AutoReset = true;

        ChangeIntervalCommand = new Command<string>(ChangeInterval);
        ToggleChartTypeCommand = new Command(ToggleChartType);

        ChartType = IsDarkMode() ? "dark_candle.png" : "light_candle.png";

        GenerateInitialCandlesticks();
    }
    #endregion

    #region Public Methods
    public void StartRealTimeCandlestickGeneration() => _timer.Start();
    public void StopRealTimeCandlestickGeneration() => _timer.Stop();

    public void Dispose()
    {
        _timer.Stop();
        _timer.Elapsed -= UpdateCurrentCandle;
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Private Methods

    private static bool IsDarkMode()
    {
        if (Application.Current?.RequestedTheme == AppTheme.Dark) return true;
        return false;
    }

    private void ToggleChartType()
    {
        if (ChartType.Contains("candle", StringComparison.OrdinalIgnoreCase))
        {
            ChartType = IsDarkMode() ? "dark_line.png" : "light_line.png";
        }
        else
        {
            ChartType = IsDarkMode() ? "dark_candle.png" : "light_candle.png";
        }
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

                if (CandlestickSeries.Count > MaxStoredCandles)
                {
                    CandlestickSeries.RemoveAt(0);
                }
            });

            _lastCandleTime = DateTime.Now;
            _lastClosePrice = finalizedCandle.Close;
        }
    }

    private ChartData CreateNewCandlestickTemplate()
    {
        return new ChartData
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
        const int initialCount = 200;
        DateTime startTime = DateTime.Now.AddSeconds(-initialCount * _intervalInSeconds);
        _lastCandleTime = startTime;

        for (int i = 0; i < initialCount; i++)
        {
            var candle = GenerateNewCandlestick();
            CandlestickSeries.Add(candle);
            _lastClosePrice = candle.Close;
            _lastCandleTime = candle.Timestamp;
        }
    }

    private ChartData GenerateNewCandlestick()
    {
        double open = _lastClosePrice;
        double high = open + _randomGenerator.NextDouble() * 10;
        double low = open - _randomGenerator.NextDouble() * 10;
        double close = low + _randomGenerator.NextDouble() * (high - low);
        double volume = _randomGenerator.NextDouble() * 1000;

        return new ChartData
        {
            Open = open,
            High = high,
            Low = low,
            Close = close,
            Volume = volume,
            Timestamp = _lastCandleTime.AddSeconds(_intervalInSeconds)
        };
    }
    #endregion
}