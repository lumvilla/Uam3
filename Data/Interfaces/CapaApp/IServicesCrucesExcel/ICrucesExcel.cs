using Shared.CapaAplicacion.Portal;
using Shared.CapaAplicacion.UserDisable;

namespace Data.Interfaces.CapaApp.IServicesCrucesExcel;

public interface ICrucesExcel
{
    Task<IEnumerable<UsuarioDisableDto>> UsuariosPortalDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosDADisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosSiebelFijoDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosSiebelMovilDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosFenixDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosOpenDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosDwhMovilDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosDwhFijoDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosSapErpDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosSapGrcDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosIamDisable();

    // Nuevos sistemas
    Task<IEnumerable<UsuarioDisableDto>> UsuariosCbsDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosCmDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosElkDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosPcrfDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBigDataDisable();

    // Queries existentes
    string GetQueryPortal();
    string GetQueryDA();
    string GetQuerySiebelMovil();
    string GetQuerySiebelFijo();
    string GetQueryFenix();
    string GetQueryOpen();
    string GetQueryDwhMovil();
    string GetQueryDwhFijo();
    string GetQuerySapErp();
    string GetQuerySapGrc();
    string GetQueryIam();

    // Queries nuevas
    string GetQueryCbs();
    string GetQueryCm();
    string GetQueryElk();
    string GetQueryPcrf();
    string GetQueryBigData();
}
