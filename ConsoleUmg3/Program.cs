//using BaseDatoSqLite.Conexion;
//using BaseDatoSqLite.Context;
//using Data.Interfaces.CapaApp.InsertExcelBD;
//using Data.Interfaces.CapaApp.IServicesCrucesExcel;
//using Data.Repository.CapaAPP.InsertExcelBD;
//using Data.Repository.CapaAPP.ServicesCrucesExcel;
//using Helper;
//using Microsoft.Data.Sqlite;

using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("La aplicación ConsoleUam3 se está ejecutando...");
        Console.WriteLine("Presiona Enter para finalizar...");
        Console.ReadLine(); // Mantiene la consola abierta
    }
}

//using Microsoft.Extensions.Configuration;
//using RpaServiceDesk.IniciarSesionSD;
//using RpaServiceDesk.ValidarEstadoOc;
//using Shared;

//class Program
//{
//    static async Task Main(string[] args)
//    {
//        Console.OutputEncoding = System.Text.Encoding.UTF8;

//        // 1. Cargo la configuración
//        var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
//        var configuration = new ConfigurationBuilder()
//            .AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true)
//            .Build();

//        // Mapeo secciones del appsettings
//        var rutasRobot = configuration.GetSection("RutaRobot").Get<RutaRobotOptions>();
//        var nombresRobot = configuration.GetSection("NombreArchivoRobotCapaAPP").Get<NombreArchivoRobotOptionsCapaAPP>();

//        string rutaExcel = configuration["ConfiguracionGeneral:RutaArchivoVinculados"] ?? throw new Exception("Falta RutaArchivoVinculados en appsettings.json");
//        string rutaDA = configuration["ConfiguracionGeneral:RutaArchivoDA"] ?? throw new Exception("Falta RutaArchivoDA en appsettings.json");
//        string rutaCsv = configuration["ConfiguracionGeneral:RutaArchivoPortal"] ?? throw new Exception("Falta RutaArchivoPortal en appsettings.json");
//        string rutaTerceros = configuration["ConfiguracionGeneral:RutaArchivoTerceros"] ?? throw new Exception("Falta RutaArchivoTerceros en appsettings.json");
//        string rutaLogs = configuration["ConfiguracionGeneral:RutaLogs"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Logs");
//        string connString = configuration.GetConnectionString("SQLite") ?? throw new Exception("Falta ConnectionStrings:SQLite en appsettings.json");

//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.ResetColor();

//        Console.WriteLine($"📂 Ruta Excel      : {rutaExcel}");
//        Console.WriteLine($"📂 Ruta DA     : {rutaDA}");
//        Console.WriteLine($"📂 Ruta Csv        : {rutaCsv}");
//        Console.WriteLine($"📂 Ruta Terceros   : {rutaTerceros}");
//        Console.WriteLine($"📂 Ruta Logs       : {rutaLogs}");
//        Console.WriteLine($"🔗 ConnStr         : {connString}\n");

//        // 2. Aseguro la BD
//        var builder = new SqliteConnectionStringBuilder(connString);
//        var dataSource = builder.DataSource ?? "BDUmg.db";

//        string dbFullPath = Path.IsPathRooted(dataSource)
//            ? dataSource
//            : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), dataSource));

//        var dbDirectory = Path.GetDirectoryName(dbFullPath);
//        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
//            Directory.CreateDirectory(dbDirectory);

//        // 3. Inicializo BD y repositorios
//        var factory = new ConnectionFactory(connString);
//        var dbContext = new SqliteDbContext(factory);
//        dbContext.Initialize();

//        var repo = new DatosExcelRepository(factory);
//        var logger = new FileLogger(rutaLogs);

//        //IExcelImporter importer = new ExcelImporter(repo, logger);

//        //ICargueInsumos cargueService = new CargueInsumoService(
//        //    importer: importer,
//        //    logger: logger,
//        //    rutaExcel: rutaExcel,
//        //    RutaDA: rutaDA,
//        //    rutaCsv: rutaCsv,
//        //    rutaTerceros: rutaTerceros,
//        //    rutasRobot: rutasRobot,
//        //    nombresRobot: nombresRobot
//        //);

//        ICrucesExcel userDisableService = new CrucesUserDisable(factory, logger, configuration);

//        var serviceDesk = new ServicesDesk(
//            url: "http://netvm-psdmapp03/CAisd/pdmweb1.exe",
//            usuario: "equiroav",
//            password: "@EimerYesith30"
//        );

//        // 4. Menú principal
//        bool salir = false;
//        while (!salir)
//        {
//            Console.Clear();
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            //Console.WriteLine(@"
//            //                    ███████╗██╗███╗   ███╗███████╗██████╗ 
//            //                    ██╔════╝██║████╗ ████║██╔════╝██╔══██╗
//            //                    ███████╗██║██╔████╔██║█████╗  ██████╔╝
//            //                    ╚════██║██║██║╚██╔╝██║██╔══╝  ██╔═══╝ 
//            //                    ███████║██║██║ ╚═╝ ██║███████╗██║     
//            //                    ╚══════╝╚═╝╚═╝     ╚═╝╚══════╝╚═╝     
//            //                    ");
//            Console.WriteLine();
//            Console.ResetColor();

//            Console.WriteLine($"========= MENU PRINCIPAL =========\n");
//            Console.WriteLine("📂  1️  Cargue de Insumos");
//            Console.WriteLine("🤵  2️  Usuarios a Deshabilitar");
//            Console.WriteLine("🖥️  3️  Service Desk");
//            Console.WriteLine("🧑‍💻  4️  Ejecutar SQL libre");
//            Console.WriteLine("📤  5️  Salir");
//            Console.Write("\n👉 Seleccione una opcion: ");

//            var opcion = Console.ReadLine();
//            switch (opcion)
//            {
//                case "1":
//                    //MostrarSubmenuCargue(cargueService);
//                    break;

//                case "2":
//                    await ProcesarUsuarios(userDisableService);
//                    break;
//                case "3":
//                    bool volverServiceDesk = false;
//                    while (!volverServiceDesk)
//                    {
//                        Console.Clear();
//                        Console.ForegroundColor = ConsoleColor.Magenta;
//                        Console.WriteLine("=== SUBMENU SERVICE DESK ===\n");
//                        Console.ResetColor();

//                        Console.WriteLine("1️  Realizar OC CapaApp (aun falta)");
//                        Console.WriteLine("2️  Consultar Estado de OCs");
//                        Console.WriteLine("3   no implementada");
//                        Console.WriteLine("4️  Volver al menú principal\n");
//                        Console.Write("👉 Seleccione una opcion: ");

//                        var opcionSD = Console.ReadLine();
//                        switch (opcionSD)
//                        {
//                            case "1":
//                                Console.WriteLine("\n🚧 Realizar OC aun no implementada...");
//                                Console.ReadKey();
//                                break;

//                            case "2":

//                                serviceDesk.IniciarSesion();

//                                var estadoOC = new EstadoOC(serviceDesk.Driver);
//                                estadoOC.SeleccionarChangeOrder();
//                                estadoOC.ProcesarMultiplesOCs();

//                                Console.WriteLine("\n✅ Proceso completado.");
//                                Console.ReadKey();

//                                serviceDesk.Cerrar();

//                                break;

//                            case "3":
//                                Console.WriteLine("\n🚧 Funcionalidadno implementada");
//                                Console.ReadKey();
//                                break;

//                            case "4":
//                                volverServiceDesk = true;
//                                break;

//                            default:
//                                Console.ForegroundColor = ConsoleColor.Red;
//                                Console.WriteLine("❌ Opción no valida.");
//                                Console.ResetColor();
//                                Console.ReadKey();
//                                break;
//                        }
//                    }
//                    break;





//                case "4":
//                    //var sqlConsole = new SqlConsoleExecutor(factory);
//                    //sqlConsole.Ejecutar();
//                    break;

//                case "5":
//                    salir = true;
//                    Console.ForegroundColor = ConsoleColor.Green;
//                    Console.WriteLine("\n✅ Gracias por usr la aplicacion");
//                    Console.ResetColor();
//                    break;

//                default:
//                    Console.ForegroundColor = ConsoleColor.Red;
//                    Console.WriteLine("❌ Opcion no valida.");
//                    Console.ResetColor();
//                    Console.ReadKey();
//                    break;
//            }
//        }
//    }

//    private static void MostrarSubmenuCargue(ICargueInsumos cargueService)
//    {
//        bool volver = false;
//        while (!volver)
//        {
//            Console.Clear();
//            Console.ForegroundColor = ConsoleColor.Blue;
//            Console.WriteLine("=== SUBMENU: CARGUE DE INSUMOS ===\n");
//            Console.ResetColor();
//            Console.Write($"\n Al cargar los insuos se borraran los que estan en tb de la base de datos y se ingresaran los nuevos \n");
//            Console.WriteLine("\n📂  1️  Cargar archivos vinculados");
//            Console.WriteLine("📂  2️  Cargar archivos Portal");
//            Console.WriteLine("📂  3️  Cargar archivos Terceros");
//            Console.WriteLine("📂  4️  Cargar Insumos Robot");
//            Console.WriteLine("📂  5  Cargar Insumos DA");
//            Console.WriteLine("\n🔙  6  Volver al menu principal");
//            Console.Write("\n👉 Seleccione una opción: ");

//            var subopcion = Console.ReadLine();
//            switch (subopcion)
//            {
//                //case "1":
//                //    cargueService.EjecutarCargaVinculados();
//                //    break;
//                //case "2":
//                //    cargueService.EjecutarCargaPortal();
//                //    break;
//                //case "3":
//                //    cargueService.EjecutarCargaTerceros();
//                //    break;
//                //case "4":
//                //    cargueService.EjecutarCargaRobot();
//                //    break;
//                //case "5":
//                //    cargueService.EjecutarCargaDA();
//                    break;
//                case "6":
//                    volver = true;
//                    break;
//                default:
//                    Console.ForegroundColor = ConsoleColor.Red;
//                    Console.WriteLine("❌ Opcion no valida.");
//                    Console.ResetColor();
//                    Console.ReadKey();
//                    break;
//            }
//        }
//    }

//    private static async Task ProcesarUsuarios(ICrucesExcel userDisableService)
//    {
//        Console.Clear();
//        Console.WriteLine("⏳ Procesando usuarios para deshabilitar...\n");
//        try
//        {
//            await userDisableService.UsuariosPortalDisable();
//            await userDisableService.UsuariosDADisable();
//            await userDisableService.UsuariosSiebelMovilDisable();
//            await userDisableService.UsuariosSiebelFijoDisable();
//            await userDisableService.UsuariosFenixDisable();
//            await userDisableService.UsuariosOpenDisable();
//            await userDisableService.UsuariosDwhMovilDisable();
//            await userDisableService.UsuariosDwhFijoDisable();
//            await userDisableService.UsuariosSapGrcDisable();
//            await userDisableService.UsuariosSapErpDisable();
//            await userDisableService.UsuariosIamDisable();

//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine("\n✅ Procesamiento completado con exito.");
//            Console.ResetColor();
//        }
//        catch (Exception ex)
//        {
//            Console.ForegroundColor = ConsoleColor.Red;
//            Console.WriteLine($"❌ Error al procesar usuarios: {ex.Message}");
//            Console.ResetColor();
//        }
//        Console.WriteLine("\nPresione cualquier tecla para continuar...");
//        Console.ReadKey();
//    }
//}

////static async Task MostrarMenuUsuariosADeshabilitar(ICrucesExcel userDisableService)
////{
////    bool volver = false;
////    while (!volver)
////    {
////        Console.Clear();
////        Console.WriteLine("=== SUBMENU: USUARIOS A DESHABILITAR ===");
////        Console.WriteLine("1. Usuarios Portal");
////        Console.WriteLine("2. Usuarios DA");
////        Console.WriteLine("3. Usuarios Siebel Movile");
////        Console.WriteLine("3. Usuarios Siebel Fijo");
////        Console.WriteLine("4. Volver al menu principal");
////        Console.Write("Seleccione una opción: ");

////        var subopcion = Console.ReadLine();
////        switch (subopcion)
////        {
////            case "1":
////                Console.Clear();
////                Console.WriteLine(" Procesando usuarios para deshabilitar...");
////                Console.WriteLine();
////                try
////                {
////                    var usuarios = await userDisableService.UsuariosPortalDisable();
////                    Console.WriteLine();
////                    var da = await userDisableService.UsuariosDADisable();

////                }
////                catch (Exception ex)
////                {
////                    Console.WriteLine($" Error al procesar usuarios: {ex.Message}");
////                }
////                Console.WriteLine("\nPresione cualquier tecla para continuar...");
////                Console.ReadKey();
////                break;

////            case "2":
////                Console.WriteLine(" Procesando usuarios del DA para deshabilitar...");
////                Console.WriteLine();
////                try
////                {
////                    var usuarios = await userDisableService.UsuariosDADisable();
////                    Console.WriteLine($"\n Proceso completado. Total de usuarios: {usuarios.Count()}");
////                }
////                catch (Exception ex)
////                {
////                    Console.WriteLine($" Error al procesar usuarios: {ex.Message}");
////                }
////                Console.WriteLine("\nPresione cualquier tecla para continuar...");
////                Console.ReadKey();
////                break;

////            case "3":
////                Console.WriteLine(" Funcionalidad próximamente disponible...");
////                Console.ReadKey();
////                break;

////            case "4":
////                volver = true;
////                break;

////            default:
////                Console.WriteLine("Opcion no valida.");
////                Console.ReadKey();
////                break;
////        }
////    }
////}
