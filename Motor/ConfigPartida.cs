using System.Collections.Generic;
using System.Text;
using ArenaElemental.Datos;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    // Lo que cada jugador le anuncia al otro antes de empezar una partida LAN:
    // el nombre de su equipo y sus 3 personajes (como posiciones de la lista de CatalogoBase).
    // El anfitrión además envía el escenario y la semilla del azar.
    //
    // Se manda como una línea de texto:  CONFIG|nombre|1,4,7|escenario|semilla
    public class ConfigPartida
    {
        public string NombreEquipo;
        public List<Personaje> Equipo = new List<Personaje>();
        public int IndiceEscenario = -1;   // solo viene del anfitrión (-1 = no viene)
        public int Semilla;                // solo viene del anfitrión

        // Convierte la configuración en el texto que se envía por la red.
        public string ATexto()
        {
            // Las posiciones de los personajes, separadas por comas.
            string ids = "";
            foreach (Personaje p in Equipo)
            {
                if (ids != "") ids = ids + ",";
                ids = ids + CatalogoBase.Personajes.IndexOf(p);
            }

            // El nombre no puede llevar "|" ni saltos de línea porque son los separadores del mensaje.
            string nombre = NombreEquipo.Replace('|', '/').Replace('\n', ' ').Replace('\r', ' ');

            string texto = "CONFIG|" + nombre + "|" + ids;
            if (IndiceEscenario >= 0)
                texto = texto + "|" + IndiceEscenario + "|" + Semilla;
            return texto;
        }

        // Convierte el texto recibido en una configuración. Devuelve null si el texto está mal armado,
        // si hay personajes que no existen o repetidos.
        public static ConfigPartida DeTexto(string mensaje)
        {
            string[] partes = mensaje.Split('|');
            if (partes[0] != "CONFIG") return null;
            if (partes.Length != 3 && partes.Length != 5) return null;
            if (partes[1].Trim() == "") return null;

            ConfigPartida config = new ConfigPartida();
            config.NombreEquipo = partes[1].Trim();

            // Los 3 personajes
            string[] ids = partes[2].Split(',');
            if (ids.Length != EquipoCombate.TamanoEquipo) return null;
            foreach (string id in ids)
            {
                int posicion;
                if (!int.TryParse(id, out posicion)) return null;
                if (posicion < 0 || posicion >= CatalogoBase.Personajes.Count) return null;

                Personaje p = CatalogoBase.Personajes[posicion];
                if (config.Equipo.Contains(p)) return null;   // repetido
                config.Equipo.Add(p);
            }

            // El escenario y la semilla (solo los manda el anfitrión)
            if (partes.Length == 5)
            {
                int escenario;
                int semilla;
                if (!int.TryParse(partes[3], out escenario)) return null;
                if (!int.TryParse(partes[4], out semilla)) return null;
                if (escenario < 0 || escenario >= CatalogoBase.Escenarios.Count) return null;

                config.IndiceEscenario = escenario;
                config.Semilla = semilla;
            }
            return config;
        }
    }
}
