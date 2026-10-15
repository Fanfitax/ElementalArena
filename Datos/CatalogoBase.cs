using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Datos
{
    /// <summary>
    /// Datos fijos del juego guardados en listas estáticas (no hay base de datos):
    /// los 12 personajes predefinidos y los 4 escenarios.
    /// Cada personaje tiene dos habilidades: la primera de ataque y la segunda de utilidad.
    /// </summary>
    public static class CatalogoBase
    {
        public static List<Personaje> Personajes = new List<Personaje>();
        public static List<Escenario> Escenarios = new List<Escenario>();

        // El constructor estático se ejecuta una sola vez, la primera vez que se usa la clase.
        static CatalogoBase()
        {
            // Personaje(id, nombre, HP, ATK, DEF, ENG, EMP, habilidad de ataque, habilidad de utilidad)
            // Habilidad.Xxx(nombre, costo de energía, recarga en turnos, ...)

            // ---- FUEGO: daño puro (su utilidad potencia el ataque) ----
            Personajes.Add(new CartaFuego(1, "Pyra", 90, 18, 4, 10, 3,
                Habilidad.Golpe("Llamarada", 4, 3, 2.0, 0.5),
                Habilidad.PotenciarAtaque("Aura Ígnea", 3, 4, 0.30, 2)));
            Personajes.Add(new CartaFuego(2, "Ignar", 100, 16, 6, 10, 4,
                Habilidad.Golpe("Tajo Carmesí", 4, 3, 1.7, 0.7),
                Habilidad.PotenciarAtaque("Grito de Guerra", 3, 4, 0.35, 2)));
            Personajes.Add(new CartaFuego(3, "Cenizo", 80, 20, 3, 10, 2,
                Habilidad.GolpeConRetroceso("Explosión de Brasas", 4, 3, 2.3, 0.12),
                Habilidad.PotenciarAtaque("Combustión Interna", 3, 5, 0.45, 2)));

            // ---- AGUA: apoyo (su utilidad cura) ----
            Personajes.Add(new CartaAgua(4, "Marina", 100, 13, 5, 10, 8,
                Habilidad.Golpe("Chorro a Presión", 3, 3, 1.6, 0.3),
                Habilidad.Curacion("Marea Curativa", 4, 4, 0.20)));
            Personajes.Add(new CartaAgua(5, "Tidal", 110, 12, 6, 10, 7,
                Habilidad.GolpeVampirico("Oleada Voraz", 4, 3, 1.5, 0.4),
                Habilidad.Curacion("Oleaje Reparador", 4, 5, 0.20)));
            Personajes.Add(new CartaAgua(6, "Cascada", 95, 14, 5, 10, 9,
                Habilidad.GolpeDoble("Torrente", 3, 2, 0.8, 1),
                Habilidad.CuracionEquipo("Lluvia Sanadora", 5, 5, 0.15)));

            // ---- TIERRA: tanques (su utilidad sube la defensa o devuelve daño) ----
            Personajes.Add(new CartaTierra(7, "Gaia", 125, 12, 9, 8, 6,
                Habilidad.Golpe("Raíces Punzantes", 3, 3, 1.8, 0.3),
                Habilidad.PotenciarDefensa("Muro de Roca", 3, 4, 2)));
            Personajes.Add(new CartaTierra(8, "Gólem", 135, 11, 11, 8, 5,
                Habilidad.Golpe("Puño Sísmico", 4, 4, 2.0, 0.2),
                Habilidad.Reflejo("Coraza Espinosa", 3, 4, 0.5, 2)));
            Personajes.Add(new CartaTierra(9, "Sílex", 120, 13, 8, 8, 4,
                Habilidad.GolpeDoble("Esquirla Cortante", 3, 3, 0.9, 0),
                Habilidad.Reflejo("Filo Reflectante", 4, 5, 0.6, 2)));

            // ---- AIRE: veloces (su utilidad da energía, potencia o cura) ----
            Personajes.Add(new CartaAire(10, "Céfiro", 85, 15, 4, 12, 4,
                Habilidad.GolpeDoble("Ráfaga Veloz", 3, 2, 1.0, 2),
                Habilidad.RecargaEnergia("Viento a Favor", 0, 3, 6)));
            Personajes.Add(new CartaAire(11, "Huracán", 80, 17, 3, 12, 3,
                Habilidad.Golpe("Tornado Cortante", 4, 3, 2.1, 0.3),
                Habilidad.PotenciarAtaque("Ojo de la Tormenta", 3, 4, 0.35, 2)));
            Personajes.Add(new CartaAire(12, "Brisa", 88, 14, 5, 12, 6,
                Habilidad.GolpeDoble("Cuchilla de Viento", 3, 2, 0.7, 0),
                Habilidad.Curacion("Soplo Sanador", 3, 3, 0.20)));

            Escenarios.Add(new Escenario(1, "Volcán Ardiente", Elemento.Fuego,  10, "Ríos de lava potencian a las cartas de Fuego."));
            Escenarios.Add(new Escenario(2, "Lago Cristalino", Elemento.Agua,   10, "Aguas serenas potencian a las cartas de Agua."));
            Escenarios.Add(new Escenario(3, "Cañón de Roca",   Elemento.Tierra, 10, "Paredes de piedra potencian a las cartas de Tierra."));
            Escenarios.Add(new Escenario(4, "Cumbre Ventosa",  Elemento.Aire,   10, "Vientos de altura potencian a las cartas de Aire."));
        }
    }
}
