using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.Calendar;
using Microsoft.Maui.ApplicationModel;

namespace SmartExpense;

/// <summary>
/// LoginPage is a ContentPage that serves as the user interface for logging into the SmartExpense application. It binds to a LoginViewModel, which handles the logic for user authentication and navigation. The page listens for events from the ViewModel to navigate to the main page upon successful login or to the registration page when the user opts to create a new account.
/// </summary>
public partial class SetupProfilePage : ContentPage
{
    public SetupProfilePage(SetupProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        // Provide the page's navigation instance to the view model (avoid injecting INavigation via DI)

        // Navigate to the main page when setup completes. Constructing a full MainPage can be
        // heavy (many views/controls). We assign Application.Current.MainPage on the UI thread
        // and rely on lazy-loading of heavy tab content to keep the transition smooth.
        viewModel.LoginSucceeded += () =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var mainPage = Handler?.MauiContext?.Services.GetRequiredService<MainPage>();
                if (mainPage is not null)
                {
                    // Wrap the MainPage in a NavigationPage so PushAsync navigation works
                    Application.Current!.MainPage = new NavigationPage(mainPage);
                }
            });
        };

        SizeChanged += OnSizeChanged;
    }

    private void Calendar_SelectionChanged(object sender,
     CalendarSelectionChangedEventArgs e)
    {
    }

    private void Calendar_ActionButtonClicked(object? sender, EventArgs e)
    {
        if (BindingContext is SetupProfileViewModel vm)
        {
            vm.DateSelectedCommand.Execute(calendar.SelectedDate);
        }

        calendar.IsOpen = false;
    }

    private void Calendar_ActionButtonCanceled(object? sender, EventArgs e)
    {
        calendar.IsOpen = false;
    }

    private void Calendar_SelectionChanged1(object sender,
    CalendarSelectionChangedEventArgs e)
    {
    }

    private void Calendar1_ActionButtonClicked(object? sender, EventArgs e)
    {
        if (BindingContext is SetupProfileViewModel vm)
        {
            vm.DateSelectedCommand.Execute(calendar1.SelectedDate);
        }

        calendar1.IsOpen = false;
    }

    private void Calendar1_ActionButtonCanceled(object? sender, EventArgs e)
    {
        calendar1.IsOpen = false;
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        bool showImage = Width >= 900;

        CoverImage.IsVisible = showImage;
        ImageColumn.Width = showImage ? GridLength.Star : new GridLength(0);

        NameGrid.ColumnDefinitions.Clear();

        if (!showImage)
        {
            // STACK VERTICALLY
            NameGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            Grid.SetColumn(FirstNameLayout, 0);
            Grid.SetRow(FirstNameLayout, 0);

            Grid.SetColumn(LastNameLayout, 0);
            Grid.SetRow(LastNameLayout, 1);

            NameGrid.RowDefinitions.Clear();
            NameGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            NameGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }
        else
        {
            // SIDE BY SIDE
            NameGrid.RowDefinitions.Clear();
            NameGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            NameGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            Grid.SetRow(FirstNameLayout, 0);
            Grid.SetColumn(FirstNameLayout, 0);

            Grid.SetRow(LastNameLayout, 0);
            Grid.SetColumn(LastNameLayout, 1);
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        calendar.IsOpen = true;
    }

    private void TapGestureRecognizer_Tapped1(object sender, TappedEventArgs e)
    {
        calendar1.IsOpen = true;
    }
}
