using Shared.CapaAplicacion.DA;
using Shared.CapaAplicacion.Interfaces;
using Shared.CapaAplicacion.Portal;
using Shared.CapaAplicacion.Terceros;
using Shared.CapaAplicacion.Vinculados;

namespace Data.Interfaces.CapaApp.InsertExcelBD;

public interface IDatosExcelRepository
{
    void InsertVinculadosBulk(IEnumerable<Vinculados> listaVinculados);
    void InsertDABulk(IEnumerable<DirectorioActivo> listaDA);
    void InsertTercerosBulk(IEnumerable<Terceros> listaTerceros);
    void BulkInsertPortal(IEnumerable<Portal> portales);
    void BulkInsertDatoCapaApp<T>(IEnumerable<T> registros, string nombreTabla) where T : IRegistros;

   
    // Métodos de lectura
    IEnumerable<DirectorioActivo> GetDirectorioActivo();
    IEnumerable<Vinculados> GetRetirosVinculados();
    IEnumerable<Terceros> GetRetirosTerceros();


}
