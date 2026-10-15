namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Facción Aire (veloz): fuerte contra Tierra, débil contra Fuego.
    /// Sus habilidades de utilidad dan energía, potencian el ataque o curan.
    /// </summary>
    public class CartaAire : Personaje
    {
        public CartaAire(int id, string nombre, int hpMax, int atk, int defensa,
                         int energiaMax, int empatia, Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
            : base(id, nombre, Elemento.Aire, hpMax, atk, defensa, energiaMax, empatia, habilidadAtaque, habilidadUtilidad)
        {
        }

        public override Personaje CrearCopia()
        {
            return new CartaAire(Id, Nombre, HpMax, Atk, Defensa, EnergiaMax, Empatia,
                                 HabilidadAtaque.Clonar(), HabilidadUtilidad.Clonar());
        }

        public override double ObtenerMultiplicador(Elemento enemigo)
        {
            switch (enemigo)
            {
                case Elemento.Tierra: return MultiplicadorVentaja;      // Aire > Tierra
                case Elemento.Fuego: return MultiplicadorDesventaja;    // Fuego > Aire
                default: return MultiplicadorNeutro;
            }
        }
    }
}
