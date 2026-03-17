using BaseDatoSqLite.Conexion;
using BaseDatoSqLite.Context;
using Data.Interfaces.CapaApp.InsertExcelBD;
using Data.Interfaces.CapaApp.IServicesCrucesExcel;
using Data.Repository.CapaAPP.InsertExcelBD;
using Data.Repository.CapaAPP.ServicesCrucesExcel;
using Helper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using RpaServiceDesk.IniciarSesionSD;
using RpaServiceDesk.ValidarEstadoOc;
using Shared;
using System.IO;

namespace ControleSoxUI.Services
{
    public class AppInitializer
    {
        public IConfiguration Configuration { get; }
        public ConnectionFactory Factory { get; }
        public SqliteDbContext DbContext { get; }
        public FileLogger Logger { get; }

        public AppInitializer()
        {
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true)
                .Build();

            string connString = Configuration.GetConnectionString("SQLite") ?? throw new Exception("Falta ConnectionStrings:SQLite en appsettings.json");
            Factory = new ConnectionFactory(connString);
            DbContext = new SqliteDbContext(Factory);
            DbContext.Initialize();

            string rutaLogs = Configuration["ConfiguracionGeneral:RutaLogs"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            Logger = new FileLogger(rutaLogs);
        }
    }
}
