namespace Data.Interfaces.CapaApp.Uam3.IDataInsumosRobot
{
    public interface IDataInsumosRobot
    {
        DateTime? ObtenerUltimaFechaCargue(string tabla);

        void CreateAndBulkInsertDynamic(string tableName, string origen, List<string> columnNames, List<Dictionary<string, object?>> rows);

        // NUEVO: Obtener datos dinámicos de una tabla
        IEnumerable<dynamic> ObtenerDatosDinamicos(string nombreTabla);
        IEnumerable<string> ObtenerColumnasTabla(string nombreTabla);

        // 2. Obtener datos paginados (Skip/Take) y el total de registros
        (IEnumerable<dynamic> Datos, int TotalRegistros) ObtenerDatosPaginados(string nombreTabla, int skip, int take, string? orderBy, string? filter);



    }
}
