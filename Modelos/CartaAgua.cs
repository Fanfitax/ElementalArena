namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Facción Agua (apoyo). Su habilidad de utilidad cura.
    /// Hereda todo de Personaje; solo escribe lo que es propio de su elemento:
    /// el triángulo elemental (ObtenerMultiplicador) y la manera de copiarse (CrearCopia).
    /// </summary>
    public class CartaAgua : Personaje
    {
        public CartaAgua(int id, string nombre, int hpMax, int atk, int defensa,
                         int energiaMax, int empatia, Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
            : base(id, nombre, Elemento.Agua, hpMax, atk, defensa, energiaMax, empatia, habilidadAtaque, habilidadUtilidad)
        {
        }

        /// <summary>Una copia nueva con la vida y la energía llenas (y copias de las habilidades).</summary>
        public override Personaje CrearCopia()
        {
            return new CartaAgua(Id, Nombre, HpMax, Atk, Defensa, EnergiaMax, Empatia,
                                 HabilidadAtaque.Clonar(), HabilidadUtilidad.Clonar());
        }

        /// <summary>Triángulo: Agua gana contra Fuego y pierde contra Tierra.</summary>
        public override double ObtenerMultiplicador(Elemento enemigo)
        {
            if (enemigo == Elemento.Fuego) return MultiplicadorVentaja;
            if (enemigo == Elemento.Tierra) return MultiplicadorDesventaja;
            return MultiplicadorNeutro;
        }
    }
}
