using Syncfusion.Maui.Picker;
using Todo.Features.EditNote.ViewModels;
using Todo.Features.Notes.Views;

namespace Todo.Features.EditNote.Views;

public partial class EditNotePage : ContentPage
{
    private readonly EditNoteViewModel _viewModel;
    private readonly bool _usesSchedulingPopups;

    public EditNotePage(EditNoteViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
#if WINDOWS
        _usesSchedulingPopups = true;
#else
        _usesSchedulingPopups = DeviceInfo.Current.Idiom == DeviceIdiom.Phone;
#endif

        if (_usesSchedulingPopups)
        {
            _viewModel.IsDueDatePickerVisible = false;
            _viewModel.IsReminderPickerVisible = false;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateSchedulingEditors();
        }

        BindingContext = viewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_usesSchedulingPopups)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EditNoteViewModel.IsDueDatePickerVisible)
            or nameof(EditNoteViewModel.IsReminderPickerVisible))
            UpdateSchedulingEditors();
    }

    private void UpdateSchedulingEditors()
    {
        DueDateInlineEditor.IsVisible = !_usesSchedulingPopups && _viewModel.IsDueDatePickerVisible;
        ReminderInlineEditor.IsVisible = !_usesSchedulingPopups && _viewModel.IsReminderPickerVisible;
        DueDatePopup.IsVisible = _usesSchedulingPopups && _viewModel.IsDueDatePickerVisible;
        ReminderPopup.IsVisible = _usesSchedulingPopups && _viewModel.IsReminderPickerVisible;
    }

    private void OnReminderPickerSelectionChanged(object? sender, EventArgs e)
    {
        if (sender is not SfDateTimePicker { SelectedDate: DateTime selectedDate })
            return;

        _viewModel.DraftReminderDateSelection = selectedDate;
        _viewModel.DraftReminderTimeSelection = selectedDate.TimeOfDay;
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
