using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleSoxUI.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ControleSoxUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? currentView;

    public IRelayCommand<string> NavigateCommand { get; }
    public IRelayCommand ExitCommand { get; }

    public MainViewModel()
    {

        NavigateCommand = new RelayCommand<string>(OnNavigate);
        ExitCommand = new RelayCommand(OnExit);
    }

    private void OnNavigate(string? viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
            return;

        CurrentView = viewName switch
        {
            // === SECCIÓN 1: RETIROS ===
            "CargueRetiros" => new CargueRetirosView(),
            "RetirosVinculados" => new RetirosVinculadosView(),
            "RetirosTerceros" => new RetirosTercerosView(),

            // === SECCIÓN 2: UAM3 ===
            "CargueUam3" => new CargueUam3View(),
            "UsuariosUam3" => new UsuariosUam3View(),
            "ServiceDeskUam3" => new ServiceDeskUam3View(),
            "ReportesUam3" => new ReportesUam3View(),

            // === SECCIÓN 3: RETIROS NO SOX ===
            "CargueNoSox" => new CargueNoSoxView(),
            "UsuariosNoSox" => new UsuariosNoSoxView(),
            "ServiceDeskNoSox" => new ServiceDeskNoSoxView(),
            "ReportesNoSox" => new ReportesNoSoxView(),

            // === SECCIÓN 4, 5, 6 ===
            "EjecutarSQL" => new EjecutarSQLView(),
            "Guia" => new GuiaView(),

            _ => null
        };
    }

    private void OnExit()
    {
        Application.Current.Shutdown();
    }
}
