using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Datos
{
    /// <summary>
    /// Los datos fijos del juego guardados en listas estáticas (no hay base de datos):
    /// los 12 personajes y los 4 escenarios.
    ///
    /// Cada personaje se crea así:
    ///   new CartaXxx(id, nombre, vida, ataque, defensa, energía máx., empatía, habilidad de ataque, habilidad de utilidad)
    /// y cada habilidad así:
    ///   new Habilidad(nombre, efecto, costo de energía, recarga en turnos, potencia, extra, duración)
    /// (en la clase Habilidad hay una tabla que explica qué significan potencia, extra y duración en cada efecto).
    /// </summary>
    public static class CatalogoBase
    {
        public static List<Personaje> Personajes = new List<Personaje>();
        public static List<Escenario> Escenarios = new List<Escenario>();

        // Este constructor estático se ejecuta una sola vez, la primera vez que se usa la clase.
        static CatalogoBase()
        {
            CargarPersonajes();
            CargarEscenarios();
        }

        private static void CargarPersonajes()
        {
            // ---- FUEGO: daño puro (su utilidad potencia el ataque) ----
            Personajes.Add(new CartaFuego(1, "Pyra", 90, 19, 4, 10, 3,
                new Habilidad("Llamarada", EfectoHabilidad.GolpePotenciado, 4, 3, 2.0, 0.5, 0),
                new Habilidad("Aura Ígnea", EfectoHabilidad.PotenciarAtaque, 3, 4, 0.30, 0, 2)));

            Personajes.Add(new CartaFuego(2, "Ignar", 105, 16, 6, 10, 4,
                new Habilidad("Tajo Carmesí", EfectoHabilidad.GolpePotenciado, 4, 3, 1.7, 0.7, 0),
                new Habilidad("Grito de Guerra", EfectoHabilidad.PotenciarAtaque, 3, 4, 0.35, 0, 2)));

            Personajes.Add(new CartaFuego(3, "Cenizo", 85, 20, 4, 10, 2,
                new Habilidad("Explosión de Brasas", EfectoHabilidad.GolpeConRetroceso, 4, 3, 2.3, 0.12, 0),
                new Habilidad("Combustión Interna", EfectoHabilidad.PotenciarAtaque, 3, 5, 0.45, 0, 2)));

            // ---- AGUA: apoyo (su utilidad cura) ----
            Personajes.Add(new CartaAgua(4, "Marina", 100, 13, 5, 10, 8,
                new Habilidad("Chorro a Presión", EfectoHabilidad.GolpePotenciado, 3, 3, 1.6, 0.3, 0),
                new Habilidad("Marea Curativa", EfectoHabilidad.Curar, 4, 5, 0.10, 0, 0)));

            Personajes.Add(new CartaAgua(5, "Tidal", 100, 12, 6, 10, 7,
                new Habilidad("Oleada Voraz", EfectoHabilidad.GolpeVampirico, 4, 3, 1.5, 0.3, 0),
                new Habilidad("Oleaje Reparador", EfectoHabilidad.Curar, 4, 5, 0.10, 0, 0)));

            Personajes.Add(new CartaAgua(6, "Cascada", 92, 13, 5, 10, 9,
                new Habilidad("Torrente", EfectoHabilidad.DobleGolpe, 3, 2, 0.8, 0, 1),
                new Habilidad("Lluvia Sanadora", EfectoHabilidad.CurarEquipo, 5, 6, 0.05, 0, 0)));

            // ---- TIERRA: tanques (su utilidad sube la defensa o devuelve daño) ----
            Personajes.Add(new CartaTierra(7, "Gaia", 125, 13, 9, 8, 6,
                new Habilidad("Raíces Punzantes", EfectoHabilidad.GolpePotenciado, 3, 3, 1.8, 0.3, 0),
                new Habilidad("Muro de Roca", EfectoHabilidad.PotenciarDefensa, 3, 4, 0, 0, 2)));

            Personajes.Add(new CartaTierra(8, "Gólem", 130, 11, 11, 8, 5,
                new Habilidad("Puño Sísmico", EfectoHabilidad.GolpePotenciado, 4, 4, 2.0, 0.2, 0),
                new Habilidad("Coraza Espinosa", EfectoHabilidad.Reflejo, 3, 4, 0.5, 0, 2)));

            Personajes.Add(new CartaTierra(9, "Sílex", 120, 15, 8, 8, 4,
                new Habilidad("Esquirla Cortante", EfectoHabilidad.DobleGolpe, 3, 3, 0.9, 0, 0),
                new Habilidad("Filo Reflectante", EfectoHabilidad.Reflejo, 4, 5, 0.6, 0, 2)));

            // ---- AIRE: veloces (su utilidad da energía, potencia el ataque o cura) ----
            Personajes.Add(new CartaAire(10, "Céfiro", 88, 16, 4, 12, 4,
                new Habilidad("Ráfaga Veloz", EfectoHabilidad.DobleGolpe, 3, 2, 1.0, 0, 2),
                new Habilidad("Viento a Favor", EfectoHabilidad.RecargarEnergia, 0, 3, 0, 0, 6)));

            Personajes.Add(new CartaAire(11, "Huracán", 80, 17, 3, 12, 3,
                new Habilidad("Tornado Cortante", EfectoHabilidad.GolpePotenciado, 4, 3, 2.1, 0.3, 0),
                new Habilidad("Ojo de la Tormenta", EfectoHabilidad.PotenciarAtaque, 3, 4, 0.35, 0, 2)));

            Personajes.Add(new CartaAire(12, "Brisa", 88, 14, 5, 12, 6,
                new Habilidad("Cuchilla de Viento", EfectoHabilidad.DobleGolpe, 3, 2, 0.7, 0, 0),
                new Habilidad("Soplo Sanador", EfectoHabilidad.Curar, 3, 3, 0.17, 0, 0)));
        }

        private static void CargarEscenarios()
        {
            // Escenario(id, nombre, elemento afín, % de bonus de ataque, descripción)
            Escenarios.Add(new Escenario(1, "Volcán Ardiente", Elemento.Fuego, 10, "Ríos de lava potencian a las cartas de Fuego."));
            Escenarios.Add(new Escenario(2, "Lago Cristalino", Elemento.Agua, 10, "Aguas serenas potencian a las cartas de Agua."));
            Escenarios.Add(new Escenario(3, "Cañón de Roca", Elemento.Tierra, 10, "Paredes de piedra potencian a las cartas de Tierra."));
            Escenarios.Add(new Escenario(4, "Cumbre Ventosa", Elemento.Aire, 10, "Vientos de altura potencian a las cartas de Aire."));
        }
    }
}
