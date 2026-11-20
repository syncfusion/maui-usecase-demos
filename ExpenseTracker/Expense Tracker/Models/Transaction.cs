using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartExpense;

/// <summary>
/// The <see cref="Transaction"/> class represents a financial transaction used in-memory.
/// </summary>
public class Transaction : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Category { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public string Notes { get; set; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
