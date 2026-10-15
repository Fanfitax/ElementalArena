namespace ArenaElemental.Motor
{
    public enum TipoAccion
    {
        Atacar,
        Habilidad,
        Relevo,
        Pasar
    }

    public enum EstadoCombate
    {
        EnCurso,
        VictoriaJugador,
        VictoriaRival
    }

    /// <summary>Acción que un equipo elige en su turno.</summary>
    public class AccionCombate
    {
        private AccionCombate(TipoAccion tipo, int indice)
        {
            Tipo = tipo;
            Indice = indice;
        }

        public TipoAccion Tipo { get; private set; }

        /// <summary>
        /// Relevo: posición (0-2) del personaje que entra.
        /// Habilidad de apoyo: posición del aliado a apoyar (-1 = el personaje activo).
        /// </summary>
        public int Indice { get; private set; }

        public static AccionCombate Atacar() { return new AccionCombate(TipoAccion.Atacar, -1); }
        public static AccionCombate Habilidad() { return new AccionCombate(TipoAccion.Habilidad, -1); }
        public static AccionCombate HabilidadSobreAliado(int indiceAliado) { return new AccionCombate(TipoAccion.Habilidad, indiceAliado); }
        public static AccionCombate Relevar(int indice) { return new AccionCombate(TipoAccion.Relevo, indice); }
        public static AccionCombate Pasar() { return new AccionCombate(TipoAccion.Pasar, -1); }
    }
}
