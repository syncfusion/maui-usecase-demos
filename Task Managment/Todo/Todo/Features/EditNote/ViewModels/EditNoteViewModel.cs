using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Todo.Common;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.Services;

namespace Todo.Features.EditNote.ViewModels;

/// <summary>
/// ViewModel for the small centered dialog used to create or edit a
/// note. Matches the dialog chrome shown in
/// <c>createnewlist.png</c> / <c>renamelist.png</c>: title, body text
/// input, <c>Cancel</c> / <c>Save</c> buttons.
///
/// <para>
/// <see cref="DueDateSelection"/> and <see cref="ReminderDateSelection"/>
/// are bound to Syncfusion date pickers on the edit page.
/// </para>
/// </summary>
public partial class EditNoteViewModel : ViewModelBase
{
    private readonly INoteService _noteService;
    private readonly INavigationService _navigation;
    private readonly Note? _existing;
    private readonly string? _listId;

    /// <summary>The note currently being edited, or <c>null</c> when
    /// creating a new note.</summary>
    public Note? Target => _existing;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private bool hasDueDate;

    [ObservableProperty]
    private bool isDueDatePickerVisible;

    [ObservableProperty]
    private DateTime dueDateSelection = DateTime.Today;

    [ObservableProperty]
    private NoteRepeatFrequency repeatFrequency;

    [ObservableProperty]
    private bool isRepeatPickerVisible;

    [ObservableProperty]
    private bool hasReminder;

    [ObservableProperty]
    private bool isReminderPickerVisible;

    [ObservableProperty]
    private DateTime reminderDateSelection = DateTime.Today;

    [ObservableProperty]
    private TimeSpan reminderTimeSelection = new(9, 0, 0);

    public IReadOnlyList<NoteRepeatFrequency> RepeatOptions { get; } =
        Enum.GetValues<NoteRepeatFrequency>();

    [ObservableProperty]
    private bool isExisting;

    public string DialogTitle => IsExisting ? "Edit note" : "Create note";
    public string PrimaryButtonText => IsExisting ? "Save" : "Create";
    public string CreatedAgeText { get; private set; } = "Created just now";
    public bool HasCreatedInfo => _existing is not null;

    public EditNoteViewModel(INoteService noteService, INavigationService navigation)
        : this(noteService, navigation, null, null)
    {
    }

    public EditNoteViewModel(INoteService noteService, INavigationService navigation, Note? existing, string? listId = null)
    {
        _noteService = noteService;
        _navigation = navigation;
        _existing = existing;
        _listId = listId;

        if (existing is not null)
        {
            IsExisting = true;
            Title = existing.Title ?? string.Empty;
            HasDueDate = existing.DueDate.HasValue;
            IsDueDatePickerVisible = HasDueDate;
            DueDateSelection = existing.DueDate ?? DateTime.Today;
            RepeatFrequency = existing.RepeatFrequency;
            IsRepeatPickerVisible = RepeatFrequency != NoteRepeatFrequency.Never;
            HasReminder = existing.ReminderDate.HasValue;
            IsReminderPickerVisible = HasReminder;
            ReminderDateSelection = existing.ReminderDate ?? DateTime.Today;
            ReminderTimeSelection = existing.ReminderDate?.TimeOfDay ?? new TimeSpan(9, 0, 0);
            var age = DateTime.Now - existing.CreatedAt;
            CreatedAgeText = age.TotalMinutes < 1 ? "Created just now" :
                age.TotalHours < 1 ? $"Created {(int)age.TotalMinutes} min ago" :
                age.TotalDays < 1 ? $"Created {(int)age.TotalHours} hours ago" :
                $"Created {(int)age.TotalDays} days ago";
            OnPropertyChanged(nameof(CreatedAgeText));
            OnPropertyChanged(nameof(HasCreatedInfo));
        }
    }

    partial void OnHasDueDateChanged(bool value) => IsDueDatePickerVisible = value;
    partial void OnDueDateSelectionChanged(DateTime value) => IsDueDatePickerVisible = false;
    partial void OnRepeatFrequencyChanged(NoteRepeatFrequency value) => IsRepeatPickerVisible = false;
    partial void OnHasReminderChanged(bool value) => IsReminderPickerVisible = value;
    partial void OnReminderDateSelectionChanged(DateTime value) => IsReminderPickerVisible = false;
    partial void OnReminderTimeSelectionChanged(TimeSpan value) => IsReminderPickerVisible = false;

    [RelayCommand]
    private async Task SaveAsync()
    {
        var trimmed = (Title ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
            return;

        if (_existing is null)
        {
            await _noteService.CreateAsync(
                trimmed,
                dueDate: HasDueDate ? DueDateSelection : null,
                reminderDate: HasReminder ? ReminderDateSelection.Date.Add(ReminderTimeSelection) : null,
                listId: _listId,
                repeatFrequency: RepeatFrequency);
            await _navigation.PopModalAsync(true);
            return;
        }

        _existing.Title = trimmed;
        _existing.DueDate = HasDueDate ? DueDateSelection : null;
        _existing.ReminderDate = HasReminder
            ? ReminderDateSelection.Date.Add(ReminderTimeSelection)
            : null;
        _existing.RepeatFrequency = RepeatFrequency;
        await _noteService.UpdateAsync(_existing);
        await _navigation.PopModalAsync(true);
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigation.PopModalAsync(false);
    }

    [RelayCommand]
    private void ToggleDueDatePicker() => IsDueDatePickerVisible = !IsDueDatePickerVisible;

    [RelayCommand]
    private void ToggleRepeatPicker() => IsRepeatPickerVisible = !IsRepeatPickerVisible;

    [RelayCommand]
    private void ToggleReminderPicker() => IsReminderPickerVisible = !IsReminderPickerVisible;

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_existing is null) return;
        await _noteService.SoftDeleteAsync(_existing.Id);
        await _navigation.PopModalAsync(true);
    }

}
