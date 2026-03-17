using BaseDatoSqLite.Conexion;
using Dapper;
using Data.Interfaces.CapaApp.Uam3.IDataInsumosRobot;
using Microsoft.Data.Sqlite;
using System.Text;
using System.Text.RegularExpressions;

namespace Data.Repository.CapaAPP.Uam3Service.RepositoryDataRobot;

public class DataServicesRobot : IDataInsumosRobot
{
    private readonly ConnectionFactory _factory;

    public DataServicesRobot(ConnectionFactory factory)
    {
        _factory = factory;
    }

    public DateTime? ObtenerUltimaFechaCargue(string tabla)
    {
        using var connection = _factory.CreateConnection();

        var sql = $"SELECT MAX(FechaCargue) FROM {tabla};";
        var resultado = connection.ExecuteScalar<object>(sql);

        if (resultado == null || resultado == DBNull.Value)
            return null;

        if (resultado is DateTime dt)
            return dt;

        if (DateTime.TryParse(resultado.ToString(), out var fecha))
            return fecha;

        return null;
    }



    // metodos para los insumos robot capa app

    public void CreateAndBulkInsertDynamic(string tableName, string origen, List<string> columnNames, List<Dictionary<string, object?>> rows)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            connection.Execute($"DROP TABLE IF EXISTS {tableName};", transaction: transaction);

            var createTableScript = new StringBuilder();
            createTableScript.AppendLine($"CREATE TABLE {tableName} (");
            createTableScript.AppendLine("  Id INTEGER PRIMARY KEY AUTOINCREMENT,");
            createTableScript.AppendLine("  FechaCargue TEXT,");
            createTableScript.AppendLine("  Origen TEXT,");

            foreach (var col in columnNames)
            {
                createTableScript.AppendLine($"  [{col}] TEXT,");
            }

            createTableScript.Length -= 3; // Eliminar ",\r\n"
            createTableScript.AppendLine(");");

            connection.Execute(createTableScript.ToString(), transaction: transaction);

            var insertScript = new StringBuilder();
            insertScript.Append($"INSERT INTO {tableName} (FechaCargue, Origen");

            foreach (var col in columnNames)
            {
                insertScript.Append($", [{col}]");
            }

            insertScript.Append(") VALUES (@FechaCargue, @Origen");

            foreach (var col in columnNames)
            {
                insertScript.Append($", @{col}");
            }
            insertScript.Append(");");

            using var cmd = connection.CreateCommand();
            cmd.Transaction = (SqliteTransaction)transaction;
            cmd.CommandText = insertScript.ToString();

            var pFecha = cmd.CreateParameter(); pFecha.ParameterName = "@FechaCargue"; cmd.Parameters.Add(pFecha);
            var pOrigen = cmd.CreateParameter(); pOrigen.ParameterName = "@Origen"; cmd.Parameters.Add(pOrigen);

            var paramCache = new Dictionary<string, SqliteParameter>();

            foreach (var col in columnNames)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = $"@{col}";
                cmd.Parameters.Add(p);
                paramCache[col] = p;
            }

            var fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            foreach (var row in rows)
            {
                pFecha.Value = fechaActual;
                pOrigen.Value = origen;

                foreach (var col in columnNames)
                {
                    if (row.TryGetValue(col, out var val) && val != null)
                    {
                        paramCache[col].Value = val.ToString();
                    }
                    else
                    {
                        paramCache[col].Value = DBNull.Value;
                    }
                }

                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            Console.WriteLine($"Tabla {tableName} creada y cargada con {rows.Count} registros.");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new Exception($"Error creando tabla dinámica {tableName}: {ex.Message}");
        }
    }


    public IEnumerable<dynamic> ObtenerDatosDinamicos(string nombreTabla)
    {
        using var connection = _factory.CreateConnection();

        var tablaSanitizada = nombreTabla.Replace(";", "").Replace("--", "").Replace(" ", "");

        return connection.Query($"SELECT * FROM {tablaSanitizada}");
    }

    public IEnumerable<string> ObtenerColumnasTabla(string nombreTabla)
    {
        using var connection = _factory.CreateConnection();
        var reader = connection.ExecuteReader($"SELECT * FROM {nombreTabla} LIMIT 0");
        return Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
    }

    public (IEnumerable<dynamic> Datos, int TotalRegistros) ObtenerDatosPaginados(string nombreTabla, int skip, int take, string? orderBy, string? filter)
    {
        using var connection = _factory.CreateConnection();

        var nombreSanitizado = nombreTabla.Replace(";", "").Replace(" ", "");


        string whereClause = "";
        if (!string.IsNullOrEmpty(filter))
        {
            if (filter.IndexOf("LIKE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                whereClause = $" WHERE {filter}";
            }
            else
            {
                string sqlFilter = TraducirFiltroASql(filter);
                if (!string.IsNullOrWhiteSpace(sqlFilter))
                {
                    whereClause = $" WHERE {sqlFilter}";
                }
            }
        }

        var sqlCount = $"SELECT COUNT(*) FROM {nombreSanitizado} {whereClause}";

        int total = connection.ExecuteScalar<int>(sqlCount);

        var sb = new StringBuilder();
        sb.Append($"SELECT * FROM {nombreSanitizado} {whereClause}");

        if (!string.IsNullOrEmpty(orderBy))
        {
            sb.Append($" ORDER BY {orderBy}");
        }
        else
        {
            sb.Append(" ORDER BY rowid DESC");
        }

        sb.Append($" LIMIT {take} OFFSET {skip}");

        var datos = connection.Query(sb.ToString());

        return (datos, total);
    }

    private string TraducirFiltroASql(string filter)
    {
        if (string.IsNullOrEmpty(filter)) return "";

        try
        {
            // Log para debug
            Console.WriteLine($"[DEBUG] Filtro original: {filter}");

            string sql = filter;

            // 1. Eliminar referencias al iterador "it" si aparecen
            // Convierte it["Columna"] a Columna
            sql = Regex.Replace(sql, @"it\[[""']([^""']+)[""']\]", "$1", RegexOptions.IgnoreCase);

            // 2. Eliminar operadores ternarios de protección de nulos
            // (campo == null ? "" : campo) -> campo
            sql = Regex.Replace(sql, @"\((\w+)\s*==\s*null\s*\?\s*[""'][""']\s*:\s*\1\)", "$1", RegexOptions.IgnoreCase);

            // 3. Eliminar llamadas a métodos de conversión
            sql = Regex.Replace(sql, @"\.ToString\(\)", "", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\.ToLower\(\)", "", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\.ToUpper\(\)", "", RegexOptions.IgnoreCase);

            // 4. Traducir Contains -> LIKE (insensible a mayúsculas con LOWER)
            // Patrón: campo.Contains("valor")
            sql = Regex.Replace(sql,
                @"(\w+)\.Contains\([""']([^""']*)[""']\)",
                "LOWER([$1]) LIKE LOWER('%$2%')",
                RegexOptions.IgnoreCase);

            // 5. Traducir StartsWith -> LIKE
            sql = Regex.Replace(sql,
                @"(\w+)\.StartsWith\([""']([^""']*)[""']\)",
                "LOWER([$1]) LIKE LOWER('$2%')",
                RegexOptions.IgnoreCase);

            // 6. Traducir EndsWith -> LIKE
            sql = Regex.Replace(sql,
                @"(\w+)\.EndsWith\([""']([^""']*)[""']\)",
                "LOWER([$1]) LIKE LOWER('%$2')",
                RegexOptions.IgnoreCase);

            // 7. Traducir operadores lógicos
            sql = Regex.Replace(sql, @"\s+AndAlso\s+", " AND ", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\s+OrElse\s+", " OR ", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\s+&&\s+", " AND ", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\s+\|\|\s+", " OR ", RegexOptions.IgnoreCase);

            // 8. Traducir operadores de comparación
            sql = Regex.Replace(sql, @"\s*==\s*", " = ", RegexOptions.IgnoreCase);
            sql = Regex.Replace(sql, @"\s*!=\s*", " <> ", RegexOptions.IgnoreCase);

            // 9. Reemplazar comillas dobles por simples (SQLite prefiere simples)
            sql = sql.Replace("\"", "'");

            // 10. Envolver nombres de columnas que no estén ya entre corchetes
            // Solo si no están dentro de funciones LOWER()
            sql = Regex.Replace(sql,
                @"(?<!LOWER\()\b([A-Z_][A-Z0-9_]*)\b(?!\()",
                "[$1]",
                RegexOptions.IgnoreCase);

            Console.WriteLine($"[DEBUG] Filtro traducido: {sql}");

            return sql;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Traducción de filtro: {ex.Message}");
            // En caso de error, retorna vacío para que no rompa la consulta
            return "";
        }
    }

}
