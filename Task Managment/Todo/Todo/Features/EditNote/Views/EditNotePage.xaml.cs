using System.ComponentModel;
using Todo.Features.EditNote.ViewModels;
using Todo.Features.Notes.Views;

namespace Todo.Features.EditNote.Views;

public partial class EditNotePage : ContentPage
{
    private readonly EditNoteViewModel _viewModel;
    private readonly bool _isPhone;

    public EditNotePage(EditNoteViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _isPhone = DeviceInfo.Current.Idiom == DeviceIdiom.Phone;

        if (_isPhone)
        {
            _viewModel.IsDueDatePickerVisible = false;
            _viewModel.IsReminderPickerVisible = false;
        }

        BindingContext = viewModel;
        if (_isPhone)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateSchedulingEditors();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_isPhone)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EditNoteViewModel.IsDueDatePickerVisible)
            or nameof(EditNoteViewModel.IsReminderPickerVisible))
            UpdateSchedulingEditors();
    }

    private void UpdateSchedulingEditors()
    {
        DueDateInlineEditor.IsVisible = !_isPhone && _viewModel.IsDueDatePickerVisible;
        ReminderInlineEditor.IsVisible = !_isPhone && _viewModel.IsReminderPickerVisible;
        DueDatePopup.IsVisible = _isPhone && _viewModel.IsDueDatePickerVisible;
        ReminderPopup.IsVisible = _isPhone && _viewModel.IsReminderPickerVisible;
    }

    private static void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Border border)
            PointerHoverSurface.SetHovered(border);
    }

    private static void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Border border)
            PointerHoverSurface.Reset(border);
    }
}
