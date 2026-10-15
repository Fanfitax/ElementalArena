namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Facción Agua (apoyo): fuerte contra Fuego, débil contra Tierra.
    /// Sus habilidades de utilidad curan al equipo.
    /// </summary>
    public class CartaAgua : Personaje
    {
        public CartaAgua(int id, string nombre, int hpMax, int atk, int defensa,
                         int energiaMax, int empatia, Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
            : base(id, nombre, Elemento.Agua, hpMax, atk, defensa, energiaMax, empatia, habilidadAtaque, habilidadUtilidad)
        {
        }

        public override Personaje CrearCopia()
        {
            return new CartaAgua(Id, Nombre, HpMax, Atk, Defensa, EnergiaMax, Empatia,
                                 HabilidadAtaque.Clonar(), HabilidadUtilidad.Clonar());
        }

        public override double ObtenerMultiplicador(Elemento enemigo)
        {
            switch (enemigo)
            {
                case Elemento.Fuego: return MultiplicadorVentaja;       // Agua > Fuego
                case Elemento.Tierra: return MultiplicadorDesventaja;   // Tierra > Agua
                default: return MultiplicadorNeutro;
            }
        }
    }
}
