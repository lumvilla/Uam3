using Shared.CapaAplicacion.UserDisable;

namespace Data.Interfaces.CapaApp.IServicesCrucesExcel;

public interface ICrucesExcelBD
{
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdSiebelDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdPortalDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdDwhFijoDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdDwhMovilDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdFenixDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdOpenDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdSapErpDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdSapGrcDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdIamDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdOrionDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdTheDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdCbsDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosBdCmDisable();

    string GetQueryBD(string tabla);
}
