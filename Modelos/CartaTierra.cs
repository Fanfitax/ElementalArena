namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Facción Tierra (tanques): fuerte contra Agua, débil contra Aire.
    /// Sus habilidades de utilidad suben la defensa o devuelven el daño recibido.
    /// </summary>
    public class CartaTierra : Personaje
    {
        public CartaTierra(int id, string nombre, int hpMax, int atk, int defensa,
                           int energiaMax, int empatia, Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
            : base(id, nombre, Elemento.Tierra, hpMax, atk, defensa, energiaMax, empatia, habilidadAtaque, habilidadUtilidad)
        {
        }

        public override Personaje CrearCopia()
        {
            return new CartaTierra(Id, Nombre, HpMax, Atk, Defensa, EnergiaMax, Empatia,
                                   HabilidadAtaque.Clonar(), HabilidadUtilidad.Clonar());
        }

        public override double ObtenerMultiplicador(Elemento enemigo)
        {
            switch (enemigo)
            {
                case Elemento.Agua: return MultiplicadorVentaja;        // Tierra > Agua
                case Elemento.Aire: return MultiplicadorDesventaja;     // Aire > Tierra
                default: return MultiplicadorNeutro;
            }
        }
    }
}
