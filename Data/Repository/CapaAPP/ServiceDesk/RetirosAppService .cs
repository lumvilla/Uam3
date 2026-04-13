using BaseDatoSqLite.Conexion;
using Dapper;
using Data.Interfaces.CapaApp.IServiceDesk;
using Shared.CapaAplicacion.UserDisable;

namespace Data.Repository.CapaAPP.ServiceDesk;

public class RetirosAppService : IRetirosAppService
{
    private readonly ConnectionFactory _factory;

    public RetirosAppService(ConnectionFactory factory) => _factory = factory;

    // ── Obtener datos temporales ──────────────────────────────────────
    public async Task<IEnumerable<RetirosUserApp>> ObtenerDatosTemporales()
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT 
                id_retiro_app       AS Id_RetiroApp,
                fecha_ejecucion     AS FechaEjecucion,
                app                 AS App,
                cedula_app          AS CedulaApp,
                cedula_consolidado  AS CedulaConsolidado,
                login_consolidado   AS LoginConsolidado,
                login_app           AS LoginApp,
                nombre_consolidado  AS NombreConsolidado,
                estado_app          AS EstadoApp,
                estado_consolidado  AS EstadoConsolidado,
                fecha_retiro        AS FechaRetiro,
                tipo_cruce          AS TipoCruce,
                numero_oc           AS NumeroOc,
                fecha_oc            AS FechaOc,
                estado_oc           AS EstadoOc,
                validacion_estado   AS ValidacionEstado
            FROM retiro_app_temp
            ORDER BY app, fecha_retiro DESC";

        return await connection.QueryAsync<RetirosUserApp>(sql);
    }

    // ── Generar OC: marca como PENDIENTE (el robot o usuario asigna el número luego) ──
    public async Task<bool> GenerarOrdenCambio(string app, string fecha)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Verificar que hay registros para esa app y fecha
            var countSql = @"
                SELECT COUNT(*) FROM retiro_app_temp
                WHERE app = @App COLLATE NOCASE
                  AND DATE(fecha_ejecucion) = DATE(@Fecha)
                  AND (estado_oc IS NULL OR estado_oc = '')";

            var count = await connection.ExecuteScalarAsync<int>(countSql,
                new { App = app, Fecha = fecha }, transaction);

            if (count == 0)
            {
                transaction.Rollback();
                return false;
            }

            // Marcar como PENDIENTE — el número de OC lo asigna el robot o CargaManualOc
            var updateSql = @"
                UPDATE retiro_app_temp
                SET estado_oc = 'PENDIENTE'
                WHERE app = @App COLLATE NOCASE
                  AND DATE(fecha_ejecucion) = DATE(@Fecha)
                  AND (estado_oc IS NULL OR estado_oc = '')";

            await connection.ExecuteAsync(updateSql,
                new { App = app, Fecha = fecha }, transaction);

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // ── Guardar OC Manual: asigna número OC y mueve a histórico ──────
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
            // 1. Actualizar numero_oc, estado_oc y fecha_oc en temp
            var updateSql = @"
                UPDATE retiro_app_temp
                SET numero_oc = @NumeroOc,
                    estado_oc = @EstadoOc,
                    fecha_oc  = @FechaOc
                WHERE app = @App COLLATE NOCASE
                  AND DATE(fecha_ejecucion) = DATE(@FechaSeleccionada)";

            var rowsUpdated = await connection.ExecuteAsync(updateSql, new
            {
                NumeroOc = numeroOc,
                EstadoOc = estadoOc,
                FechaOc = fechaOC,
                App = app,
                FechaSeleccionada = fechaSeleccionada
            }, transaction);

            if (rowsUpdated == 0)
            {
                transaction.Rollback();
                return false;
            }

            // 2. Mover a retiro_app (histórico definitivo)
            var insertSql = @"
                INSERT INTO retiro_app (
                    fecha_ejecucion, app, cedula_app, cedula_consolidado,
                    login_consolidado, login_app, nombre_consolidado,
                    estado_app, estado_consolidado, fecha_retiro,
                    tipo_cruce, numero_oc, fecha_oc, estado_oc, validacion_estado
                )
                SELECT
                    fecha_ejecucion, app, cedula_app, cedula_consolidado,
                    login_consolidado, login_app, nombre_consolidado,
                    estado_app, estado_consolidado, fecha_retiro,
                    tipo_cruce, numero_oc, fecha_oc, estado_oc, validacion_estado
                FROM retiro_app_temp
                WHERE app = @App COLLATE NOCASE
                  AND DATE(fecha_ejecucion) = DATE(@FechaSeleccionada)";

            await connection.ExecuteAsync(insertSql, new
            {
                App = app,
                FechaSeleccionada = fechaSeleccionada
            }, transaction);

            // 3. Eliminar de temp
            var deleteSql = @"
                DELETE FROM retiro_app_temp
                WHERE app = @App COLLATE NOCASE
                  AND DATE(fecha_ejecucion) = DATE(@FechaSeleccionada)";

            await connection.ExecuteAsync(deleteSql, new
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
