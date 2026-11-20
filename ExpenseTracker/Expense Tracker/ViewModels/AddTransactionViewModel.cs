using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.TabView;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SmartExpense;

/// <summary>
/// ViewModel responsible for Add and Edit Transaction workflows.
/// Handles form state, validation, dirty tracking, and save orchestration
/// while remaining UI-thread safe and reusable across popup and page hosts.
/// </summary>
public partial class AddTransactionViewModel : BaseViewModel
{
    #region Private fields

    private readonly ITransactionService _transactionService;
    private readonly IAppStateService _appState;

    // Original values captured when entering edit mode
    private string? _origCategoryName;
    private string? _origSubCategory;
    private decimal _origAmount;
    private DateTime _origDate;
    private int _origTypeIndex = -1;
    private string? _origNotes;

    private bool _isSaving;
    private int _selectedTypeIndex = -1;

    #endregion

    #region Constructor

    public AddTransactionViewModel(
        ITransactionService transactionService,
        IAppStateService appState)
    {
        _transactionService = transactionService;
        _appState = appState;

        DateSelectedCommand = new Command<DateTime>(date =>
        {
            TransactionDate = date;
        });
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when an edit operation completes successfully.
    /// Used by popup hosts to close without coupling navigation into the ViewModel.
    /// </summary>
    public event Action? EditCompleted;

    /// <summary>
    /// When set, indicates which main tab the UI should return to after a successful add.
    /// 0 = Dashboard, 1 = Add, 2 = Transactions, etc. If null, default is 0 (Dashboard).
    /// </summary>
    public int? ReturnTabIndex { get; set; }

    #endregion

    #region Public observable properties (form fields)

    /// <summary>
    /// Shared category collection from application state.
    /// </summary>
    public ObservableCollection<Category> Categories => _appState.Categories;

    [ObservableProperty]
    private string transactionId = string.Empty;

    partial void OnTransactionIdChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            CaptureOriginalState();
        else
            ClearOriginalState();

        RefreshCanSave();
    }

    [ObservableProperty]
    private string titleText = string.Empty;

    partial void OnTitleTextChanged(string value) => RefreshCanSave();

    [ObservableProperty]
    private decimal amount;

    partial void OnAmountChanged(decimal value) => RefreshCanSave();

    [ObservableProperty]
    private Category? selectedCategory;

    partial void OnSelectedCategoryChanged(Category? value)
    {
        SubCategories.Clear();
        SelectedSubCategory = null;

        if (value != null &&
            CategorySubcategoryMap.Map.TryGetValue(value.Name, out var subs))
        {
            foreach (var s in subs)
                SubCategories.Add(s);
        }

        RefreshCanSave();
    }

    /// <summary>
    /// Indicates whether a save operation is in progress.
    /// Used to disable UI affordances during async execution.
    /// </summary>
    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }

    /// <summary>
    /// Available subcategories derived from the selected category.
    /// </summary>
    public ObservableCollection<string> SubCategories { get; } = new();

    [ObservableProperty]
    private string? selectedSubCategory;

    partial void OnSelectedSubCategoryChanged(string? value) => RefreshCanSave();

    [ObservableProperty]
    private DateTime transactionDate = DateTime.Now;

    partial void OnTransactionDateChanged(DateTime value) => RefreshCanSave();

    [ObservableProperty]
    private string notes = string.Empty;

    /// <summary>
    /// Segmented control index: 0 = Expense, 1 = Income.
    /// Manual implementation preserves dependent property notifications.
    /// </summary>
    public int SelectedTypeIndex
    {
        get => _selectedTypeIndex;
        set
        {
            if (_selectedTypeIndex == value)
                return;

            _selectedTypeIndex = value;
            OnPropertyChanged(nameof(SelectedTypeIndex));
            OnPropertyChanged(nameof(SelectedType));
            OnPropertyChanged(nameof(IsIncome));
            RefreshCanSave();
        }
    }

    #endregion

    #region Computed properties

    /// <summary>
    /// Derived transaction type from SelectedTypeIndex.
    /// </summary>
    public TransactionType SelectedType => (TransactionType)SelectedTypeIndex;

    /// <summary>
    /// Convenience flag for income selection.
    /// </summary>
    public bool IsIncome => SelectedType == TransactionType.Income;

    /// <summary>
    /// Determines whether the current form state is valid and dirty enough to save.
    /// Logic is shared for both Add and Edit modes.
    /// </summary>
    public bool CanSave =>
        !IsSaving &&
        !IsBusy &&
        !string.IsNullOrWhiteSpace(SelectedSubCategory) &&
        Amount > 0 &&
        SelectedCategory is not null &&
        SelectedTypeIndex >= 0 &&
        (string.IsNullOrWhiteSpace(TransactionId) || HasChanges());

    #endregion

    #region Commands

    /// <summary>
    /// Persists the transaction and notifies listeners of changes.
    /// Behavior differs subtly between Add and Edit modes and must remain stable.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsBusy = true;
        IsSaving = true;
        SaveCommand.NotifyCanExecuteChanged();

        try
        {
            // Yield once to allow UI to render busy state before I/O
            await Task.Yield();

            var transaction = new Transaction
            {
                Id = string.IsNullOrWhiteSpace(TransactionId)
                    ? Guid.NewGuid().ToString("N")
                    : TransactionId,
                Title = SelectedSubCategory,
                Amount = Math.Abs(Amount),
                Category = SelectedCategory!.Name,
                Type = IsIncome ? TransactionType.Income : TransactionType.Expense,
                Date = TransactionDate,
                Notes = Notes?.Trim() ?? string.Empty
            };

            await _transactionService.SaveTransactionAsync(transaction);

            // Yield to prevent first-save UI saturation on mobile
            await Task.Yield();

            var when = DateTime.UtcNow;
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    WeakReferenceMessenger.Default.Send(
                        new TransactionsChangedMessage(when));
                });
            }
            catch
            {
                WeakReferenceMessenger.Default.Send(
                    new TransactionsChangedMessage(when));
            }

            IsSaving = false;

            // Edit mode exits immediately after save
            if (!string.IsNullOrWhiteSpace(TransactionId))
            {
                EditCompleted?.Invoke();
                return;
            }

            // Add mode UX feedback: show confirmation, reset form and
            // navigate back to the Dashboard tab on mobile.
            //var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            //if (page is not null)
            //    await page.DisplayAlertAsync(
            //        "Saved",
            //        "Transaction saved successfully.",
            //        "OK");

            ResetForm();

                try
                {
                    // Use messenger to request MainPage switch to the desired tab.
                    // If ReturnTabIndex is not provided, default to Dashboard (0).
                    var target = ReturnTabIndex ?? 0;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        WeakReferenceMessenger.Default.Send(new NavigateToTabMessage(target));
                    });
                }
                catch { }

            // If this page was pushed onto a Navigation stack, pop it so the
            // user returns to the MainPage where the tab change is visible.
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var nav = Application.Current?.MainPage?.Navigation
                              ?? Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation;

                    if (nav is not null && nav.NavigationStack.Count > 1)
                    {
                        try { await nav.PopAsync(); } catch { }
                    }
                });
            }
            catch { }
        }
        catch (Exception ex)
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is not null)
            {
                await page.DisplayAlertAsync(
                    "Save failed",
                    $"Could not save the transaction.\n{ex.Message}",
                    "OK");
            }
        }
        finally
        {
            IsBusy = false;
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Loads categories into shared application state if not already initialized.
    /// </summary>
    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        if (Categories.Count > 0)
            return;

        var categories = await _transactionService.GetCategoriesAsync();
        if (categories.Count == 0)
            categories = CategorySeedData.GetDefaultCategories().ToList();

        foreach (var c in categories)
        {
            if (!Categories.Any(x => x.Name == c.Name))
                Categories.Add(c);
        }

        SelectedCategory = null;
        RefreshCanSave();
    }

    /// <summary>
    /// Sets transaction type based on segmented control selection.
    /// </summary>
    [RelayCommand]
    private void SetType(bool income)
    {
        SelectedTypeIndex = income ? 1 : 0;
    }

    /// <summary>
    /// Command invoked when a date is selected from the calendar.
    /// </summary>
    public ICommand DateSelectedCommand { get; }

    #endregion

    #region Lifecycle / initialization

    /// <summary>
    /// Preloads the ViewModel for edit mode using an existing transaction.
    /// Categories must already be loaded before invocation.
    /// </summary>
    public void LoadForEdit(Transaction tx)
    {
        if (tx is null)
            return;

        Amount = Math.Abs(tx.Amount);
        TransactionDate = tx.Date;
        Notes = tx.Notes ?? string.Empty;
        SelectedTypeIndex = tx.Type == TransactionType.Income ? 1 : 0;
        SelectedCategory = Categories.FirstOrDefault(c => c.Name == tx.Category);
        SelectedSubCategory = tx.Title;

        // Assign last to trigger original state capture
        TransactionId = tx.Id;
    }

    #endregion

    #region Private helper methods

    private void RefreshCanSave()
    {
        SaveCommand?.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanSave));
    }

    private bool HasChanges()
    {
        if (string.IsNullOrWhiteSpace(_origCategoryName) &&
            string.IsNullOrWhiteSpace(_origSubCategory) &&
            _origAmount == 0 &&
            _origDate == default &&
            _origTypeIndex == -1)
            return true;

        if (!string.Equals(
                SelectedCategory?.Name ?? string.Empty,
                _origCategoryName ?? string.Empty,
                StringComparison.Ordinal))
            return true;

        if (!string.Equals(
                SelectedSubCategory ?? string.Empty,
                _origSubCategory ?? string.Empty,
                StringComparison.Ordinal))
            return true;

        if (Math.Abs(Amount - _origAmount) > 0.0001m)
            return true;

        if (TransactionDate != _origDate)
            return true;

        if (SelectedTypeIndex != _origTypeIndex)
            return true;

        if (!string.Equals(
                Notes ?? string.Empty,
                _origNotes ?? string.Empty,
                StringComparison.Ordinal))
            return true;

        return false;
    }

    private void CaptureOriginalState()
    {
        _origCategoryName = SelectedCategory?.Name;
        _origSubCategory = SelectedSubCategory;
        _origAmount = Amount;
        _origDate = TransactionDate;
        _origTypeIndex = SelectedTypeIndex;
        _origNotes = Notes;
    }

    private void ClearOriginalState()
    {
        _origCategoryName = null;
        _origSubCategory = null;
        _origAmount = 0;
        _origDate = default;
        _origTypeIndex = -1;
        _origNotes = null;
    }

    private void ResetForm()
    {
        // Clear all user-editable fields so the form behaves like a fresh "Create" page
        SelectedSubCategory = null;
        SubCategories.Clear();
        SelectedCategory = null;
        Amount = 0;
        Notes = string.Empty;
        TransactionDate = DateTime.Now;
        SelectedTypeIndex = -1;
        TransactionId = string.Empty;
        IsSaving = false;
        RefreshCanSave();
    }

    #endregion
}