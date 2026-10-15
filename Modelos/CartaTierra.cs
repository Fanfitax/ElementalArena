namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Facción Tierra (tanques). Su habilidad de utilidad sube la defensa o devuelve daño.
    /// Hereda todo de Personaje; solo escribe lo que es propio de su elemento:
    /// el triángulo elemental (ObtenerMultiplicador) y la manera de copiarse (CrearCopia).
    /// </summary>
    public class CartaTierra : Personaje
    {
        public CartaTierra(int id, string nombre, int hpMax, int atk, int defensa,
                           int energiaMax, int empatia, Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
            : base(id, nombre, Elemento.Tierra, hpMax, atk, defensa, energiaMax, empatia, habilidadAtaque, habilidadUtilidad)
        {
        }

        /// <summary>Una copia nueva con la vida y la energía llenas (y copias de las habilidades).</summary>
        public override Personaje CrearCopia()
        {
            return new CartaTierra(Id, Nombre, HpMax, Atk, Defensa, EnergiaMax, Empatia,
                                   HabilidadAtaque.Clonar(), HabilidadUtilidad.Clonar());
        }

        /// <summary>Triángulo: Tierra gana contra Agua y pierde contra Aire.</summary>
        public override double ObtenerMultiplicador(Elemento enemigo)
        {
            if (enemigo == Elemento.Agua) return MultiplicadorVentaja;
            if (enemigo == Elemento.Aire) return MultiplicadorDesventaja;
            return MultiplicadorNeutro;
        }
    }
}
