using Shared.CapaAplicacion.Interfaces;

namespace Data.Interfaces.CapaApp.Uam3.IAppInsumosRobot;

public interface IAppInsumosRobot
{
    //interface para importar insumos robot capa app

    void EjecutarCargaRobot(Action<string>? onProgress = null);

    Dictionary<string, string> ObtenerConfiguracionAppsActivas();

}
