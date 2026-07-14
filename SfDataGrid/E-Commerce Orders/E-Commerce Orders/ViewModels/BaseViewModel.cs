using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace E_Commerce_Orders.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(backingField, value))
            {
                backingField = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
