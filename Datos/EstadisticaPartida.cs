using System;
using System.Collections.Generic;

namespace ArenaElemental.Datos
{
    /// <summary>Fila del historial de partidas de un jugador.</summary>
    public class EstadisticaPartida
    {
        public int IdPartida { get; set; }
        public string Jugador { get; set; }
        public string Equipo { get; set; }
        public string Escenario { get; set; }
        public string Resultado { get; set; }
        public int Turnos { get; set; }
        public int DanoTotal { get; set; }
        public DateTime Fecha { get; set; }
    }

    /// <summary>Totales calculados a partir del historial.</summary>
    public class ResumenJugador
    {
        public ResumenJugador(IEnumerable<EstadisticaPartida> partidas)
        {
            foreach (EstadisticaPartida p in partidas)
            {
                Partidas++;
                if (p.Resultado == "Victoria") Victorias++;
                else if (p.Resultado == "Derrota") Derrotas++;
                DanoTotal += p.DanoTotal;
                TurnosTotales += p.Turnos;
            }
        }

        public int Partidas { get; private set; }
        public int Victorias { get; private set; }
        public int Derrotas { get; private set; }
        public int DanoTotal { get; private set; }
        public int TurnosTotales { get; private set; }

        public double PorcentajeVictorias
        {
            get { return Partidas == 0 ? 0 : 100.0 * Victorias / Partidas; }
        }

        public double PromedioTurnos
        {
            get { return Partidas == 0 ? 0 : (double)TurnosTotales / Partidas; }
        }
    }
}
