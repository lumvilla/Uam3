using BaseDatoSqLite.Conexion;
using BaseDatoSqLite.Context;
using Data.Interfaces.CapaApp.ICargueInsumosRetiros;
using Data.Interfaces.CapaApp.InsertExcelBD;
using Data.Interfaces.CapaApp.IServiceDesk;
using Data.Interfaces.CapaApp.IServicesCrucesExcel;
using Data.Interfaces.CapaApp.Uam3.IAppInsumosRobot;
using Data.Interfaces.CapaApp.Uam3.IDataInsumosRobot;
using Data.Repository.CapaAPP.InsertExcelBD;
using Data.Repository.CapaAPP.InsumosRetiros;
using Data.Repository.CapaAPP.ServiceDesk;
using Data.Repository.CapaAPP.ServicesCrucesExcel;
using Data.Repository.CapaAPP.Uam3Service.RepositoryDataRobot;
using Data.Repository.CapaAPP.Uam3Service.RepositoryInsumosRobot;
using Helper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using RetirosUIBlazor.Components;
using RpaServiceDesk.IniciarSesionSD;
using Shared;
using System.IO; // Necesario para Path.Combine y Directory
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Archivos (Similar a la consola, se usa el builder.Configuration)
var configuration = builder.Configuration;

// Mapeo de secciones (Asegúrate de tener appsettings.json en el proyecto Blazor)
builder.Services.Configure<RutaRobotOptions>(configuration.GetSection("RutaRobot"));
builder.Services.Configure<NombreArchivoRobotOptionsCapaAPP>(configuration.GetSection("NombreArchivoRobotCapaAPP"));

// 2. Rutas y ConnectionString (CORRECCIÓN CRÍTICA DE RUTA ABSOLUTA PARA SQLITE)
string connString = configuration.GetConnectionString("SQLite")
    ?? throw new Exception("Falta ConnectionStrings:SQLite en appsettings.json");

// --- INICIO DE CORRECCIÓN DE RUTA ABSOLUTA ---
var contentRoot = builder.Environment.ContentRootPath;

// 1. Extraer la ruta del archivo de la cadena de conexión (ej: "Data\BDUmg.db")
// Usamos Regex para ser robustos al formato "Data Source=ruta;otros_parametros"
string dbRelativePath = "";
var match = Regex.Match(connString, "Data Source=([^;]+)");
if (match.Success)
{
    dbRelativePath = match.Groups[1].Value.Trim();
}

if (string.IsNullOrWhiteSpace(dbRelativePath))
{
    throw new Exception("No se pudo extraer la ruta del archivo de la cadena de conexión SQLite.");
}

// 2. Construir la ruta ABSOLUTA
string absoluteDbPath = Path.Combine(contentRoot, dbRelativePath);

// Opcional: Asegurar que el directorio 'Data' exista antes de que la BD intente abrirlo.
string dataDirectory = Path.Combine(contentRoot, "Data");
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}

// 3. Recrear la cadena de conexión con la ruta absoluta
// Esto reemplaza la ruta relativa por la absoluta, manteniendo "Cache=Shared"
string absoluteConnString = connString.Replace(dbRelativePath, absoluteDbPath);
connString = absoluteConnString;
// --- FIN DE CORRECCIÓN DE RUTA ABSOLUTA ---

builder.Services.AddSingleton(new ConnectionFactory(connString));

// Asegurar la BD (Esta lógica es mejor en un servicio inicial o en el contexto, pero la ponemos aquí por simplicidad inicial)
var factory = new ConnectionFactory(connString);
var dbContext = new SqliteDbContext(factory);
// dbContext.Initialize() - En Blazor, esto podría ser parte de un servicio inicial o manejado en tiempo de ejecución.
// Por ahora, solo registramos los servicios.

// 3. Inicialización de Repositorios, Loggers, y Servicios de Negocio (Inyección de Dependencias)
var rutasRobot = configuration.GetSection("RutaRobot").Get<RutaRobotOptions>() ?? new RutaRobotOptions();
var nombresRobot = configuration.GetSection("NombreArchivoRobotCapaAPP").Get<NombreArchivoRobotOptionsCapaAPP>() ?? new NombreArchivoRobotOptionsCapaAPP();
string rutaLogs = configuration["ConfiguracionGeneral:RutaLogs"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Logs");

builder.Services.AddSingleton<Helper.ILogger>(new FileLogger(rutaLogs));

// Asegúrate de que IExcelImporter y ICrucesExcel usan sus interfaces correctas en el constructor.
builder.Services.AddScoped<IDatosExcelRepository, DatosExcelRepository>();
builder.Services.AddScoped<IDataInsumosRobot, DataServicesRobot>();
//builder.Services.AddScoped<IExcelImporter, ExcelImporter>();

// Crear una implementación de ICargueInsumos que use los valores de configuración
//builder.Services.AddScoped<ICargueInsumos, CargueInsumoService>(sp =>
//{
//    var importer = sp.GetRequiredService<IExcelImporter>();
//    var logger = sp.GetRequiredService<Helper.ILogger>();
//    return new CargueInsumoService(
//      importer: importer,
//      logger: logger,
//      rutaExcel: configuration["ConfiguracionGeneral:RutaArchivoVinculados"]!,
//      RutaDA: configuration["ConfiguracionGeneral:RutaArchivoDA"]!,
//      rutaCsv: configuration["ConfiguracionGeneral:RutaArchivoPortal"]!,
//      rutaTerceros: configuration["ConfiguracionGeneral:RutaArchivoTerceros"]!,
//      rutasRobot: rutasRobot,
//      nombresRobot: nombresRobot
//    );
//});

builder.Services.AddScoped<IRetirosInsumos, InsumosRetiroService>(sp =>
{
    var logger = sp.GetRequiredService<Helper.ILogger>();
    var repo = sp.GetRequiredService<IDatosExcelRepository>();
    return new InsumosRetiroService(
      logger: logger,
      repo: repo
    );
});
builder.Services.AddScoped<IAppInsumosRobot, InsumosRobotService>(sp =>
{
    var logger = sp.GetRequiredService<Helper.ILogger>();
    var repo = sp.GetRequiredService<IDataInsumosRobot>();
    return new InsumosRobotService(
      logger: logger,
      repo: repo,
      rutasRobot: rutasRobot,
      nombresRobot: nombresRobot
    );
});

// Registrar ICrucesExcel (asumiendo que tiene la misma lógica que la consola)
builder.Services.AddScoped<ICrucesExcel, CrucesUserDisable>();
builder.Services.AddScoped<ICrucesDisplayService, CrucesDisplayService>();
builder.Services.AddScoped<IRetirosAppService, RetirosAppService>();

builder.Services.AddScoped<SqlService>();
// Capa Base de Datos
builder.Services.AddScoped<ICrucesExcelBD, CrucesUserDisableBD>();
builder.Services.AddScoped<ICrucesDisplayServiceBD, CrucesDisplayServiceBD>();
builder.Services.AddScoped<IRetirosDbService, RetirosDbService>();

// Retiros No Sox
builder.Services.AddScoped<ICrucesExcelNoSox, CrucesUserDisableNoSox>();
builder.Services.AddScoped<ICrucesDisplayServiceNoSox, CrucesDisplayServiceNoSox>();
builder.Services.AddScoped<IRetirosNoSoxService, RetirosNoSoxService>();
// Registrar Radzen Blazor Components y DI
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

// ✅ CORRECTO - Configura el modo interactivo como predeterminado
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options => options.DetailedErrors = true) // <--- AGREGA ESTO
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
    });


// Si necesitas el objeto ServicesDesk (RPA) como Singleton/Scoped, regístralo también.
// OJO: Selenium (usado en ServicesDesk) NO ES RECOMENDABLE en Blazor Server por ser sincrónico y consumir muchos recursos de servidor.
builder.Services.AddScoped<ServicesDesk>(sp => new ServicesDesk(
  url: configuration["ServiceDesk:Url"]!, // ¡Usa configuración!
    usuario: configuration["ServiceDesk:Usuario"]!,
  password: configuration["ServiceDesk:Password"]!
));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// *Inicializacion de la BD*
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ConnectionFactory>();
    var db = new SqliteDbContext(context);
    try
    {
        db.Initialize();
        Console.WriteLine("SQLite Database initialized successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FATAL ERROR during DB initialization: {ex.Message}");
        // Se deja que el error se propague para que el servidor no inicie si la BD es vital.
        throw;
    }
}
app.UseDeveloperExceptionPage();



app.Run();