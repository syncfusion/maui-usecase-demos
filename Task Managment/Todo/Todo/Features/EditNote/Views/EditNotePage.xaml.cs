using Todo.Features.EditNote.ViewModels;
using Todo.Features.Notes.Views;

namespace Todo.Features.EditNote.Views;

public partial class EditNotePage : ContentPage
{
    public EditNotePage(EditNoteViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private static void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Border border)
            PointerHoverSurface.SetHovered(border);
    }

    private static void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Border border)
            PointerHoverSurface.Reset(border);
    }
}
