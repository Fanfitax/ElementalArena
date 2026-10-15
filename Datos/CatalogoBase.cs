using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Datos
{
    /// <summary>
    /// Copia en código de los datos base (12 personajes y 4 escenarios).
    /// Debe coincidir con los INSERT de los scripts SQL. Se usa cuando no hay base de datos disponible.
    /// </summary>
    public static class CatalogoBase
    {
        public static IList<Personaje> CrearPersonajes()
        {
            List<Personaje> lista = new List<Personaje>();
            //                                   id  nombre     elemento          HP   ATK DEF ENG EMP costo cd
            lista.Add(new CartaFuego (1,  "Pyra",    90, 18,  4, 10, 3, 4, 3));
            lista.Add(new CartaFuego (2,  "Ignar",  100, 16,  6, 10, 4, 4, 3));
            lista.Add(new CartaFuego (3,  "Cenizo",  80, 20,  3, 10, 2, 4, 3));
            lista.Add(new CartaAgua  (4,  "Marina", 100, 13,  5, 10, 8, 4, 3));
            lista.Add(new CartaAgua  (5,  "Tidal",  110, 12,  6, 10, 7, 4, 3));
            lista.Add(new CartaAgua  (6,  "Cascada", 95, 14,  5, 10, 9, 4, 3));
            lista.Add(new CartaTierra(7,  "Gaia",   125, 12,  9,  8, 6, 3, 4));
            lista.Add(new CartaTierra(8,  "Gólem",  135, 11, 11,  8, 5, 3, 4));
            lista.Add(new CartaTierra(9,  "Sílex",  120, 13,  8,  8, 4, 3, 4));
            lista.Add(new CartaAire  (10, "Céfiro",  85, 15,  4, 12, 4, 3, 3));
            lista.Add(new CartaAire  (11, "Huracán", 80, 17,  3, 12, 3, 3, 3));
            lista.Add(new CartaAire  (12, "Brisa",   88, 14,  5, 12, 6, 3, 3));
            return lista;
        }

        public static IList<Escenario> CrearEscenarios()
        {
            List<Escenario> lista = new List<Escenario>();
            lista.Add(new Escenario(1, "Volcán Ardiente", Elemento.Fuego,  10, "Ríos de lava potencian a las cartas de Fuego."));
            lista.Add(new Escenario(2, "Lago Cristalino", Elemento.Agua,   10, "Aguas serenas potencian a las cartas de Agua."));
            lista.Add(new Escenario(3, "Cañón de Roca",   Elemento.Tierra, 10, "Paredes de piedra potencian a las cartas de Tierra."));
            lista.Add(new Escenario(4, "Cumbre Ventosa",  Elemento.Aire,   10, "Vientos de altura potencian a las cartas de Aire."));
            return lista;
        }
    }
}
