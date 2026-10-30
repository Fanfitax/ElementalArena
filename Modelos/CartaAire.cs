namespace ArenaElemental.Modelos
{
    // Facción Aire (veloces). Su habilidad de utilidad da energía, potencia el ataque o cura.
    // Hereda todo de Personaje; solo escribe lo que es propio de su elemento:
    // el triángulo elemental (ObtenerMultiplicador) y la manera de copiarse (CrearCopia).
    public class CartaAire : Personaje
    {
        public CartaAire(int id, string nombre, string nombreIngles, int hpMax, int atk, int defensa,
                         int energiaMax, int empatia, Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
            : base(id, nombre, nombreIngles, Elemento.Aire, hpMax, atk, defensa, energiaMax, empatia, habilidadAtaque, habilidadUtilidad)
        {
        }

        // Una copia nueva con la vida y la energía llenas (y copias de las habilidades).
        public override Personaje CrearCopia()
        {
            return new CartaAire(Id, Clave, NombreIngles, HpMax, Atk, Defensa, EnergiaMax, Empatia,
                                 HabilidadAtaque.Clonar(), HabilidadUtilidad.Clonar());
        }

        // Triángulo: Aire gana contra Tierra y pierde contra Fuego.
        public override double ObtenerMultiplicador(Elemento enemigo)
        {
            if (enemigo == Elemento.Tierra) return MultiplicadorVentaja;
            if (enemigo == Elemento.Fuego) return MultiplicadorDesventaja;
            return MultiplicadorNeutro;
        }
    }
}
