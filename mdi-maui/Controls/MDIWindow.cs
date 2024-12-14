using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Shapes;
using System.ComponentModel;

namespace mdi_maui.Controls;

public partial class MDIWindow : ContentView, IDisposable, INotifyPropertyChanged
{
    #region Constants
    private const double ResizeHandleSize = 20;
    private const double MinWidth = 110;
    private const double MinHeight = 100;
    #endregion

    #region Fields
    private Point _panStart;
    private Point _startPosition;
    private bool _isMoving;
    private bool _isResizing;
    private bool _isClosing;
    private bool _isMaximized;
    private Rect _previousBounds;

    private readonly PanGestureRecognizer _panGesture;

    private readonly Grid _mainGrid;

    private readonly ContentView _contentView;
    private readonly BoxView _resizeHandle;
    private readonly Image _closeButtonImage;
    private readonly Image _maximizeButtonImage;

    private readonly Grid _titleBar;
    private readonly Border _windowBorder;
    private readonly Label _titleLabel;
    private readonly Image _iconImage;
    private readonly BoxView _separator;
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
        _panGesture = new PanGestureRecognizer();
        _mainGrid = CreateMainGrid();
        _contentView = new ContentView();
        _resizeHandle = CreateResizeHandle();
        CloseCommand = new RelayCommand(Close);

        _closeButtonImage = new Image { WidthRequest = 24 };
        _maximizeButtonImage = new Image { WidthRequest = 24 };

        _titleLabel = new Label();
        _iconImage = new Image();
        _separator = new BoxView();

        _windowBorder = CreateWindowBorder();
        _titleBar = CreateTitleBar();

        InitializeWindow();

        UpdateTheme();

        if (Application.Current != null)
        {
            Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
        }

        UpdateMaximizeButton();
    }

    #endregion

    #region Properties
    public static new readonly BindableProperty XProperty = BindableProperty.Create(nameof(X), typeof(double), typeof(MDIWindow), 0.0, propertyChanged: OnPositionChanged);
    public new double X
    {
        get => (double)GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public static new readonly BindableProperty YProperty = BindableProperty.Create(nameof(Y), typeof(double), typeof(MDIWindow), 0.0, propertyChanged: OnPositionChanged);
    public new double Y
    {
        get => (double)GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    public static readonly BindableProperty WindowWidthProperty = BindableProperty.Create(nameof(WindowWidth), typeof(double), typeof(MDIWindow), 400.0, propertyChanged: OnSizeChanged);
    public double WindowWidth
    {
        get => (double)GetValue(WindowWidthProperty);
        set => SetValue(WindowWidthProperty, value);
    }

    public static readonly BindableProperty WindowHeightProperty = BindableProperty.Create(nameof(WindowHeight), typeof(double), typeof(MDIWindow), 300.0, propertyChanged: OnSizeChanged);
    public double WindowHeight
    {
        get => (double)GetValue(WindowHeightProperty);
        set => SetValue(WindowHeightProperty, value);
    }

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(MDIWindow), string.Empty);
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(MDIWindow), string.Empty);
    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly BindableProperty WindowContentProperty = BindableProperty.Create(nameof(WindowContent), typeof(View), typeof(MDIWindow), default(View));
    public View WindowContent
    {
        get => (View)GetValue(WindowContentProperty);
        set => SetValue(WindowContentProperty, value);
    }

    public static readonly BindableProperty CloseCommandProperty = BindableProperty.Create(nameof(CloseCommand), typeof(IRelayCommand), typeof(MDIWindow), default(IRelayCommand));
    public IRelayCommand CloseCommand
    {
        get => (IRelayCommand)GetValue(CloseCommandProperty);
        private set => SetValue(CloseCommandProperty, value);
    }

    public static readonly BindableProperty ResizeProperty = BindableProperty.Create(nameof(Resize), typeof(bool), typeof(MDIWindow), true, propertyChanged: OnResizePropertyChanged);
    public bool Resize
    {
        get => (bool)GetValue(ResizeProperty);
        set => SetValue(ResizeProperty, value);
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        private set => SetProperty(ref _isActive, value);
    }

    private bool _isModified;
    public bool IsModified
    {
        get => _isModified;
        set => SetProperty(ref _isModified, value);
    }

    public MDIContainer? ParentContainer { get; set; }
    public bool HasIcon => !string.IsNullOrEmpty(Icon);
    #endregion

    #region Public Methods
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        Activated?.Invoke(this, EventArgs.Empty);

        _iconImage.Opacity = 1.0;
        _titleLabel.Opacity = 1.0;
        _closeButtonImage.Opacity = 1.0;
        _maximizeButtonImage.Opacity = 1.0;
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        Deactivated?.Invoke(this, EventArgs.Empty);

        _iconImage.Opacity = 0.7;
        _titleLabel.Opacity = 0.7;
        _closeButtonImage.Opacity = 0.7;
        _maximizeButtonImage.Opacity = 0.7;
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
            Dispose();
        }
        finally
        {
            _isClosing = false;
        }
    }

    public void Dispose()
    {
        _panGesture.PanUpdated -= OnPanUpdated;
        _resizeHandle.GestureRecognizers.Clear();

        Activated = null;
        Deactivated = null;
        Closing = null;
        Closed = null;
        Content = null;
        BindingContext = null;

        if (Application.Current != null)
        {
            Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;
        }
    }

    public void AdjustPosition()
    {
        if (ParentContainer == null) return;

        double containerWidth = ParentContainer.Width;
        double containerHeight = ParentContainer.Height;

        X = Math.Clamp(X, 0, containerWidth - WindowWidth);
        Y = Math.Clamp(Y, 0, containerHeight - WindowHeight);

        WindowWidth = Math.Min(WindowWidth, containerWidth);
        WindowHeight = Math.Min(WindowHeight, containerHeight);
    }

    public double GetDistanceTo(MDIWindow otherWindow)
    {
        double dx = (X + WindowWidth / 2) - (otherWindow.X + otherWindow.WindowWidth / 2);
        double dy = (Y + WindowHeight / 2) - (otherWindow.Y + otherWindow.WindowHeight / 2);
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public void AlignTo(MDIWindow otherWindow)
    {
        double leftDiff = Math.Abs(X - (otherWindow.X + otherWindow.WindowWidth));
        double rightDiff = Math.Abs((X + WindowWidth) - otherWindow.X);
        double topDiff = Math.Abs(Y - (otherWindow.Y + otherWindow.WindowHeight));
        double bottomDiff = Math.Abs((Y + WindowHeight) - otherWindow.Y);
        double minDiff = new[] { leftDiff, rightDiff, topDiff, bottomDiff }.Min();

        if (minDiff == leftDiff)
        {
            X = otherWindow.X + otherWindow.WindowWidth;
        }
        else if (minDiff == rightDiff)
        {
            X = otherWindow.X - WindowWidth;
        }
        else if (minDiff == topDiff)
        {
            Y = otherWindow.Y + otherWindow.WindowHeight;
        }
        else if (minDiff == bottomDiff)
        {
            Y = otherWindow.Y - WindowHeight;
        }

        AdjustPosition();
    }

    public void Maximize()
    {
        if (ParentContainer == null) return;

        _isMaximized = true;

        _previousBounds = new Rect(X, Y, WindowWidth, WindowHeight);

        X = 0;
        Y = 0;
        WindowWidth = ParentContainer.Width;
        WindowHeight = ParentContainer.Height;

        UpdateMaximizeButton();
    }

    public void Restore()
    {
        if (!_isMaximized) return;

        _isMaximized = false;

        X = _previousBounds.X;
        Y = _previousBounds.Y;
        WindowWidth = _previousBounds.Width;
        WindowHeight = _previousBounds.Height;

        UpdateMaximizeButton();
    }
    #endregion

    #region Private Methods
    private void InitializeWindow()
    {
        _panGesture.PanUpdated += OnPanUpdated;

        var resizePanGesture = new PanGestureRecognizer();
        resizePanGesture.PanUpdated += OnResizePanUpdated;

        if (Resize)
        {
            _resizeHandle.GestureRecognizers.Add(resizePanGesture);
            _resizeHandle.IsVisible = true;
            _maximizeButtonImage.IsVisible = true;
        }
        else
        {
            _resizeHandle.IsVisible = false;
            _maximizeButtonImage.IsVisible = false;
        }

        _contentView.SetBinding(ContentView.ContentProperty, new Binding(nameof(WindowContent), source: this));

        var closeTapGesture = new TapGestureRecognizer();
        closeTapGesture.SetBinding(TapGestureRecognizer.CommandProperty, new Binding(nameof(CloseCommand), source: this));
        _closeButtonImage.GestureRecognizers.Add(closeTapGesture);

        BackgroundColor = Colors.Transparent;

        Content = _windowBorder;
    }

    private static Grid CreateMainGrid()
    {
        return new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(32) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            }
        };
    }

    private static BoxView CreateResizeHandle()
    {
        return new BoxView
        {
            BackgroundColor = Colors.Transparent,
            WidthRequest = ResizeHandleSize,
            HeightRequest = ResizeHandleSize,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End
        };
    }

    private static Border CreateWindowBorder()
    {
        var border = new Border
        {
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(10)
            }
        };

        return border;
    }

    private Grid CreateTitleBar()
    {
        var titleBar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto }, 
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            HeightRequest = 32
        };

        titleBar.GestureRecognizers.Add(_panGesture);
        titleBar.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(OnTitleBarTapped) });

        _iconImage.WidthRequest = 16;
        _iconImage.HeightRequest = 16;
        _iconImage.Margin = 8;
        _iconImage.VerticalOptions = LayoutOptions.Center;
        _iconImage.HorizontalOptions = LayoutOptions.Start;
        _iconImage.SetBinding(Image.SourceProperty, new Binding(nameof(Icon), source: this));
        _iconImage.SetBinding(IsVisibleProperty, new Binding(nameof(HasIcon), source: this));

        titleBar.Children.Add(_iconImage);
        Grid.SetColumn(_iconImage, 0);
        Grid.SetRow(_iconImage, 0);

        _titleLabel.FontSize = 13;
        _titleLabel.VerticalOptions = LayoutOptions.Center;
        _titleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));

        titleBar.Children.Add(_titleLabel);
        Grid.SetColumn(_titleLabel, 1);
        Grid.SetRow(_titleLabel, 0);

        var maximizeTapGesture = new TapGestureRecognizer();
        maximizeTapGesture.Tapped += OnMaximizeButtonTapped;
        _maximizeButtonImage.GestureRecognizers.Add(maximizeTapGesture);
        _maximizeButtonImage.WidthRequest = 16;
        _maximizeButtonImage.HeightRequest = 16;

        titleBar.Children.Add(_maximizeButtonImage);
        Grid.SetColumn(_maximizeButtonImage, 2);
        Grid.SetRow(_maximizeButtonImage, 0);

        var closeButtonBorder = new Border
        {
            Stroke = Colors.Transparent,
            Content = _closeButtonImage,
            Margin = new(0, 0, 4, 0)
        };

        titleBar.Children.Add(closeButtonBorder);
        Grid.SetColumn(closeButtonBorder, 3);
        Grid.SetRow(closeButtonBorder, 0);

        _mainGrid.Children.Add(titleBar);
        Grid.SetRow(titleBar, 0);

        _separator.VerticalOptions = LayoutOptions.End;
        _separator.HeightRequest = 1;
        _mainGrid.Children.Add(_separator);
        Grid.SetRow(_separator, 0);

        _mainGrid.Children.Add(_contentView);
        Grid.SetRow(_contentView, 1);

        _mainGrid.Children.Add(_resizeHandle);
        Grid.SetRow(_resizeHandle, 1);

        _windowBorder.Content = _mainGrid;

        return titleBar;
    }

    private void UpdateTheme()
    {
        var currentTheme = Application.Current?.RequestedTheme ?? AppTheme.Light;
        var titleBarBackgroundColor = GetColor("#E4E4D5", "#22201f90");
        var windowBackgroundColor = GetColor("#FFFFFF", "#1a1a1a");
        var borderColor = GetColor("#20000000", "#20FFFFFF");
        var separatorColor = GetColor("#09000000", "#09FFFFFF");
        var textColor = GetColor("#000000", "#FFFFFF");

        _titleBar.BackgroundColor = titleBarBackgroundColor;
        _windowBorder.BackgroundColor = windowBackgroundColor;
        _windowBorder.Stroke = borderColor;
        _separator.BackgroundColor = separatorColor;
        _titleLabel.TextColor = textColor;

        var closeButtonImageSource = GetImageSource("light_close.png", "dark_close.png");
        _closeButtonImage.Source = closeButtonImageSource;

        if (Resize)
        {
            UpdateMaximizeButton();
        }
        else
        {
            _maximizeButtonImage.Source = null;
        }
    }

    private static Color GetColor(string lightColor, string darkColor)
    {
        var currentTheme = Application.Current?.RequestedTheme ?? AppTheme.Light;
        return currentTheme == AppTheme.Dark ? Color.FromArgb(darkColor) : Color.FromArgb(lightColor);
    }

    private static string GetImageSource(string lightImage, string darkImage)
    {
        var currentTheme = Application.Current?.RequestedTheme ?? AppTheme.Light;
        return currentTheme == AppTheme.Dark ? darkImage : lightImage;
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        UpdateTheme();
    }

    private void UpdateMaximizeButton()
    {
        if (!Resize)
        {
            _maximizeButtonImage.Source = null;
            return;
        }

        string maximizeImage = _isMaximized
            ? GetImageSource("light_restore.png", "dark_restore.png")
            : GetImageSource("light_maximize.png", "dark_maximize.png");

        _maximizeButtonImage.Source = maximizeImage;
    }

    private void OnMaximizeButtonTapped(object? sender, EventArgs e)
    {
        if (_isMaximized)
        {
            Restore();
        }
        else
        {
            Maximize();
        }
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (_isMaximized) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _isMoving = true;
                _panStart = new Point(e.TotalX, e.TotalY);
                _startPosition = new Point(X, Y);
                Activate();
                break;

            case GestureStatus.Running:
                if (_isMoving)
                {
                    X = _startPosition.X + e.TotalX;
                    Y = _startPosition.Y + e.TotalY;
                    AdjustPosition();
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isMoving = false;
                ParentContainer?.SnapWindow(this);
                break;
        }
    }

    private void OnResizePanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (!Resize || _isMaximized) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _isResizing = true;
                _panStart = new Point(WindowWidth, WindowHeight);
                Activate();
                break;

            case GestureStatus.Running:
                if (_isResizing)
                {
                    double newWidth = Math.Max(MinWidth, _panStart.X + e.TotalX);
                    double newHeight = Math.Max(MinHeight, _panStart.Y + e.TotalY);

                    if (ParentContainer != null)
                    {
                        newWidth = Math.Min(newWidth, ParentContainer.Width - X);
                        newHeight = Math.Min(newHeight, ParentContainer.Height - Y);
                    }

                    WindowWidth = newWidth;
                    WindowHeight = newHeight;
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isResizing = false;
                break;
        }
    }

    private void OnTitleBarTapped()
    {
        Activate();
    }

    private void OnTitleBarTapped(object? sender, EventArgs e)
    {
        Activate();
    }

    private static void OnPositionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MDIWindow window)
        {
            window.OnPropertyChanged(nameof(X));
            window.OnPropertyChanged(nameof(Y));
            AbsoluteLayout.SetLayoutBounds(window, new Rect(window.X, window.Y, window.WindowWidth, window.WindowHeight));
        }
    }

    private static void OnSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MDIWindow window)
        {
            window.OnPropertyChanged(nameof(WindowWidth));
            window.OnPropertyChanged(nameof(WindowHeight));
            AbsoluteLayout.SetLayoutBounds(window, new Rect(window.X, window.Y, window.WindowWidth, window.WindowHeight));
        }
    }

    private static void OnResizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MDIWindow window)
        {
            bool resize = (bool)newValue;

            window._resizeHandle.IsVisible = resize;
            window._maximizeButtonImage.IsVisible = resize;
            window.AdjustPosition();
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