using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.Calendar;
using System.Windows.Input;

namespace SmartExpense;

public partial class AddTransactionPopupContent : ContentView
{
    private AddTransactionViewModel? ViewModel => BindingContext as AddTransactionViewModel;
    private bool _initialized;

    public static readonly BindableProperty CancelCommandProperty =
       BindableProperty.Create(
           nameof(CancelCommand),
           typeof(ICommand),
           typeof(AddTransactionPopupContent));

    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public AddTransactionPopupContent()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler is not null && !_initialized)
        {
            _initialized = true;
            Dispatcher.Dispatch(async () =>
            {
                try
                {
                    if (ViewModel?.LoadCategoriesCommand is not null)
                        await ViewModel.LoadCategoriesCommand.ExecuteAsync(null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AddTransactionPopupContent init error: {ex}");
                }
            });
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        calendar.IsOpen = true;
    }

    private void Calendar_SelectionChanged(object sender,
     CalendarSelectionChangedEventArgs e)
    {
        // ? DO NOTHING HERE
        // Selection is temporary until OK is pressed
    }

    private void Calendar_ActionButtonClicked(object? sender, EventArgs e)
    {
        if (BindingContext is AddTransactionViewModel vm)
            vm.DateSelectedCommand.Execute(calendar.SelectedDate);

        calendar.IsOpen = false;
    }

    private void Calendar_ActionButtonCanceled(object? sender, EventArgs e)
    {
        // ? Just close, no commit
        calendar.IsOpen = false;
    }
}
