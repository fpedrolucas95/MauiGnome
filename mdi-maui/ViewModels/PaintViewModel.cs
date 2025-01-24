using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows.Input;

namespace mdi_maui.ViewModels;

public partial class PaintViewModel : INotifyPropertyChanged
{
    #region Fields
    private string _toolType;
    private Color _toolColor;
    private int _toolWidth;
    #endregion

    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    #endregion

    #region Constructor
    public PaintViewModel()
    {
        _toolType = string.Empty;
        _toolColor = Colors.Black;
        _toolWidth = 5;

        ChangeColorCommand = new RelayCommand<string>(ChangeColor);
    }
    #endregion

    #region Properties
    public string ToolType
    {
        get => _toolType;
        set => SetProperty(ref _toolType, value, nameof(ToolType));
    }

    public Color ToolColor
    {
        get => _toolColor;
        set => SetProperty(ref _toolColor, value, nameof(ToolColor));
    }

    public int ToolWidth
    {
        get => _toolWidth;
        set => SetProperty(ref _toolWidth, value, nameof(ToolWidth));
    }
    #endregion

    #region Commands
    public ICommand ChangeColorCommand { get; }
    #endregion

    #region Private Methods
    private void ChangeColor(string? colorName)
    {
        if (string.IsNullOrWhiteSpace(colorName)) return;

        var colorMapping = new Dictionary<string, Color>
        {
            ["Red"] = Colors.Red,
            ["Blue"] = Colors.Blue,
            ["Black"] = Colors.Black,
            ["White"] = Colors.White,
            ["Green"] = Colors.Green,
            ["Yellow"] = Colors.Yellow
        };

        if (colorMapping.TryGetValue(colorName, out var selectedColor))
        {
            ToolColor = selectedColor;
        }
    }

    private bool SetProperty<T>(ref T backingField, T value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value)) return false;
        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    #endregion

    #region Protected Methods
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
