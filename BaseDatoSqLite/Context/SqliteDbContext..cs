using BaseDatoSqLite.Conexion;
using Dapper;

namespace BaseDatoSqLite.Context
{
    public class SqliteDbContext
    {
        private readonly ConnectionFactory _factory;

        public SqliteDbContext(ConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Initialize()
        {
            using var conn = _factory.CreateConnection();

            var scripts = new[]
            {
                // tablas principales
                @"CREATE TABLE IF NOT EXISTS directorio_activo (
                    id_da INTEGER PRIMARY KEY AUTOINCREMENT,
                    fecha_cargue TEXT NOT NULL,
                    origen TEXT,
                    login TEXT,
                    identificacion TEXT,
                    nombre_completo TEXT,
                    estado TEXT,
                    ultimo_login TEXT,
                    expira TEXT
                );",

                @"CREATE TABLE IF NOT EXISTS vinculados (
                    id_vinculado INTEGER PRIMARY KEY AUTOINCREMENT,
                    fecha_cargue TEXT NOT NULL,
                    origen TEXT,
                    fecha_retiro TEXT,
                    nombre TEXT,
                    cedula TEXT,
                    company TEXT
                );",

                @"CREATE TABLE IF NOT EXISTS terceros (
                    id_tercero INTEGER PRIMARY KEY AUTOINCREMENT,
                    fecha_cargue TEXT NOT NULL,
                    origen TEXT,
                    login TEXT,
                    estado_entidad TEXT,
                    nombre_completo TEXT,
                    cedula TEXT,
                    fecha_retiro TEXT
                );",



                @"CREATE TABLE IF NOT EXISTS retiro_app_temp (
                    id_retiro_app INTEGER PRIMARY KEY AUTOINCREMENT,
                    fecha_ejecucion TEXT NOT NULL,
                    app TEXT,
                    cedula_app TEXT,
                    cedula_consolidado TEXT,
                    login_consolidado TEXT,
                    login_app TEXT,
                    nombre_consolidado TEXT,
                    estado_app TEXT,
                    estado_consolidado TEXT,
                    fecha_retiro TEXT,
                    tipo_cruce TEXT,
                    numero_oc TEXT,
                    fecha_oc TEXT,
                    estado_oc TEXT,
                    validacion_estado TEXT

                );",


                @"CREATE TABLE IF NOT EXISTS retiro_app (
                    id_retiro_app INTEGER PRIMARY KEY AUTOINCREMENT,
                    fecha_ejecucion TEXT NOT NULL,
                    app TEXT,
                    cedula_app TEXT,
                    cedula_consolidado TEXT,
                    login_consolidado TEXT,
                    login_app TEXT,
                    nombre_consolidado TEXT,
                    estado_app TEXT,
                    estado_consolidado TEXT,
                    fecha_retiro TEXT,
                    tipo_cruce TEXT,
                    numero_oc TEXT,
                    fecha_oc TEXT,
                    estado_oc TEXT,
                    validacion_estado TEXT

                );",
                @"CREATE TABLE IF NOT EXISTS portal (
                    id_portal INTEGER PRIMARY KEY AUTOINCREMENT,
                    fecha_cargue TEXT NOT NULL,
                    origen TEXT,
                    nombre TEXT,
                    cedula TEXT,
                    login TEXT
                );",

                // vista consolidada
                @"DROP VIEW IF EXISTS consolidado;
                  CREATE VIEW consolidado AS
                  SELECT 
                      'vinculados' AS Tipo,
                      v.id_vinculado AS Id,
                      v.fecha_cargue AS fecha_cargue,
                      v.origen AS origen,
                      i.Login_ID AS login,
                      v.fecha_retiro AS fecha_retiro,
                      v.cedula AS cedula,
                      v.nombre AS nombre,
                      v.company AS estado_entidad
                  FROM vinculados v
                  LEFT JOIN iam i 
                    ON v.cedula = i.numero_de_identificacion

                  UNION ALL

                  SELECT 
                      'terceros' AS Tipo,
                      t.id_tercero AS Id,
                      t.fecha_cargue AS fecha_cargue,
                      t.origen AS origen,
                      t.login AS login,
                      t.fecha_retiro AS fecha_retiro,
                      t.cedula AS cedula,  
                      t.nombre_completo AS nombre,
                      t.estado_entidad AS estado_entidad
                  FROM terceros t;"
                ,

                // creo los indices para mejorar la busquedas en las tb
                @"CREATE INDEX IF NOT EXISTS idx_directorio_identificacion ON directorio_activo(identificacion);",
                @"CREATE INDEX IF NOT EXISTS idx_directorio_estado ON directorio_activo(estado);",

                @"CREATE INDEX IF NOT EXISTS idx_vinculados_cedula ON vinculados(cedula);",
                @"CREATE INDEX IF NOT EXISTS idx_vinculados_fecha_retiro ON vinculados(fecha_retiro);",
                @"CREATE INDEX IF NOT EXISTS idx_vinculados_fecha_retiro ON vinculados(fecha_retiro);",

                @"CREATE INDEX IF NOT EXISTS idx_terceros_login ON terceros(login);",
                @"CREATE INDEX IF NOT EXISTS idx_terceros_cedula ON terceros(cedula);",
                @"CREATE INDEX IF NOT EXISTS idx_terceros_estado ON terceros(estado_entidad);",

                @"CREATE INDEX IF NOT EXISTS idx_portal_login ON portal(login);",
                @"CREATE INDEX IF NOT EXISTS idx_portal_cedula ON portal(cedula);",

                @"CREATE INDEX IF NOT EXISTS idx_retiro_cedula ON retiro_app(fecha_ejecucion);",
                @"CREATE INDEX IF NOT EXISTS idx_retiro_cedula ON retiro_app(cedula);",
                @"CREATE INDEX IF NOT EXISTS idx_retiro_login ON retiro_app(login_consolidado);",
                @"CREATE INDEX IF NOT EXISTS idx_retiro_login ON retiro_app(login_app);",
                @"CREATE INDEX IF NOT EXISTS idx_retiro_estado ON retiro_app(estado_consolidado);"

//                @"CREATE INDEX IF NOT EXISTS idx_dwh_fijo_login_json ON dwh_fijo(json_extract(datos_json, '$.user_login') COLLATE NOCASE);"
//,
//                @"CREATE INDEX IF NOT EXISTS idx_dwh_movil_login_json ON dwh_movil(json_extract(datos_json, '$.user_login') COLLATE NOCASE);",

//                @"CREATE INDEX IF NOT EXISTS idx_fenix_login_json ON fenix(json_extract(datos_json, '$.USUARIO_ID') COLLATE NOCASE);",
//                @"CREATE INDEX IF NOT EXISTS idx_fenix_estado_json ON fenix(json_extract(datos_json, '$.ESTADO_USUARIO') COLLATE NOCASE);",
//                @"CREATE INDEX IF NOT EXISTS idx_fenix_cc_json ON fenix(json_extract(datos_json, '$.REGISTRO') COLLATE NOCASE);",

//                @"CREATE INDEX IF NOT EXISTS idx_open_une_login_json ON open_une(json_extract(datos_json, '$.LOGIN') COLLATE NOCASE);",
//                @"CREATE INDEX IF NOT EXISTS idx_open_une_login_json ON open_une(json_extract(datos_json, '$.ESTADO') COLLATE NOCASE);",

//                @"CREATE INDEX IF NOT EXISTS idx_sap_erp_login_json ON sap_erp(json_extract(datos_json, '$.Usuarios') COLLATE NOCASE);",
//                @"CREATE INDEX IF NOT EXISTS idx_sap_erp_estado_json ON sap_erp(json_extract(datos_json, '$.Tipo usuario') COLLATE NOCASE);",

//                //@"CREATE INDEX IF NOT EXISTS idx_sap_grc_estado_json ON sap_grc(json_extract(datos_json, '$.Tipo usuario') COLLATE NOCASE);",
//                //@"CREATE INDEX IF NOT EXISTS idx_sap_grc_login_json ON sap_grc(json_extract(datos_json, '$.Usuarios') COLLATE NOCASE);",

//                @"CREATE INDEX IF NOT EXISTS idx_sap_grc_login_json ON sap_grc(json_extract(datos_json, '$.LOGIN') COLLATE NOCASE);",
//                @"CREATE INDEX IF NOT EXISTS idx_sap_grc_estado_json ON sap_grc(json_extract(datos_json, '$.RESPONSABILIDAD') COLLATE NOCASE);",

//                @"CREATE INDEX IF NOT EXISTS idx_siebel_login ON siebel(login COLLATE NOCASE);",
//                @"CREATE INDEX IF NOT EXISTS idx_siebel_responsabilidad ON siebel(responsabilidad COLLATE NOCASE);",

//                @"CREATE INDEX IF NOT EXISTS idx_iam_login_json ON iam(json_extract(datos_json, '$.Login ID') COLLATE NOCASE);",
//                @"CREATE INDEX IF NOT EXISTS idx_iam_cc_json ON iam(json_extract(datos_json, '$.Numero de identificacion') COLLATE NOCASE);",
//                @"CREATE INDEX IF NOT EXISTS idx_iam_estado_json ON iam(json_extract(datos_json, '$.Estado de la Identidad') COLLATE NOCASE);",

//                @"CREATE INDEX IF NOT EXISTS idx_the_usuario_json ON the(json_extract(datos_json, '$.Usuario') COLLATE NOCASE);"

            };

            foreach (var sql in scripts)
            {
                conn.Execute(sql);
            }
        }
    }
}
//@"CREATE TABLE IF NOT EXISTS portal (
//                    id_portal INTEGER PRIMARY KEY AUTOINCREMENT,
//                    fecha_cargue TEXT NOT NULL,
//                    origen TEXT,
//                    nombre TEXT,
//                    cedula TEXT,
//                    login TEXT
//                );",

//                @"CREATE TABLE IF NOT EXISTS crm_portal (
//                    id_crm_portal INTEGER PRIMARY KEY AUTOINCREMENT,
//                    fecha_cargue TEXT NOT NULL,
//                    origen TEXT,
//                    datos_json TEXT NOT NULL 
//                );",

//                @"CREATE TABLE IF NOT EXISTS dwh_fijo (
//                    id_dhw_fijo INTEGER PRIMARY KEY AUTOINCREMENT,
//                    fecha_cargue TEXT NOT NULL,
//                    origen TEXT,
//                    datos_json TEXT NOT NULL 
//                );",

//                @"CREATE TABLE IF NOT EXISTS dwh_movil (
//                    id_dhw_movil INTEGER PRIMARY KEY AUTOINCREMENT,
//                    fecha_cargue TEXT NOT NULL,
//                    origen TEXT,
//                    datos_json TEXT NOT NULL 
//                );",

//                @"CREATE TABLE IF NOT EXISTS fenix (
//                    id_fenix INTEGER PRIMARY KEY AUTOINCREMENT,
//                    fecha_cargue TEXT NOT NULL,
//                    origen TEXT,
//                    datos_json TEXT NOT NULL 
//                );",

//                @"CREATE TABLE IF NOT EXISTS open_une (
//                    id_open INTEGER PRIMARY KEY AUTOINCREMENT,
//                    fecha_cargue TEXT NOT NULL,
//                    origen TEXT,
//                    datos_json TEXT NOT NULL 
//                );",

//                @"CREATE TABLE IF NOT EXISTS sap_erp (
//                    id_sap_erp INTEGER PRIMARY KEY AUTOINCREMENT,
//                    fecha_cargue TEXT NOT NULL,
//                    origen TEXT,
//                    datos_json TEXT NOT NULL 
//                );",

//                @"CREATE TABLE IF NOT EXISTS sap_grc (
//                    id_sap_grc INTEGER PRIMARY KEY AUTOINCREMENT,
//                    fecha_cargue TEXT NOT NULL,
//                    origen TEXT,
//                    datos_json TEXT NOT NULL 
//                );",

//                @"CREATE TABLE IF NOT EXISTS siebel (
//                    id_siebel INTEGER PRIMARY KEY AUTOINCREMENT,
//                    fecha_cargue TEXT NOT NULL,
//                    origen TEXT,
//                    datos_json TEXT NOT NULL,

//                        -- columnas calculadas a partir del JSON
//                    login AS (json_extract(datos_json, '$.LOGIN')) STORED,
//                    responsabilidad AS (json_extract(datos_json, '$.RESPONSABILIDAD')) STORED
//                );",

//                @"CREATE TABLE IF NOT EXISTS iam (
//                    id_iam INTEGER PRIMARY KEY AUTOINCREMENT,
//                    fecha_cargue TEXT NOT NULL,
//                    origen TEXT,
//                    datos_json TEXT NOT NULL 
//                );",

//                @"CREATE TABLE IF NOT EXISTS the (
//                    id_the INTEGER PRIMARY KEY AUTOINCREMENT,
//                    fecha_cargue TEXT NOT NULL,
//                    origen TEXT,
//                    datos_json TEXT NOT NULL 
//                );",