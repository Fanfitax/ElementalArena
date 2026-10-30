using System;
using System.Collections.Generic;

namespace ArenaElemental.Datos
{
    // Una partida jugada: es una fila de la tabla de estadísticas.
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

    // Los totales de un jugador, calculados a partir de su lista de partidas.
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

        // Victorias entre partidas, en porcentaje.
        public double PorcentajeVictorias
        {
            get
            {
                if (Partidas == 0) return 0;
                return 100.0 * Victorias / Partidas;
            }
        }

        // Rondas promedio por partida.
        public double PromedioTurnos
        {
            get
            {
                if (Partidas == 0) return 0;
                return (double)TurnosTotales / Partidas;
            }
        }
    }

    // Una fila del ranking: un jugador con sus totales y sus puntos.
    // Puntos: victoria = 3, derrota = 1 (por jugar), abandono = 0.
    public class PuestoRanking
    {
        public const int PuntosPorVictoria = 3;
        public const int PuntosPorDerrota = 1;

        public PuestoRanking(string jugador, List<EstadisticaPartida> partidas)
        {
            ResumenJugador r = new ResumenJugador(partidas);

            Jugador = jugador;
            Partidas = r.Partidas;
            Victorias = r.Victorias;
            Derrotas = r.Derrotas;
            PorcentajeVictorias = r.PorcentajeVictorias;
            DanoTotal = r.DanoTotal;
            Puntos = Victorias * PuntosPorVictoria + Derrotas * PuntosPorDerrota;
        }

        public int Posicion { get; set; }
        public string Jugador { get; private set; }
        public int Puntos { get; private set; }
        public int Partidas { get; private set; }
        public int Victorias { get; private set; }
        public int Derrotas { get; private set; }
        public double PorcentajeVictorias { get; private set; }
        public int DanoTotal { get; private set; }

        // El porcentaje de victorias ya escrito para la tabla (por ejemplo "66.7%").
        public string Efectividad
        {
            get { return PorcentajeVictorias.ToString("0.#") + "%"; }
        }
    }
}
