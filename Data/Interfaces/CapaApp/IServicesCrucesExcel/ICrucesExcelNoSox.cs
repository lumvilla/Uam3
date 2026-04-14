using Shared.CapaAplicacion.UserDisable;

namespace Data.Interfaces.CapaApp.IServicesCrucesExcel;

public interface ICrucesExcelNoSox
{
    // Grupo A - LOGIN + ESTADO IN ('OPEN','EXPIRED','EXPIRED(GRACE)')
    Task<IEnumerable<UsuarioDisableDto>> UsuariosGtcDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosOpenEdatelDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosOpenEtpDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosOsmDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosMssDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosTigoGestionDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosViafirmaDisable();

    // Grupo B - USER_NAME + DISABLE_ACCOUNT = 'Enabled'
    Task<IEnumerable<UsuarioDisableDto>> UsuariosMaebog1Disable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosMaebog2Disable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosMaebaqDisable();
    Task<IEnumerable<UsuarioDisableDto>> UsuariosMaecoreDisable();

    // Grupo C - LOGIN + ESTADO = 0
    Task<IEnumerable<UsuarioDisableDto>> UsuariosServiceDeskDisable();

    string GetQueryNoSoxGrupoA(string tabla);
    string GetQueryNoSoxGrupoB(string tabla);
    string GetQueryNoSoxGrupoC(string tabla);
}
