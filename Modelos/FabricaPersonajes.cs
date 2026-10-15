using System;

namespace ArenaElemental.Modelos
{
    /// <summary>Crea la clase hija correcta según el elemento (usada al leer personajes de la base de datos).</summary>
    public static class FabricaPersonajes
    {
        public static Personaje Crear(int id, string nombre, Elemento elemento,
                                      int hpMax, int atk, int defensa, int energiaMax, int empatia,
                                      int costoHabilidad, int cooldownHabilidad)
        {
            switch (elemento)
            {
                case Elemento.Fuego:
                    return new CartaFuego(id, nombre, hpMax, atk, defensa, energiaMax, empatia, costoHabilidad, cooldownHabilidad);
                case Elemento.Agua:
                    return new CartaAgua(id, nombre, hpMax, atk, defensa, energiaMax, empatia, costoHabilidad, cooldownHabilidad);
                case Elemento.Tierra:
                    return new CartaTierra(id, nombre, hpMax, atk, defensa, energiaMax, empatia, costoHabilidad, cooldownHabilidad);
                case Elemento.Aire:
                    return new CartaAire(id, nombre, hpMax, atk, defensa, energiaMax, empatia, costoHabilidad, cooldownHabilidad);
                default:
                    throw new ArgumentOutOfRangeException("elemento", elemento, "Elemento no soportado.");
            }
        }

        /// <summary>Convierte el texto guardado en la base de datos ("Fuego", "Agua"...) en Elemento.</summary>
        public static Elemento ParsearElemento(string texto)
        {
            return (Elemento)Enum.Parse(typeof(Elemento), texto, true);
        }
    }
}
