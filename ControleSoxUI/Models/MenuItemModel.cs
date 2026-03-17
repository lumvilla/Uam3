using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ControleSoxUI.Models
{
    public class MenuItemModel : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private bool _isSelected;

        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICommand? Command { get; set; }
        public ObservableCollection<MenuItemModel> SubItems { get; set; } = new();
        public bool HasSubItems => SubItems.Count > 0;
        public string Tag { get; set; } = string.Empty;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsTopLevel // Nueva propiedad
        {
            get => Parent == null;
        }

        // Asumo que tienes una referencia al padre para los sub-items:
        public MenuItemModel Parent { get; set; }
    }
}