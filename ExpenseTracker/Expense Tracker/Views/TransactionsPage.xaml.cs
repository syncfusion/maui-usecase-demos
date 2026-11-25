

namespace SmartExpense;

/// <summary>
/// Represents the Transactions UI surface responsible for hosting the Syncfusion DataGrid,
/// DataPager, and edit flows for transactions. This view coordinates lifecycle timing,
/// selection synchronization, and navigation while delegating all state and logic to
/// <see cref="TransactionsViewModel"/>.
/// </summary>
public partial class TransactionsPage : ContentView
{

    #region Constructor

    public TransactionsPage()
    {
        InitializeComponent();

        Content = DeviceInfo.Platform == DevicePlatform.WinUI
        || DeviceInfo.Platform == DevicePlatform.MacCatalyst
        ? new TransactionsPage_Desktop()
        : new TransactionsPage_Mobile();
    }

    #endregion
}