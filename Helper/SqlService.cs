using Dapper;
using System.Diagnostics;
using BaseDatoSqLite.Conexion;
using Microsoft.Data.Sqlite;

namespace Helper
{
    public class SqlService
    {
        private readonly ConnectionFactory _factory;

        public SqlService(ConnectionFactory factory)
        {
            _factory = factory;
        }


        public class SqlExecutionResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public int RecordsAffected { get; set; }
            public double ExecutionTimeMs { get; set; }

            // Listas para las columnas y datos dinámicos
            public List<string>? ColumnNames { get; set; }
            public List<IDictionary<string, object>>? Data { get; set; }
        }

        public async Task<SqlExecutionResult> ExecuteSqlAsync(string sql)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new SqlExecutionResult();

            if (string.IsNullOrWhiteSpace(sql))
            {
                result.Success = false;
                result.Message = "La consulta está vacía.";
                return result;
            }

            try
            {
                using var connection = _factory.CreateConnection();
                connection.Open();

                var cleanSql = sql.Trim();

                // Detectamos si es una consulta que devuelve datos
                if (cleanSql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                    cleanSql.StartsWith("PRAGMA", StringComparison.OrdinalIgnoreCase) ||
                    cleanSql.StartsWith("WITH", StringComparison.OrdinalIgnoreCase) ||
                    cleanSql.StartsWith("EXPLAIN", StringComparison.OrdinalIgnoreCase))
                {
                    // Ejecutamos la consulta
                    var rows = await connection.QueryAsync(cleanSql);
                    var rowsList = rows.ToList();

                    result.Success = true;
                    result.RecordsAffected = -1;

                    if (rowsList.Count > 0)
                    {
                        // 1. Obtener las columnas de la primera fila
                        // Dapper devuelve DapperRow, que implementa IDictionary
                        var firstRow = (IDictionary<string, object>)rowsList[0];
                        result.ColumnNames = firstRow.Keys.ToList();

                        // 2. MATERIALIZACIÓN SEGURA (Esta es la parte crítica que te falta)
                        // En lugar de castear, creamos nuevos diccionarios limpios.
                        // Esto elimina tipos extraños de Dapper o DBNulls que rompen la vista.
                        var safeData = new List<IDictionary<string, object>>();

                        foreach (var row in rowsList)
                        {
                            var rowDict = (IDictionary<string, object>)row;
                            var newDict = new Dictionary<string, object>();

                            foreach (var key in result.ColumnNames)
                            {
                                // Obtenemos el valor
                                var val = rowDict[key];

                                // Convertimos todo a string o null explícito.
                                // Esto garantiza que el Grid de Blazor pueda leerlo sin errores.
                                newDict[key] = val?.ToString() ?? null;
                            }
                            safeData.Add(newDict);
                        }

                        result.Data = safeData;
                    }
                    else
                    {
                        result.ColumnNames = new List<string>();
                        result.Data = new List<IDictionary<string, object>>();
                    }
                }
                else
                {
                    // INSERT, UPDATE, DELETE, CREATE, DROP
                    var affected = await connection.ExecuteAsync(cleanSql);
                    result.Success = true;
                    result.RecordsAffected = affected;
                    result.Message = $"Comando ejecutado exitosamente. Filas afectadas: {affected}";
                }
            }
            catch (SqliteException ex)
            {
                result.Success = false;
                result.Message = $"Error de SQLite ({ex.SqliteErrorCode}): {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                // Logueamos en consola del servidor para que veas el error real si ocurre
                Console.WriteLine($"ERROR SQL SERVICE: {ex}");
                result.Message = $"Error General: {ex.Message}";
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            }

            return result;
        }
    }
}