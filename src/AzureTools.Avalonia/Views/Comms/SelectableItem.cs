using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AzureTools.Views.Comms
{
    public class SelectableItem : INotifyPropertyChanged
    {
        public bool IsSelected
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                OnPropertyChanged();
            }
        }

        public object Item
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
