using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Shapes;
using System.ComponentModel;

namespace mdi_maui.Controls;

public partial class MDIWindow : ContentView, IDisposable, INotifyPropertyChanged
{
    #region Consts
    private const double RESIZE_HANDLE_SIZE = 20;
    private const double MIN_WIDTH = 110;
    private const double MIN_HEIGHT = 100;
    private const string TITLE_BAR_BG_COLOR = "#22201f90";
    private const string WINDOW_BG_COLOR = "#1a1a1a";
    private const string BORDER_COLOR = "#20FFFFFF";
    private const string SEPARATOR_COLOR = "#09FFFFFF";
    #endregion

    #region Fields
    private Point panStart;
    private Point startPosition;
    private bool isMoving;
    private bool isResizing;
    private bool _isClosing;
    private bool _isActive;
    private bool _isModified;
    public bool HasIcon => !string.IsNullOrEmpty(Icon);
    private readonly PanGestureRecognizer panGesture;
    private readonly Grid mainGrid;
    private readonly ContentView contentView;
    private readonly BoxView resizeHandle;
    #endregion

    #region Events
    public event EventHandler? Activated;
    public event EventHandler? Deactivated;
    public event EventHandler? Closing;
    public event EventHandler? Closed;
    public new event PropertyChangedEventHandler? PropertyChanged;
    #endregion

    #region Constructor
    public MDIWindow()
    {
        CloseCommand = new RelayCommand(Close);
        panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;

        var resizePanGesture = new PanGestureRecognizer();
        resizePanGesture.PanUpdated += OnResizePanUpdated;

        resizeHandle = new BoxView
        {
            BackgroundColor = Colors.Transparent,
            WidthRequest = RESIZE_HANDLE_SIZE,
            HeightRequest = RESIZE_HANDLE_SIZE,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End
        };
        resizeHandle.GestureRecognizers.Add(resizePanGesture);

        contentView = new ContentView();
        contentView.SetBinding(ContentView.ContentProperty, new Binding(nameof(WindowContent), source: this));

        var border = new Border
        {
            Stroke = Color.FromArgb(BORDER_COLOR),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            BackgroundColor = Color.FromArgb(WINDOW_BG_COLOR)
        };

        var titleBarGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            BackgroundColor = Color.FromArgb(TITLE_BAR_BG_COLOR),
            Margin = new Thickness(4, 0),
            HeightRequest = 32
        };

        titleBarGrid.GestureRecognizers.Add(panGesture);

        var iconImage = new Image
        {
            WidthRequest = 16,
            HeightRequest = 16,
            Margin = 8,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start
        };
        iconImage.SetBinding(Image.SourceProperty, new Binding(nameof(Icon), source: this));
        iconImage.SetBinding(IsVisibleProperty, new Binding(nameof(HasIcon), source: this));

        var titleLabel = new Label
        {
            FontSize = 13,
            FontFamily = "Quicksand-Light",
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        };
        titleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));

        var closeButtonImage = new Image { Source = "close.png", WidthRequest = 24 };
        var closeTapGesture = new TapGestureRecognizer();
        closeTapGesture.SetBinding(TapGestureRecognizer.CommandProperty, new Binding(nameof(CloseCommand), source: this));
        closeButtonImage.GestureRecognizers.Add(closeTapGesture);

        var closeButtonBorder = new Border { Stroke = Colors.Transparent, Content = closeButtonImage };

        titleBarGrid.Add(iconImage, 0, 0);
        titleBarGrid.Add(titleLabel, 1, 0);
        titleBarGrid.Add(closeButtonBorder, 3, 0);

        var separator = new BoxView
        {
            VerticalOptions = LayoutOptions.End,
            HeightRequest = 1,
            BackgroundColor = Color.FromArgb(SEPARATOR_COLOR)
        };

        mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(32) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            }
        };

        mainGrid.Add(titleBarGrid);
        mainGrid.Add(separator);
        mainGrid.Add(contentView);
        mainGrid.Add(resizeHandle);

        Grid.SetRow(separator, 0);
        Grid.SetRow(contentView, 1);
        Grid.SetRow(resizeHandle, 1);

        border.Content = mainGrid;
        Content = border;
        BackgroundColor = Colors.Transparent;
    }
    #endregion

    #region Properties
    public static new readonly BindableProperty XProperty = BindableProperty.Create(nameof(X), typeof(double), typeof(MDIWindow), 0.0, propertyChanged: OnPositionPropertyChanged);
    public static new readonly BindableProperty YProperty = BindableProperty.Create(nameof(Y), typeof(double), typeof(MDIWindow), 0.0, propertyChanged: OnPositionPropertyChanged);
    public static readonly BindableProperty WindowWidthProperty = BindableProperty.Create(nameof(WindowWidth), typeof(double), typeof(MDIWindow), 400.0, propertyChanged: OnSizePropertyChanged);
    public static readonly BindableProperty WindowHeightProperty = BindableProperty.Create(nameof(WindowHeight), typeof(double), typeof(MDIWindow), 300.0, propertyChanged: OnSizePropertyChanged);
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(MDIWindow), string.Empty);
    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(MDIWindow), string.Empty);
    public static readonly BindableProperty WindowContentProperty = BindableProperty.Create(nameof(WindowContent), typeof(View), typeof(MDIWindow), default(View));
    public static readonly BindableProperty CloseCommandProperty = BindableProperty.Create(nameof(CloseCommand), typeof(IRelayCommand), typeof(MDIWindow), default(IRelayCommand));

    public new double X { get => (double)GetValue(XProperty); set => SetValue(XProperty, value); }
    public new double Y { get => (double)GetValue(YProperty); set => SetValue(YProperty, value); }
    public double WindowWidth { get => (double)GetValue(WindowWidthProperty); set => SetValue(WindowWidthProperty, value); }
    public double WindowHeight { get => (double)GetValue(WindowHeightProperty); set => SetValue(WindowHeightProperty, value); }
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public View WindowContent { get => (View)GetValue(WindowContentProperty); set => SetValue(WindowContentProperty, value); }
    public IRelayCommand CloseCommand { get => (IRelayCommand)GetValue(CloseCommandProperty); private set => SetValue(CloseCommandProperty, value); }
    public bool IsActive { get => _isActive; private set => SetProperty(ref _isActive, value); }
    public bool IsModified { get => _isModified; set => SetProperty(ref _isModified, value); }
    #endregion

    #region Public Methods
    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            Activated?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            Deactivated?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Close()
    {
        if (_isClosing) return;
        _isClosing = true;
        try
        {
            if (IsModified && !SaveChanges())
                return;
            Closing?.Invoke(this, EventArgs.Empty);
            Closed?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _isClosing = false;
        }
    }

    public void Dispose()
    {
        panGesture.PanUpdated -= OnPanUpdated;
        resizeHandle.GestureRecognizers.Clear();
        Activated = null;
        Deactivated = null;
        Closing = null;
        Closed = null;
        Content = null;
        BindingContext = null;
    }
    #endregion

    #region Private Methods
    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                isMoving = true;
                panStart = new Point(e.TotalX, e.TotalY);
                startPosition = new Point(X, Y);
                Activate();
                break;
            case GestureStatus.Running:
                if (isMoving)
                {
                    X = startPosition.X + e.TotalX;
                    Y = startPosition.Y + e.TotalY;
                }
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                isMoving = false;
                break;
        }
    }

    private void OnResizePanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                isResizing = true;
                panStart = new Point(WindowWidth, WindowHeight);
                Activate();
                break;
            case GestureStatus.Running:
                if (isResizing)
                {
                    WindowWidth = Math.Max(MIN_WIDTH, panStart.X + e.TotalX);
                    WindowHeight = Math.Max(MIN_HEIGHT, panStart.Y + e.TotalY);
                }
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                isResizing = false;
                break;
        }
    }

    private static void OnPositionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MDIWindow window)
        {
            window.OnPropertyChanged(nameof(X));
            window.OnPropertyChanged(nameof(Y));
            AbsoluteLayout.SetLayoutBounds(window, new Rect(window.X, window.Y, window.WindowWidth, window.WindowHeight));
        }
    }

    private static void OnSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MDIWindow window)
        {
            window.OnPropertyChanged(nameof(WindowWidth));
            window.OnPropertyChanged(nameof(WindowHeight));
            AbsoluteLayout.SetLayoutBounds(window, new Rect(window.X, window.Y, window.WindowWidth, window.WindowHeight));
        }
    }
    #endregion

    #region Protected Methods
    protected virtual bool SaveChanges() => true;

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, string propertyName = "", Action? onChanged = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;
        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);
        return true;
    }
    #endregion
}
