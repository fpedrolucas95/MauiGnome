using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Data;
using System.Windows.Input;

namespace mdi_maui.ViewModels;

public partial class CalcViewModel : ObservableObject
{
    #region Fields
    private string display = "0";
    private bool isNewNumber = true;
    private string expression = string.Empty;
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
        CalculateCommand = new RelayCommand(OnCalculate);
        ClearCommand = new RelayCommand(OnClear);
        DecimalCommand = new RelayCommand(OnDecimalPressed);
        PercentCommand = new RelayCommand(OnPercentPressed);
    }
    #endregion

    #region Properties
    public string Display
    {
        get => display;
        set => SetProperty(ref display, value);
    }
    #endregion

    #region Private Methods
    private void OnNumberPressed(string? number)
    {
        if (number == null) return;

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
        if (op == null) return;

        expression += Display + op;
        isNewNumber = true;
    }

    private void OnCalculate()
    {
        try
        {
            expression += Display;
            var result = EvaluateExpression(expression);
            Display = result.ToString();
            expression = string.Empty;
            isNewNumber = true;
        }
        catch (Exception)
        {
            Display = "Erro";
            expression = string.Empty;
            isNewNumber = true;
        }
    }

    private void OnClear()
    {
        Display = "0";
        isNewNumber = true;
        expression = string.Empty;
    }

    private void OnDecimalPressed()
    {
        if (isNewNumber)
        {
            Display = "0.";
            isNewNumber = false;
        }
        else if (!Display.Contains('.'))
        {
            Display += ".";
        }
    }

    private void OnPercentPressed()
    {
        if (double.TryParse(Display, out double number))
        {
            number /= 100;
            Display = number.ToString();
        }
    }

    private static double EvaluateExpression(string expression)
    {
        var table = new DataTable();
        var value = table.Compute(expression, string.Empty);
        return Convert.ToDouble(value);
    }
    #endregion
}