using System;

namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Facción Tierra: fuerte contra Agua, débil contra Aire.
    /// Habilidad: Muro de Roca (aumenta su defensa temporalmente durante 2 turnos rivales).
    /// </summary>
    public class CartaTierra : Personaje
    {
        private const int DuracionMuro = 2;

        private int bonusDefensa;
        private int turnosMuro;

        public CartaTierra(int id, string nombre, int hpMax, int atk, int defensa,
                           int energiaMax, int empatia, int costoHabilidad, int cooldownHabilidad)
            : base(id, nombre, Elemento.Tierra, hpMax, atk, defensa, energiaMax, empatia,
                   "Muro de Roca", costoHabilidad, cooldownHabilidad)
        {
        }

        public override bool HabilidadEsOfensiva
        {
            get { return false; }
        }

        /// <summary>Bonus de defensa que está activo ahora mismo (0 si no hay muro).</summary>
        public int BonusDefensaActual
        {
            get { return bonusDefensa; }
        }

        public int TurnosMuroRestantes
        {
            get { return turnosMuro; }
        }

        public override int DefensaEfectiva
        {
            get { return Defensa + bonusDefensa; }
        }

        public override Personaje CrearCopia()
        {
            return new CartaTierra(Id, Nombre, HpMax, Atk, Defensa, EnergiaMax, Empatia, CostoHabilidad, CooldownHabilidad);
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

        /// <param name="objetivo">Se ignora: la habilidad siempre afecta a la propia carta.</param>
        public override string UsarHabilidadEspecial(Personaje objetivo)
        {
            if (!PuedeUsarHabilidad()) return MensajeHabilidadNoDisponible();

            ConsumirHabilidad();
            bonusDefensa = Math.Max(2, Defensa / 2 + Empatia / 2);
            turnosMuro = DuracionMuro;
            return Nombre + " levanta un " + NombreHabilidad + ": +" + bonusDefensa
                   + " de defensa durante " + turnosMuro + " turno(s).";
        }

        public override void IniciarTurno()
        {
            base.IniciarTurno();
            if (turnosMuro > 0)
            {
                turnosMuro--;
                if (turnosMuro == 0) bonusDefensa = 0;
            }
        }

        public override void AlSalirDelCampo()
        {
            bonusDefensa = 0;
            turnosMuro = 0;
        }
    }
}
