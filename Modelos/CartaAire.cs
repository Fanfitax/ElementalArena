namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Facción Aire: fuerte contra Tierra, débil contra Fuego.
    /// Habilidad: Ráfaga Veloz (dos golpes al 75% de potencia cada uno y recupera 2 de energía).
    /// </summary>
    public class CartaAire : Personaje
    {
        private const double FactorGolpe = 0.75;
        private const int EnergiaRecuperada = 2;

        public CartaAire(int id, string nombre, int hpMax, int atk, int defensa,
                         int energiaMax, int empatia, int costoHabilidad, int cooldownHabilidad)
            : base(id, nombre, Elemento.Aire, hpMax, atk, defensa, energiaMax, empatia,
                   "Ráfaga Veloz", costoHabilidad, cooldownHabilidad)
        {
        }

        public override bool HabilidadEsOfensiva
        {
            get { return true; }
        }

        public override Personaje CrearCopia()
        {
            return new CartaAire(Id, Nombre, HpMax, Atk, Defensa, EnergiaMax, Empatia, CostoHabilidad, CooldownHabilidad);
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

        public override string UsarHabilidadEspecial(Personaje objetivo)
        {
            ValidarObjetivo(objetivo);
            if (!PuedeUsarHabilidad()) return MensajeHabilidadNoDisponible();
            if (!objetivo.EstaVivo) return NombreHabilidad + " no puede dirigirse a un objetivo derrotado.";

            double mult = ObtenerMultiplicador(objetivo.Elemento);
            ConsumirHabilidad();

            int total = 0;
            for (int golpe = 0; golpe < 2; golpe++)
            {
                // Se recalcula en cada golpe por si el primer impacto cambia el estado del objetivo.
                total += objetivo.RecibirDano(CalcularDanoPotenciado(objetivo, FactorGolpe, 0.0));
            }
            int ganada = RecuperarEnergia(EnergiaRecuperada);

            return Nombre + " desata " + NombreHabilidad + " sobre " + objetivo.Nombre
                   + " (2 golpes, " + total + " de daño total" + DescribirEfectividad(mult) + ")"
                   + (ganada > 0 ? " y recupera " + ganada + " de energía." : ".");
        }
    }
}
