using System;
using System.Collections.Generic;

namespace ArenaElemental.Datos
{
    /// <summary>Una partida jugada: es una fila de la tabla de estadísticas.</summary>
    public class EstadisticaPartida
    {
        public int IdPartida { get; set; }
        public string Jugador { get; set; }
        public string Equipo { get; set; }
        public string Escenario { get; set; }
        public string Resultado { get; set; }   // "Victoria", "Derrota" o "Abandono"
        public int Turnos { get; set; }
        public int DanoTotal { get; set; }
        public DateTime Fecha { get; set; }
    }

    /// <summary>Los totales de un jugador, calculados a partir de su lista de partidas.</summary>
    public class ResumenJugador
    {
        public ResumenJugador(List<EstadisticaPartida> partidas)
        {
            foreach (EstadisticaPartida p in partidas)
            {
                Partidas++;
                if (p.Resultado == "Victoria") Victorias++;
                if (p.Resultado == "Derrota") Derrotas++;
                DanoTotal += p.DanoTotal;
                TurnosTotales += p.Turnos;
            }
        }

        public int Partidas { get; private set; }
        public int Victorias { get; private set; }
        public int Derrotas { get; private set; }
        public int DanoTotal { get; private set; }
        public int TurnosTotales { get; private set; }

        /// <summary>Victorias entre partidas, en porcentaje.</summary>
        public double PorcentajeVictorias
        {
            get
            {
                if (Partidas == 0) return 0;
                return 100.0 * Victorias / Partidas;
            }
        }

        /// <summary>Rondas promedio por partida.</summary>
        public double PromedioTurnos
        {
            get
            {
                if (Partidas == 0) return 0;
                return (double)TurnosTotales / Partidas;
            }
        }
    }
}
