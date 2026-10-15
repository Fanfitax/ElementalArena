namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Facción Fuego (daño puro): fuerte contra Aire, débil contra Agua.
    /// Sus habilidades de utilidad potencian el ataque.
    /// </summary>
    public class CartaFuego : Personaje
    {
        public CartaFuego(int id, string nombre, int hpMax, int atk, int defensa,
                          int energiaMax, int empatia, Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
            : base(id, nombre, Elemento.Fuego, hpMax, atk, defensa, energiaMax, empatia, habilidadAtaque, habilidadUtilidad)
        {
        }

        public override Personaje CrearCopia()
        {
            return new CartaFuego(Id, Nombre, HpMax, Atk, Defensa, EnergiaMax, Empatia,
                                  HabilidadAtaque.Clonar(), HabilidadUtilidad.Clonar());
        }

        public override double ObtenerMultiplicador(Elemento enemigo)
        {
            switch (enemigo)
            {
                case Elemento.Aire: return MultiplicadorVentaja;      // Fuego > Aire
                case Elemento.Agua: return MultiplicadorDesventaja;   // Agua > Fuego
                default: return MultiplicadorNeutro;
            }
        }
    }
}
