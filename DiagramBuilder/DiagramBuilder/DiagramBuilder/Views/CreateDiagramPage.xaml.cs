using DiagramBuilder.ViewModels;
using Syncfusion.Maui.Buttons;

namespace DiagramBuilder.Views;

public partial class CreateDiagramPage : ContentPage
{
    private readonly CreateDiagramViewModel _viewModel;

    public CreateDiagramPage(CreateDiagramViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Appearing += async (_, _) => await viewModel.InitializeAsync();
        
        // Build category buttons manually
        BuildCategoryRail();
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
