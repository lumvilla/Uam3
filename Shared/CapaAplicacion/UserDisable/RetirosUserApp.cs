namespace Shared.CapaAplicacion.UserDisable;

public class RetirosUserApp
{
    public int Id_RetiroApp { get; set; }
    public DateTime FechaEjecucion { get; set; } = DateTime.Now;
    public string App { get; set; } = string.Empty;
    public string CedulaApp { get; set; } = string.Empty;
    public string CedulaConsolidado { get; set; } = string.Empty;
    public string LoginConsolidado { get; set; } = string.Empty;
    public string LoginApp { get; set; } = string.Empty;
    public string? NombreConsolidado { get; set; }
    public string FechaRetiro { get; set; } = string.Empty;
    public string? EstadoConsolidado { get; set; }
    public string? EstadoApp { get; set; }
    public string? NumeroOc { get; set; }
    public string? FechaOc { get; set; }
    public string? EstadoOc { get; set; }
    public string? ValidacionEstado { get; set; }
    public string? TipoCruce { get; set; }

    // Estado actual en el insumo (para detectar reingresos)
    public string? EstadoActualApp { get; set; }

    // Estado actual en el consolidado
    public string? EstadoActualConsolidado { get; set; }
}
