using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace FileExplorerApp
{
    public class FileItem : INotifyPropertyChanged
    {

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Details { get; set; }
        public string Icon { get; set; }

        public bool IsFolder { get; set; }

        public ObservableCollection<FileItem> Children { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
