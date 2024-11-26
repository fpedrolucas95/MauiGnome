using Microsoft.Maui.Layouts;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace mdi_maui.Controls;

public partial class MDIContainer : ContentView
{
    #region Properties
    public static readonly BindableProperty WindowsProperty = BindableProperty.Create(nameof(Windows), typeof(ObservableCollection<MDIWindow>), typeof(MDIContainer), propertyChanged: OnWindowsChanged);
    public static readonly BindableProperty ActiveWindowProperty = BindableProperty.Create(nameof(ActiveWindow), typeof(MDIWindow), typeof(MDIContainer), default(MDIWindow), propertyChanged: OnActiveWindowChanged, defaultBindingMode: BindingMode.TwoWay);
    public ObservableCollection<MDIWindow> Windows { get => (ObservableCollection<MDIWindow>)GetValue(WindowsProperty); set => SetValue(WindowsProperty, value); }
    public MDIWindow? ActiveWindow { get => (MDIWindow?)GetValue(ActiveWindowProperty); set => SetValue(ActiveWindowProperty, value); }
    #endregion

    #region Fields
    private readonly AbsoluteLayout _layout;
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
    }
    #endregion

    #region Private Methods
    private static void OnWindowsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MDIContainer container)
        {
            if (oldValue is ObservableCollection<MDIWindow> oldCollection)
                oldCollection.CollectionChanged -= container.Windows_CollectionChanged;
            if (newValue is ObservableCollection<MDIWindow> newCollection)
            {
                newCollection.CollectionChanged += container.Windows_CollectionChanged;
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

    private void Windows_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (MDIWindow window in e.NewItems)
                AddWindowToLayout(window);
        if (e.OldItems != null)
            foreach (MDIWindow window in e.OldItems)
                RemoveWindowFromLayout(window);
    }

    private void Window_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is MDIWindow window)
            if (e.PropertyName == nameof(MDIWindow.X) ||
                e.PropertyName == nameof(MDIWindow.Y) ||
                e.PropertyName == nameof(MDIWindow.WindowWidth) ||
                e.PropertyName == nameof(MDIWindow.WindowHeight))
                AbsoluteLayout.SetLayoutBounds(window, new Rect(window.X, window.Y, window.WindowWidth, window.WindowHeight));
    }

    private void OnWindowActivated(object? sender, EventArgs e)
    {
        if (sender is MDIWindow window)
            SetActiveWindow(window);
    }

    private void OnWindowClosing(object? sender, EventArgs e)
    {
    }

    private void UpdateWindows()
    {
        _layout.Children.Clear();
        if (Windows != null)
        {
            foreach (var window in Windows)
                AddWindowToLayout(window);
        }
    }

    private void AddWindowToLayout(MDIWindow window)
    {
        if (window == null) return;

        window.Activated += OnWindowActivated;
        window.Closing += OnWindowClosing;
        window.PropertyChanged += Window_PropertyChanged;

        AbsoluteLayout.SetLayoutFlags(window, AbsoluteLayoutFlags.None);
        AbsoluteLayout.SetLayoutBounds(window, new Rect(window.X, window.Y, window.WindowWidth, window.WindowHeight));

        _layout.Children.Add(window);
        SetActiveWindow(window);
    }

    private void RemoveWindowFromLayout(MDIWindow window)
    {
        if (window == null) return;

        window.Activated -= OnWindowActivated;
        window.Closing -= OnWindowClosing;
        window.PropertyChanged -= Window_PropertyChanged;

        _layout.Children.Remove(window);

        if (ActiveWindow == window)
            ActiveWindow = null;
    }
    #endregion

    #region Public Methods
    public void SetActiveWindow(MDIWindow? window)
    {
        if (ActiveWindow != window)
        {
            ActiveWindow?.Deactivate();
            ActiveWindow = window;
            ActiveWindow?.Activate();
        }
    }
    #endregion
}