using Microsoft.Maui.Layouts;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace mdi_maui.Controls;

public partial class MDIContainer : ContentView
{
    #region Properties
    public static readonly BindableProperty WindowsProperty = BindableProperty.Create(
        nameof(Windows),
        typeof(ObservableCollection<MDIWindow>),
        typeof(MDIContainer),
        propertyChanged: OnWindowsChanged);

    public ObservableCollection<MDIWindow> Windows
    {
        get => (ObservableCollection<MDIWindow>)GetValue(WindowsProperty);
        set => SetValue(WindowsProperty, value);
    }

    public static readonly BindableProperty ActiveWindowProperty = BindableProperty.Create(
        nameof(ActiveWindow),
        typeof(MDIWindow),
        typeof(MDIContainer),
        null,
        propertyChanged: OnActiveWindowChanged,
        defaultBindingMode: BindingMode.TwoWay);

    public MDIWindow? ActiveWindow
    {
        get => (MDIWindow?)GetValue(ActiveWindowProperty);
        set => SetValue(ActiveWindowProperty, value);
    }
    #endregion

    #region Fields
    private readonly AbsoluteLayout _layout;
    private double _windowOffset;
    private int _zIndex;
    #endregion

    #region Constructor
    public MDIContainer()
    {
        _layout = new AbsoluteLayout
        {
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        Content = _layout;
        SizeChanged += OnSizeChanged;
    }
    #endregion

    #region Private Methods
    private static void OnWindowsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MDIContainer container)
        {
            if (oldValue is ObservableCollection<MDIWindow> oldCollection)
                oldCollection.CollectionChanged -= container.OnWindowsCollectionChanged;

            if (newValue is ObservableCollection<MDIWindow> newCollection)
            {
                newCollection.CollectionChanged += container.OnWindowsCollectionChanged;
                container.UpdateWindows();
            }
            else
            {
                container._layout.Children.Clear();
            }
        }
    }

    private static void OnActiveWindowChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MDIContainer container)
            container.SetActiveWindow(newValue as MDIWindow);
    }

    private void OnWindowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (MDIWindow window in e.NewItems)
                AddWindow(window);
        }

        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (MDIWindow window in e.OldItems)
                RemoveWindow(window);
        }
    }

    private void UpdateWindows()
    {
        _layout.Children.Clear();
        if (Windows != null)
        {
            foreach (var window in Windows)
                AddWindow(window);
        }
    }

    private void AddWindow(MDIWindow window)
    {
        window.Activated += OnWindowActivated;
        window.Closing += OnWindowClosing;
        window.PropertyChanged += OnWindowPropertyChanged;

        window.ParentContainer = this;
        PositionWindow(window);

        AbsoluteLayout.SetLayoutFlags(window, AbsoluteLayoutFlags.None);
        AbsoluteLayout.SetLayoutBounds(window, new Rect(window.X, window.Y, window.WindowWidth, window.WindowHeight));

        _layout.Children.Add(window);
        SetActiveWindow(window);
    }

    private void RemoveWindow(MDIWindow window)
    {
        window.Activated -= OnWindowActivated;
        window.Closing -= OnWindowClosing;
        window.PropertyChanged -= OnWindowPropertyChanged;

        _layout.Children.Remove(window);

        if (ActiveWindow == window)
            ActiveWindow = null;

        window.Dispose();
    }

    private void PositionWindow(MDIWindow window)
    {
        const double initialOffset = 20;

        window.X = initialOffset + _windowOffset;
        window.Y = initialOffset + _windowOffset;

        _windowOffset = (_windowOffset + 20) % 100;

        window.AdjustPosition();
    }

    private void OnWindowActivated(object? sender, EventArgs e)
    {
        if (sender is MDIWindow window)
            SetActiveWindow(window);
    }

    private void OnWindowClosing(object? sender, EventArgs e)
    {
        if (sender is MDIWindow window && Windows.Contains(window))
        {
            Windows.Remove(window);
        }
    }

    private void OnWindowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is MDIWindow window && (e.PropertyName == nameof(MDIWindow.X) ||
                                            e.PropertyName == nameof(MDIWindow.Y) ||
                                            e.PropertyName == nameof(MDIWindow.WindowWidth) ||
                                            e.PropertyName == nameof(MDIWindow.WindowHeight)))
        {
            AbsoluteLayout.SetLayoutBounds(window, new Rect(window.X, window.Y, window.WindowWidth, window.WindowHeight));
        }
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        foreach (var window in Windows ?? Enumerable.Empty<MDIWindow>())
        {
            window.AdjustPosition();
        }
    }

    public void SetActiveWindow(MDIWindow? window)
    {
        if (ActiveWindow != window)
        {
            ActiveWindow?.Deactivate();
            ActiveWindow = window;
            ActiveWindow?.Activate();

            if (window != null)
            {
                window.ZIndex = ++_zIndex;
            }
        }
    }

    public void SnapWindow(MDIWindow window)
    {
        const double threshold = 20;
        MDIWindow? closestWindow = null;
        double closestDistance = double.MaxValue;

        foreach (var otherWindow in _layout.Children.OfType<MDIWindow>())
        {
            if (otherWindow == window) continue;

            double distance = window.GetDistanceTo(otherWindow);
            if (distance < threshold && distance < closestDistance)
            {
                closestDistance = distance;
                closestWindow = otherWindow;
            }
        }

        closestWindow?.AlignTo(window);
    }
    #endregion
}
