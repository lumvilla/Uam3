using Shared.CapaAplicacion.UserDisable;

namespace Data.Interfaces.CapaApp.IUserRetiros;

public interface IRetirosUser
{
    //capa app
    Task GuardarUsuariosRetirosApp(IEnumerable<RetirosUserApp> usuarios);


    //capa bd

}
