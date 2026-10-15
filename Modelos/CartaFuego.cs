namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Facción Fuego: fuerte contra Aire, débil contra Agua.
    /// Habilidad: Llamarada (daño x2 que ignora el 50% de la defensa).
    /// </summary>
    public class CartaFuego : Personaje
    {
        private const double FactorLlamarada = 2.0;
        private const double DefensaIgnoradaLlamarada = 0.5;

        public CartaFuego(int id, string nombre, int hpMax, int atk, int defensa,
                          int energiaMax, int empatia, int costoHabilidad, int cooldownHabilidad)
            : base(id, nombre, Elemento.Fuego, hpMax, atk, defensa, energiaMax, empatia,
                   "Llamarada", costoHabilidad, cooldownHabilidad)
        {
        }

        public override bool HabilidadEsOfensiva
        {
            get { return true; }
        }

        public override Personaje CrearCopia()
        {
            return new CartaFuego(Id, Nombre, HpMax, Atk, Defensa, EnergiaMax, Empatia, CostoHabilidad, CooldownHabilidad);
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

        public override string UsarHabilidadEspecial(Personaje objetivo)
        {
            ValidarObjetivo(objetivo);
            if (!PuedeUsarHabilidad()) return MensajeHabilidadNoDisponible();
            if (!objetivo.EstaVivo) return NombreHabilidad + " no puede dirigirse a un objetivo derrotado.";

            double mult = ObtenerMultiplicador(objetivo.Elemento);
            int dano = CalcularDanoPotenciado(objetivo, FactorLlamarada, DefensaIgnoradaLlamarada);
            ConsumirHabilidad();
            int real = objetivo.RecibirDano(dano);
            return Nombre + " lanza " + NombreHabilidad + " sobre " + objetivo.Nombre
                   + " y causa " + real + " de daño (ignora 50% de defensa)" + DescribirEfectividad(mult) + ".";
        }
    }
}
