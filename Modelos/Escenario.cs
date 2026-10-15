namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Escenario de combate. Le da un bonus de ataque a los personajes de su elemento afín
    /// (por ejemplo, el Volcán Ardiente da +10% de ataque a los de Fuego).
    /// </summary>
    public class Escenario
    {
        public Escenario(int id, string nombre, Elemento elementoAfin, int bonusAtaquePct, string descripcion)
        {
            Id = id;
            Nombre = nombre;
            ElementoAfin = elementoAfin;
            BonusAtaquePct = bonusAtaquePct;
            Descripcion = descripcion;
        }

        public int Id { get; private set; }
        public string Nombre { get; private set; }
        public Elemento ElementoAfin { get; private set; }
        public int BonusAtaquePct { get; private set; }
        public string Descripcion { get; private set; }

        /// <summary>Multiplicador de ataque para un elemento: 1.1 si es el afín del escenario y 1.0 si no.</summary>
        public double MultiplicadorPara(Elemento elemento)
        {
            if (elemento == ElementoAfin)
                return 1.0 + BonusAtaquePct / 100.0;
            return 1.0;
        }

        /// <summary>Le pone al personaje el multiplicador de este escenario.</summary>
        public void AplicarA(Personaje personaje)
        {
            personaje.MultiplicadorEscenario = MultiplicadorPara(personaje.Elemento);
        }

        public override string ToString()
        {
            return Nombre + " (" + ElementoAfin + " +" + BonusAtaquePct + "% ATK)";
        }
    }
}
