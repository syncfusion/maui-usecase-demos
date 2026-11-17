using Todo.Features.EditNote.ViewModels;
using Todo.Features.EditNote.Views;
using Todo.Features.Settings.ViewModels;
using Todo.Features.Settings.Views;

namespace Todo.Common;

public sealed class PageFactory : IPageFactory
{
    public Page CreateEditNotePage(EditNoteViewModel viewModel) => new EditNotePage(viewModel);

    public Page CreateSettingsPage(SettingsViewModel viewModel) => new SettingsPage(viewModel);
}
