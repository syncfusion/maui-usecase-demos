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

    /// <summary>The note currently being edited, or <c>null</c> when
    /// creating a new note.</summary>
    public Note? Target => _existing;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private bool hasDueDate;

    [ObservableProperty]
    private DateTime dueDateSelection = DateTime.Today;

    [ObservableProperty]
    private NoteRepeatFrequency repeatFrequency;

    [ObservableProperty]
    private bool hasReminder;

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

    public EditNoteViewModel(INoteService noteService, INavigationService navigation)
        : this(noteService, navigation, null)
    {
    }

    public EditNoteViewModel(INoteService noteService, INavigationService navigation, Note? existing)
    {
        _noteService = noteService;
        _navigation = navigation;
        _existing = existing;

        if (existing is not null)
        {
            IsExisting = true;
            Title = existing.Title ?? string.Empty;
            HasDueDate = existing.DueDate.HasValue;
            DueDateSelection = existing.DueDate?.Date ?? DateTime.Today;
            RepeatFrequency = existing.RepeatFrequency;
            HasReminder = existing.ReminderDate.HasValue;
            ReminderDateSelection = existing.ReminderDate?.Date ?? DateTime.Today;
            ReminderTimeSelection = existing.ReminderDate?.TimeOfDay ?? new TimeSpan(9, 0, 0);
        }
    }

    partial void OnTitleChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var trimmed = (Title ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed)) return;

        DateTime? dueDate = HasDueDate ? DueDateSelection.Date : null;
        DateTime? reminderDate = HasReminder
            ? ReminderDateSelection.Date.Add(ReminderTimeSelection)
            : null;

        if (_existing is null)
        {
            await _noteService.CreateAsync(trimmed, dueDate, reminderDate, repeatFrequency: RepeatFrequency);
        }
        else
        {
            _existing.Title = trimmed;
            _existing.DueDate = dueDate;
            _existing.ReminderDate = reminderDate;
            _existing.RepeatFrequency = RepeatFrequency;
            await _noteService.UpdateAsync(_existing);
        }

        await _navigation.PopModalAsync(true);
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigation.PopModalAsync(false);
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Title);
}
