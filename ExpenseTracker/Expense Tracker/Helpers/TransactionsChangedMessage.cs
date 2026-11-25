using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SmartExpense;

/// <summary>
/// This message class is used to notify subscribers that the transactions have changed. It carries a DateTime value representing the time of the change, allowing subscribers to react accordingly, such as refreshing transaction lists or updating related UI components.
/// </summary>
public sealed class TransactionsChangedMessage : ValueChangedMessage<DateTime>
{
    public TransactionsChangedMessage(DateTime value) : base(value)
    {
    }
}
