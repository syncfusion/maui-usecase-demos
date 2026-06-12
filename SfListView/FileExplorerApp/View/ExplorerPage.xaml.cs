namespace FileExplorerApp;


//[QueryProperty(nameof(FolderName), "FolderName")]
//public partial class ExplorerPage : ContentPage
//{
//    public string FolderName
//    {
//        set
//        {
//            if (BindingContext is FileExplorerViewModel vm)
//            {
//               // vm.LoadFolder(value);
//            }
//        }
//    }

//    public ExplorerPage()
//    {
//        InitializeComponent();
//    }
//}


[QueryProperty(nameof(Folder), "Folder")]
public partial class ExplorerPage : ContentPage
{
    public FileItem Folder
    {
        set
        {
            if (BindingContext is FileExplorerViewModel vm)
            {
                vm.OpenFolder(value);
            }
        }
    }

    public ExplorerPage()
    {
        InitializeComponent();
    }
}