using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class FilterViewModel : ViewModelBase
    {
        private string _nameFilter = string.Empty;
        public string NameFilter
        {
            get => _nameFilter;
            set { _nameFilter = value; OnPropertyChanged(); }
        }

        private string _typeFilter = "All";
        public string TypeFilter
        {
            get => _typeFilter;
            set { _typeFilter = value; OnPropertyChanged(); }
        }

        private string _statusFilter = "All";
        public string StatusFilter
        {
            get => _statusFilter;
            set { _statusFilter = value; OnPropertyChanged(); }
        }

        // OnPropertyChanged from ViewModelBase
    }
}
