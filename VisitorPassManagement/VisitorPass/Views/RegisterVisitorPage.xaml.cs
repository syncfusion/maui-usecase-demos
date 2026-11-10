using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-071..074 — Registration surface page (standalone host).
///
/// Thin ContentPage wrapper around the shared <see cref="RegisterVisitorView"/>
/// so the form layout exists exactly once. Forwards the shared
/// view's <see cref="RegisterVisitorView.DismissRequested"/> and
/// <see cref="RegisterVisitorView.PassGenerated"/> events to anyone
/// who subscribes to this page directly.
///
/// Note: the Dashboard's register trigger inflates the shared view
/// directly inside its idiom-aware containers (VP-073), so the
/// page's events are only relevant for direct hosts.
/// </summary>
public partial class RegisterVisitorPage : ContentPage
{
    public RegisterVisitorPage(RegisterVisitorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // The shared view sits as the page's root content; forward
        // its lifecycle events to the page's own events.
        if (Content is RegisterVisitorView inner)
        {
            inner.DismissRequested += (_, e) => DismissRequested?.Invoke(this, e);
            inner.PassGenerated += (_, v) => PassGenerated?.Invoke(this, v);
        }
    }

    /// <summary>Forwards the inner view's DismissRequested.</summary>
    public event EventHandler? DismissRequested;

    /// <summary>Forwards the inner view's PassGenerated.</summary>
    public event EventHandler<Models.Visitor>? PassGenerated;
}
