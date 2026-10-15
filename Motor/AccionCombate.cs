namespace ArenaElemental.Motor
{
    /// <summary>Lo que un equipo puede hacer en su turno.</summary>
    public enum TipoAccion
    {
        Atacar,
        Habilidad,
        Relevo,
        Pasar
    }

    /// <summary>Cómo va el combate.</summary>
    public enum EstadoCombate
    {
        EnCurso,
        VictoriaJugador,
        VictoriaRival
    }

    /// <summary>
    /// La acción que un equipo elige en su turno. No se crea con "new": se usan los métodos
    /// de abajo (AccionCombate.Atacar(), AccionCombate.Habilidad(0)...), que son más fáciles de leer.
    /// </summary>
    public class AccionCombate
    {
        private AccionCombate(TipoAccion tipo, int numeroHabilidad, int indice)
        {
            Tipo = tipo;
            NumeroHabilidad = numeroHabilidad;
            Indice = indice;
        }

        public TipoAccion Tipo { get; private set; }

        /// <summary>Solo para Habilidad: 0 = habilidad de ataque, 1 = habilidad de utilidad.</summary>
        public int NumeroHabilidad { get; private set; }

        /// <summary>Solo para curar a un aliado: su posición en el equipo (-1 = no se eligió aliado).</summary>
        public int Indice { get; private set; }

        public static AccionCombate Atacar()
        {
            return new AccionCombate(TipoAccion.Atacar, -1, -1);
        }

        public static AccionCombate Habilidad(int numero)
        {
            return new AccionCombate(TipoAccion.Habilidad, numero, -1);
        }

        public static AccionCombate HabilidadSobreAliado(int numero, int indiceAliado)
        {
            return new AccionCombate(TipoAccion.Habilidad, numero, indiceAliado);
        }

        public static AccionCombate Relevar()
        {
            return new AccionCombate(TipoAccion.Relevo, -1, -1);
        }

        public static AccionCombate Pasar()
        {
            return new AccionCombate(TipoAccion.Pasar, -1, -1);
        }
    }
}
