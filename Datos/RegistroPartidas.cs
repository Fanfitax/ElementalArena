using System;
using System.Collections.Generic;

namespace ArenaElemental.Datos
{
    /// <summary>
    /// El historial de partidas jugadas. Vive solo en memoria (una lista estática):
    /// se borra al cerrar el programa.
    /// </summary>
    public static class RegistroPartidas
    {
        private static List<EstadisticaPartida> partidas = new List<EstadisticaPartida>();

        /// <summary>Guarda una partida nueva en el historial.</summary>
        public static void Agregar(string jugador, string equipo, string escenario, string resultado, int turnos, int danoTotal)
        {
            EstadisticaPartida p = new EstadisticaPartida();
            p.IdPartida = partidas.Count + 1;
            p.Jugador = jugador;
            p.Equipo = equipo;
            p.Escenario = escenario;
            p.Resultado = resultado;
            p.Turnos = turnos;
            p.DanoTotal = danoTotal;
            p.Fecha = DateTime.Now;
            partidas.Add(p);
        }

        /// <summary>Las partidas de un jugador, de la más reciente a la más antigua (sin importar mayúsculas).</summary>
        public static List<EstadisticaPartida> DeJugador(string jugador)
        {
            List<EstadisticaPartida> lista = new List<EstadisticaPartida>();
            for (int i = partidas.Count - 1; i >= 0; i--)
            {
                if (partidas[i].Jugador.ToLower() == jugador.ToLower())
                    lista.Add(partidas[i]);
            }
            return lista;
        }
    }
}
