using System;
using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Datos
{
    /// <summary>Repositorio en memoria: catálogo fijo y estadísticas solo mientras el programa está abierto.</summary>
    public class RepositorioMemoria : IRepositorio
    {
        private class Equipo
        {
            public int Id;
            public int IdJugador;
            public string Nombre;
        }

        private class Partida
        {
            public EstadisticaPartida Datos;
            public int IdJugador;
        }

        private readonly Dictionary<string, int> jugadores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly List<Equipo> equipos = new List<Equipo>();
        private readonly List<Partida> partidas = new List<Partida>();
        private readonly IList<Escenario> escenarios = CatalogoBase.CrearEscenarios();

        public string Descripcion
        {
            get { return "Modo sin conexión (las estadísticas no se guardan al cerrar)"; }
        }

        public IList<Personaje> ObtenerPersonajes()
        {
            return CatalogoBase.CrearPersonajes();
        }

        public IList<Escenario> ObtenerEscenarios()
        {
            return escenarios;
        }

        public int ObtenerOCrearJugador(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", "nombreUsuario");
            string clave = nombreUsuario.Trim();
            int id;
            if (!jugadores.TryGetValue(clave, out id))
            {
                id = jugadores.Count + 1;
                jugadores.Add(clave, id);
            }
            return id;
        }

        public int GuardarEquipo(int idJugador, string nombreEquipo, IList<Personaje> personajes)
        {
            foreach (Equipo e in equipos)
            {
                if (e.IdJugador == idJugador && string.Equals(e.Nombre, nombreEquipo, StringComparison.OrdinalIgnoreCase))
                    return e.Id;
            }
            Equipo nuevo = new Equipo();
            nuevo.Id = equipos.Count + 1;
            nuevo.IdJugador = idJugador;
            nuevo.Nombre = nombreEquipo;
            equipos.Add(nuevo);
            return nuevo.Id;
        }

        public void GuardarPartida(int idJugador, int idEquipo, int idEscenario, string resultado, int turnos, int danoTotal)
        {
            Equipo equipo = equipos.Find(delegate(Equipo e) { return e.Id == idEquipo; });
            Escenario escenario = null;
            foreach (Escenario s in escenarios) if (s.Id == idEscenario) escenario = s;

            Partida p = new Partida();
            p.IdJugador = idJugador;
            p.Datos = new EstadisticaPartida();
            p.Datos.IdPartida = partidas.Count + 1;
            p.Datos.Equipo = equipo != null ? equipo.Nombre : "?";
            p.Datos.Escenario = escenario != null ? escenario.Nombre : "?";
            p.Datos.Resultado = resultado;
            p.Datos.Turnos = turnos;
            p.Datos.DanoTotal = danoTotal;
            p.Datos.Fecha = DateTime.Now;
            partidas.Add(p);
        }

        public IList<EstadisticaPartida> ObtenerPartidas(int idJugador)
        {
            List<EstadisticaPartida> lista = new List<EstadisticaPartida>();
            for (int i = partidas.Count - 1; i >= 0; i--)
                if (partidas[i].IdJugador == idJugador) lista.Add(partidas[i].Datos);
            return lista;
        }
    }
}
