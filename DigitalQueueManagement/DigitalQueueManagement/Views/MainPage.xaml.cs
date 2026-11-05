using DigitalQueueManagement.Model;
using DigitalQueueManagement.ViewModel;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Charts;

namespace DigitalQueueManagement.Views;

public partial class MainPage : ContentPage
{
    private static readonly Color Cobalt = Color.FromArgb("#183E91");
    private static readonly Color Canvas = Color.FromArgb("#F3F6FC");
    private static readonly Color Slate50 = Color.FromArgb("#F8FAFC");
    private static readonly Color Slate100 = Color.FromArgb("#F1F5F9");
    private static readonly Color Slate200 = Color.FromArgb("#E2E8F0");
    private static readonly Color Slate400 = Color.FromArgb("#94A3B8");
    private static readonly Color Slate500 = Color.FromArgb("#64748B");
    private static readonly Color Slate700 = Color.FromArgb("#334155");
    private static readonly Color Slate900 = Color.FromArgb("#0F172A");
    private static readonly Color Teal = Color.FromArgb("#159B85");
    private static readonly Color Amber = Color.FromArgb("#D18A16");
    private static readonly Color Rose = Color.FromArgb("#DC6670");

    private readonly QueueSessionViewModel _viewModel = new(new QueueStore());
    private readonly AppShellViewModel _shellViewModel;
    private IReadOnlyList<SfSegmentItem> _headerTabItems = Array.Empty<SfSegmentItem>();
    private ManagePage? _managePage;
    private bool? _lastWideLayout;
    private bool? _lastMobileLayout;
    private VerticalStackLayout? _tokenList;
    private Label? _tokenCountLabel;

    public MainPage()
    {
        InitializeComponent();
        _shellViewModel = new AppShellViewModel(_viewModel);
        BindingContext = _shellViewModel;
        _headerTabItems = BuildHeaderTabItems();
        HeaderTabs.ItemsSource = _headerTabItems;
        HeaderTabs.SelectionChanged += OnHeaderTabsSelectionChanged;
        UpdateHeaderTabIcons();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        SizeChanged += OnPageSizeChanged;
        UpdateShellPresentation();
        RenderPage();
    }

    private static IReadOnlyList<SfSegmentItem> BuildHeaderTabItems()
    {
        return new[]
        {
            new SfSegmentItem
            {
                Text = "Overview",
                ImageSource = CreateHeaderTabIcon("dashboard", Slate500),
                ImageSize = 15
            },
            new SfSegmentItem
            {
                Text = "Manage",
                ImageSource = CreateHeaderTabIcon("tune", Slate500),
                ImageSize = 15
            }
        };
    }

    private static FontImageSource CreateHeaderTabIcon(string glyph, Color color) =>
        new()
    {
        Glyph = glyph,
        FontFamily = "MaterialIcons",
        Color = color,
        Size = 15
    };

    private void OnHeaderTabsSelectionChanged(
        object? sender,
        Syncfusion.Maui.Buttons.SelectionChangedEventArgs e)
    {
        UpdateHeaderTabIcons();
        if (e.NewIndex == 0)
        {
            _viewModel.SetHeader("Overview");
        }
        else if (e.NewIndex == 1)
        {
            _viewModel.SetHeader("Manage");
        }
    }

    private void UpdateHeaderTabIcons()
    {
        for (var index = 0; index < _headerTabItems.Count; index++)
        {
            var glyph = index == 0 ? "dashboard" : "tune";
            var color = index == _viewModel.HeaderTabIndex ? Colors.White : Slate500;
            _headerTabItems[index].ImageSource = CreateHeaderTabIcon(glyph, color);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(QueueSessionViewModel.ActiveHeaderTab) or nameof(QueueSessionViewModel.ActiveTab))
        {
            UpdateHeaderTabIcons();
            UpdateShellPresentation();
            RenderPage();
        }
    }

    private void OnOptionsClicked(object? sender, EventArgs e) => ShowResetConfirmation();

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width > 0)
        {
            var contentWidth = Math.Min(Width, 1440);
            var isMobile = Width < 768;
            DesktopHeader.IsVisible = !isMobile;
            MobileHeader.IsVisible = isMobile;
            MobileBottomNavigation.IsVisible = isMobile;
            HeaderLayout.WidthRequest = contentWidth;
            HeaderLayout.Margin = Thickness.Zero;
            PageHost.WidthRequest = contentWidth;
            PageHost.Margin = Thickness.Zero;
            var wideLayout = contentWidth > 1050;
            if (_lastWideLayout != wideLayout || _lastMobileLayout != isMobile)
            {
                _lastWideLayout = wideLayout;
                _lastMobileLayout = isMobile;
                UpdateShellPresentation();
                RenderPage();
            }
        }
    }

    private void UpdateShellPresentation()
    {
        var isMobile = Width is > 0 and < 768;
        if (MobileHeaderTitle is not null)
        {
            MobileHeaderTitle.Text = _viewModel.ActiveHeaderTab == "Overview"
                ? "Queue Overview"
                : "Queue Management";
            MobileHeaderDate.IsVisible = _viewModel.ActiveHeaderTab == "Overview";
        }

        if (MobileOverviewButton is not null)
        {
            SetMobileNavigationButton(
                MobileOverviewButton,
                "dashboard",
                _viewModel.ActiveHeaderTab == "Overview");
            SetMobileNavigationButton(
                MobileManageButton,
                "tune",
                _viewModel.ActiveHeaderTab == "Manage");
        }

        DesktopHeader.IsVisible = !isMobile;
        MobileHeader.IsVisible = isMobile;
        MobileBottomNavigation.IsVisible = isMobile;
    }

    private static void SetMobileNavigationButton(Button button, string glyph, bool isSelected)
    {
        var color = isSelected ? Cobalt : Slate500;
        button.TextColor = color;
        button.ImageSource = new FontImageSource
        {
            Glyph = glyph,
            FontFamily = "MaterialIcons",
            Color = color,
            Size = 22
        };
    }

    private void OnMobileOverviewClicked(object? sender, EventArgs e) =>
        _viewModel.SetHeader("Overview");

    private void OnMobileManageClicked(object? sender, EventArgs e) =>
        _viewModel.SetHeader("Manage");

    private void RenderPage()
    {
        if (_viewModel.ActiveHeaderTab == "Overview")
        {
            _managePage = null;
            PageHost.Content = new OverviewPage(
                new QueueOverviewViewModel(_viewModel));
            return;
        }

        _managePage = new ManagePage(
            new ManagePageViewModel(_viewModel),
            ShowTokenDetails,
            ShowCounterManagement,
            action => RunCounterAction(action),
            action => RunCounterAction(action),
            ShowTokenPickerForTransfer);
        PageHost.Content = _managePage;
    }

    private View BuildManagePage()
    {
        var content = new VerticalStackLayout
        {
            Spacing = 18,
            Padding = new Thickness(28, 22, 28, 36)
        };
        var navigationRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Auto), new(GridLength.Star) },
            ColumnSpacing = 12
        };
        navigationRow.Add(BuildManageNavigation(), 0, 0);
        navigationRow.Add(new Border
        {
            Padding = new Thickness(12, 7),
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#A7F3D0"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) },
            Content = TextLabel("●  Operational Hub  ·  Local sample", 11, Slate700, FontAttributes.Bold),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        }, 1, 0);
        content.Add(navigationRow);
        content.Add(_viewModel.ActiveTab switch
        {
            "Counters" => new CountersPage(
                new CounterOperationsViewModel(_viewModel),
                ShowCounterManagement,
                ShowTokenDetails,
                action => RunCounterAction(action),
                action => RunCounterAction(action),
                ShowTokenPickerForTransfer),
            _ => new TokensPage(new TokenManagementViewModel(_viewModel), ShowTokenDetails)
        });
        return content;
    }

    private View BuildManageNavigation()
    {
        var navigation = new SfSegmentedControl
        {
            ItemsSource = new[]
            {
                $"Tokens  {_viewModel.TotalTokenCount}",
                $"Counters  {_viewModel.Counters.Count} active"
            },
            SegmentHeight = 38,
            WidthRequest = 280
        };
        navigation.SetBinding(
            SfSegmentedControl.SelectedIndexProperty,
            new Binding($"{nameof(AppShellViewModel.QueueSession)}.{nameof(QueueSessionViewModel.ManageTabIndex)}", BindingMode.TwoWay));
        navigation.SelectionIndicatorSettings = new SelectionIndicatorSettings
        {
            SelectionIndicatorPlacement = SelectionIndicatorPlacement.Fill,
            Background = Cobalt,
            TextColor = Colors.White
        };
        return new Border
        {
            Padding = 0,
            BackgroundColor = Colors.White,
            Stroke = Slate200,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Content = navigation
        };
    }

    private View BuildOverview()
    {
        var content = new VerticalStackLayout
        {
            Spacing = 18,
            Padding = new Thickness(28, 22, 28, 36)
        };
        content.Add(BuildMetrics(
        [
            ("Now Serving", _viewModel.Tokens.FirstOrDefault(token => token.Status == TokenStatus.Serving)?.Id ?? "—", "Counter 1", "In Progress", Cobalt),
            ("Waiting Tokens", _viewModel.WaitingTokenCount.ToString(), "In Queue", $"~{_viewModel.WaitingTokenCount * 4}m est. avg", Cobalt),
            ("Completed Tokens", _viewModel.CompletedTokenCount.ToString(), "Today", "Tokens Today", Teal),
            ("Average Wait", $"{_viewModel.WaitingTokenCount * 4}m", "Within SLA", "Queue estimate", Cobalt)
        ]));

        var body = new Grid
        {
            ColumnDefinitions = Width > 1050
                ? new ColumnDefinitionCollection { new(new GridLength(1, GridUnitType.Star)), new(new GridLength(1.35, GridUnitType.Star)) }
                : new ColumnDefinitionCollection { new(GridLength.Star) },
            ColumnSpacing = 18,
            RowSpacing = 16
        };
        var left = new VerticalStackLayout { Spacing = 16 };
        left.Add(BuildQueueLoadCard());
        left.Add(BuildCounterStatusCard());
        body.Add(left, 0, 0);
        body.Add(BuildUpcomingCard(), Width > 1050 ? 1 : 0, 0);
        content.Add(body);
        return content;
    }

    private View BuildMetrics((string Title, string Value, string Left, string Right, Color Accent)[] metrics)
    {
        var grid = new Grid
        {
            ColumnDefinitions = Width > 1050
                ? new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) }
                : new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) },
            RowDefinitions = Width > 1050
                ? new RowDefinitionCollection { new(GridLength.Auto) }
                : new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) },
            ColumnSpacing = 14,
            RowSpacing = 14
        };
        for (var index = 0; index < metrics.Length; index++)
        {
            var item = metrics[index];
            var inner = new VerticalStackLayout { Spacing = 8 };
            var top = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) }
            };
            top.Add(TextLabel(item.Title, 12, Slate500), 0, 0);
            top.Add(TextLabel("◈", 16, item.Accent, FontAttributes.Bold), 1, 0);
            inner.Add(top);
            inner.Add(TextLabel(item.Value, 30, Cobalt, FontAttributes.Bold));
            var footer = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            };
            footer.Add(TextLabel(item.Left, 10, Slate500), 0, 0);
            footer.Add(TextLabel(item.Right, 10, item.Accent, FontAttributes.Bold), 1, 0);
            inner.Add(footer);
            grid.Add(Card(inner, 16), index % (Width > 1050 ? 4 : 2), index / (Width > 1050 ? 4 : 2));
        }
        return grid;
    }

    private View BuildQueueLoadCard()
    {
        var body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(100), new(GridLength.Star) },
            ColumnSpacing = 18,
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) }
        };
        body.Add(CardHeading("Queue Load", "Current load capacity metric", "Optimal", Teal), 0, 0);
        body.SetColumnSpan(body.Children.Last(), 2);

        var gauge = new Border
        {
            WidthRequest = 94,
            HeightRequest = 94,
            Stroke = Cobalt,
            StrokeThickness = 8,
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(47) },
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    TextLabel($"{_viewModel.QueueLoad}%", 22, Slate900, FontAttributes.Bold),
                    TextLabel("LOAD", 9, Slate400, FontAttributes.Bold)
                }
            },
            HorizontalOptions = LayoutOptions.Start
        };
        body.Add(gauge, 0, 1);
        var metrics = new VerticalStackLayout
        {
            Spacing = 14,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                LabelPair("Waiting Tokens", $"{_viewModel.WaitingTokenCount} waiting"),
                LabelPair("Active Terminals", $"{_viewModel.OnlineCounterCount} of {_viewModel.Counters.Count} active")
            }
        };
        body.Add(metrics, 1, 1);
        return Card(body, 18);
    }

    private View BuildCounterStatusCard()
    {
        var stack = new VerticalStackLayout { Spacing = 12 };
        stack.Add(CardHeading("Counter Status", "Local terminal assignment", $"{_viewModel.OnlineCounterCount} Desks Online", Cobalt));
        foreach (var counter in _viewModel.Counters)
        {
            var token = _viewModel.CurrentTokenFor(counter.Name);
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new(40), new(GridLength.Star), new(GridLength.Auto) },
                ColumnSpacing = 12,
                Padding = new Thickness(12, 10),
                BackgroundColor = Slate50
            };
            row.Add(TextLabel(counter.Name.Replace("Counter ", string.Empty), 13, Cobalt, FontAttributes.Bold), 0, 0);
            var details = new VerticalStackLayout { Spacing = 2 };
            details.Add(TextLabel($"{counter.Name} · {token?.Id ?? "Available"}", 12, Slate900, FontAttributes.Bold));
            details.Add(TextLabel(counter.Service, 10, Slate500));
            row.Add(details, 1, 0);
            var status = _viewModel.GetCounterAvailabilityText(counter);
            var statusColors = CounterStatusColors(status);
            row.Add(StatusPill(status, statusColors.Background, statusColors.Foreground), 2, 0);
            var button = new Button
            {
                Text = string.Empty,
                BackgroundColor = Colors.Transparent,
                Opacity = 0,
                HeightRequest = 54,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            button.Clicked += (_, _) =>
            {
                _viewModel.SelectCounter(counter.Name);
                ShowCounterOperations(counter);
            };
            var rowContainer = new Grid();
            rowContainer.Add(new Border
            {
                Padding = 0,
                BackgroundColor = Slate50,
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(9) },
                Content = row
            });
            rowContainer.Add(button);
            stack.Add(rowContainer);
        }
        return Card(stack, 18);
    }

    private View BuildUpcomingCard()
    {
        var stack = new VerticalStackLayout { Spacing = 12 };
        var heading = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) }
        };
        var title = new VerticalStackLayout { Spacing = 3 };
        title.Add(TextLabel("Upcoming Tokens", 14, Slate900, FontAttributes.Bold));
        title.Add(TextLabel("Next in line across all counters", 11, Slate500));
        heading.Add(title, 0, 0);
        heading.Add(BuildUpcomingFilter(), 1, 0);
        stack.Add(heading);
        var upcoming = _viewModel.Tokens
            .Where(token => token.Status == TokenStatus.Waiting)
            .Where(token => _viewModel.UpcomingFilter == "All" || token.Priority.ToString() == _viewModel.UpcomingFilter)
            .Take(6).ToArray();
        if (upcoming.Length == 0)
        {
            stack.Add(EmptyState("No tokens match this filter.", "Change the priority filter to see the queue."));
        }
        foreach (var token in upcoming)
        {
            stack.Add(TokenSummaryRow(token, () => OpenTokenDetails(token)));
        }
        return Card(stack, 18);
    }

    private View BuildUpcomingFilter()
    {
        var picker = new Picker
        {
            ItemsSource = new[] { "All", "Normal", "Priority" },
            SelectedItem = _viewModel.UpcomingFilter,
            FontSize = 11,
            WidthRequest = 115,
            HeightRequest = 38,
            TextColor = Slate700,
            BackgroundColor = Slate50
        };
        picker.SelectedIndexChanged += (_, _) =>
        {
            if (picker.SelectedItem is string filter)
            {
                _viewModel.SetUpcomingFilter(filter);
                RenderPage();
            }
        };
        return picker;
    }

    private View BuildTokensPlaceholder()
    {
        var stack = new VerticalStackLayout { Spacing = 14 };
        stack.Add(BuildMetrics(
        [
            ("Total tokens", _viewModel.TotalTokenCount.ToString(), "Active pool", "All services combined", Cobalt),
            ("Waiting", _viewModel.WaitingTokenCount.ToString(), "In queue", "Estimated from local queue", Amber),
            ("Serving now", _viewModel.ServingTokenCount.ToString(), "Engaged now", $"Across {_viewModel.Counters.Count} counters", Teal),
            ("Skipped / on hold", _viewModel.SkippedTokenCount.ToString(), "Action required", "Recallable on request", Rose)
        ]));

        var directory = new Grid
        {
            ColumnDefinitions = Width > 1050
                ? new ColumnDefinitionCollection { new(260), new(GridLength.Star) }
                : new ColumnDefinitionCollection { new(GridLength.Star) },
            ColumnSpacing = 16,
            RowSpacing = 16
        };
        directory.Add(BuildTokenFilters(), 0, 0);

        var list = new VerticalStackLayout { Spacing = 10 };
        var title = new VerticalStackLayout { Spacing = 3 };
        title.Add(TextLabel("Token directory", 18, Slate900, FontAttributes.Bold));
        title.Add(TextLabel("Review, update, and manage queue tokens", 11, Slate500));
        _tokenCountLabel = TextLabel(string.Empty, 10, Slate500);
        list.Add(title);
        var countRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            Children = { _tokenCountLabel }
        };
        list.Add(countRow);
        _tokenList = new VerticalStackLayout { Spacing = 8 };
        list.Add(_tokenList);
        directory.Add(Card(list, 16), Width > 1050 ? 1 : 0, 0);
        stack.Add(directory);
        RefreshTokenList();
        return stack;
    }

    private View BuildTokenFilters()
    {
        var filter = new VerticalStackLayout { Spacing = 10 };
        filter.Add(TextLabel("Filter tokens", 14, Slate900, FontAttributes.Bold));
        filter.Add(TextLabel($"{_viewModel.VisibleTokens.Count} of {_viewModel.TotalTokenCount} showing", 10, Slate500));
        var search = new Entry
        {
            Placeholder = "Search token number...",
            Text = _viewModel.SearchText,
            FontSize = 12,
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
            BackgroundColor = Slate50,
            TextColor = Slate700,
            PlaceholderColor = Slate400,
            HeightRequest = 40
        };
        search.TextChanged += (_, args) =>
        {
            _viewModel.SearchText = args.NewTextValue ?? string.Empty;
            if (_tokenCountLabel is not null)
            {
                _tokenCountLabel.Text = $"{_viewModel.VisibleTokens.Count} of {_viewModel.TotalTokenCount} showing";
            }
            RefreshTokenList();
        };
        filter.Add(search);
        filter.Add(TextLabel("QUEUE STATUS", 9, Slate400, FontAttributes.Bold));
        foreach (var status in new[] { "All Tokens", "Waiting", "Serving", "Skipped", "Completed" })
        {
            var count = status == "All Tokens"
                ? _viewModel.TotalTokenCount
                : _viewModel.Tokens.Count(token => token.Status.ToString() == status);
            var selected = _viewModel.StatusFilter == status;
            var button = TextButton($"{status}      {count}", () =>
            {
                _viewModel.SetStatusFilter(status);
                RenderPage();
            }, selected);
            button.HorizontalOptions = LayoutOptions.Fill;
            filter.Add(button);
        }
        var waitingCount = Math.Max(1, _viewModel.WaitingTokenCount);
        var priorityRate = (int)Math.Round(_viewModel.PriorityWaitingCount * 100d / waitingCount);
        filter.Add(Card(new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                TextLabel("Priority routing rate", 10, Slate500),
                TextLabel($"{priorityRate}% Priority", 13, Cobalt, FontAttributes.Bold),
                TextLabel("Based on waiting tokens", 9, Slate400)
            }
        }, 12));
        return Card(filter, 14);
    }

    private void RefreshTokenList()
    {
        if (_tokenList is null)
        {
            return;
        }

        _tokenList.Children.Clear();
        if (_viewModel.VisibleTokens.Count == 0)
        {
            _tokenList.Add(EmptyState("No tokens found.", "Try changing the status filter or search term."));
            return;
        }

        foreach (var token in _viewModel.VisibleTokens)
        {
            _tokenList.Add(TokenSummaryRow(token, () => ShowTokenDetails(token)));
        }
    }

    private void ShowTokenDetails(QueueToken token)
    {
        var content = new VerticalStackLayout { Spacing = 14 };
        var heading = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(24), new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto)
            },
            ColumnSpacing = 8
        };
        heading.Add(new Label
        {
            Text = "confirmation_number",
            FontFamily = "MaterialIcons",
            FontSize = 17,
            TextColor = Cobalt,
            VerticalTextAlignment = TextAlignment.Center
        }, 0, 0);
        heading.Add(TextLabel("Token Details", 16, Slate900, FontAttributes.Bold), 1, 0);
        heading.Add(DialogIconButton("edit", () => ShowTokenForm(token), Slate500), 2, 0);
        heading.Add(DialogIconButton("delete", () => DeleteToken(token), Rose), 3, 0);
        heading.Add(DialogIconButton("close", HideModal, Slate500), 4, 0);
        content.Add(heading);

        var statusColor = token.Status switch
        {
            TokenStatus.Serving => Teal,
            TokenStatus.Skipped => Rose,
            TokenStatus.Completed => Slate500,
            _ => Cobalt
        };
        var statusBackground = token.Status switch
        {
            TokenStatus.Serving => Color.FromArgb("#E9FBF4"),
            TokenStatus.Skipped => Color.FromArgb("#FFF1F2"),
            TokenStatus.Completed => Slate100,
            _ => Color.FromArgb("#EEF4FF")
        };
        var hero = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            ColumnSpacing = 12
        };
        var serviceLine = new HorizontalStackLayout
        {
            Spacing = 5,
            Children =
            {
                new Label
                {
                    Text = "confirmation_number",
                    FontFamily = "MaterialIcons",
                    FontSize = 11,
                    TextColor = Slate500
                },
                TextLabel(token.Service, 10, Slate500)
            }
        };
        var tokenDetails = new VerticalStackLayout
        {
            Spacing = 5,
            Children =
            {
                TextLabel("ACTIVE TOKEN", 9, Slate500, FontAttributes.Bold),
                TextLabel(token.Id, 29, Cobalt, FontAttributes.Bold),
                serviceLine
            }
        };
        hero.Add(tokenDetails, 0, 0);
        hero.Add(new VerticalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End,
            Children =
            {
                StatusPill(token.Status.ToString(), statusBackground, statusColor),
                StatusPill(token.Priority.ToString(), Color.FromArgb("#FFF6E8"), Amber)
            }
        }, 1, 0);
        content.Add(new Border
        {
            Padding = 16,
            BackgroundColor = Color.FromArgb("#F1F5FC"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Content = hero
        });

        var detailCards = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) },
            ColumnSpacing = 10
        };
        detailCards.Add(DetailInfoCard("schedule", "WAIT TIME", token.Wait, "Current queue estimate", Cobalt), 0, 0);
        detailCards.Add(DetailInfoCard("desktop_windows", "ASSIGNED DESK", token.CounterName, token.Service, Cobalt), 1, 0);
        content.Add(detailCards);

        var actions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) },
            ColumnSpacing = 10
        };
        if (token.Status == TokenStatus.Skipped)
        {
            actions.Add(DialogButton("Recall Token", () => RunQueueAction(() => _viewModel.RecallToken(token.Id)), Color.FromArgb("#EEF4FF"), Cobalt, Colors.Transparent), 0, 0);
        }
        else if (token.Status == TokenStatus.Serving)
        {
            actions.Add(DialogButton("Transfer Token", () => ShowTransferForm(token), Color.FromArgb("#EEF4FF"), Cobalt, Colors.Transparent), 0, 0);
        }
        else
        {
            actions.Add(DialogButton("Edit Token", () => ShowTokenForm(token), Color.FromArgb("#EEF4FF"), Cobalt, Colors.Transparent), 0, 0);
        }

        actions.Add(DialogButton("Close Details", HideModal, Cobalt, Colors.White, Colors.Transparent), 1, 0);
        content.Add(actions);
        ShowModal(content);
    }

    private void ShowTokenForm(QueueToken token)
    {
        var servicePicker = BuildPicker(QueueServices.All, token.Service);
        var priorityPicker = BuildPicker(Enum.GetNames<TokenPriority>(), token.Priority.ToString());
        var onlineCounters = _viewModel.Counters.Where(counter => counter.IsOnline).Select(counter => counter.Name).ToArray();
        var counterPicker = BuildPicker(onlineCounters, token.CounterName);
        var form = BuildForm($"Edit {token.Id}");
        AddFormField(form, "Service", servicePicker);
        AddFormField(form, "Priority", priorityPicker);
        AddFormField(form, "Counter", counterPicker);
        form.Add(FormButtons(
            "Cancel",
            "Save token",
            HideModal,
            async () =>
            {
                if (servicePicker.SelectedItem is not string service
                    || priorityPicker.SelectedItem is not string priorityText
                    || counterPicker.SelectedItem is not string counterName)
                {
                    await DisplayAlertAsync("Incomplete token", "Choose a service, priority, and online counter.", "OK");
                    return;
                }

                if (!Enum.TryParse<TokenPriority>(priorityText, out var priority))
                {
                    await DisplayAlertAsync("Invalid priority", "Choose Normal or Priority.", "OK");
                    return;
                }

                try
                {
                    var updated = _viewModel.UpdateToken(token.Id, service, priority, counterName);
                    HideModal();
                    RenderPage();
                    ShowTokenDetails(updated);
                }
                catch (ArgumentException exception)
                {
                    await DisplayAlertAsync("Unable to save token", exception.Message, "OK");
                }
                catch (InvalidOperationException exception)
                {
                    await DisplayAlertAsync("Unable to save token", exception.Message, "OK");
                }
            }));
        ShowModal(form);
    }

    private async void DeleteToken(QueueToken token)
    {
        if (!await DisplayAlertAsync($"Delete {token.Id}?", "This removes the token from the local queue.", "Delete", "Cancel"))
        {
            return;
        }

        if (!_viewModel.DeleteToken(token.Id))
        {
            await DisplayAlertAsync("Token not found", "The token has already been removed.", "OK");
        }

        HideModal();
        RenderPage();
    }

    private void ShowTransferForm(QueueToken token)
    {
        var destinations = _viewModel.Counters
            .Where(counter => counter.IsOnline && counter.Name != token.CounterName)
            .ToArray();
        if (destinations.Length == 0)
        {
            ShowModal(new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    TextLabel("No transfer destination", 16, Slate900, FontAttributes.Bold),
                    TextLabel("Add or enable another counter before transferring this token.", 11, Slate500),
                    DialogButton("Close", HideModal, Cobalt, Colors.White, Colors.Transparent)
                }
            });
            return;
        }

        var selectedDestination = destinations[0].Name;
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(24), new(GridLength.Star), new(30) },
            ColumnSpacing = 8
        };
        header.Add(new Label
        {
            Text = "north_east",
            FontFamily = "MaterialIcons",
            FontSize = 17,
            TextColor = Cobalt,
            VerticalTextAlignment = TextAlignment.Center
        }, 0, 0);
        header.Add(TextLabel("Transfer Token", 16, Slate900, FontAttributes.Bold), 1, 0);
        header.Add(DialogIconButton("close", HideModal, Slate500), 2, 0);

        var tokenSummary = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) },
            Padding = new Thickness(14, 11),
            ColumnSpacing = 10,
            RowSpacing = 4
        };
        tokenSummary.Add(TextLabel($"Current Counter · {token.CounterName}", 10, Slate700, FontAttributes.Bold), 0, 0);
        tokenSummary.Add(TextLabel(token.Id, 10, Cobalt, FontAttributes.Bold), 1, 0);
        tokenSummary.Add(TextLabel($"{token.Service} · {token.Priority} priority", 9, Slate500), 0, 1);
        tokenSummary.SetColumnSpan(tokenSummary.Children.Last(), 2);

        var destinationHeading = TextLabel("SELECT DESTINATION COUNTER", 8, Slate500, FontAttributes.Bold);
        var destinationList = new VerticalStackLayout { Spacing = 7 };
        var destinationCards = new List<(QueueCounter Counter, Border Surface, Label Selector)>();
        foreach (var counter in _viewModel.Counters)
        {
            var isAvailable = counter.IsOnline && counter.Name != token.CounterName;
            var isSelected = isAvailable && counter.Name == selectedDestination;
            var selector = new Label
            {
                Text = isSelected ? "radio_button_checked" : "radio_button_unchecked",
                FontFamily = "MaterialIcons",
                FontSize = 18,
                TextColor = isSelected ? Cobalt : Color.FromArgb("#CBD5E1"),
                VerticalTextAlignment = TextAlignment.Center
            };
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new(22), new(GridLength.Star), new(GridLength.Auto) },
                ColumnSpacing = 8
            };
            row.Add(selector, 0, 0);
            row.Add(new HorizontalStackLayout
            {
                Spacing = 5,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    TextLabel(counter.Name, 10, isAvailable ? Slate900 : Slate500, FontAttributes.Bold),
                    TextLabel(counter.Service, 8, Slate500)
                }
            }, 1, 0);
            row.Add(StatusPill(
                counter.Name == token.CounterName ? "Current" : counter.IsOnline ? "Available" : "Offline",
                Color.FromArgb("#E9FBF4"),
                counter.IsOnline ? Teal : Slate500), 2, 0);
            var surface = new Border
            {
                Padding = new Thickness(10, 9),
                BackgroundColor = isSelected ? Color.FromArgb("#F5F9FF") : Colors.White,
                Stroke = isSelected ? Cobalt : Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
                Opacity = isAvailable ? 1 : 0.6,
                Content = row
            };
            if (isAvailable)
            {
                var tap = new TapGestureRecognizer();
                tap.Tapped += (_, _) =>
                {
                    selectedDestination = counter.Name;
                    foreach (var item in destinationCards)
                    {
                        var selected = item.Counter.Name == selectedDestination;
                        item.Surface.Stroke = selected ? Cobalt : Color.FromArgb("#E2E8F0");
                        item.Surface.BackgroundColor = selected ? Color.FromArgb("#F5F9FF") : Colors.White;
                        item.Selector.Text = selected ? "radio_button_checked" : "radio_button_unchecked";
                        item.Selector.TextColor = selected ? Cobalt : Color.FromArgb("#CBD5E1");
                    }
                };
                surface.GestureRecognizers.Add(tap);
            }

            destinationCards.Add((counter, surface, selector));
            destinationList.Add(surface);
        }

        var note = new Border
        {
            Padding = new Thickness(11, 9),
            BackgroundColor = Color.FromArgb("#F1F6FD"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Content = TextLabel("The selected token will continue waiting at the destination counter.", 9, Slate700)
        };
        var confirm = DialogButton("Confirm Transfer", () =>
        {
            var transferred = _viewModel.TransferToken(token.Id, selectedDestination);
            if (transferred is null)
            {
                _ = DisplayAlertAsync("Transfer unavailable", "The token or destination is no longer available.", "OK");
                return;
            }

            HideModal();
            RenderPage();
        }, Cobalt, Colors.White, Colors.Transparent);
        var actions = new VerticalStackLayout { Spacing = 8 };
        confirm.FontSize = Width < 768 ? 14 : confirm.FontSize;
        confirm.MinimumHeightRequest = Width < 768 ? 56 : confirm.MinimumHeightRequest;
        actions.Add(confirm);
        if (Width < 768)
        {
            actions.Add(DialogButton("Cancel", HideModal, Color.FromArgb("#E5EEFF"), Slate900, Colors.Transparent));
        }

        var content = new VerticalStackLayout
        {
            Spacing = 13,
            Children =
            {
                header,
                new Border
                {
                    Padding = 0,
                    BackgroundColor = Color.FromArgb("#F1F5FC"),
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(9) },
                    Content = tokenSummary
                },
                destinationHeading,
                destinationList,
                note,
                actions
            }
        };
        ShowModal(content);
    }

    private void RunQueueAction(Action action)
    {
        try
        {
            action();
            HideModal();
            RenderPage();
        }
        catch (ArgumentException exception)
        {
            _ = DisplayAlertAsync("Queue action unavailable", exception.Message, "OK");
        }
        catch (InvalidOperationException exception)
        {
            _ = DisplayAlertAsync("Queue action unavailable", exception.Message, "OK");
        }
    }

    private VerticalStackLayout BuildForm(string title)
    {
        var heading = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            ColumnSpacing = 10
        };
        heading.Add(TextLabel(title, 18, Slate900, FontAttributes.Bold), 0, 0);
        heading.Add(TextButton("×", HideModal, false), 1, 0);
        var form = new VerticalStackLayout { Spacing = 14 };
        form.Add(heading);
        return form;
    }

    private static void AddFormField(VerticalStackLayout form, string label, View field)
    {
        var stack = new VerticalStackLayout { Spacing = 5 };
        stack.Add(TextLabel(label, 11, Slate700, FontAttributes.Bold));
        stack.Add(field);
        form.Add(stack);
    }

    private static Picker BuildPicker(IEnumerable<string> items, string selected)
    {
        var picker = new Picker
        {
            ItemsSource = items.ToArray(),
            SelectedItem = selected,
            FontSize = 12,
            TextColor = Slate700,
            BackgroundColor = Slate50,
            HeightRequest = 42
        };
        return picker;
    }

    private Grid FormButtons(string cancelLabel, string saveLabel, Action onCancel, Func<Task> onSave)
    {
        var buttons = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) },
            ColumnSpacing = 10,
            Margin = new Thickness(0, 6, 0, 0)
        };
        buttons.Add(TextButton(cancelLabel, onCancel, false), 0, 0);
        var save = TextButton(saveLabel, () => { }, true);
        save.Clicked += async (_, _) => await onSave();
        buttons.Add(save, 1, 0);
        return buttons;
    }

    private void ShowModal(View content)
    {
        var mobile = Width is > 0 and < 768;
        if (mobile)
        {
            MobileBottomSheetHost.Show(content, PageHost, Width, PageHost.Height);
        }
        else
        {
            DesktopPopupHost.Show(content, Width);
        }
    }

    private static Border DetailInfoCard(string icon, string title, string value, string subtitle, Color accent)
    {
        var heading = new HorizontalStackLayout
        {
            Spacing = 6,
            Children =
            {
                new Label
                {
                    Text = icon,
                    FontFamily = "MaterialIcons",
                    FontSize = 12,
                    TextColor = accent
                },
                TextLabel(title, 8, Slate500, FontAttributes.Bold)
            }
        };
        return new Border
        {
            Padding = 13,
            BackgroundColor = Color.FromArgb("#F4F7FD"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(9) },
            Content = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    heading,
                    TextLabel(value, 13, Slate900, FontAttributes.Bold),
                    TextLabel(subtitle, 9, Slate500)
                }
            }
        };
    }

    private static Button DialogButton(
        string text,
        Action action,
        Color background,
        Color foreground,
        Color border)
    {
        var button = new Button
        {
            Text = text,
            FontFamily = "Segoe UI",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = foreground,
            BackgroundColor = background,
            BorderColor = border,
            BorderWidth = 1,
            CornerRadius = 8,
            Padding = new Thickness(10, 8),
            MinimumHeightRequest = 40,
            HorizontalOptions = LayoutOptions.Fill
        };
        button.Clicked += (_, _) => action();
        return button;
    }

    private static Button DialogIconButton(string icon, Action action, Color color)
    {
        var button = new Button
        {
            Text = icon,
            FontFamily = "MaterialIcons",
            FontSize = 16,
            TextColor = color,
            BackgroundColor = Colors.Transparent,
            Padding = 0,
            WidthRequest = 30,
            HeightRequest = 30,
            MinimumHeightRequest = 30,
            MinimumWidthRequest = 30
        };
        SemanticProperties.SetDescription(button, icon switch
        {
            "close" => "Close dialog",
            "delete" => "Delete",
            "edit" => "Edit",
            _ => icon
        });
        button.Clicked += (_, _) => action();
        return button;
    }

    private void HideModal()
    {
        DesktopPopupHost.Hide();
        MobileBottomSheetHost.Hide();
    }

    private View BuildCountersPlaceholder()
    {
        var stack = new VerticalStackLayout { Spacing = 12 };
        stack.Add(BuildMetrics(
        [
            ("Active counter", _viewModel.SelectedCounter?.Name ?? "—", _viewModel.SelectedCounterAvailabilityText, $"{_viewModel.SelectedCounter?.Service ?? "No service"} terminal", Teal),
            ("Waiting", _viewModel.WaitingTokenCount.ToString(), "Tokens", "Across all counters", Cobalt),
            ("Completed", _viewModel.CompletedTokenCount.ToString(), "Tokens", "Today", Teal),
            ("Skipped", _viewModel.SkippedTokenCount.ToString(), "Tokens", "Available for recall", Rose)
        ]));

        var workspace = new Grid
        {
            ColumnDefinitions = Width > 1050
                ? new ColumnDefinitionCollection { new(320), new(GridLength.Star) }
                : new ColumnDefinitionCollection { new(GridLength.Star) },
            RowDefinitions = Width > 1050
                ? new RowDefinitionCollection { new(GridLength.Auto) }
                : new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) },
            ColumnSpacing = 16,
            RowSpacing = 16
        };
        var terminals = new VerticalStackLayout { Spacing = 10 };
        var terminalHeader = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) }
        };
        terminalHeader.Add(TextLabel("Terminals & counters", 14, Slate900, FontAttributes.Bold), 0, 0);
        terminalHeader.Add(TextLabel($"{_viewModel.OnlineCounterCount} Active", 10, Slate500), 1, 0);
        terminals.Add(terminalHeader);
        foreach (var counter in _viewModel.Counters)
        {
            var current = _viewModel.CurrentTokenFor(counter.Name);
            var selected = counter.Name == _viewModel.ActiveCounterName;
            var item = new VerticalStackLayout
            {
                Spacing = 7,
                Children =
                {
                    TextLabel($"{counter.Name} · {_viewModel.GetCounterAvailabilityText(counter)}", 12, selected ? Cobalt : Slate900, FontAttributes.Bold),
                    TextLabel(counter.Service, 10, Slate500),
                    TextLabel($"{current?.Status.ToString().ToUpperInvariant() ?? "READY"}   {current?.Id ?? "No token"}", 9, Slate500)
                }
            };
            var card = new Border
            {
                Padding = 13,
                BackgroundColor = selected ? Color.FromArgb("#F1F5FF") : Colors.White,
                Stroke = selected ? Cobalt : Slate200,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
                Content = item
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => ShowCounterOperations(counter);
            card.GestureRecognizers.Add(tap);
            terminals.Add(card);
        }
        terminals.Add(TextButton("Manage terminals", ShowCounterManagement, false));
        workspace.Add(Card(terminals, 16), 0, 0);
        workspace.Add(BuildCounterOperations(), Width > 1050 ? 1 : 0, Width > 1050 ? 0 : 1);
        stack.Add(workspace);
        return stack;
    }

    private View BuildCounterOperations()
    {
        var counter = _viewModel.SelectedCounter;
        var token = _viewModel.SelectedCounterToken;
        var content = new VerticalStackLayout { Spacing = 14 };
        if (counter is null)
        {
            content.Add(EmptyState("No counters available.", "Create a counter to start serving tokens."));
            return Card(content, 18);
        }

        var heading = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) }
        };
        var counterStatus = _viewModel.GetCounterAvailabilityText(counter);
        var counterStatusColors = CounterStatusColors(counterStatus);
        heading.Add(new HorizontalStackLayout
        {
            Spacing = 8,
            Children =
            {
                StatusPill(counterStatus, counterStatusColors.Background, counterStatusColors.Foreground),
                StatusPill(token?.Priority.ToString() ?? "Normal", Color.FromArgb("#FFF6E8"), Amber)
            }
        }, 0, 0);
        heading.Add(StatusPill(counter.Name, Color.FromArgb("#EEF4FF"), Cobalt), 1, 0);
        content.Add(heading);
        content.Add(new VerticalStackLayout
        {
            Spacing = 6,
            Padding = new Thickness(0, 10),
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                TextLabel(token?.Id ?? "—", 48, Slate900, FontAttributes.Bold),
                TextLabel(token?.Service ?? counter.Service, 12, Cobalt)
            }
        });
        var info = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) },
            Padding = 14,
            BackgroundColor = Color.FromArgb("#F4F7FD")
        };
        info.Add(new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                TextLabel("Wait time", 10, Slate500),
                TextLabel(token?.Wait ?? "—", 14, Slate900, FontAttributes.Bold)
            }
        }, 0, 0);
        info.Add(new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                TextLabel("Current status", 10, Slate500),
                TextLabel(token?.Status.ToString() ?? "Ready", 14, Teal, FontAttributes.Bold)
            }
        }, 1, 0);
        content.Add(new Border
        {
            BackgroundColor = Color.FromArgb("#F4F7FD"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(9) },
            Content = info
        });

        var primaryActions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) },
            ColumnSpacing = 8
        };
        var callNext = TextButton("Call next", () => RunCounterAction(() => _viewModel.CallNext(counter.Name)), true);
        callNext.HorizontalOptions = LayoutOptions.Fill;
        callNext.IsEnabled = _viewModel.CanCallNextFor(counter.Name);
        var complete = TextButton(token?.Status == TokenStatus.Completed ? "Completed" : "Complete service",
            () => RunCounterAction(() => _viewModel.CompleteCurrent(counter.Name)), true);
        complete.BackgroundColor = Teal;
        complete.BorderColor = Teal;
        complete.IsEnabled = token?.Status == TokenStatus.Serving && counter.IsOnline;
        primaryActions.Add(callNext, 0, 0);
        primaryActions.Add(complete, 1, 0);
        content.Add(primaryActions);

        var queueActions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Star), new(GridLength.Star), new(GridLength.Star)
            },
            ColumnSpacing = 8
        };
        var skip = TextButton("Skip", () => RunCounterAction(() => _viewModel.SkipCurrent(counter.Name)), false);
        skip.IsEnabled = token?.Status == TokenStatus.Serving && counter.IsOnline;
        var recall = TextButton("Recall", () => RunCounterAction(() => _viewModel.RecallCurrent(counter.Name)), false);
        recall.IsEnabled = _viewModel.Tokens.Any(item => item.CounterName == counter.Name && item.Status == TokenStatus.Skipped) && counter.IsOnline;
        var transfer = TextButton("Transfer", () =>
        {
            if (token is null)
            {
                ShowTokenPickerForTransfer(counter.Name);
            }
            else
            {
                ShowTransferForm(token);
            }
        }, false);
        transfer.IsEnabled = token?.Status == TokenStatus.Serving && counter.IsOnline;
        queueActions.Add(skip, 0, 0);
        queueActions.Add(recall, 1, 0);
        queueActions.Add(transfer, 2, 0);
        content.Add(queueActions);
        return Card(content, 18);
    }

    private void ShowCounterOperations(QueueCounter counter)
    {
        _viewModel.SelectCounter(counter.Name);
        RenderPage();
        var token = _viewModel.CurrentTokenFor(counter.Name);
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(40), new(GridLength.Star), new(GridLength.Auto), new(30) },
            ColumnSpacing = 8
        };
        header.Add(new Border
        {
            WidthRequest = 36,
            HeightRequest = 36,
            BackgroundColor = Cobalt,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Content = new Label
            {
                Text = "desktop_windows",
                FontFamily = "MaterialIcons",
                FontSize = 17,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        }, 0, 0);
        header.Add(new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                TextLabel("Counter Operations", 15, Slate900, FontAttributes.Bold),
                TextLabel($"{counter.Name} — {counter.Service}", 9, Slate500)
            }
        }, 1, 0);
        var counterStatus = _viewModel.GetCounterAvailabilityText(counter);
        var counterStatusColors = CounterStatusColors(counterStatus);
        header.Add(StatusPill(counterStatus, counterStatusColors.Background, counterStatusColors.Foreground), 2, 0);
        header.Add(DialogIconButton("close", HideModal, Slate500), 3, 0);

        var currentToken = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(4), new(GridLength.Star) }
        };
        currentToken.Add(new BoxView { Color = Cobalt, VerticalOptions = LayoutOptions.Fill }, 0, 0);
        var currentContent = new Grid
        {
            Padding = new Thickness(16, 13),
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto) },
            RowSpacing = 5
        };
        var currentHeading = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) }
        };
        currentHeading.Add(TextLabel(counter.Name.ToUpperInvariant(), 8, Slate500, FontAttributes.Bold), 0, 0);
        currentHeading.Add(TextLabel(token?.Status == TokenStatus.Serving ? "CURRENTLY SERVING" : "NEXT TOKEN", 8, Slate500, FontAttributes.Bold), 1, 0);
        currentContent.Add(currentHeading, 0, 0);
        currentContent.Add(TextLabel(token?.Id ?? "—", 28, Cobalt, FontAttributes.Bold), 0, 1);
        currentContent.Add(new HorizontalStackLayout
        {
            Spacing = 5,
            Children =
            {
                new Label { Text = "confirmation_number", FontFamily = "MaterialIcons", FontSize = 10, TextColor = Slate500 },
                TextLabel(token?.Service ?? counter.Service, 10, Slate500)
            }
        }, 0, 2);
        currentToken.Add(new Border
        {
            Padding = 0,
            BackgroundColor = Color.FromArgb("#F1F5FC"),
            Stroke = Color.FromArgb("#DCE7F5"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Content = currentContent
        }, 1, 0);

        var timing = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) },
            ColumnSpacing = 10
        };
        timing.Add(DetailInfoCard("schedule", "WAIT TIME", token?.Wait ?? "—", "Current queue wait", Cobalt), 0, 0);
        timing.Add(DetailInfoCard("monitor_heart", "SERVING TIME", token?.Status == TokenStatus.Serving ? "04:58" : "—", "Local sample duration", Teal), 1, 0);

        var complete = DialogButton(
            "Complete Service",
            () => RunCounterAction(() => _viewModel.CompleteCurrent(counter.Name)),
            _viewModel.CanCompleteCurrent ? Teal : Color.FromArgb("#94A3B8"),
            Colors.White,
            Colors.Transparent);
        complete.IsEnabled = _viewModel.CanCompleteCurrent;
        var callNext = DialogButton(
            "Call Next",
            () => RunCounterAction(() => _viewModel.CallNext(counter.Name)),
            Cobalt,
            Colors.White,
            Colors.Transparent);
        callNext.IsEnabled = _viewModel.CanCallNextFor(counter.Name);
        var mobile = Width < 768;
        if (mobile)
        {
            callNext.FontSize = 14;
            callNext.MinimumHeightRequest = 60;
            callNext.ImageSource = new FontImageSource
            {
                Glyph = "arrow_forward",
                FontFamily = "MaterialIcons",
                Color = Colors.White,
                Size = 18
            };
            complete.Text = "Complete";
            complete.FontSize = 14;
            complete.MinimumHeightRequest = 60;
            complete.ImageSource = new FontImageSource
            {
                Glyph = "check_circle",
                FontFamily = "MaterialIcons",
                Color = Colors.White,
                Size = 18
            };
        }
        var primaryActions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) },
            ColumnSpacing = 9
        };
        primaryActions.Add(mobile ? callNext : complete, 0, 0);
        primaryActions.Add(mobile ? complete : callNext, 1, 0);

        var queueActions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Star), new(GridLength.Star), new(GridLength.Star)
            },
            ColumnSpacing = 8
        };
        var skip = DialogButton("Skip", () => RunCounterAction(() => _viewModel.SkipCurrent(counter.Name)), Slate50, Slate700, Slate200);
        skip.IsEnabled = _viewModel.CanSkipCurrent;
        var recall = DialogButton("Recall", () => RunCounterAction(() => _viewModel.RecallCurrent(counter.Name)), Slate50, Slate700, Slate200);
        recall.IsEnabled = _viewModel.CanRecallCurrent;
        var transfer = DialogButton("Transfer", () =>
        {
            if (token is not null)
            {
                ShowTransferForm(token);
            }
            else
            {
                _ = DisplayAlertAsync("No serving token", "Call a token before transferring it.", "OK");
            }
        }, Slate50, Cobalt, Slate200);
        transfer.IsEnabled = _viewModel.CanTransferCurrent;
        if (mobile)
        {
            skip.ImageSource = new FontImageSource { Glyph = "fast_forward", FontFamily = "MaterialIcons", Color = Rose, Size = 16 };
            recall.ImageSource = new FontImageSource { Glyph = "campaign", FontFamily = "MaterialIcons", Color = Cobalt, Size = 16 };
            transfer.ImageSource = new FontImageSource { Glyph = "swap_horiz", FontFamily = "MaterialIcons", Color = Amber, Size = 16 };
            foreach (var action in new[] { skip, recall, transfer })
            {
                action.FontSize = 11;
                action.MinimumHeightRequest = 68;
            }
        }
        queueActions.Add(skip, 0, 0);
        queueActions.Add(recall, 1, 0);
        queueActions.Add(transfer, 2, 0);

        var queueLabel = TextLabel(mobile ? "QUEUE CONTROLS" : "QUEUE ACTIONS", mobile ? 10 : 8, Slate500, FontAttributes.Bold);
        var footer = TextLabel("Operator: Desk operator", 9, Slate500);
        var content = new VerticalStackLayout
        {
            Spacing = 13,
            Children = { header, currentToken, timing, primaryActions, queueLabel, queueActions, footer }
        };
        ShowModal(content);
    }

    private void RunCounterAction(Func<QueueToken?> action)
    {
        try
        {
            var result = action();
            RefreshCounterSurface();
            if (result is null)
            {
                _ = DisplayAlertAsync("No token in queue", "There is no waiting token available for this counter.", "OK");
            }
        }
        catch (ArgumentException exception)
        {
            _ = DisplayAlertAsync("Counter action unavailable", exception.Message, "OK");
        }
        catch (InvalidOperationException exception)
        {
            _ = DisplayAlertAsync("Counter action unavailable", exception.Message, "OK");
        }
    }

    private void RunCounterAction(Func<bool> action)
    {
        try
        {
            var changed = action();
            RefreshCounterSurface();
            if (!changed)
            {
                _ = DisplayAlertAsync("No active token", "This action is not available for the selected counter.", "OK");
            }
        }
        catch (ArgumentException exception)
        {
            _ = DisplayAlertAsync("Counter action unavailable", exception.Message, "OK");
        }
        catch (InvalidOperationException exception)
        {
            _ = DisplayAlertAsync("Counter action unavailable", exception.Message, "OK");
        }
    }

    private void RefreshCounterSurface()
    {
        if ((DesktopPopupHost.IsOpen || MobileBottomSheetHost.IsOpen)
            && _viewModel.SelectedCounter is { } counter)
        {
            ShowCounterOperations(counter);
        }
        else
        {
            RenderPage();
        }
    }

    private void ShowTokenPickerForTransfer(string counterName)
    {
        var serving = _viewModel.Tokens.FirstOrDefault(token =>
            token.CounterName == counterName && token.Status == TokenStatus.Serving);
        if (serving is not null)
        {
            ShowTransferForm(serving);
            return;
        }

        _ = DisplayAlertAsync("No serving token", "Call a token before transferring it.", "OK");
    }

    private void ShowCounterManagement()
    {
        var heading = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(24), new(GridLength.Star), new(30) },
            ColumnSpacing = 8
        };
        heading.Add(new Label
        {
            Text = "desktop_windows",
            FontFamily = "MaterialIcons",
            FontSize = 17,
            TextColor = Cobalt,
            VerticalTextAlignment = TextAlignment.Center
        }, 0, 0);
        heading.Add(TextLabel("Manage terminals", 16, Slate900, FontAttributes.Bold), 1, 0);
        heading.Add(DialogIconButton("close", HideModal, Slate500), 2, 0);
        var content = new VerticalStackLayout { Spacing = 12 };
        content.Add(heading);
        foreach (var counter in _viewModel.Counters)
        {
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new(38), new(GridLength.Star), new(34), new(34) },
                ColumnSpacing = 8
            };
            row.Add(new Border
            {
                WidthRequest = 34,
                HeightRequest = 34,
                BackgroundColor = Color.FromArgb("#EEF4FF"),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
                Content = new Label
                {
                    Text = "desktop_windows",
                    FontFamily = "MaterialIcons",
                    FontSize = 15,
                    TextColor = Cobalt,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                }
            }, 0, 0);
            row.Add(new VerticalStackLayout
            {
                Spacing = 3,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    TextLabel(counter.Name, 11, Slate900, FontAttributes.Bold),
                    TextLabel($"{counter.Service} · {_viewModel.Tokens.Count(token => token.CounterName == counter.Name)} tokens", 9, Slate500)
                }
            }, 1, 0);
            var edit = DialogIconButton("edit", () => ShowCounterForm(counter), Cobalt);
            edit.HorizontalOptions = LayoutOptions.Center;
            row.Add(edit, 2, 0);
            var delete = DialogIconButton("delete", () => ShowDeleteCounterForm(counter), Rose);
            delete.IsEnabled = _viewModel.Counters.Count > 1;
            row.Add(delete, 3, 0);
            content.Add(new Border
            {
                Padding = new Thickness(10, 9),
                BackgroundColor = Color.FromArgb("#F7F9FD"),
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(9) },
                Content = row
            });
        }

        var service = BuildPicker(QueueServices.All, QueueServices.All[0]);
        var addHeading = TextLabel("New terminal service", 9, Slate700, FontAttributes.Bold);
        var addRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            ColumnSpacing = 8
        };
        addRow.Add(service, 0, 0);
        addRow.Add(DialogButton("+  Add Counter", () =>
        {
            if (service.SelectedItem is string selectedService)
            {
                var counter = _viewModel.AddCounter(selectedService);
                ShowCounterManagement();
                _managePage?.ScrollToCounter(counter.Name);
            }
        }, Cobalt, Colors.White, Colors.Transparent), 1, 0);
        content.Add(new Border
        {
            Padding = 12,
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#DCE7F5"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(9) },
            Content = new VerticalStackLayout
            {
                Spacing = 7,
                Children = { addHeading, addRow }
            }
        });
        content.Add(DialogButton("Close", HideModal, Colors.White, Slate500, Slate200));
        ShowModal(content);
    }

    private void ShowCounterForm(QueueCounter? counter)
    {
        var service = BuildPicker(QueueServices.All, counter?.Service ?? QueueServices.All[0]);
        var online = new Switch { IsToggled = counter?.IsOnline ?? true, OnColor = Teal };
        var form = BuildForm(counter is null ? "Add counter" : $"Edit {counter.Name}");
        AddFormField(form, "Service", service);
        AddFormField(form, "Enabled", online);
        form.Add(FormButtons("Cancel", "Save counter", ShowCounterManagement, async () =>
        {
            if (counter is null || service.SelectedItem is not string selectedService)
            {
                await DisplayAlertAsync("Choose a service", "Select a service for this counter.", "OK");
                return;
            }

            try
            {
                _viewModel.UpdateCounter(counter.Name, selectedService, online.IsToggled);
                HideModal();
                RenderPage();
            }
            catch (ArgumentException exception)
            {
                await DisplayAlertAsync("Unable to save counter", exception.Message, "OK");
            }
            catch (InvalidOperationException exception)
            {
                await DisplayAlertAsync("Unable to save counter", exception.Message, "OK");
            }
        }));
        ShowModal(form);
    }

    private void ShowDeleteCounterForm(QueueCounter counter)
    {
        if (_viewModel.Counters.Count <= 1)
        {
            _ = DisplayAlertAsync("Counter required", "At least one counter must remain.", "OK");
            return;
        }

        var destinations = _viewModel.Counters
            .Where(item => item.IsOnline && item.Name != counter.Name)
            .Select(item => item.Name)
            .ToArray();
        if (destinations.Length == 0)
        {
            _ = DisplayAlertAsync("No destination available", "Enable another counter before deleting this one.", "OK");
            return;
        }

        var destination = BuildPicker(destinations, destinations[0]);
        var form = BuildForm($"Delete {counter.Name}");
        form.Add(TextLabel("Tokens assigned to this counter will move to the destination.", 11, Slate500));
        AddFormField(form, "Destination counter", destination);
        form.Add(FormButtons("Cancel", "Delete counter", ShowCounterManagement, async () =>
        {
            if (destination.SelectedItem is not string destinationName)
            {
                await DisplayAlertAsync("Choose a destination", "Select a counter to receive assigned tokens.", "OK");
                return;
            }

            try
            {
                _viewModel.DeleteCounter(counter.Name, destinationName);
                HideModal();
                RenderPage();
            }
            catch (ArgumentException exception)
            {
                await DisplayAlertAsync("Unable to delete counter", exception.Message, "OK");
            }
            catch (InvalidOperationException exception)
            {
                await DisplayAlertAsync("Unable to delete counter", exception.Message, "OK");
            }
        }));
        ShowModal(form);
    }

    private View BuildInsightsPlaceholder() =>
        BuildInsights();

    private View BuildInsights()
    {
        var stack = new VerticalStackLayout { Spacing = 14 };
        stack.Add(TextLabel("Illustrative local sample analytics", 10, Slate500));
        stack.Add(BuildMetrics(
        [
            ("In line", "8", "Tokens", "Across all services", Cobalt),
            ("Completed", "42", "Today", "+18.5% vs. avg", Teal),
            ("Counters", "3", "Active", "100% capacity", Cobalt),
            ("Average wait", "6.4m", "Within SLA", "Target under 10m", Amber)
        ]));

        var charts = new Grid
        {
            ColumnDefinitions = Width > 1050
                ? new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) }
                : new ColumnDefinitionCollection { new(GridLength.Star) },
            RowDefinitions = Width > 1050
                ? new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) }
                : new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto) },
            ColumnSpacing = 14,
            RowSpacing = 14
        };
        charts.Add(BuildServiceDistributionCard(), 0, 0);
        charts.Add(BuildAverageWaitCard(), Width > 1050 ? 1 : 0, Width > 1050 ? 0 : 1);
        charts.Add(BuildFulfillmentCard(), 0, Width > 1050 ? 1 : 2);
        charts.Add(BuildCounterPerformanceCard(), Width > 1050 ? 1 : 0, Width > 1050 ? 1 : 3);
        stack.Add(charts);
        return stack;
    }

    private View BuildServiceDistributionCard()
    {
        var chart = new SfCircularChart
        {
            HeightRequest = 190,
            BackgroundColor = Colors.Transparent
        };
        chart.Series.Add(new DoughnutSeries
        {
            ItemsSource = new[]
            {
                new AnalyticsPoint("Enquiry", 22),
                new AnalyticsPoint("Payment", 18),
                new AnalyticsPoint("Support", 10)
            },
            XBindingPath = nameof(AnalyticsPoint.Category),
            YBindingPath = nameof(AnalyticsPoint.Value),
            InnerRadius = 0.72,
            PaletteBrushes = new List<Brush>
            {
                new SolidColorBrush(Cobalt),
                new SolidColorBrush(Teal),
                new SolidColorBrush(Color.FromArgb("#7589D4"))
            }
        });
        var content = new VerticalStackLayout { Spacing = 8 };
        content.Add(ChartTitle("Tokens by service type", "Distribution across customer categories", "50 tokens"));
        content.Add(chart);
        content.Add(AnalyticsLegendRow("Enquiry", 22, "44%", Cobalt));
        content.Add(AnalyticsLegendRow("Payment", 18, "36%", Teal));
        content.Add(AnalyticsLegendRow("Support", 10, "20%", Color.FromArgb("#7589D4")));
        content.Add(TextLabel("Category total                                      100% (50 issued)", 9, Slate500));
        return Card(content, 16);
    }

    private View BuildAverageWaitCard()
    {
        var content = new VerticalStackLayout { Spacing = 15 };
        content.Add(ChartTitle("Average wait time", "Average customer queue duration by service", "Scale: 15m max"));
        foreach (var item in new[]
        {
            ("Enquiry", 7.5, Cobalt),
            ("Payment", 4.2, Teal),
            ("Support", 11d, Color.FromArgb("#EFA867"))
        })
        {
            var row = new VerticalStackLayout { Spacing = 5 };
            var header = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) }
            };
            header.Add(TextLabel(item.Item1, 11, Slate700), 0, 0);
            header.Add(TextLabel($"{item.Item2:0.#}m", 11, Slate900, FontAttributes.Bold), 1, 0);
            row.Add(header);
            var bar = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new(new GridLength(item.Item2, GridUnitType.Star)), new(new GridLength(15 - item.Item2, GridUnitType.Star)) },
                HeightRequest = 9,
                BackgroundColor = Slate100
            };
            bar.Add(new BoxView { Color = item.Item3, CornerRadius = 5 }, 0, 0);
            row.Add(new Border
            {
                Padding = 0,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(5) },
                Content = bar
            });
            content.Add(row);
        }
        content.Add(TextLabel("0m                     5m target                10m SLA                 15m max", 9, Slate400));
        return Card(content, 16);
    }

    private View BuildFulfillmentCard()
    {
        var chart = new SfCircularChart { HeightRequest = 190, BackgroundColor = Colors.Transparent };
        chart.Series.Add(new DoughnutSeries
        {
            ItemsSource = new[]
            {
                new AnalyticsPoint("Completed", 42),
                new AnalyticsPoint("Pending", 8)
            },
            XBindingPath = nameof(AnalyticsPoint.Category),
            YBindingPath = nameof(AnalyticsPoint.Value),
            InnerRadius = 0.72,
            PaletteBrushes = new List<Brush> { new SolidColorBrush(Teal), new SolidColorBrush(Color.FromArgb("#DCE8F5")) }
        });
        var content = new VerticalStackLayout { Spacing = 8 };
        content.Add(ChartTitle("Completed vs pending", "Session fulfillment status", "Healthy"));
        content.Add(chart);
        content.Add(AnalyticsLegendRow("Completed · Processed & cleared", 42, "84%", Teal));
        content.Add(AnalyticsLegendRow("Pending · Waiting dispatch", 8, "16%", Color.FromArgb("#93B4E2")));
        return Card(content, 16);
    }

    private View BuildCounterPerformanceCard()
    {
        var chart = new SfCartesianChart
        {
            HeightRequest = 210,
            BackgroundColor = Colors.Transparent
        };
        chart.XAxes.Add(new CategoryAxis
        {
            LabelStyle = new ChartAxisLabelStyle { TextColor = Slate500, FontSize = 9 }
        });
        chart.YAxes.Add(new NumericalAxis
        {
            LabelStyle = new ChartAxisLabelStyle { TextColor = Slate500, FontSize = 9 }
        });
        chart.Series.Add(new ColumnSeries
        {
            ItemsSource = new[]
            {
                new AnalyticsPoint("Counter 1", 18),
                new AnalyticsPoint("Counter 2", 14),
                new AnalyticsPoint("Counter 3", 10)
            },
            XBindingPath = nameof(AnalyticsPoint.Category),
            YBindingPath = nameof(AnalyticsPoint.Value),
            Fill = Cobalt,
            Width = 0.6,
            Spacing = 0.25,
            ShowDataLabels = true
        });
        var content = new VerticalStackLayout { Spacing = 8 };
        content.Add(ChartTitle("Counter performance", "Completed tokens handled per desk", "100% operational"));
        content.Add(chart);
        content.Add(TextLabel("Total completed: 42 tokens   ·   Desk throughput: ~14 tokens/desk", 10, Slate500));
        return Card(content, 16);
    }

    private static View ChartTitle(string title, string subtitle, string badge)
    {
        var heading = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            ColumnSpacing = 8
        };
        heading.Add(new VerticalStackLayout
        {
            Spacing = 3,
            Children =
            {
                TextLabel(title, 13, Slate900, FontAttributes.Bold),
                TextLabel(subtitle, 10, Slate500)
            }
        }, 0, 0);
        heading.Add(StatusPill(badge, Color.FromArgb("#EEF4FF"), Cobalt), 1, 0);
        return heading;
    }

    private static View AnalyticsLegendRow(string label, int value, string percent, Color color)
    {
        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Star), new(GridLength.Auto), new(44)
            },
            ColumnSpacing = 12
        };
        row.Add(new HorizontalStackLayout
        {
            Spacing = 7,
            Children =
            {
                new BoxView { Color = color, WidthRequest = 8, HeightRequest = 8, CornerRadius = 4, VerticalOptions = LayoutOptions.Center },
                TextLabel(label, 10, Slate700)
            }
        }, 0, 0);
        row.Add(TextLabel($"{value} tokens", 10, Slate500), 1, 0);
        row.Add(TextLabel(percent, 10, Slate900, FontAttributes.Bold), 2, 0);
        return row;
    }

    private View TokenSummaryRow(QueueToken token, Action onSelected)
    {
        var layout = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(90), new(GridLength.Star), new(GridLength.Auto)
            },
            ColumnSpacing = 12,
            Padding = new Thickness(12, 10),
            BackgroundColor = Slate50
        };
        layout.Add(TextLabel(token.Id, 13, Cobalt, FontAttributes.Bold), 0, 0);
        var info = new VerticalStackLayout { Spacing = 2 };
        info.Add(TextLabel(token.Service, 12, Slate900, FontAttributes.Bold));
        info.Add(TextLabel($"{token.CounterName} · {token.Priority}", 10, Slate500));
        layout.Add(info, 1, 0);
        layout.Add(StatusPill(token.Status.ToString(), StatusColor(token.Status).Background, StatusColor(token.Status).Foreground), 2, 0);
        var card = new Border
        {
            Padding = 0,
            BackgroundColor = Colors.White,
            Stroke = Slate200,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(9) },
            Content = layout
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => onSelected();
        card.GestureRecognizers.Add(tap);
        SemanticProperties.SetDescription(card, $"Open token {token.Id}, {token.Status}, {token.Service}");
        return card;
    }

    private static (Color Background, Color Foreground) StatusColor(TokenStatus status) =>
        status switch
        {
            TokenStatus.Waiting => (Color.FromArgb("#EEF4FF"), Cobalt),
            TokenStatus.Serving => (Color.FromArgb("#E9FBF4"), Teal),
            TokenStatus.Skipped => (Color.FromArgb("#FFF1F2"), Rose),
            _ => (Slate100, Slate500)
        };

    private static View CardHeading(string title, string subtitle, string badge, Color badgeColor)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            ColumnSpacing = 10
        };
        var labels = new VerticalStackLayout { Spacing = 3 };
        labels.Add(TextLabel(title, 14, Slate900, FontAttributes.Bold));
        labels.Add(TextLabel(subtitle, 11, Slate500));
        grid.Add(labels, 0, 0);
        grid.Add(StatusPill(badge, badgeColor == Teal ? Color.FromArgb("#E9FBF4") : Color.FromArgb("#EEF4FF"), badgeColor), 1, 0);
        return grid;
    }

    private static View LabelPair(string label, string value)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) }
        };
        grid.Add(TextLabel(label, 11, Slate500), 0, 0);
        grid.Add(TextLabel(value, 11, Slate900, FontAttributes.Bold), 1, 0);
        return grid;
    }

    private static View StatusPill(string text, Color background, Color foreground) =>
        new Border
        {
            Padding = new Thickness(9, 5),
            BackgroundColor = background,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            Content = TextLabel(text, 10, foreground, FontAttributes.Bold),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };

    private static (Color Background, Color Foreground) CounterStatusColors(string status) =>
        status switch
        {
            "Online" => (Color.FromArgb("#E9FBF4"), Teal),
            "Waiting" => (Color.FromArgb("#FFF6E8"), Amber),
            _ => (Slate100, Slate500)
        };

    private static View Card(View content, double padding = 16) =>
        new Border
        {
            Padding = padding,
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E2E8F0"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            Content = content
        };

    private static Label TextLabel(
        string text,
        double fontSize = 12,
        Color? color = null,
        FontAttributes attributes = FontAttributes.None) =>
        new()
        {
            Text = text,
            FontFamily = "Segoe UI",
            FontSize = fontSize,
            TextColor = color ?? Slate900,
            FontAttributes = attributes,
            VerticalTextAlignment = TextAlignment.Center
        };

    private static Button TextButton(string text, Action action, bool primary)
    {
        var button = new Button
        {
            Text = text,
            FontFamily = "Segoe UI",
            FontSize = 12,
            FontAttributes = primary ? FontAttributes.Bold : FontAttributes.None,
            TextColor = primary ? Colors.White : Slate500,
            BackgroundColor = primary ? Cobalt : Colors.Transparent,
            BorderColor = primary ? Cobalt : Slate200,
            BorderWidth = primary ? 0 : 1,
            CornerRadius = 9,
            Padding = new Thickness(14, 8),
            MinimumHeightRequest = 34,
            MinimumWidthRequest = 44,
            HorizontalOptions = LayoutOptions.Start
        };
        button.Clicked += (_, _) => action();
        return button;
    }

    private static View EmptyState(string title, string description) =>
        new VerticalStackLayout
        {
            Padding = new Thickness(18),
            Spacing = 5,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                TextLabel(title, 13, Slate700, FontAttributes.Bold),
                TextLabel(description, 11, Slate500)
            }
        };

    private void ShowResetConfirmation()
    {
        var mobile = Width < 768;
        var title = TextLabel("Reset Queue", 18, Slate900, FontAttributes.Bold);
        title.HorizontalTextAlignment = TextAlignment.Center;
        title.HorizontalOptions = LayoutOptions.Fill;
        var description = TextLabel(
            mobile
                ? "This will restore the original sample queue and remove all current changes."
                : "This restores the original queue and clears temporary filters and counter selection.",
            12,
            Slate500);
        description.HorizontalTextAlignment = TextAlignment.Center;
        description.HorizontalOptions = LayoutOptions.Fill;

        var resetAction = DialogButton("Reset Queue", () =>
        {
            _viewModel.ResetQueue();
            HideModal();
            RenderPage();
        }, mobile ? Color.FromArgb("#B91C1C") : Color.FromArgb("#C9003D"), Colors.White, Colors.Transparent);
        var cancelAction = DialogButton("Cancel", HideModal, Slate100, Slate900, Slate200);
        View actions;
        if (mobile)
        {
            resetAction.FontSize = 14;
            resetAction.MinimumHeightRequest = 56;
            cancelAction.FontSize = 14;
            cancelAction.MinimumHeightRequest = 56;
            cancelAction.BackgroundColor = Color.FromArgb("#E5EEFF");
            cancelAction.BorderColor = Colors.Transparent;
            actions = new VerticalStackLayout
            {
                Spacing = 10,
                Children = { resetAction, cancelAction }
            };
        }
        else
        {
            var desktopActions = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) },
                ColumnSpacing = 10
            };
            desktopActions.Add(cancelAction, 0, 0);
            desktopActions.Add(resetAction, 1, 0);
            actions = desktopActions;
        }

        var content = new VerticalStackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Fill
        };
        if (mobile)
        {
            var header = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(30) } };
            header.Add(new BoxView(), 0, 0);
            header.Add(DialogIconButton("close", HideModal, Slate500), 1, 0);
            content.Add(header);
        }

        content.Add(new Border
        {
            WidthRequest = 50,
            HeightRequest = 50,
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Color.FromArgb("#FFF1F2"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Content = new Label
            {
                Text = "warning",
                FontFamily = "MaterialIcons",
                FontSize = 23,
                TextColor = Color.FromArgb("#E11D1D"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        });
        content.Add(title);
        content.Add(description);
        content.Add(actions);
        ShowModal(content);
    }

    private void OpenTokenDetails(QueueToken token) => ShowTokenDetails(token);

    private sealed class KeyGlyphDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2;
            canvas.DrawCircle(7, 8, 4);
            canvas.DrawLine(10, 11, 17, 18);
            canvas.DrawLine(14, 15, 17, 12);
            canvas.DrawLine(16, 17, 19, 14);
        }
    }

    private sealed record AnalyticsPoint(string Category, double Value);

}
