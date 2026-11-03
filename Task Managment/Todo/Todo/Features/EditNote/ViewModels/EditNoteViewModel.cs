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
    private bool _isInitializing;

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
        _isInitializing = true;

        if (existing is not null)
        {
            IsExisting = true;
            Title = existing.Title ?? string.Empty;
            HasDueDate = existing.DueDate.HasValue;
            IsDueDatePickerVisible = HasDueDate;
            DueDateSelection = existing.DueDate?.Date ?? DateTime.Today;
            RepeatFrequency = existing.RepeatFrequency;
            IsRepeatPickerVisible = RepeatFrequency != NoteRepeatFrequency.Never;
            HasReminder = existing.ReminderDate.HasValue;
            IsReminderPickerVisible = HasReminder;
            ReminderDateSelection = existing.ReminderDate?.Date ?? DateTime.Today;
            ReminderTimeSelection = existing.ReminderDate?.TimeOfDay ?? new TimeSpan(9, 0, 0);
            var age = DateTime.Now - existing.CreatedAt;
            CreatedAgeText = age.TotalMinutes < 1 ? "Created just now" :
                age.TotalHours < 1 ? $"Created {(int)age.TotalMinutes} min ago" :
                age.TotalDays < 1 ? $"Created {(int)age.TotalHours} hours ago" :
                $"Created {(int)age.TotalDays} days ago";
            OnPropertyChanged(nameof(CreatedAgeText));
            OnPropertyChanged(nameof(HasCreatedInfo));
        }

        _isInitializing = false;
    }

    partial void OnTitleChanged(string value)
    {
        SaveCommand.NotifyCanExecuteChanged();
        _ = AutoSaveAsync();
    }

    partial void OnHasDueDateChanged(bool value)
    {
        IsDueDatePickerVisible = value;
        _ = AutoSaveAsync();
    }

    partial void OnRepeatFrequencyChanged(NoteRepeatFrequency value)
    {
        IsRepeatPickerVisible = value != NoteRepeatFrequency.Never;
        _ = AutoSaveAsync();
    }

    partial void OnHasReminderChanged(bool value)
    {
        IsReminderPickerVisible = value;
        _ = AutoSaveAsync();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (_existing is null)
        {
            var trimmed = (Title ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(trimmed)) return;

            var dueDate = HasDueDate ? DueDateSelection.Date : null;
            var reminderDate = HasReminder
                ? ReminderDateSelection.Date.Add(ReminderTimeSelection)
                : null;

            await _noteService.CreateAsync(trimmed, dueDate, reminderDate, listId: _listId, repeatFrequency: RepeatFrequency);
        }
        else
        {
            await AutoSaveAsync(force: true);
        }

        await _navigation.PopModalAsync(true);
    }

    private async Task AutoSaveAsync(bool force = false)
    {
        if (_existing is null || _isInitializing) return;

        var trimmed = (Title ?? string.Empty).Trim();
        if (!force && trimmed.Length == 0) return;

        _existing.Title = trimmed;
        _existing.DueDate = HasDueDate ? DueDateSelection.Date : null;
        _existing.ReminderDate = HasReminder
            ? ReminderDateSelection.Date.Add(ReminderTimeSelection)
            : null;
        _existing.RepeatFrequency = RepeatFrequency;
        await _noteService.UpdateAsync(_existing);
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

    partial void OnDueDateSelectionChanged(DateTime value) => _ = AutoSaveAsync();
    partial void OnReminderDateSelectionChanged(DateTime value) => _ = AutoSaveAsync();
    partial void OnReminderTimeSelectionChanged(TimeSpan value) => _ = AutoSaveAsync();

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_existing is null) return;
        await _noteService.SoftDeleteAsync(_existing.Id);
        await _navigation.PopModalAsync(true);
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Title);
}
