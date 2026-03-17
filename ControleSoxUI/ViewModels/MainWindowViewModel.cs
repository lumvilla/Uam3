using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ControleSoxUI.Models;
using ControleSoxUI.Services;

namespace ControleSoxUI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;
        private bool _isPaneOpen = true;
        private string _selectedContent = "Bienvenido al Sistema de Control de Retiros";
        private MenuItemModel? _selectedMenuItem;

        public MainWindowViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
            _navigationService.NavigationRequested += OnNavigationRequested;

            InitializeMenuItems();
            TogglePaneCommand = new RelayCommand(TogglePane);
            MenuItemClickCommand = new RelayCommand<MenuItemModel>(OnMenuItemClick);
        }

        public ObservableCollection<MenuItemModel> MenuItems { get; set; } = new();

        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set
            {
                _isPaneOpen = value;
                OnPropertyChanged();
            }
        }

        public string SelectedContent
        {
            get => _selectedContent;
            set
            {
                _selectedContent = value;
                OnPropertyChanged();
            }
        }

        public MenuItemModel? SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                _selectedMenuItem = value;
                OnPropertyChanged();
            }
        }

        public ICommand TogglePaneCommand { get; }
        public ICommand MenuItemClickCommand { get; }

        private void InitializeMenuItems()
        {
            MenuItems = new ObservableCollection<MenuItemModel>
            {
                // 1. Cargue de insumos Retiros
                new MenuItemModel
                {
                    Title = "Cargue de Insumos Retiros",
                    Icon = "\uE8F4", // Icon Upload
                    Tag = "CargueRetiros",
                    SubItems = new ObservableCollection<MenuItemModel>
                    {
                        new MenuItemModel
                        {
                            Title = "Cargue Directorio Activo",
                            Icon = "\uE7EE", // Icon ContactInfo
                            Description = "Se cargará un Excel con un directorio Activo para realizar cruces y carga de login de user o estado de los user",
                            Tag = "CargueDA"
                        },
                        new MenuItemModel
                        {
                            Title = "Cargue Retiros Vinculados",
                            Icon = "\uE8A5", // Icon People
                            Description = "Se cargará un Excel con los retiros de Workday",
                            Tag = "CargueVinculados"
                        },
                        new MenuItemModel
                        {
                            Title = "Cargue Retiros Terceros",
                            Icon = "\uE716", // Icon Group
                            Description = "Se cargará un Excel con los retiros de Terceros (desde Iam)",
                            Tag = "CargueTerceros"
                        }
                    }
                },

                // 2. UAM3
                new MenuItemModel
                {
                    Title = "UAM3",
                    Icon = "\uE77B", // Icon Settings
                    Tag = "UAM3",
                    SubItems = new ObservableCollection<MenuItemModel>
                    {
                        new MenuItemModel
                        {
                            Title = "Cargue de Insumos UAM3",
                            Icon = "\uE896", // Icon Database
                            Tag = "CargueUAM3"
                        },
                        new MenuItemModel
                        {
                            Title = "User a Deshabilitar",
                            Icon = "\uE72E", // Icon BlockContact
                            Tag = "UserDeshabilitar"
                        },
                        new MenuItemModel
                        {
                            Title = "Services Desk",
                            Icon = "\uE8F3", // Icon Support
                            Tag = "ServicesDesk"
                        },
                        new MenuItemModel
                        {
                            Title = "Reportes",
                            Icon = "\uE9F9", // Icon Report
                            Tag = "ReportesUAM3"
                        }
                    }
                },

                // 3. Retiros User No Sox
                new MenuItemModel
                {
                    Title = "Retiros User No Sox",
                    Icon = "\uE8EB", // Icon UserRemove
                    Tag = "RetirosNoSox",
                    SubItems = new ObservableCollection<MenuItemModel>
                    {
                        new MenuItemModel
                        {
                            Title = "Cargue de Insumos No Sox",
                            Icon = "\uE8F4", // Icon Upload
                            Tag = "CargueNoSox"
                        },
                        new MenuItemModel
                        {
                            Title = "User a Deshabilitar",
                            Icon = "\uE72E", // Icon BlockContact
                            Tag = "UserDeshabilitarNoSox"
                        },
                        new MenuItemModel
                        {
                            Title = "Services Desk",
                            Icon = "\uE8F3", // Icon Support
                            Tag = "ServicesDeskNoSox",
                            SubItems = new ObservableCollection<MenuItemModel>
                            {
                                new MenuItemModel
                                {
                                    Title = "Realizar OC Retiros",
                                    Icon = "\uE8A7", // Icon AddEvent
                                    Tag = "RealizarOC"
                                },
                                new MenuItemModel
                                {
                                    Title = "Consultar Estado OC",
                                    Icon = "\uE946", // Icon StatusCircleQuestionMark
                                    Tag = "ConsultarOC"
                                }
                            }
                        },
                        new MenuItemModel
                        {
                            Title = "Reportes",
                            Icon = "\uE9F9", // Icon Report
                            Tag = "ReportesNoSox"
                        }
                    }
                },

                // 4. Ejecutar SQL
                new MenuItemModel
                {
                    Title = "Ejecutar SQL",
                    Icon = "\uE8B5", // Icon Code
                    Tag = "SQL"
                },

                // 5. Guía
                new MenuItemModel
                {
                    Title = "Guía",
                    Icon = "\uE897", // Icon Help
                    Tag = "Guia"
                },

                // 6. Salir
                new MenuItemModel
                {
                    Title = "Salir",
                    Icon = "\uE7E8", // Icon Leave
                    Tag = "Salir"
                }
            };
        }

        private void TogglePane()
        {
            IsPaneOpen = !IsPaneOpen;
        }

        private void OnMenuItemClick(MenuItemModel? menuItem)
        {
            if (menuItem == null) return;

            // Si tiene subitems, toggle expand
            if (menuItem.HasSubItems)
            {
                // Colapsar otros items del mismo nivel (opcional)
                foreach (var item in MenuItems)
                {
                    if (item != menuItem && item.IsExpanded)
                    {
                        item.IsExpanded = false;
                    }
                }

                // Toggle el item actual
                menuItem.IsExpanded = !menuItem.IsExpanded;
                return;
            }


            // Marcar el item como seleccionado
            ClearSelection(MenuItems);
            menuItem.IsSelected = true;

            // Si es un item final, navegar
            SelectedMenuItem = menuItem;

            switch (menuItem.Tag)
            {
                case "Salir":
                    if (MessageBox.Show("¿Está seguro que desea salir?", "Confirmar",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Application.Current.Shutdown();
                    }
                    break;

                case "CargueDA":
                    SelectedContent = $"📂 {menuItem.Title}\n\n{menuItem.Description}";
                    break;

                case "CargueVinculados":
                    SelectedContent = $"📂 {menuItem.Title}\n\n{menuItem.Description}";
                    break;

                case "CargueTerceros":
                    SelectedContent = $"📂 {menuItem.Title}\n\n{menuItem.Description}";
                    break;

                case "SQL":
                    SelectedContent = "🔍 Ejecutar consultas SQL personalizadas";
                    break;

                case "Guia":
                    SelectedContent = "📖 Guía de uso del sistema";
                    break;

                default:
                    SelectedContent = $"🎯 {menuItem.Title}\n\nFuncionalidad en desarrollo...";
                    break;
            }

            _navigationService.NavigateTo(menuItem.Tag);
        }

        private void ClearSelection(ObservableCollection<MenuItemModel> items)
        {
            foreach (var item in items)
            {
                item.IsSelected = false;
                if (item.HasSubItems && item.SubItems != null)
                {
                    ClearSelection(item.SubItems);
                }
            }
        }
        private void OnNavigationRequested(string destination)
        {
            // Aquí puedes manejar la navegación adicional si es necesario
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // RelayCommand helper class
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        public void Execute(object? parameter) => _execute((T?)parameter);
    }


}