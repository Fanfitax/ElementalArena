using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    /// <summary>
    /// Equipo de 3 personajes en una partida. Siempre hay un personaje activo
    /// y los demás esperan para entrar de relevo.
    /// </summary>
    public class EquipoCombate
    {
        public const int TamanoEquipo = 3;

        private readonly List<Personaje> miembros = new List<Personaje>();
        private int indiceActivo;
        private int danoInfligido;

        /// <summary>Crea el equipo con COPIAS nuevas de los personajes (el catálogo original no se modifica).</summary>
        public EquipoCombate(string nombre, IEnumerable<Personaje> personajes, bool esJugador)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El equipo necesita nombre.", "nombre");
            if (personajes == null) throw new ArgumentNullException("personajes");

            foreach (Personaje p in personajes)
            {
                if (p == null) throw new ArgumentException("El equipo no admite personajes nulos.", "personajes");
                foreach (Personaje existente in miembros)
                {
                    if (existente.Id == p.Id)
                        throw new ArgumentException("El personaje " + p.Nombre + " está repetido en el equipo.", "personajes");
                }
                miembros.Add(p.CrearCopia());
            }
            if (miembros.Count != TamanoEquipo)
                throw new ArgumentException("El equipo debe tener exactamente " + TamanoEquipo + " personajes.", "personajes");

            Nombre = nombre.Trim();
            EsJugador = esJugador;
            indiceActivo = 0;
        }

        public string Nombre { get; private set; }
        public bool EsJugador { get; private set; }

        public ReadOnlyCollection<Personaje> Miembros
        {
            get { return miembros.AsReadOnly(); }
        }

        public int IndiceActivo
        {
            get { return indiceActivo; }
        }

        public Personaje Activo
        {
            get { return miembros[indiceActivo]; }
        }

        public int Vivos
        {
            get
            {
                int n = 0;
                foreach (Personaje p in miembros) if (p.EstaVivo) n++;
                return n;
            }
        }

        public bool TodosDerrotados
        {
            get { return Vivos == 0; }
        }

        /// <summary>Daño total que este equipo ha causado en la partida.</summary>
        public int DanoInfligido
        {
            get { return danoInfligido; }
        }

        internal void SumarDanoInfligido(int cantidad)
        {
            if (cantidad > 0) danoInfligido += cantidad;
        }

        private bool ultimaAccionFueRelevo;

        /// <summary>True si el equipo relevó en su turno anterior (no se puede relevar dos turnos seguidos).</summary>
        public bool RelevoBloqueado
        {
            get { return ultimaAccionFueRelevo; }
        }

        internal void RegistrarAccionDelTurno(bool fueRelevo)
        {
            ultimaAccionFueRelevo = fueRelevo;
        }

        public bool PuedeRelevarA(int indice)
        {
            return indice >= 0 && indice < miembros.Count
                   && indice != indiceActivo && miembros[indice].EstaVivo;
        }

        public bool HayRelevoDisponible
        {
            get
            {
                for (int i = 0; i < miembros.Count; i++) if (PuedeRelevarA(i)) return true;
                return false;
            }
        }

        /// <summary>Cambia el personaje activo. Devuelve el texto del evento.</summary>
        public string Relevar(int indice)
        {
            if (!PuedeRelevarA(indice))
                throw new InvalidOperationException("No se puede relevar a esa posición.");
            Personaje saliente = Activo;
            saliente.AlSalirDelCampo();
            indiceActivo = indice;
            return Activo.AlEntrarEnRelevo(saliente);
        }

        /// <summary>
        /// Mejor candidato de relevo contra un enemigo (ventaja elemental propia, sin desventaja ajena,
        /// más vida). Devuelve -1 si no hay ninguno disponible.
        /// </summary>
        public int MejorRelevoContra(Personaje enemigo)
        {
            int mejor = -1;
            double mejorPuntaje = double.MinValue;
            for (int i = 0; i < miembros.Count; i++)
            {
                if (!PuedeRelevarA(i)) continue;
                Personaje p = miembros[i];
                double puntaje = p.ObtenerMultiplicador(enemigo.Elemento)
                                 - enemigo.ObtenerMultiplicador(p.Elemento) * 0.5
                                 + (double)p.Hp / p.HpMax * 0.25;
                if (puntaje > mejorPuntaje)
                {
                    mejorPuntaje = puntaje;
                    mejor = i;
                }
            }
            return mejor;
        }
    }
}
