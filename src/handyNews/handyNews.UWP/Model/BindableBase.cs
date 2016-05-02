using System.ComponentModel;

namespace handyNews.UWP.Model
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, string propertyName)
        {
            if (Equals(storage, value)) 
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            
            return true;
        }
    }
}
