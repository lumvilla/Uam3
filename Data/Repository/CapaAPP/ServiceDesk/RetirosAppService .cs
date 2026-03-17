using BaseDatoSqLite.Conexion;
using Dapper;
using Data.Interfaces.CapaApp.IServiceDesk;
using Shared.CapaAplicacion.UserDisable;

namespace Data.Repository.CapaAPP.ServiceDesk;

public class RetirosAppService : IRetirosAppService
{

    private readonly ConnectionFactory _factory;

    public RetirosAppService(ConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<RetirosUserApp>> ObtenerDatosTemporales()
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT 
                id_retiro_app AS IdRetiroApp,
                fecha_ejecucion AS FechaEjecucion,
                app AS App,
                cedula_app AS CedulaApp,
                cedula_consolidado AS CedulaConsolidado,
                login_consolidado AS LoginConsolidado,
                login_app AS LoginApp,
                nombre_consolidado AS NombreConsolidado,
                estado_app AS EstadoApp,
                estado_consolidado AS EstadoConsolidado,
                fecha_retiro AS FechaRetiro,
                tipo_cruce AS TipoCruce,
                numero_oc AS NumeroOc,
                fecha_oc AS FechaOc,
                estado_oc AS EstadoOc,
                validacion_estado AS ValidacionEstado
            FROM retiro_app_temp
            ORDER BY app, fecha_retiro DESC";

        var result = await connection.QueryAsync<RetirosUserApp>(sql);
        return result;
    }

    public async Task<bool> GenerarOrdenCambio(string app, string Fecha)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var countSql = "SELECT COUNT(*) FROM retiro_app_temp";
            var count = await connection.ExecuteScalarAsync<int>(countSql, transaction: transaction);

            if (count == 0)
                return false;

            var numeroOc = GenerarNumeroOC();
            var fechaOc = DateTime.Now;

            var createTableSql = @"
                select * from retiro_apps
                )";

            await connection.ExecuteAsync(createTableSql, transaction: transaction);

            var insertSql = @"
                INSERT INTO retiro_apps (
                    fecha_ejecucion, app, cedula_app, cedula_consolidado, login_consolidado, 
                    login_app, nombre_consolidado, estado_app, estado_consolidado, fecha_retiro, 
                    tipo_cruce, numero_oc, fecha_oc, estado_oc, validacion_estado
                )
                SELECT 
                    fecha_ejecucion, app, cedula_app, cedula_consolidado, login_consolidado, 
                    login_app, nombre_consolidado, estado_app, estado_consolidado, fecha_retiro, 
                    tipo_cruce, @NumeroOc, @FechaOc, 'PENDIENTE', NULL
                FROM retiro_app_temp";

            await connection.ExecuteAsync(insertSql, new { NumeroOc = numeroOc, FechaOc = fechaOc.ToString("yyyy-MM-dd") }, transaction);

            var deleteSql = $"DELETE FROM retiro_app_temp where app = {app}";
            await connection.ExecuteAsync(deleteSql, transaction: transaction);

            transaction.Commit();
            return true;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    private string GenerarNumeroOC()
    {
        // este metodo lo hare para selenium
        var ahora = DateTime.Now;
        return $"OC-{ahora:yyyyMMdd}-{ahora:HHmmss}";
    }


    public async Task<bool> GuardarOCManual(
     string app,
     string fechaSeleccionada,
     string numeroOc,
     string estadoOc,
     string fechaOC)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            var updateTempSql = @"
            UPDATE retiro_app_temp
            SET numero_oc = @NumeroOc,
                estado_oc = @EstadoOc,
                fecha_oc = @FechaOc
            WHERE app = @App
              AND fecha_retiro = @FechaSeleccionada";

            await connection.ExecuteAsync(updateTempSql, new
            {
                NumeroOc = numeroOc,
                EstadoOc = estadoOc,
                FechaOc = fechaOC,
                App = app,
                FechaSeleccionada = fechaSeleccionada
            }, transaction);

            var insertFinalSql = @"
            INSERT INTO retiro_apps (
                fecha_ejecucion,
                app,
                cedula_app,
                cedula_consolidado,
                login_consolidado,
                login_app,
                nombre_consolidado,
                estado_app,
                estado_consolidado,
                fecha_retiro,
                tipo_cruce,
                numero_oc,
                fecha_oc,
                estado_oc,
                validacion_estado
            )
            SELECT
                fecha_ejecucion,
                app,
                cedula_app,
                cedula_consolidado,
                login_consolidado,
                login_app,
                nombre_consolidado,
                estado_app,
                estado_consolidado,
                fecha_retiro,
                tipo_cruce,
                numero_oc,
                fecha_oc,
                estado_oc,
                validacion_estado
            FROM retiro_app_temp
            WHERE app = @App
              AND fecha_retiro = @FechaSeleccionada";

            await connection.ExecuteAsync(insertFinalSql, new
            {
                App = app,
                FechaSeleccionada = fechaSeleccionada
            }, transaction);

            var deleteTempSql = @"
            DELETE FROM retiro_app_temp
            WHERE app = @App
              AND fecha_retiro = @FechaSeleccionada";

            await connection.ExecuteAsync(deleteTempSql, new
            {
                App = app,
                FechaSeleccionada = fechaSeleccionada
            }, transaction);

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


}
