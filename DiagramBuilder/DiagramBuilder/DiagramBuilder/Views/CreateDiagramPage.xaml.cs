using DiagramBuilder.ViewModels;

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
        
        // Wire the category list selection change to update the ViewModel
        if (this.FindByName("CategoryList") is Syncfusion.Maui.ListView.SfListView categoryList)
        {
            categoryList.SelectionChanging += OnCategorySelectionChanging;
        }
    }
    
    private void OnCategorySelectionChanging(object? sender, Syncfusion.Maui.ListView.SelectionChangingEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is string selectedCategory)
        {
            _viewModel.SelectedCategoryLabel = selectedCategory;
        }
    }

    /// <summary>VM-raised CloseRequested → pop modal (no business logic in code-behind).</summary>
    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CreateDiagramViewModel.CloseRequested) && _viewModel.CloseRequested)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
