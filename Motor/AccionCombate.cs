namespace ArenaElemental.Motor
{
    // Lo que un equipo puede hacer en su turno.
    public enum TipoAccion
    {
        Atacar,
        Habilidad,
        Relevo,
        Pasar
    }

    // Cómo va el combate.
    public enum EstadoCombate
    {
        EnCurso,
        VictoriaJugador,
        VictoriaRival
    }

    // La acción que un equipo elige en su turno. Se crea con "new", por ejemplo:
    //   new AccionCombate(TipoAccion.Atacar)
    //   new AccionCombate(TipoAccion.Habilidad, 0)         habilidad de ataque
    //   new AccionCombate(TipoAccion.Habilidad, 1, 2)      habilidad de utilidad sobre el aliado en la posición 2
    public class AccionCombate
    {
        public TipoAccion Tipo { get; private set; }

        // Solo para Habilidad: 0 = habilidad de ataque, 1 = habilidad de utilidad.
        public int NumeroHabilidad { get; private set; }

        // Solo para curar a un aliado: su posición en el equipo (-1 = no se eligió aliado).
        public int Indice { get; private set; }

        public AccionCombate(TipoAccion tipo)
        {
            Tipo = tipo;
            NumeroHabilidad = -1;
            Indice = -1;
        }

        public AccionCombate(TipoAccion tipo, int numeroHabilidad)
        {
            Tipo = tipo;
            NumeroHabilidad = numeroHabilidad;
            Indice = -1;
        }

        public AccionCombate(TipoAccion tipo, int numeroHabilidad, int indiceAliado)
        {
            Tipo = tipo;
            NumeroHabilidad = numeroHabilidad;
            Indice = indiceAliado;
        }
    }
}
