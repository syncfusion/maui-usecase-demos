using DiagramBuilder.ViewModels;
using Syncfusion.Maui.Buttons;

namespace DiagramBuilder.Views;

public partial class CreateDiagramPage : ContentPage
{
    private readonly CreateDiagramViewModel _viewModel;
    private bool? _isCompactLayout;

    public CreateDiagramPage(CreateDiagramViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Appearing += async (_, _) => await viewModel.InitializeAsync();
        SizeChanged += (_, _) => ApplyResponsiveLayout();
        
        // Build category buttons manually
        BuildCategoryRail();
    }

    private void ApplyResponsiveLayout()
    {
        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        var compact = Width < 800;
        CreateDialog.WidthRequest = Math.Max(0, Math.Min(980, Width - 24));
        CreateDialog.HeightRequest = Math.Max(0, Math.Min(640, Height - 24));

        if (_isCompactLayout == compact)
        {
            return;
        }

        _isCompactLayout = compact;
        CreateBody.RowDefinitions.Clear();
        CreateBody.ColumnDefinitions.Clear();

        if (compact)
        {
            CreateBody.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            CreateBody.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            CreateBody.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            MobileCategoryPicker.IsVisible = true;
            Grid.SetRow(MobileCategoryPicker, 0);
            Grid.SetColumn(MobileCategoryPicker, 0);
            Grid.SetColumnSpan(MobileCategoryPicker, 1);

            CategoryRailContainer.IsVisible = false;
            Grid.SetRow(CategoryRailContainer, 1);
            Grid.SetColumn(CategoryRailContainer, 0);

            Grid.SetRow(TemplateScrollView, 1);
            Grid.SetColumn(TemplateScrollView, 0);
            Grid.SetColumnSpan(TemplateScrollView, 1);

            CreateHeader.Padding = new Thickness(16, 12, 12, 8);
            TemplateContent.Padding = new Thickness(12, 12);
            TemplateContent.Spacing = 12;
        }
        else
        {
            CreateBody.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            CreateBody.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(240) });
            CreateBody.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            MobileCategoryPicker.IsVisible = false;
            Grid.SetRow(MobileCategoryPicker, 0);
            Grid.SetColumn(MobileCategoryPicker, 0);
            Grid.SetColumnSpan(MobileCategoryPicker, 2);

            CategoryRailContainer.IsVisible = true;
            Grid.SetRow(CategoryRailContainer, 0);
            Grid.SetColumn(CategoryRailContainer, 0);

            Grid.SetRow(TemplateScrollView, 0);
            Grid.SetColumn(TemplateScrollView, 1);
            Grid.SetColumnSpan(TemplateScrollView, 1);

            CreateHeader.Padding = new Thickness(24, 18, 20, 12);
            TemplateContent.Padding = new Thickness(40, 20, 40, 20);
            TemplateContent.Spacing = 20;
        }
    }
    
    private void BuildCategoryRail()
    {
        if (this.FindByName("CategoryRail") is not VerticalStackLayout categoryRail)
        {
            return;
        }
        
        categoryRail.Children.Clear();
        
        foreach (var category in _viewModel.Categories)
        {
            var btn = new SfButton
            {
                Text = category,
                Margin = new Thickness(0),
                Padding = new Thickness(18, 0),
                CornerRadius = 2,
                HeightRequest = 44,
                HorizontalOptions = LayoutOptions.Fill,
                HorizontalTextAlignment = TextAlignment.Start,
                FontSize = 15,
                Background = Colors.Transparent,
                StrokeThickness = 0,
            };
            
            // Add tap handler to select category
            btn.Clicked += (_, _) =>
            {
                _viewModel.SelectedCategoryLabel = category;
                UpdateCategoryButtons();
            };
            
            categoryRail.Children.Add(btn);
        }
        
        UpdateCategoryButtons();
    }
    
    private void UpdateCategoryButtons()
    {
        if (this.FindByName("CategoryRail") is not VerticalStackLayout categoryRail)
        {
            return;
        }
        
        int index = 0;
        foreach (var child in categoryRail.Children)
        {
            if (child is SfButton btn && index < _viewModel.Categories.Count)
            {
                var category = _viewModel.Categories[index];
                bool isSelected = category == _viewModel.SelectedCategoryLabel;
                
                if (isSelected)
                {
                    btn.Background = Microsoft.Maui.Graphics.Color.Parse("#E3F2FD");
                    btn.TextColor = Application.Current?.Resources["PrimaryColor"] as Color ?? Colors.Blue;
                    btn.FontAttributes = FontAttributes.Bold;
                }
                else
                {
                    btn.Background = Colors.Transparent;
                    btn.TextColor = Application.Current?.Resources["TextPrimary"] as Color ?? Colors.Black;
                    btn.FontAttributes = FontAttributes.None;
                }
                
                index++;
            }
        }
    }

    /// <summary>VM-raised CloseRequested → pop modal (no business logic in code-behind).</summary>
    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CreateDiagramViewModel.CloseRequested) && _viewModel.CloseRequested)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
        else if (e.PropertyName == nameof(CreateDiagramViewModel.SelectedCategoryLabel))
        {
            UpdateCategoryButtons();
        }
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
