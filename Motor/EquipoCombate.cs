using System;
using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    /// <summary>
    /// Un equipo de 3 personajes. Uno está en el campo (el "activo") y los otros dos esperan
    /// en una COLA (Queue): el primero que entró a la cola es el primero que sale a pelear.
    /// Cuando se hace un relevo, entra el primero de la cola y el que sale se forma al final.
    /// </summary>
    public class EquipoCombate
    {
        public const int TamanoEquipo = 3;

        private List<Personaje> miembros = new List<Personaje>();   // los 3 personajes (para mostrarlos)
        private Queue<Personaje> banca = new Queue<Personaje>();    // la COLA de relevos
        private Personaje activo;                                   // el que está peleando
        private int danoInfligido;
        private bool relevoBloqueado;

        /// <summary>
        /// Crea el equipo con COPIAS de los personajes (así los datos originales del catálogo no se modifican).
        /// El primero de la lista sale al campo y los otros dos se forman en la cola, en ese orden.
        /// </summary>
        public EquipoCombate(string nombre, List<Personaje> personajes, bool esJugador)
        {
            if (nombre == null || nombre.Trim().Length == 0)
                throw new ArgumentException("El equipo necesita nombre.");
            if (personajes == null || personajes.Count != TamanoEquipo)
                throw new ArgumentException("El equipo debe tener exactamente " + TamanoEquipo + " personajes.");

            // No se puede repetir un personaje.
            for (int i = 0; i < personajes.Count; i++)
            {
                for (int j = i + 1; j < personajes.Count; j++)
                {
                    if (personajes[i].Id == personajes[j].Id)
                        throw new ArgumentException("El personaje " + personajes[i].Nombre + " está repetido en el equipo.");
                }
            }

            foreach (Personaje p in personajes)
                miembros.Add(p.CrearCopia());

            Nombre = nombre.Trim();
            EsJugador = esJugador;

            activo = miembros[0];
            for (int i = 1; i < miembros.Count; i++)
                banca.Enqueue(miembros[i]);
        }

        public string Nombre { get; private set; }
        public bool EsJugador { get; private set; }

        /// <summary>Los 3 personajes del equipo (vivos o derrotados).</summary>
        public List<Personaje> Miembros
        {
            get { return miembros; }
        }

        /// <summary>El personaje que está peleando ahora.</summary>
        public Personaje Activo
        {
            get { return activo; }
        }

        /// <summary>Cuántos personajes esperan en la cola.</summary>
        public int CantidadBanca
        {
            get { return banca.Count; }
        }

        /// <summary>El que entraría en el próximo relevo (Peek de la cola). Es null si la cola está vacía.</summary>
        public Personaje ProximoRelevo
        {
            get
            {
                if (banca.Count == 0) return null;
                return banca.Peek();
            }
        }

        /// <summary>Copia del contenido de la cola, en orden, para mostrarla en pantalla.</summary>
        public Personaje[] ObtenerBanca()
        {
            return banca.ToArray();
        }

        public bool HayRelevoDisponible
        {
            get { return banca.Count > 0; }
        }

        /// <summary>Cuántos personajes del equipo siguen vivos.</summary>
        public int Vivos
        {
            get
            {
                int cantidad = 0;
                foreach (Personaje p in miembros)
                {
                    if (p.EstaVivo) cantidad++;
                }
                return cantidad;
            }
        }

        public bool TodosDerrotados
        {
            get { return Vivos == 0; }
        }

        /// <summary>El daño total que este equipo le ha causado al rival en la partida.</summary>
        public int DanoInfligido
        {
            get { return danoInfligido; }
        }

        public void SumarDanoInfligido(int cantidad)
        {
            if (cantidad > 0) danoInfligido = danoInfligido + cantidad;
        }

        /// <summary>True si el equipo relevó en su turno anterior (no se puede relevar dos turnos seguidos).</summary>
        public bool RelevoBloqueado
        {
            get { return relevoBloqueado; }
        }

        /// <summary>Se llama al terminar cada turno del equipo para recordar si fue un relevo.</summary>
        public void RegistrarAccionDelTurno(bool fueRelevo)
        {
            relevoBloqueado = fueRelevo;
        }

        /// <summary>
        /// Relevo: sale el personaje activo y entra el primero de la cola (Dequeue).
        /// Si el que sale sigue vivo, se forma al final de la cola (Enqueue).
        /// Devuelve el texto para el historial.
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
