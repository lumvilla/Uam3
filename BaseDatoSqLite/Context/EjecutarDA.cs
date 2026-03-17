//using BaseDatoSqLite.Conexion;
//using System.DirectoryServices;
//using Microsoft.Data.Sqlite;
//using Shared.CapaAplicacion.DA;
//using System.Reflection.PortableExecutable;

//namespace BaseDatoSqLite.Context;

//public class EjecutarDA
//{
//    private readonly ConnectionFactory _factory;

//    public EjecutarDA(ConnectionFactory factory)
//    {
//        _factory = factory;

//    }

//    public async Task EjecutarCargaAsync()
//    {
//        var usuarios = new List<DirectorioActivo>();

//        // 🔹 1. Conexión LDAP
//        using var entry = new DirectoryEntry("LDAP://dc=epmtelco,dc=com,dc=co");
//        using var searcher = new DirectorySearcher(entry)
//        {
//            Filter = "(objectClass=user)",
//            PageSize = 1000,
//            PropertiesToLoad =
//                {
//                    "sAMAccountName", "employeeId", "name",
//                    "userAccountControl", "lastLogonTimeStamp", "accountExpires"
//                }
//        };

//        foreach (SearchResult result in searcher.FindAll())
//        {
//            try
//            {
//                string login = result.Properties["sAMAccountName"].Count > 0
//                    ? result.Properties["sAMAccountName"][0].ToString()!
//                    : "";

//                if (string.IsNullOrEmpty(login) || login.EndsWith("$"))
//                    continue; // saltar cuentas de equipos

//                var dto = new DirectorioActivo
//                {
//                    Login = login,
//                    Identificacion = result.Properties["employeeId"].Count > 0
//                        ? result.Properties["employeeId"][0].ToString()
//                        : null,
//                    NombreCompleto = result.Properties["name"].Count > 0
//                        ? result.Properties["name"][0].ToString()
//                        : null,
//                    Estado = ObtenerEstado(result),
//                    UltimoLogon = ObtenerFecha(result, "lastLogonTimeStamp"),
//                    Expira = ObtenerFecha(result, "accountExpires"),
//                    Origen = "ActiveDirectory"
//                };

//                usuarios.Add(dto);
//            }
//            catch
//            {
//                // ignorar errores de parsing individuales
//            }
//        }

//        // 🔹 2. Inserción masiva a SQLite
//        await InsertarMasivoAsync(usuarios);
//    }

//    private static string ObtenerEstado(SearchResult result)
//    {
//        if (result.Properties["userAccountControl"].Count > 0)
//        {
//            int uac = (int)result.Properties["userAccountControl"][0];
//            const int ADS_UF_ACCOUNTDISABLE = 0x0002;
//            return (uac & ADS_UF_ACCOUNTDISABLE) != 0 ? "DESHABILITADO" : "ACTIVO";
//        }
//        return "ACTIVO";
//    }

//    private static DateTime? ObtenerFecha(SearchResult result, string field)
//    {
//        if (result.Properties[field].Count == 0) return null;
//        long fileTime = (long)result.Properties[field][0];
//        if (fileTime <= 0) return null;
//        return DateTime.FromFileTimeUtc(fileTime);
//    }

//    private async Task InsertarMasivoAsync(List<DirectorioActivo> usuarios)
//    {
//        using var connection = new SqliteConnection(_factory);
//        await connection.OpenAsync();

//        using var transaction = await connection.BeginTransactionAsync();
//        using var cmd = connection.CreateCommand();

//        cmd.CommandText = @"
//                INSERT INTO directorio_activo
//                (FechaCargue, Origen, Login, Identificacion, NombreCompleto, Estado, UltimoLogon, Expira)
//                VALUES ($FechaCargue, $Origen, $Login, $Identificacion, $NombreCompleto, $Estado, $UltimoLogon, $Expira);";

//        foreach (var u in usuarios)
//        {
//            cmd.Parameters.Clear();
//            cmd.Parameters.AddWithValue("$FechaCargue", u.FechaCargue);
//            cmd.Parameters.AddWithValue("$Origen", u.Origen ?? (object)DBNull.Value);
//            cmd.Parameters.AddWithValue("$Login", u.Login);
//            cmd.Parameters.AddWithValue("$Identificacion", u.Identificacion ?? (object)DBNull.Value);
//            cmd.Parameters.AddWithValue("$NombreCompleto", u.NombreCompleto ?? (object)DBNull.Value);
//            cmd.Parameters.AddWithValue("$Estado", u.Estado ?? (object)DBNull.Value);
//            cmd.Parameters.AddWithValue("$UltimoLogon", u.UltimoLogon?.ToString("o") ?? (object)DBNull.Value);
//            cmd.Parameters.AddWithValue("$Expira", u.Expira?.ToString("o") ?? (object)DBNull.Value);

//            await cmd.ExecuteNonQueryAsync();
//        }

//        await transaction.CommitAsync();
//    }




//}
//}
