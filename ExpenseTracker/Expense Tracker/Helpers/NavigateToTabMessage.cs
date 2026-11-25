namespace SmartExpense;

/// <summary>
/// Sent by any ViewModel that wants to programmatically switch the main tab bar.
/// Index: 0=Dashboard, 1=Add, 2=Transactions, 3=Reports, 4=About
/// </summary>
public class NavigateToTabMessage(int tabIndex)
{
    public int TabIndex { get; } = tabIndex;
}
