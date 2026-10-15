using System;
using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    /// <summary>
    /// Equipo de 3 personajes. Hay un personaje activo (en el campo) y los otros dos
    /// esperan en una COLA (Queue). Cuando se hace un relevo entra el primero de la cola
    /// y el que sale se forma al final.
    /// </summary>
    public class EquipoCombate
    {
        public const int TamanoEquipo = 3;

        private List<Personaje> miembros = new List<Personaje>();   // los 3 personajes (para mostrarlos)
        private Queue<Personaje> banca = new Queue<Personaje>();    // COLA de relevos
        private Personaje activo;
        private int danoInfligido;
        private bool ultimaAccionFueRelevo;

        /// <summary>Crea el equipo con COPIAS de los personajes (así el catálogo original no se modifica).</summary>
        public EquipoCombate(string nombre, List<Personaje> personajes, bool esJugador)
        {
            if (nombre == null || nombre.Trim().Length == 0)
                throw new ArgumentException("El equipo necesita nombre.");
            if (personajes == null || personajes.Count != TamanoEquipo)
                throw new ArgumentException("El equipo debe tener exactamente " + TamanoEquipo + " personajes.");

            foreach (Personaje p in personajes)
            {
                foreach (Personaje existente in miembros)
                {
                    if (existente.Id == p.Id)
                        throw new ArgumentException("El personaje " + p.Nombre + " está repetido en el equipo.");
                }
                miembros.Add(p.CrearCopia());
            }

            Nombre = nombre.Trim();
            EsJugador = esJugador;

            // El primero sale al campo; los demás se forman en la cola en el mismo orden.
            activo = miembros[0];
            for (int i = 1; i < miembros.Count; i++)
                banca.Enqueue(miembros[i]);
        }

        public string Nombre { get; private set; }
        public bool EsJugador { get; private set; }

        public List<Personaje> Miembros
        {
            get { return miembros; }
        }

        /// <summary>Personaje que está peleando ahora.</summary>
        public Personaje Activo
        {
            get { return activo; }
        }

        /// <summary>Cantidad de personajes esperando en la cola.</summary>
        public int CantidadBanca
        {
            get { return banca.Count; }
        }

        /// <summary>El personaje que entraría si se hace un relevo (Peek de la cola). Null si no hay nadie.</summary>
        public Personaje ProximoRelevo
        {
            get { return banca.Count > 0 ? banca.Peek() : null; }
        }

        /// <summary>Copia del contenido de la cola, en orden, para mostrarla en pantalla.</summary>
        public Personaje[] ObtenerBanca()
        {
            return banca.ToArray();
        }

        public int Vivos
        {
            get
            {
                int n = 0;
                foreach (Personaje p in miembros)
                {
                    if (p.EstaVivo) n++;
                }
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

        /// <summary>True si el equipo relevó en su turno anterior (no se puede relevar dos turnos seguidos).</summary>
        public bool RelevoBloqueado
        {
            get { return ultimaAccionFueRelevo; }
        }

        internal void RegistrarAccionDelTurno(bool fueRelevo)
        {
            ultimaAccionFueRelevo = fueRelevo;
        }

        public bool HayRelevoDisponible
        {
            get { return banca.Count > 0; }
        }

        /// <summary>
        /// Relevo: sale el activo y entra el primero de la cola (Dequeue).
        /// Si el que sale sigue vivo, se forma al final de la cola (Enqueue).
        /// Devuelve el texto del evento.
        /// </summary>
        public string Relevar()
        {
            if (banca.Count == 0)
                throw new InvalidOperationException("No hay nadie en la banca para entrar.");

            Personaje saliente = activo;
            saliente.AlSalirDelCampo();

            activo = banca.Dequeue();
            if (saliente.EstaVivo)
                banca.Enqueue(saliente);

            return activo.AlEntrarEnRelevo(saliente);
        }
    }
}
