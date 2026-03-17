using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ControleSoxUI.Services;
using Data.Repository.CapaAPP.InsertExcelBD;
using Microsoft.Win32;

namespace ControleSoxUI.ViewModels
{
    public class CargueInsumosViewModel : INotifyPropertyChanged
    {
        private readonly ICargueInsumosService _cargueService;

        private TipoInsumo _tipoInsumoSeleccionado = TipoInsumo.DirectorioActivo;
        private bool _estaCargando;
        private string _mensajeEstado = "Seleccione un tipo de insumo y un archivo para cargar";
        private string _archivoSeleccionado = string.Empty;
        private int _registrosCargados;
        private bool _mostrarProgreso;
        private string _progresoTexto = string.Empty;

        public CargueInsumosViewModel(ICargueInsumosService cargueService)
        {
            _cargueService = cargueService;

            SeleccionarArchivoCommand = new RelayCommand(SeleccionarArchivo, () => !EstaCargando);
            CargarArchivoCommand = new RelayCommand(async () => await CargarArchivoAsync(), PuedeCargar);
            BuscarDatosCommand = new RelayCommand(async () => await BuscarDatosAsync(), () => !EstaCargando);

            // Inicializar tipos de insumo
            TiposInsumo = new ObservableCollection<TipoInsumoDisplay>
            {
                new TipoInsumoDisplay { Tipo = TipoInsumo.DirectorioActivo, Nombre = "Directorio Activo", Icono = "\uE7EE" },
                new TipoInsumoDisplay { Tipo = TipoInsumo.Vinculados, Nombre = "Vinculados", Icono = "\uE8A5" },
                new TipoInsumoDisplay { Tipo = TipoInsumo.Terceros, Nombre = "Terceros", Icono = "\uE716" }
            };

            TipoSeleccionado = TiposInsumo[0];
            DatosGrid = new ObservableCollection<Dictionary<string, object>>();
        }

        #region Propiedades

        public ObservableCollection<TipoInsumoDisplay> TiposInsumo { get; }
        public ObservableCollection<Dictionary<string, object>> DatosGrid { get; }

        private TipoInsumoDisplay _tipoSeleccionado;
        public TipoInsumoDisplay TipoSeleccionado
        {
            get => _tipoSeleccionado;
            set
            {
                _tipoSeleccionado = value;
                OnPropertyChanged();
                TipoInsumoSeleccionado = value.Tipo;
                MensajeEstado = $"Tipo seleccionado: {value.Nombre}. Seleccione un archivo para cargar.";
            }
        }

        public TipoInsumo TipoInsumoSeleccionado
        {
            get => _tipoInsumoSeleccionado;
            set
            {
                _tipoInsumoSeleccionado = value;
                OnPropertyChanged();
            }
        }

        public bool EstaCargando
        {
            get => _estaCargando;
            set
            {
                _estaCargando = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set
            {
                _mensajeEstado = value;
                OnPropertyChanged();
            }
        }

        public string ArchivoSeleccionado
        {
            get => _archivoSeleccionado;
            set
            {
                _archivoSeleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneArchivoSeleccionado));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool TieneArchivoSeleccionado => !string.IsNullOrEmpty(ArchivoSeleccionado);

        public int RegistrosCargados
        {
            get => _registrosCargados;
            set
            {
                _registrosCargados = value;
                OnPropertyChanged();
            }
        }

        public bool MostrarProgreso
        {
            get => _mostrarProgreso;
            set
            {
                _mostrarProgreso = value;
                OnPropertyChanged();
            }
        }

        public string ProgresoTexto
        {
            get => _progresoTexto;
            set
            {
                _progresoTexto = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Comandos

        public ICommand SeleccionarArchivoCommand { get; }
        public ICommand CargarArchivoCommand { get; }
        public ICommand BuscarDatosCommand { get; }

        #endregion

        #region Métodos

        private void SeleccionarArchivo()
        {
            var dialog = new OpenFileDialog
            {
                Title = $"Seleccionar archivo de {TipoSeleccionado.Nombre}",
                Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                ArchivoSeleccionado = dialog.FileName;
                MensajeEstado = $"Archivo seleccionado: {System.IO.Path.GetFileName(ArchivoSeleccionado)}";
            }
        }

        private bool PuedeCargar()
        {
            return !EstaCargando && TieneArchivoSeleccionado;
        }

        private async Task CargarArchivoAsync()
        {
            if (string.IsNullOrEmpty(ArchivoSeleccionado))
            {
                MessageBox.Show("Debe seleccionar un archivo primero", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EstaCargando = true;
            MostrarProgreso = true;
            ProgresoTexto = "Iniciando carga...";

            try
            {
                var progress = new Progress<string>(mensaje =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProgresoTexto = mensaje;
                    });
                });

                var resultado = await _cargueService.CargarArchivoAsync(
                    ArchivoSeleccionado,
                    TipoInsumoSeleccionado,
                    progress);

                if (resultado.Exitoso)
                {
                    RegistrosCargados = resultado.RegistrosCargados;
                    MensajeEstado = $"✅ {resultado.Mensaje}";

                    MessageBox.Show(
                        $"Archivo cargado exitosamente\n\n" +
                        $"Archivo: {System.IO.Path.GetFileName(ArchivoSeleccionado)}\n" +
                        $"Tipo: {TipoSeleccionado.Nombre}\n" +
                        $"Registros: {resultado.RegistrosCargados}",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Limpiar selección
                    ArchivoSeleccionado = string.Empty;

                    // Cargar datos automáticamente después de la carga
                    await BuscarDatosAsync();
                }
                else
                {
                    MensajeEstado = $"❌ {resultado.Mensaje}";

                    MessageBox.Show(
                        $"Error al cargar el archivo:\n\n{resultado.Error}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error inesperado: {ex.Message}";
                MessageBox.Show(
                    $"Error inesperado:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
                MostrarProgreso = false;
                ProgresoTexto = string.Empty;
            }
        }

        private async Task BuscarDatosAsync()
        {
            EstaCargando = true;
            MensajeEstado = "Buscando datos...";

            try
            {
                var datos = await _cargueService.ObtenerDatosAsync(TipoInsumoSeleccionado);

                DatosGrid.Clear();
                foreach (var registro in datos)
                {
                    DatosGrid.Add(registro);
                }

                MensajeEstado = $"✅ Se encontraron {datos.Count} registros";
                RegistrosCargados = datos.Count;
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al buscar: {ex.Message}";
                MessageBox.Show(
                    $"Error al buscar datos:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    // Clase auxiliar para el ComboBox
    public class TipoInsumoDisplay
    {
        public TipoInsumo Tipo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
    }
}