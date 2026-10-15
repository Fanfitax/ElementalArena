using System;

namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Facción Agua: fuerte contra Fuego, débil contra Tierra.
    /// Habilidad: Marea Curativa (cura al aliado indicado o a sí misma; escala con la Empatía).
    /// </summary>
    public class CartaAgua : Personaje
    {
        private const double PorcentajeCuracion = 0.25;
        private const int CuracionPorEmpatia = 2;

        public CartaAgua(int id, string nombre, int hpMax, int atk, int defensa,
                         int energiaMax, int empatia, int costoHabilidad, int cooldownHabilidad)
            : base(id, nombre, Elemento.Agua, hpMax, atk, defensa, energiaMax, empatia,
                   "Marea Curativa", costoHabilidad, cooldownHabilidad)
        {
        }

        public override bool HabilidadEsOfensiva
        {
            get { return false; }
        }

        public override Personaje CrearCopia()
        {
            return new CartaAgua(Id, Nombre, HpMax, Atk, Defensa, EnergiaMax, Empatia, CostoHabilidad, CooldownHabilidad);
        }

        public override double ObtenerMultiplicador(Elemento enemigo)
        {
            switch (enemigo)
            {
                case Elemento.Fuego: return MultiplicadorVentaja;       // Agua > Fuego
                case Elemento.Tierra: return MultiplicadorDesventaja;   // Tierra > Agua
                default: return MultiplicadorNeutro;
            }
        }

        /// <summary>Cantidad de vida que cura la habilidad: 25% del HP máx. propio + 2 por punto de Empatía.</summary>
        public int CuracionDeHabilidad
        {
            get { return (int)Math.Round(HpMax * PorcentajeCuracion) + Empatia * CuracionPorEmpatia; }
        }

        /// <param name="objetivo">Aliado a curar; null = ella misma.</param>
        public override string UsarHabilidadEspecial(Personaje objetivo)
        {
            Personaje destino = objetivo ?? this;
            if (!PuedeUsarHabilidad()) return MensajeHabilidadNoDisponible();
            if (!destino.EstaVivo) return NombreHabilidad + " no puede revivir a " + destino.Nombre + ".";

            int cantidad = CuracionDeHabilidad;
            ConsumirHabilidad();
            int curado = destino.Curar(cantidad);
            return Nombre + " usa " + NombreHabilidad + " y cura " + curado + " HP a "
                   + (destino == this ? "sí misma" : destino.Nombre) + ".";
        }
    }
}
