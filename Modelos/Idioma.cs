namespace ArenaElemental.Modelos
{
    // Idioma del juego: 0 = Español, 1 = English.
    // Cada texto que ve el jugador se escribe así:  Idioma.Texto("texto en español", "text in English")
    // El menú principal cambia el valor de "Actual" y desde ese momento todos los textos salen en ese idioma.
    public static class Idioma
    {
        public const int Espanol = 0;
        public const int Ingles = 1;

        public static int Actual = Espanol;

        // Devuelve el texto en español o en inglés, según el idioma elegido.
        public static string Texto(string espanol, string ingles)
        {
            if (Actual == Ingles) return ingles;
            return espanol;
        }

        // El nombre de cada elemento en el idioma elegido (el enum siempre está en español).
        public static string NombreElemento(Elemento elemento)
        {
            if (elemento == Elemento.Fuego) return Texto("Fuego", "Fire");
            if (elemento == Elemento.Agua) return Texto("Agua", "Water");
            if (elemento == Elemento.Tierra) return Texto("Tierra", "Earth");
            return Texto("Aire", "Air");
        }

        // El resultado de una partida se guarda siempre en español ("Victoria", "Derrota", "Abandono")
        // y solo se traduce al mostrarlo en pantalla.
        public static string NombreResultado(string resultado)
        {
            if (resultado == "Victoria") return Texto("Victoria", "Victory");
            if (resultado == "Derrota") return Texto("Derrota", "Defeat");
            if (resultado == "Abandono") return Texto("Abandono", "Forfeit");
            return resultado;
        }
    }
}
