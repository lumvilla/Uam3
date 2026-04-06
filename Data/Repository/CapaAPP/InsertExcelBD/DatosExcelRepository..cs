using BaseDatoSqLite.Conexion;
using Dapper;
using Data.Interfaces.CapaApp.InsertExcelBD;
using Microsoft.Data.Sqlite;
using Shared;
using Shared.CapaAplicacion.DA;
using Shared.CapaAplicacion.Interfaces;
using Shared.CapaAplicacion.Portal;
using Shared.CapaAplicacion.Terceros;
using Shared.CapaAplicacion.Vinculados;
using System.Text;
using System.Text.RegularExpressions;

namespace Data.Repository.CapaAPP.InsertExcelBD;

public class DatosExcelRepository : IDatosExcelRepository
{
    private readonly ConnectionFactory _factory;

    public DatosExcelRepository(ConnectionFactory factory)
    {
        _factory = factory;
    }

    public void InsertVinculadosBulk(IEnumerable<Vinculados> listaVinculados)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();


        connection.Execute("DELETE FROM vinculados;", transaction: transaction);

        var sql = @"
                    INSERT INTO vinculados 
                    ( fecha_cargue, origen, fecha_retiro, nombre, cedula, company)
                    VALUES ( @FechaCargue, @Origen, @FechaRetiro, @Nombre, @Cedula, @Company);
                ";

        var parametros = listaVinculados.Select(v => new
        {
            FechaCargue = v.FechaCargue.ToString("yyyy-MM-dd HH:mm:ss"), 
            v.Origen,
            v.FechaRetiro,
            v.Nombre,
            v.Cedula,
            v.Company
        });

        int filas = connection.Execute(sql, parametros, transaction);



        transaction.Commit();

        Console.WriteLine($" Se insertaron {filas} filas en la tabla vinculado.");
    }

    public void InsertDABulk(IEnumerable<DirectorioActivo> listaDA)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();


        connection.Execute("DELETE FROM directorio_activo;", transaction: transaction);

        var sql = @"
                    INSERT INTO directorio_activo 
                    (fecha_cargue, origen, login, identificacion, nombre_completo, Estado)
                    VALUES (@FechaCargue, @Origen, @Login, @Identificacion, @NombreCompleto, @Estado);
                ";

        var parametros = listaDA.Select(d => new
        {
            FechaCargue = d.FechaCargue.ToString("yyyy-MM-dd HH:mm:ss"),
            d.Origen,
            d.Login,
            d.Identificacion,
            d.NombreCompleto,
            d.Estado
        });

        int filas = connection.Execute(sql, parametros, transaction);



        transaction.Commit();

        Console.WriteLine($" Se insertaron {filas} filas en la tabla DirectorioActivo.");
    }


    public void InsertTercerosBulk(IEnumerable<Terceros> listaTerceros)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        connection.Execute("DELETE FROM terceros;", transaction: transaction);

        var sql = @" 
                    INSERT INTO terceros (fecha_cargue, origen, login, estado_entidad, nombre_completo, cedula, fecha_retiro)
                    VALUES (@FechaCargue, @Origen, @Login, @EstadoEntidad, @NombreCompleto, @Cedula, @FechaRetiro);
                ";


        connection.Execute(sql, listaTerceros, transaction);

        transaction.Commit();
    }


    public void BulkInsertPortal(IEnumerable<Portal> portales)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        connection.Execute("DELETE FROM portal;", transaction: transaction);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
                            INSERT INTO portal (fecha_cargue, origen, nombre, cedula, login)
                            VALUES ($FechaCargue, $Origen, $Nombre, $Cedula, $Login);
                        ";


        var fechaParam = cmd.CreateParameter();
        fechaParam.ParameterName = "$FechaCargue";
        cmd.Parameters.Add(fechaParam);

        var origenParam = cmd.CreateParameter();
        origenParam.ParameterName = "$Origen";
        cmd.Parameters.Add(origenParam);

        var nombreParam = cmd.CreateParameter();
        nombreParam.ParameterName = "$Nombre";
        cmd.Parameters.Add(nombreParam);

        var cedulaParam = cmd.CreateParameter();
        cedulaParam.ParameterName = "$Cedula";
        cmd.Parameters.Add(cedulaParam);

        var loginParam = cmd.CreateParameter();
        loginParam.ParameterName = "$Login";
        cmd.Parameters.Add(loginParam);

        foreach (var portal in portales)
        {
            fechaParam.Value = portal.FechaCargue;
            origenParam.Value = portal.Origen;
            nombreParam.Value = portal.Nombre;
            cedulaParam.Value = portal.Cedula;
            loginParam.Value = portal.Login;

            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }



    // este metodo esta por ahora solo para app de consola y lo voy a quitar despues

    public void BulkInsertDatoCapaApp<T>(IEnumerable<T> registros, string nombreTabla) where T : IRegistros
    {
        using var connection = _factory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();
        using var cmd = connection.CreateCommand();

        connection.Execute($"DELETE FROM {nombreTabla};", transaction: transaction);

        cmd.CommandText = $@"
            INSERT INTO {nombreTabla} (fecha_cargue, origen, datos_json)
            VALUES ($FechaCargue, $Origen, $DatosJson);
        ";

        var fechaParam = cmd.CreateParameter();
        fechaParam.ParameterName = "$FechaCargue";
        cmd.Parameters.Add(fechaParam);

        var origenParam = cmd.CreateParameter();
        origenParam.ParameterName = "$Origen";
        cmd.Parameters.Add(origenParam);

        var datosParam = cmd.CreateParameter();
        datosParam.ParameterName = "$DatosJson";
        cmd.Parameters.Add(datosParam);

        foreach (var registro in registros)
        {
            fechaParam.Value = registro.FechaCargue;
            origenParam.Value = registro.Origen ?? string.Empty;
            datosParam.Value = registro.DatosJson ?? string.Empty;

            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

   




    public IEnumerable<DirectorioActivo> GetDirectorioActivo()
    {
        using var connection = _factory.CreateConnection();
        // Nota: Asegúrate de que los nombres de las columnas en la tabla coincidan con las propiedades del modelo (o usa aliases).
        var sql = @"
                    SELECT 
                        fecha_cargue    AS FechaCargue,
                        origen          AS Origen,
                        login           AS Login,
                        estado  AS Estado,
                        nombre_completo AS NombreCompleto,
                        identificacion          AS Identificacion
                    FROM directorio_activo;";

        return connection.Query<DirectorioActivo>(sql);
    }

    public IEnumerable<Vinculados> GetRetirosVinculados()
    {
        using var connection = _factory.CreateConnection();

        var sql = @"
                SELECT 
                    id_vinculado    AS IdVinculado,
                    fecha_cargue    AS FechaCargue,
                    origen          AS Origen,
                    company         AS Company,
                    nombre          AS Nombre,
                    cedula          AS Cedula,
                    COALESCE(
                        strftime('%d/%m/%Y', fecha_retiro),
                        substr(fecha_retiro, 1, 10)
                    )               AS FechaRetiro
                FROM vinculados;";

        return connection.Query<Vinculados>(sql);
    }

    public IEnumerable<Terceros> GetRetirosTerceros()
    {
        using var connection = _factory.CreateConnection();

        var sql = @"
                    SELECT 
                        id_tercero      AS IdTerceros,
                        fecha_cargue    AS FechaCargue,
                        origen          AS Origen,
                        login           AS Login,
                        estado_entidad  AS EstadoEntidad,
                        nombre_completo AS NombreCompleto,
                        cedula          AS Cedula,
                        fecha_retiro    AS FechaRetiro
                    FROM terceros;";

        return connection.Query<Terceros>(sql);
    }



}
