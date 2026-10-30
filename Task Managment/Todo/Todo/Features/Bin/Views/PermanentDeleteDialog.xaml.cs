using Todo.Common;
using Todo.Features.Dialogs.ViewModels;

namespace Todo.Features.Bin.Views;

/// <summary>
/// Centered modal dialog shown before a note is hard-deleted from the
/// in-memory store. The chrome and copy match <c>deletepermently.png</c>:
/// "Delete permanently?" title, a destructive body, and Cancel / Delete
/// buttons.
///
/// <para>
/// The static <see cref="ShowAsync"/> helper hides the navigation
/// plumbing from the calling View (per Module-04 task 4-7). The dialog
/// is a <see cref="ContentPage"/> so it can be pushed modally.
/// </para>
/// </summary>
public partial class PermanentDeleteDialog : ContentPage
{
    public PermanentDeleteDialog(ConfirmDialogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// Pushes a fully-wired <see cref="PermanentDeleteDialog"/> onto
    /// the active modal stack and awaits the user's decision. Returns
    /// <see langword="true"/> when the user confirms the deletion,
    /// <see langword="false"/> otherwise. The caller supplies the shared
    /// navigation service registered by the application.
    /// </summary>
    public static async Task<bool> ShowAsync(INavigationService navigation)
    {
        var vm = new ConfirmDialogViewModel(navigation)
        {
            Title = "Delete permanently?",
            Message = "This note will be permanently deleted and cannot be recovered.",
            CancelText = "Cancel",
            ConfirmText = "Delete",
            IsDestructive = true
        };
        var page = new PermanentDeleteDialog(vm);
        var result = await navigation.PushModalAsync(page);
        return result is true;
    }
}
