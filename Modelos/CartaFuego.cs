namespace ArenaElemental.Modelos
{
    // Facción Fuego (daño puro). Su habilidad de utilidad potencia el ataque.
    // Hereda todo de Personaje; solo escribe lo que es propio de su elemento:
    // el triángulo elemental (ObtenerMultiplicador) y la manera de copiarse (CrearCopia).
    public class CartaFuego : Personaje
    {
        public CartaFuego(int id, string nombre, string nombreIngles, int hpMax, int atk, int defensa,
                          int energiaMax, int empatia, Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
            : base(id, nombre, nombreIngles, Elemento.Fuego, hpMax, atk, defensa, energiaMax, empatia, habilidadAtaque, habilidadUtilidad)
        {
        }

        // Una copia nueva con la vida y la energía llenas (y copias de las habilidades).
        public override Personaje CrearCopia()
        {
            return new CartaFuego(Id, Clave, NombreIngles, HpMax, Atk, Defensa, EnergiaMax, Empatia,
                                  HabilidadAtaque.Clonar(), HabilidadUtilidad.Clonar());
        }

        // Triángulo: Fuego gana contra Aire y pierde contra Agua.
        public override double ObtenerMultiplicador(Elemento enemigo)
        {
            if (enemigo == Elemento.Aire) return MultiplicadorVentaja;
            if (enemigo == Elemento.Agua) return MultiplicadorDesventaja;
            return MultiplicadorNeutro;
        }
    }
}
