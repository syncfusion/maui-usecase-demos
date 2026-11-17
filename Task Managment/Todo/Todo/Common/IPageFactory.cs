using Todo.Features.EditNote.ViewModels;
using Todo.Features.Settings.ViewModels;

namespace Todo.Common;

public interface IPageFactory
{
    Page CreateEditNotePage(EditNoteViewModel viewModel);
    Page CreateSettingsPage(SettingsViewModel viewModel);
}
