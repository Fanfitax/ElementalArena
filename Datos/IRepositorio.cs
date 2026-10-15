using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Datos
{
    /// <summary>Acceso a datos del juego. Tiene una versión SQL y otra en memoria.</summary>
    public interface IRepositorio
    {
        /// <summary>Texto para mostrar al usuario ("Base de datos SQL" o "Modo sin conexión").</summary>
        string Descripcion { get; }

        IList<Personaje> ObtenerPersonajes();
        IList<Escenario> ObtenerEscenarios();

        /// <summary>Devuelve el id del jugador; lo crea si no existe.</summary>
        int ObtenerOCrearJugador(string nombreUsuario);

        /// <summary>Guarda (o actualiza si ya existe con ese nombre) el equipo del jugador. Devuelve su id.</summary>
        int GuardarEquipo(int idJugador, string nombreEquipo, IList<Personaje> personajes);

        void GuardarPartida(int idJugador, int idEquipo, int idEscenario, string resultado, int turnos, int danoTotal);

        IList<EstadisticaPartida> ObtenerPartidas(int idJugador);
    }
}
