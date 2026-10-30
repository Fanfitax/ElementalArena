using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Datos
{
    // Los datos fijos del juego guardados en listas estáticas (no hay base de datos):
    // los 12 personajes y los 4 escenarios.
    //
    // Cada personaje se crea así:
    //   new CartaXxx(id, nombre, nombre en inglés, vida, ataque, defensa, energía máx., empatía, habilidad de ataque, habilidad de utilidad)
    // y cada habilidad así:
    //   new Habilidad(nombre, nombre en inglés, efecto, costo de energía, recarga en turnos, potencia, extra, duración)
    // (en la clase Habilidad hay una tabla que explica qué significan potencia, extra y duración en cada efecto).
    // Los escenarios llevan también su nombre y su descripción en inglés (el idioma se elige en el menú, ver Idioma.cs).
    public static class CatalogoBase
    {
        public static List<Personaje> Personajes = new List<Personaje>();
        public static List<Escenario> Escenarios = new List<Escenario>();

        // Se llama una sola vez al iniciar el programa (desde Main) para llenar las dos listas.
        public static void Cargar()
        {
            CargarPersonajes();
            CargarEscenarios();
        }

        private static void CargarPersonajes()
        {
            // ---- FUEGO: daño puro (su utilidad potencia el ataque) ----
            Personajes.Add(new CartaFuego(1, "Hestia", "Hestia", 90, 19, 4, 10, 3,
                new Habilidad("Llamarada", "Blaze", EfectoHabilidad.GolpePotenciado, 4, 3, 2.0, 0.5, 0),
                new Habilidad("Aura Ígnea", "Fiery Aura", EfectoHabilidad.PotenciarAtaque, 3, 4, 0.30, 0, 2)));

            Personajes.Add(new CartaFuego(2, "Prometeo", "Prometheus", 105, 16, 6, 10, 4,
                new Habilidad("Tajo Carmesí", "Crimson Slash", EfectoHabilidad.GolpePotenciado, 4, 3, 1.7, 0.7, 0),
                new Habilidad("Grito de Guerra", "War Cry", EfectoHabilidad.PotenciarAtaque, 3, 4, 0.35, 0, 2)));

            Personajes.Add(new CartaFuego(3, "Faetón", "Phaethon", 85, 20, 4, 10, 2,
                new Habilidad("Explosión de Brasas", "Ember Blast", EfectoHabilidad.GolpeConRetroceso, 4, 3, 2.3, 0.12, 0),
                new Habilidad("Combustión Interna", "Inner Combustion", EfectoHabilidad.PotenciarAtaque, 3, 5, 0.45, 0, 2)));

            // ---- AGUA: apoyo (su utilidad cura) ----
            Personajes.Add(new CartaAgua(4, "Tetis", "Thetis", 100, 13, 5, 10, 8,
                new Habilidad("Chorro a Presión", "Pressure Jet", EfectoHabilidad.GolpePotenciado, 3, 3, 1.6, 0.3, 0),
                new Habilidad("Marea Curativa", "Healing Tide", EfectoHabilidad.Curar, 4, 5, 0.10, 0, 0)));

            Personajes.Add(new CartaAgua(5, "Poseidón", "Poseidon", 100, 12, 6, 10, 7,
                new Habilidad("Oleada Voraz", "Ravenous Surge", EfectoHabilidad.GolpeVampirico, 4, 3, 1.5, 0.3, 0),
                new Habilidad("Oleaje Reparador", "Restoring Swell", EfectoHabilidad.Curar, 4, 5, 0.10, 0, 0)));

            Personajes.Add(new CartaAgua(6, "Anfitrite", "Amphitrite", 92, 13, 5, 10, 9,
                new Habilidad("Torrente", "Torrent", EfectoHabilidad.DobleGolpe, 3, 2, 0.8, 0, 1),
                new Habilidad("Lluvia Sanadora", "Healing Rain", EfectoHabilidad.CurarEquipo, 5, 6, 0.05, 0, 0)));

            // ---- TIERRA: tanques (su utilidad sube la defensa o devuelve daño) ----
            Personajes.Add(new CartaTierra(7, "Atlas", "Atlas", 125, 13, 9, 8, 6,
                new Habilidad("Raíces Punzantes", "Piercing Roots", EfectoHabilidad.GolpePotenciado, 3, 3, 1.8, 0.3, 0),
                new Habilidad("Muro de Roca", "Rock Wall", EfectoHabilidad.PotenciarDefensa, 3, 4, 0, 0, 2)));

            Personajes.Add(new CartaTierra(8, "Talos", "Talos", 130, 11, 11, 8, 5,
                new Habilidad("Puño Sísmico", "Seismic Fist", EfectoHabilidad.GolpePotenciado, 4, 4, 2.0, 0.2, 0),
                new Habilidad("Coraza Espinosa", "Thorny Armor", EfectoHabilidad.Reflejo, 3, 4, 0.5, 0, 2)));

            Personajes.Add(new CartaTierra(9, "Hefesto", "Hephaestus", 120, 15, 8, 8, 4,
                new Habilidad("Esquirla Cortante", "Cutting Shard", EfectoHabilidad.DobleGolpe, 3, 3, 0.9, 0, 0),
                new Habilidad("Filo Reflectante", "Reflecting Edge", EfectoHabilidad.Reflejo, 4, 5, 0.6, 0, 2)));

            // ---- AIRE: veloces (su utilidad da energía, potencia el ataque o cura) ----
            Personajes.Add(new CartaAire(10, "Atalanta", "Atalanta", 88, 16, 4, 12, 4,
                new Habilidad("Ráfaga Veloz", "Swift Gust", EfectoHabilidad.DobleGolpe, 3, 2, 1.0, 0, 2),
                new Habilidad("Viento a Favor", "Tailwind", EfectoHabilidad.RecargarEnergia, 0, 3, 0, 0, 6)));

            Personajes.Add(new CartaAire(11, "Tifón", "Typhon", 80, 17, 3, 12, 3,
                new Habilidad("Tornado Cortante", "Cutting Tornado", EfectoHabilidad.GolpePotenciado, 4, 3, 2.1, 0.3, 0),
                new Habilidad("Ojo de la Tormenta", "Eye of the Storm", EfectoHabilidad.PotenciarAtaque, 3, 4, 0.35, 0, 2)));

            Personajes.Add(new CartaAire(12, "Psique", "Psyche", 88, 14, 5, 12, 6,
                new Habilidad("Cuchilla de Viento", "Wind Blade", EfectoHabilidad.DobleGolpe, 3, 2, 0.7, 0, 0),
                new Habilidad("Soplo Sanador", "Healing Breath", EfectoHabilidad.Curar, 3, 3, 0.17, 0, 0)));
        }

        private static void CargarEscenarios()
        {
            // Escenario(id, nombre, elemento afín, % de bonus de ataque, descripción)
            Escenarios.Add(new Escenario(1, "Volcán Ardiente", "Blazing Volcano", Elemento.Fuego, 10, "Ríos de lava potencian a las cartas de Fuego.", "Lava rivers empower Fire cards."));
            Escenarios.Add(new Escenario(2, "Lago Cristalino", "Crystal Lake", Elemento.Agua, 10, "Aguas serenas potencian a las cartas de Agua.", "Calm waters empower Water cards."));
            Escenarios.Add(new Escenario(3, "Cañón de Roca", "Rock Canyon", Elemento.Tierra, 10, "Paredes de piedra potencian a las cartas de Tierra.", "Stone walls empower Earth cards."));
            Escenarios.Add(new Escenario(4, "Cumbre Ventosa", "Windy Summit", Elemento.Aire, 10, "Vientos de altura potencian a las cartas de Aire.", "High-altitude winds empower Air cards."));
        }
    }
}
