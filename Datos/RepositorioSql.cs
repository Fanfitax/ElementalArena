using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using ArenaElemental.Modelos;

namespace ArenaElemental.Datos
{
    /// <summary>
    /// Repositorio ADO.NET independiente del proveedor (SQL Server o MySQL según ConfiguracionBD).
    /// Las consultas usan SQL estándar con parámetros "@nombre".
    /// </summary>
    public class RepositorioSql : IRepositorio
    {
        private readonly DbProviderFactory fabrica;
        private readonly string cadenaConexion;
        private readonly bool esMySql;

        public RepositorioSql(string proveedor, string cadenaConexion)
        {
            this.fabrica = DbProviderFactories.GetFactory(proveedor);
            this.cadenaConexion = cadenaConexion;
            this.esMySql = proveedor.IndexOf("MySql", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public string Descripcion
        {
            get { return esMySql ? "Base de datos MySQL" : "Base de datos SQL Server"; }
        }

        /// <summary>Intenta abrir la conexión. Devuelve false (y el motivo) si no se puede.</summary>
        public bool ProbarConexion(out string error)
        {
            error = null;
            try
            {
                using (DbConnection cn = AbrirConexion())
                using (DbCommand cmd = CrearComando(cn, "SELECT COUNT(*) FROM Personajes"))
                {
                    Convert.ToInt32(cmd.ExecuteScalar());
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public IList<Personaje> ObtenerPersonajes()
        {
            List<Personaje> lista = new List<Personaje>();
            const string sql = "SELECT id_personaje, nombre, elemento, hp_max, atk, defensa, energia_max, empatia, "
                             + "costo_habilidad, cooldown_habilidad FROM Personajes ORDER BY id_personaje";
            using (DbConnection cn = AbrirConexion())
            using (DbCommand cmd = CrearComando(cn, sql))
            using (DbDataReader rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    lista.Add(FabricaPersonajes.Crear(
                        Convert.ToInt32(rd["id_personaje"]),
                        Convert.ToString(rd["nombre"]),
                        FabricaPersonajes.ParsearElemento(Convert.ToString(rd["elemento"])),
                        Convert.ToInt32(rd["hp_max"]),
                        Convert.ToInt32(rd["atk"]),
                        Convert.ToInt32(rd["defensa"]),
                        Convert.ToInt32(rd["energia_max"]),
                        Convert.ToInt32(rd["empatia"]),
                        Convert.ToInt32(rd["costo_habilidad"]),
                        Convert.ToInt32(rd["cooldown_habilidad"])));
                }
            }
            return lista;
        }

        public IList<Escenario> ObtenerEscenarios()
        {
            List<Escenario> lista = new List<Escenario>();
            const string sql = "SELECT id_escenario, nombre, elemento_afin, bonus_ataque_pct, descripcion "
                             + "FROM Escenarios ORDER BY id_escenario";
            using (DbConnection cn = AbrirConexion())
            using (DbCommand cmd = CrearComando(cn, sql))
            using (DbDataReader rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    lista.Add(new Escenario(
                        Convert.ToInt32(rd["id_escenario"]),
                        Convert.ToString(rd["nombre"]),
                        FabricaPersonajes.ParsearElemento(Convert.ToString(rd["elemento_afin"])),
                        Convert.ToInt32(rd["bonus_ataque_pct"]),
                        rd["descripcion"] == DBNull.Value ? null : Convert.ToString(rd["descripcion"])));
                }
            }
            return lista;
        }

        public int ObtenerOCrearJugador(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", "nombreUsuario");
            string usuario = nombreUsuario.Trim();

            using (DbConnection cn = AbrirConexion())
            {
                using (DbCommand cmd = CrearComando(cn, "SELECT id_jugador FROM Jugadores WHERE nombre_usuario = @u",
                                                    "@u", usuario))
                {
                    object existente = cmd.ExecuteScalar();
                    if (existente != null && existente != DBNull.Value) return Convert.ToInt32(existente);
                }
                return InsertarYObtenerId(cn, null, "INSERT INTO Jugadores (nombre_usuario) VALUES (@u)", "@u", usuario);
            }
        }

        public int GuardarEquipo(int idJugador, string nombreEquipo, IList<Personaje> personajes)
        {
            if (personajes == null || personajes.Count != EquipoSize)
                throw new ArgumentException("El equipo debe tener 3 personajes.", "personajes");

            using (DbConnection cn = AbrirConexion())
            using (DbTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    int idEquipo;
                    using (DbCommand cmd = CrearComando(cn, "SELECT id_equipo FROM Equipos WHERE id_jugador = @j AND nombre = @n",
                                                        "@j", idJugador, "@n", nombreEquipo))
                    {
                        cmd.Transaction = tx;
                        object existente = cmd.ExecuteScalar();
                        idEquipo = (existente == null || existente == DBNull.Value) ? 0 : Convert.ToInt32(existente);
                    }

                    if (idEquipo == 0)
                    {
                        idEquipo = InsertarYObtenerId(cn, tx, "INSERT INTO Equipos (id_jugador, nombre) VALUES (@j, @n)",
                                                      "@j", idJugador, "@n", nombreEquipo);
                    }
                    else
                    {
                        using (DbCommand borrar = CrearComando(cn, "DELETE FROM Equipo_Personajes WHERE id_equipo = @e", "@e", idEquipo))
                        {
                            borrar.Transaction = tx;
                            borrar.ExecuteNonQuery();
                        }
                    }

                    for (int i = 0; i < personajes.Count; i++)
                    {
                        using (DbCommand ins = CrearComando(cn,
                            "INSERT INTO Equipo_Personajes (id_equipo, id_personaje, orden) VALUES (@e, @p, @o)",
                            "@e", idEquipo, "@p", personajes[i].Id, "@o", i + 1))
                        {
                            ins.Transaction = tx;
                            ins.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                    return idEquipo;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public void GuardarPartida(int idJugador, int idEquipo, int idEscenario, string resultado, int turnos, int danoTotal)
        {
            const string sql = "INSERT INTO Estadisticas_Partidas (id_jugador, id_equipo, id_escenario, resultado, turnos, dano_total) "
                             + "VALUES (@j, @e, @s, @r, @t, @d)";
            using (DbConnection cn = AbrirConexion())
            using (DbCommand cmd = CrearComando(cn, sql, "@j", idJugador, "@e", idEquipo, "@s", idEscenario,
                                                "@r", resultado, "@t", turnos, "@d", danoTotal))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public IList<EstadisticaPartida> ObtenerPartidas(int idJugador)
        {
            List<EstadisticaPartida> lista = new List<EstadisticaPartida>();
            const string sql = "SELECT p.id_partida, e.nombre AS equipo, s.nombre AS escenario, p.resultado, "
                             + "p.turnos, p.dano_total, p.fecha_partida "
                             + "FROM Estadisticas_Partidas p "
                             + "INNER JOIN Equipos e ON e.id_equipo = p.id_equipo "
                             + "INNER JOIN Escenarios s ON s.id_escenario = p.id_escenario "
                             + "WHERE p.id_jugador = @j ORDER BY p.fecha_partida DESC, p.id_partida DESC";
            using (DbConnection cn = AbrirConexion())
            using (DbCommand cmd = CrearComando(cn, sql, "@j", idJugador))
            using (DbDataReader rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    EstadisticaPartida p = new EstadisticaPartida();
                    p.IdPartida = Convert.ToInt32(rd["id_partida"]);
                    p.Equipo = Convert.ToString(rd["equipo"]);
                    p.Escenario = Convert.ToString(rd["escenario"]);
                    p.Resultado = Convert.ToString(rd["resultado"]);
                    p.Turnos = Convert.ToInt32(rd["turnos"]);
                    p.DanoTotal = Convert.ToInt32(rd["dano_total"]);
                    p.Fecha = Convert.ToDateTime(rd["fecha_partida"]);
                    lista.Add(p);
                }
            }
            return lista;
        }

        // ---------- Utilidades ----------
        private const int EquipoSize = 3;

        private DbConnection AbrirConexion()
        {
            DbConnection cn = fabrica.CreateConnection();
            cn.ConnectionString = cadenaConexion;
            cn.Open();
            return cn;
        }

        /// <summary>Crea un comando con parámetros: pares nombre, valor, nombre, valor...</summary>
        private static DbCommand CrearComando(DbConnection cn, string sql, params object[] parametros)
        {
            DbCommand cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            for (int i = 0; i + 1 < parametros.Length; i += 2)
            {
                DbParameter p = cmd.CreateParameter();
                p.ParameterName = (string)parametros[i];
                p.Value = parametros[i + 1] ?? DBNull.Value;
                cmd.Parameters.Add(p);
            }
            return cmd;
        }

        /// <summary>Ejecuta un INSERT y devuelve el id autogenerado (SCOPE_IDENTITY / LAST_INSERT_ID).</summary>
        private int InsertarYObtenerId(DbConnection cn, DbTransaction tx, string sqlInsert, params object[] parametros)
        {
            string consultaId = esMySql ? "SELECT LAST_INSERT_ID()" : "SELECT CAST(SCOPE_IDENTITY() AS INT)";
            using (DbCommand cmd = CrearComando(cn, sqlInsert + "; " + consultaId, parametros))
            {
                cmd.Transaction = tx;
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }

    /// <summary>Elige el repositorio: SQL si la conexión funciona; si no, el de memoria.</summary>
    public static class RepositorioFactory
    {
        public static IRepositorio Crear(out string aviso)
        {
            aviso = null;
            try
            {
                RepositorioSql sql = new RepositorioSql(ConfiguracionBD.Proveedor, ConfiguracionBD.CadenaConexion);
                string error;
                if (sql.ProbarConexion(out error)) return sql;
                aviso = "No se pudo conectar a la base de datos:\n" + error
                        + "\n\nSe usará el modo sin conexión (sin estadísticas permanentes).";
            }
            catch (Exception ex)
            {
                aviso = "No se pudo inicializar el proveedor de base de datos:\n" + ex.Message
                        + "\n\nSe usará el modo sin conexión (sin estadísticas permanentes).";
            }
            return new RepositorioMemoria();
        }
    }
}
