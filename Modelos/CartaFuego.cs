namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Facción Fuego (daño puro). Su habilidad de utilidad potencia el ataque.
    /// Hereda todo de Personaje; solo escribe lo que es propio de su elemento:
    /// el triángulo elemental (ObtenerMultiplicador) y la manera de copiarse (CrearCopia).
    /// </summary>
    public class CartaFuego : Personaje
    {
        public CartaFuego(int id, string nombre, int hpMax, int atk, int defensa,
                          int energiaMax, int empatia, Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
            : base(id, nombre, Elemento.Fuego, hpMax, atk, defensa, energiaMax, empatia, habilidadAtaque, habilidadUtilidad)
        {
        }

        /// <summary>Una copia nueva con la vida y la energía llenas (y copias de las habilidades).</summary>
        public override Personaje CrearCopia()
        {
            return new CartaFuego(Id, Nombre, HpMax, Atk, Defensa, EnergiaMax, Empatia,
                                  HabilidadAtaque.Clonar(), HabilidadUtilidad.Clonar());
        }

        /// <summary>Triángulo: Fuego gana contra Aire y pierde contra Agua.</summary>
        public override double ObtenerMultiplicador(Elemento enemigo)
        {
            if (enemigo == Elemento.Aire) return MultiplicadorVentaja;
            if (enemigo == Elemento.Agua) return MultiplicadorDesventaja;
            return MultiplicadorNeutro;
        }
    }
}
