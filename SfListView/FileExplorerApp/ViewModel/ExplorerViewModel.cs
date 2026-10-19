using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Syncfusion.Maui.Popup;
using Syncfusion.Maui.Core.Internals;
namespace FileExplorerApp
{

    public class FileExplorerViewModel : INotifyPropertyChanged
    {
        public double ScreenWidth => DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;

        private readonly PopupService popupService = new PopupService();

        private ObservableCollection<FileItem> files;
        public ObservableCollection<FileItem> Files
        {
            get => files;
            set
            {
                files = value;
                OnPropertyChanged(nameof(Files));
            }
        }

        public ICommand ItemTappedCommand { get; }
        public ICommand LongPressCommand { get; }
        public ICommand RightTappedCommand { get; }
        public ICommand ClearSelectionCommand { get; }
        public ICommand RenameCommand { get; }
        public ICommand DeleteCommand { get; }

        private bool isSelectionMode;
        public bool IsSelectionMode
        {
            get => isSelectionMode;
            set
            {
                isSelectionMode = value;
                OnPropertyChanged(nameof(IsSelectionMode));
            }
        }

        private int selectedCount;
        public int SelectedCount
        {
            get => selectedCount;
            set
            {
                selectedCount = value;
                OnPropertyChanged(nameof(SelectedCount));
            }
        }

        private bool isBottomSheetVisible;
        public bool IsBottomSheetVisible
        {
            get => isBottomSheetVisible;
            set
            {
                isBottomSheetVisible = value;
                OnPropertyChanged(nameof(IsBottomSheetVisible));
            }
        }

        private string currentFolder;
        public string CurrentFolder
        {
            get => currentFolder;
            set
            {
                currentFolder = value;
                OnPropertyChanged(nameof(CurrentFolder));
            }
        }

        private bool isRenamePopupVisible;
        private bool isDeletePopupVisible;

        private string renameText;
        public string RenameText
        {
            get => renameText;
            set
            {
                renameText = value;
                OnPropertyChanged(nameof(RenameText));
            }
        }

        public bool IsRenamePopupVisible
        {
            get => isRenamePopupVisible;
            set
            {
                isRenamePopupVisible = value;
                OnPropertyChanged(nameof(IsRenamePopupVisible));
            }
        }

        public bool IsDeletePopupVisible
        {
            get => isDeletePopupVisible;
            set
            {
                isDeletePopupVisible = value;
                OnPropertyChanged(nameof(IsDeletePopupVisible));
            }
        }

        private FileItem currentFolderItem;
        public FileItem CurrentFolderItem
        {
            get => currentFolderItem;
            set
            {
                currentFolderItem = value;
                OnPropertyChanged(nameof(CurrentFolderItem));
            }
        }

        public FileExplorerViewModel()
        {
            Files = new ObservableCollection<FileItem>
            {
            new FileItem
            {
                Name = "Alarms",
                Details = "04/12/2025 10:49 AM",
                IsFolder = true,
                Children = new ObservableCollection<FileItem>
                {
                    new FileItem { Name = "alarm1.mp3", Details = "16/09/2025 05:40 AM" },
                    new FileItem { Name = "alarm2.mp3", Details = "04/12/2025 10:49 AM" }
                }
            },

            new FileItem
            {
                Name = "Android",
                Details = "09/10/2025 07:40 PM",
                IsFolder = true,
                Children = new ObservableCollection<FileItem>
                {
                    new FileItem { Name = "data1.bin", Details = "02/10/2025 10:38 AM" },
                    new FileItem { Name = "data2.bin", Details = "09/10/2025 07:40 AM" }
                }
            },

            new FileItem
            {
                Name = "Documents",
                Details = "19/05/2026 11:59 PM",
                IsFolder = true,
                Children = new ObservableCollection<FileItem>
                {
                    new FileItem { Name = "resume.pdf", Details = "16/05/2026 10:49 AM" },
                    new FileItem { Name = "notes.txt", Details = "19/05/2026 11:59 PM" }
                }
            },

            new FileItem
            {
                Name = "Download",
                Details = "04/04/2026 03:49 PM",
                IsFolder = true,
                Children = new ObservableCollection<FileItem>
                {
                    new FileItem { Name = "video.mp4", Details = "01/04/2026 10:30 AM" },
                    new FileItem { Name = "image.png", Details = "04/04/2026 03:49 PM" }
                }
            },

            new FileItem
            {
                Name = "Movies",
                Details = "05/09/2025 07:47 AM",
                IsFolder = true,
                Children = new ObservableCollection<FileItem>
                {
                    new FileItem { Name = "movie1.mp4", Details = "05/09/2025 07:47 AM" }
                }
            },

            new FileItem
            {
                Name = "Music",
                Details = "10/06/2025 06:49 PM",
                IsFolder = true,
                Children = new ObservableCollection<FileItem>
                {
                    new FileItem { Name = "song1.mp3", Details = "08/05/2025 08:49 AM" },
                    new FileItem { Name = "song2.mp3", Details = "10/06/2025 06:49 PM" }
                }
            }
            };

            LongPressCommand = new Command<object>(OnLongPress);
            RightTappedCommand = new Command<object>(OnRightTapped);
            ItemTappedCommand = new Command<object>(OnItemTapped);

            RenameCommand = new Command(OnRename);
            DeleteCommand = new Command(OnDelete);

            ClearSelectionCommand = new Command(ClearSelection);
        }

        private void OnRightTapped(object obj)
        {
            var args = obj as Syncfusion.Maui.ListView.ItemRightTappedEventArgs;
            SfPopup popup = CreateContextPopup(args.DataItem as FileItem);
#if MACCATALYST
            popup.Show(args.Position.X, args.Position.Y - 100);
#elif WINDOWS
            popup.Show(args.Position.X, args.Position.Y - 50);
#endif
        }

        SfPopup CreateContextPopup(FileItem item)
        {
            var popup = new SfPopup
            {
                ShowCloseButton = false,
                ShowOverlayAlways=false,
                ShowHeader=false,
                WidthRequest = 260,
            };

            popup.PopupStyle.Stroke = Colors.LightGray;
            popup.PopupStyle.StrokeThickness = 2;
            var container = new VerticalStackLayout
            {
                Spacing = 0,
                BackgroundColor = Colors.White,
                Padding = new Thickness(0),
                Margin = new Thickness(0)
            };

            Label MakeLabel(string text, Color textColor)
            {
                return new Label
                {
                    Text = text,
                    TextColor = textColor,
                    HeightRequest = 44,
                    VerticalTextAlignment = TextAlignment.Center,
                    Padding = new Thickness(12, 0),
                    BackgroundColor = Colors.Transparent
                };
            }

            // Rename (label only - no gesture attached)
            var renameLabel = MakeLabel("Rename", Colors.Black);
            container.Children.Add(renameLabel);
            container.Children.Add(new BoxView { HeightRequest = 1, BackgroundColor = Colors.LightGray });

            // Move (label only - no gesture attached)
            var cutLabel = MakeLabel("Cut", Colors.Black);
            container.Children.Add(cutLabel);
            container.Children.Add(new BoxView { HeightRequest = 1, BackgroundColor = Colors.LightGray });

            // Share (label only - no gesture attached)
            var copyLabel = MakeLabel("Copy", Colors.Black);
            container.Children.Add(copyLabel);
            container.Children.Add(new BoxView { HeightRequest = 1, BackgroundColor = Colors.LightGray });

            // Delete (label with TapGestureRecognizer only)
            var deleteLabel = MakeLabel("Delete", Colors.Red);

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                popup.IsOpen = false;
                Files.Remove(item);
            };

            deleteLabel.GestureRecognizers.Add(tap);
            container.Children.Add(deleteLabel);

            popup.ContentTemplate = new DataTemplate(()=>container);
            return popup;
        }
        private void OnLongPress(object obj)
        {
#if ANDROID || IOS
            var item = (obj as Syncfusion.Maui.ListView.ItemLongPressEventArgs).DataItem as FileItem;
            if (item == null) return;

            if (!IsSelectionMode)
                IsSelectionMode = true;

            ToggleItem(item);
#endif
        }

        private async void OnItemTapped(object obj)
        {
            var item = (obj as Syncfusion.Maui.ListView.ItemTappedEventArgs)?.DataItem as FileItem;
            if (item == null) return;

            // ✅ Navigate only if folder
            if (!IsSelectionMode && item.IsFolder)
            {
                await Shell.Current.GoToAsync(nameof(ExplorerPage), new Dictionary<string, object>
        {
            { "Folder", item } // ✅ pass entire object
        });
                return;
            }

            // Selection mode
            if (IsSelectionMode)
            {
                item.IsSelected = !item.IsSelected;
                SelectedCount = Files.Count(x => x.IsSelected);
            }
        }

        public void OpenFolder(FileItem folder)
        {
            if (folder == null) return;

            CurrentFolderItem = folder;
            CurrentFolder = CurrentFolderItem.Name;
            // ✅ IMPORTANT: DO NOT create new list
            Files = folder.Children;
        }

        private void ToggleItem(FileItem item)
        {
            item.IsSelected = !item.IsSelected;

            SelectedCount = Files.Count(x => x.IsSelected);

            IsBottomSheetVisible = SelectedCount > 0;

            if (SelectedCount == 0)
                IsSelectionMode = false;
        }

        private void ClearSelection()
        {
            foreach (var item in Files)
                item.IsSelected = false;

            SelectedCount = 0;
            IsSelectionMode = false;
            IsBottomSheetVisible = false;
        }
       
        private void OnRename()
        {
            var selected = Files.Where(x => x.IsSelected).ToList();

            if (selected.Count != 1)
                return;

            var item = selected.First();

            // ✅ Hide bottom sheet
            IsBottomSheetVisible = false;

            popupService.ShowRenamePopup(
                Application.Current.MainPage,
                item.Name,
                (newName) =>
                {
                    item.Name = newName;

                    // ✅ restore bottom sheet
                    IsBottomSheetVisible = true;
                },
                () =>
                {
                    // ✅ restore bottom sheet on cancel
                    IsBottomSheetVisible = true;
                });
        }

        public ICommand ConfirmRenameCommand => new Command(() =>
        {
            var item = Files.FirstOrDefault(x => x.IsSelected);

            if (item == null || string.IsNullOrWhiteSpace(RenameText))
                return;

            item.Name = RenameText;

            // ✅ Close popup
            IsRenamePopupVisible = false;

            // ✅ Restore bottom sheet
            IsBottomSheetVisible = true;
        });

        private void OnDelete()
        {
            var selected = Files.Where(x => x.IsSelected).ToList();
            if (selected.Count == 0)
                return;
            IsBottomSheetVisible = false;
            popupService.ShowDeletePopup(
                Application.Current.MainPage,
                () =>
                {
                    foreach (var item in selected)
                        Files.Remove(item);

                    SelectedCount = 0;
                    IsSelectionMode = false;
                },
                    () =>
                    {
                        // ✅ restore if cancelled
                        IsBottomSheetVisible = true;
                    });
        }

        public ICommand ConfirmDeleteCommand => new Command(() =>
        {
            var selected = Files.Where(x => x.IsSelected).ToList();

            foreach (var item in selected)
                Files.Remove(item);

            // ✅ Close popup
            IsDeletePopupVisible = false;

            // ✅ Reset
            SelectedCount = 0;
            IsSelectionMode = false;
        });


        public ICommand CancelPopupCommand => new Command(() =>
        {
            IsRenamePopupVisible = false;
            IsDeletePopupVisible = false;

            // ✅ Restore bottom sheet if still selecting
            if (SelectedCount > 0)
                IsBottomSheetVisible = true;
        });

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
