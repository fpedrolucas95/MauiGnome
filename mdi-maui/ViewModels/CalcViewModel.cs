using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Data;
using System.Globalization;
using System.Windows.Input;

namespace mdi_maui.ViewModels;

public partial class CalcViewModel : ObservableObject
{
    #region Consts
    private const int MAX_DISPLAY_LENGTH = 16;
    private const int DECIMAL_PLACES = 4;
    private const string ERROR_MESSAGE = "Erro";
    private const string INFINITY_MESSAGE = "∞";
    #endregion

    #region Fields
    private string display = "0";
    private bool isNewNumber = true;
    private string expression = string.Empty;
    private string lastOperator = string.Empty;
    private string lastNumber = string.Empty;
    private bool hasError = false;
    #endregion

    #region Properties
    public string Display
    {
        get => display;
        set
        {
            var formattedValue = CalcViewModel.FormatDisplayValue(value);
            SetProperty(ref display, formattedValue);
            ((RelayCommand)CalculateCommand).NotifyCanExecuteChanged();
            ((RelayCommand)DecimalCommand).NotifyCanExecuteChanged();
            ((RelayCommand)PercentCommand).NotifyCanExecuteChanged();
        }
    }
    #endregion

    #region Commands
    public ICommand NumberCommand { get; }
    public ICommand OperatorCommand { get; }
    public ICommand CalculateCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand DecimalCommand { get; }
    public ICommand PercentCommand { get; }
    #endregion

    #region Constructor
    public CalcViewModel()
    {
        NumberCommand = new RelayCommand<string>(OnNumberPressed);
        OperatorCommand = new RelayCommand<string>(OnOperatorPressed);
        CalculateCommand = new RelayCommand(OnCalculate, CanCalculate);
        ClearCommand = new RelayCommand(OnClear);
        DecimalCommand = new RelayCommand(OnDecimalPressed, CanUseDecimal);
        PercentCommand = new RelayCommand(OnPercentPressed, CanUsePercent);
    }
    #endregion

    #region Private Methods
    private void OnNumberPressed(string? number)
    {
        if (number == null || hasError) return;

        if (Display.Length >= MAX_DISPLAY_LENGTH && !isNewNumber) return;

        if (Display == "0" || isNewNumber)
        {
            Display = number;
            isNewNumber = false;
        }
        else
        {
            Display += number;
        }
    }

    private void OnOperatorPressed(string? op)
    {
        if (op == null || hasError) return;

        var currentNumber = double.Parse(Display, CultureInfo.InvariantCulture);
        if (!string.IsNullOrEmpty(lastOperator) && isNewNumber)
        {
            expression = expression[..^1] + op;
        }
        else
        {
            lastNumber = currentNumber.ToString(CultureInfo.InvariantCulture);
            expression += lastNumber + op;
        }

        lastOperator = op;
        isNewNumber = true;
    }

    private void OnCalculate()
    {
        try
        {
            if (string.IsNullOrEmpty(expression)) return;

            var currentNumber = double.Parse(Display, CultureInfo.InvariantCulture);
            expression += currentNumber.ToString(CultureInfo.InvariantCulture);

            if (expression.EndsWith("/0"))
            {
                Display = INFINITY_MESSAGE;
                hasError = true;
                return;
            }

            var result = EvaluateExpression(expression);

            if (double.IsInfinity(result) || double.IsNaN(result))
            {
                Display = INFINITY_MESSAGE;
                hasError = true;
            }
            else
            {
                Display = FormatResult(result);
            }
        }
        catch (Exception)
        {
            Display = ERROR_MESSAGE;
            hasError = true;
        }
        finally
        {
            expression = string.Empty;
            lastOperator = string.Empty;
            isNewNumber = true;
        }
    }

    private void OnClear()
    {
        Display = "0";
        isNewNumber = true;
        expression = string.Empty;
        lastOperator = string.Empty;
        lastNumber = string.Empty;
        hasError = false;
    }

    private void OnDecimalPressed()
    {
        if (hasError) return;

        if (isNewNumber)
        {
            Display = "0.";
            isNewNumber = false;
        }
        else if (!Display.Contains('.') && Display.Length < MAX_DISPLAY_LENGTH)
        {
            Display += ".";
        }
    }

    private void OnPercentPressed()
    {
        if (!double.TryParse(Display, out double number) || hasError) return;

        if (!string.IsNullOrEmpty(lastOperator) && !string.IsNullOrEmpty(lastNumber))
        {
            var baseNumber = double.Parse(lastNumber, CultureInfo.InvariantCulture);
            number = baseNumber * (number / 100);
        }
        else
        {
            number /= 100;
        }

        Display = FormatResult(number);
    }

    private bool CanCalculate() => !hasError && !string.IsNullOrEmpty(expression);
    private bool CanUseDecimal() => !hasError && !Display.Contains('.') && Display.Length < MAX_DISPLAY_LENGTH;
    private bool CanUsePercent() => !hasError && double.TryParse(Display, out _);

    private static double EvaluateExpression(string expression)
    {
        var table = new DataTable();
        expression = expression.Replace(',', '.');
        var value = table.Compute(expression, string.Empty);
        return Math.Round(Convert.ToDouble(value, CultureInfo.InvariantCulture), DECIMAL_PLACES);
    }

    private static string FormatResult(double number)
    {
        if (Math.Abs(number) >= 1e16)
            return number.ToString("E", CultureInfo.InvariantCulture);

        if (number == Math.Floor(number))
            return number.ToString("#,0", CultureInfo.InvariantCulture);

        return Math.Round(number, DECIMAL_PLACES).ToString(CultureInfo.InvariantCulture);
    }

    private static string FormatDisplayValue(string value)
    {
        if (value == ERROR_MESSAGE || value == INFINITY_MESSAGE || value.EndsWith("."))
            return value;

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
            return FormatResult(number);

        return value;
    }
    #endregion
}
