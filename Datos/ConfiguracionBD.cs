namespace ArenaElemental.Datos
{
    /// <summary>
    /// ÚNICO lugar donde se configura la conexión a la base de datos.
    ///
    /// SQL Server (por defecto):
    ///   Proveedor      = "System.Data.SqlClient"
    ///   CadenaConexion = @"Data Source=.\SQLEXPRESS;Initial Catalog=ArenaElemental;Integrated Security=True;Connect Timeout=3"
    ///
    /// MySQL (requiere instalar MySQL Connector/NET y agregar la referencia MySql.Data al proyecto):
    ///   Proveedor      = "MySql.Data.MySqlClient"
    ///   CadenaConexion = "Server=localhost;Database=ArenaElemental;Uid=root;Pwd=tu_clave;Charset=utf8;"
    ///
    /// Si no se logra conectar, el juego usa el catálogo en memoria (sin guardar estadísticas permanentes).
    /// </summary>
    public static class ConfiguracionBD
    {
        public static string Proveedor = "System.Data.SqlClient";

        public static string CadenaConexion =
            @"Data Source=.\SQLEXPRESS;Initial Catalog=ArenaElemental;Integrated Security=True;Connect Timeout=3";
    }
}
