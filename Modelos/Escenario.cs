using System;

namespace ArenaElemental.Modelos
{
    /// <summary>Escenario de combate: potencia el ataque de las cartas de su elemento afín.</summary>
    public class Escenario
    {
        private int id;
        private string nombre;
        private int bonusAtaquePct;

        public Escenario(int id, string nombre, Elemento elementoAfin, int bonusAtaquePct, string descripcion)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("id", id, "Id no puede ser negativo.");
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El escenario necesita nombre.", "nombre");
            if (bonusAtaquePct < 0) throw new ArgumentOutOfRangeException("bonusAtaquePct", bonusAtaquePct, "El bonus no puede ser negativo.");
            this.id = id;
            this.nombre = nombre;
            ElementoAfin = elementoAfin;
            this.bonusAtaquePct = bonusAtaquePct;
            Descripcion = descripcion ?? string.Empty;
        }

        public int Id { get { return id; } }
        public string Nombre { get { return nombre; } }
        public Elemento ElementoAfin { get; private set; }
        public int BonusAtaquePct { get { return bonusAtaquePct; } }
        public string Descripcion { get; private set; }

        /// <summary>Multiplicador de ataque para un elemento en este escenario (1.0 si no es el afín).</summary>
        public double MultiplicadorPara(Elemento elemento)
        {
            return elemento == ElementoAfin ? 1.0 + bonusAtaquePct / 100.0 : 1.0;
        }

        public void AplicarA(Personaje personaje)
        {
            if (personaje == null) throw new ArgumentNullException("personaje");
            personaje.MultiplicadorEscenario = MultiplicadorPara(personaje.Elemento);
        }

        public override string ToString()
        {
            return Nombre + " (" + ElementoAfin + " +" + bonusAtaquePct + "% ATK)";
        }
    }
}
