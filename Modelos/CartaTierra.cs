namespace ArenaElemental.Modelos
{
    // Facción Tierra (tanques). Su habilidad de utilidad sube la defensa o devuelve daño.
    // Hereda todo de Personaje; solo escribe lo que es propio de su elemento:
    // el triángulo elemental (ObtenerMultiplicador) y la manera de copiarse (CrearCopia).
    public class CartaTierra : Personaje
    {
        public CartaTierra(int id, string nombre, string nombreIngles, int hpMax, int atk, int defensa,
                           int energiaMax, int empatia, Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
            : base(id, nombre, nombreIngles, Elemento.Tierra, hpMax, atk, defensa, energiaMax, empatia, habilidadAtaque, habilidadUtilidad)
        {
        }

        // Una copia nueva con la vida y la energía llenas (y copias de las habilidades).
        public override Personaje CrearCopia()
        {
            return new CartaTierra(Id, Clave, NombreIngles, HpMax, Atk, Defensa, EnergiaMax, Empatia,
                                   HabilidadAtaque.Clonar(), HabilidadUtilidad.Clonar());
        }

        // Triángulo: Tierra gana contra Agua y pierde contra Aire.
        public override double ObtenerMultiplicador(Elemento enemigo)
        {
            if (enemigo == Elemento.Agua) return MultiplicadorVentaja;
            if (enemigo == Elemento.Aire) return MultiplicadorDesventaja;
            return MultiplicadorNeutro;
        }
    }
}
