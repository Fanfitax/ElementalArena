using System;
using System.Collections.Generic;
using System.IO;

namespace ArenaElemental.Datos
{
    // El historial de partidas jugadas. Vive en una lista estática y también se guarda en un archivo de texto
    // (partidas.txt, junto al programa) para que el historial y el ranking no se pierdan al cerrar el juego.
    // Cada línea del archivo es una partida con sus datos separados por "|":
    //   jugador|equipo|escenario|resultado|rondas|daño|fecha
    // Si el archivo no se puede leer o escribir, el juego sigue funcionando solo en memoria.
    public static class RegistroPartidas
    {
        private const char Separador = '|';

        private static List<EstadisticaPartida> partidas = new List<EstadisticaPartida>();
        private static string rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "partidas.txt");

        // Se llama una sola vez al iniciar el programa (desde Main): carga las partidas guardadas.
        public static void Cargar()
        {
            CargarDelArchivo();
        }

        // Guarda una partida nueva en el historial (y en el archivo).
        public static void Agregar(string jugador, string equipo, string escenario, string resultado, int turnos, int danoTotal)
        {
            EstadisticaPartida p = new EstadisticaPartida();
            p.IdPartida = partidas.Count + 1;
            p.Jugador = Limpiar(jugador);
            p.Equipo = Limpiar(equipo);
            p.Escenario = Limpiar(escenario);
            p.Resultado = resultado;
            p.Turnos = turnos;
            p.DanoTotal = danoTotal;
            p.Fecha = DateTime.Now;
            partidas.Add(p);

            GuardarEnArchivo(p);
        }

        // Las partidas de un jugador, de la más reciente a la más antigua (sin importar mayúsculas).
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

        // ---------- Ranking ----------

        // La tabla de posiciones de todos los jugadores, del primero al último.
        // Se ordena por puntos; si empatan, por victorias, luego por % de victorias y por último por nombre.
        public static List<PuestoRanking> ObtenerRanking()
        {
            // Primero se agrupan las partidas por jugador (sin importar mayúsculas).
            // Dos listas paralelas: el jugador número i tiene sus partidas en la lista número i.
            List<string> nombresEnMinuscula = new List<string>();
            List<List<EstadisticaPartida>> partidasPorJugador = new List<List<EstadisticaPartida>>();
            foreach (EstadisticaPartida p in partidas)
            {
                string clave = p.Jugador.ToLower();
                int posicion = nombresEnMinuscula.IndexOf(clave);
                if (posicion == -1)
                {
                    nombresEnMinuscula.Add(clave);
                    partidasPorJugador.Add(new List<EstadisticaPartida>());
                    posicion = nombresEnMinuscula.Count - 1;
                }
                partidasPorJugador[posicion].Add(p);
            }

            List<PuestoRanking> ranking = new List<PuestoRanking>();
            foreach (List<EstadisticaPartida> lista in partidasPorJugador)
            {
                // Se muestra el nombre tal como lo escribió la última vez.
                string nombre = lista[lista.Count - 1].Jugador;
                ranking.Add(new PuestoRanking(nombre, lista));
            }

            OrdenarRanking(ranking);
            for (int i = 0; i < ranking.Count; i++)
                ranking[i].Posicion = i + 1;
            return ranking;
        }

        // Ordena la lista de mejor a peor: en cada vuelta busca el mejor de los que faltan y lo pone en su lugar.
        private static void OrdenarRanking(List<PuestoRanking> ranking)
        {
            for (int i = 0; i < ranking.Count - 1; i++)
            {
                int mejor = i;
                for (int j = i + 1; j < ranking.Count; j++)
                {
                    if (VaAntes(ranking[j], ranking[mejor])) mejor = j;
                }

                PuestoRanking auxiliar = ranking[i];
                ranking[i] = ranking[mejor];
                ranking[mejor] = auxiliar;
            }
        }

        // Dice si el jugador "a" va antes que el "b": más puntos, luego más victorias, luego mejor % y por último el nombre.
        private static bool VaAntes(PuestoRanking a, PuestoRanking b)
        {
            if (a.Puntos != b.Puntos) return a.Puntos > b.Puntos;
            if (a.Victorias != b.Victorias) return a.Victorias > b.Victorias;
            if (a.PorcentajeVictorias != b.PorcentajeVictorias) return a.PorcentajeVictorias > b.PorcentajeVictorias;
            return string.Compare(a.Jugador, b.Jugador, StringComparison.CurrentCultureIgnoreCase) < 0;
        }

        // ---------- Archivo de texto ----------

        // El separador no puede aparecer dentro de un nombre porque rompería la línea del archivo.
        private static string Limpiar(string texto)
        {
            if (texto == null) return string.Empty;
            return texto.Replace(Separador, ' ').Replace('\r', ' ').Replace('\n', ' ').Trim();
        }

        private static void GuardarEnArchivo(EstadisticaPartida p)
        {
            string linea = p.Jugador + Separador + p.Equipo + Separador + p.Escenario + Separador + p.Resultado
                           + Separador + p.Turnos + Separador + p.DanoTotal + Separador + p.Fecha.Ticks;
            try
            {
                File.AppendAllText(rutaArchivo, linea + Environment.NewLine);
            }
            catch (Exception)
            {
                // Sin permiso o disco lleno: la partida queda solo en memoria.
            }
        }

        private static void CargarDelArchivo()
        {
            try
            {
                if (!File.Exists(rutaArchivo)) return;

                foreach (string linea in File.ReadAllLines(rutaArchivo))
                {
                    string[] campos = linea.Split(Separador);
                    if (campos.Length != 7) continue;   // una línea dañada se ignora

                    int turnos, dano;
                    long ticks;
                    if (!int.TryParse(campos[4], out turnos)) continue;
                    if (!int.TryParse(campos[5], out dano)) continue;
                    if (!long.TryParse(campos[6], out ticks)) continue;

                    EstadisticaPartida p = new EstadisticaPartida();
                    p.IdPartida = partidas.Count + 1;
                    p.Jugador = campos[0];
                    p.Equipo = campos[1];
                    p.Escenario = campos[2];
                    p.Resultado = campos[3];
                    p.Turnos = turnos;
                    p.DanoTotal = dano;
                    p.Fecha = new DateTime(ticks);
                    partidas.Add(p);
                }
            }
            catch (Exception)
            {
                // Si el archivo no se puede leer, se empieza con el historial vacío.
            }
        }
    }
}
