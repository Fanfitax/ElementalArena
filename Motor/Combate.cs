using System;
using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    /// <summary>
    /// El motor del combate por turnos entre dos equipos. No usa ninguna ventana: los formularios
    /// solo le preguntan el estado y le envían la acción que eligió el jugador.
    ///
    /// Cada turno el equipo elige UNA acción: Atacar, usar una Habilidad, hacer un Relevo o Pasar.
    /// Todo lo que ocurre se guarda en una PILA (Stack) de texto: el historial del combate.
    /// </summary>
    public class Combate
    {
        // ---------- Constantes de las reglas ----------

        /// <summary>Energía que recupera el personaje activo al pasar el turno (descansar).</summary>
        public const int EnergiaAlDescansar = 3;

        /// <summary>Después de esta ronda empieza la muerte súbita: el ataque de todos sube 15% por ronda.</summary>
        public const int TurnoMuerteSubita = 30;
        public const double IncrementoMuerteSubita = 0.15;

        /// <summary>Probabilidad (en %) de que haya un Terremoto al empezar cada ronda.</summary>
        public const int ProbabilidadTerremoto = 25;

        /// <summary>El daño del Terremoto nunca pasa de este % de la vida máxima del personaje (el "límite").</summary>
        public const int LimiteTerremotoPct = 20;

        // ---------- Datos del combate ----------

        // La PILA del historial: lo último que pasó queda arriba (Push para agregar, Peek para ver lo último).
        private Stack<string> historial = new Stack<string>();

        // Para decidir al azar si hay Terremoto. Se crea con una "semilla": dos combates con la misma
        // semilla y las mismas jugadas dan siempre el mismo resultado (así se sincronizan dos PCs en LAN).
        private Random azar;
        private bool terremotoEstaRonda;

        /// <summary>Crea un combate con una semilla al azar.</summary>
        public Combate(Escenario escenario, EquipoCombate jugador, EquipoCombate rival)
            : this(escenario, jugador, rival, new Random().Next())
        {
        }

        /// <summary>Crea un combate con una semilla elegida.</summary>
        public Combate(Escenario escenario, EquipoCombate jugador, EquipoCombate rival, int semilla)
        {
            if (escenario == null || jugador == null || rival == null)
                throw new ArgumentNullException("Faltan datos para crear el combate.");

            Semilla = semilla;
            azar = new Random(semilla);
            Escenario = escenario;
            Jugador = jugador;
            Rival = rival;

            // El escenario le da su bonus de ataque a los personajes de su elemento.
            foreach (Personaje p in jugador.Miembros) escenario.AplicarA(p);
            foreach (Personaje p in rival.Miembros) escenario.AplicarA(p);

            Estado = EstadoCombate.EnCurso;
            Turno = 1;
            EquipoEnTurno = Jugador;   // siempre empieza el Jugador

            Registrar("¡Comienza el combate en " + escenario.Nombre + "! (" + escenario.ElementoAfin
                      + " +" + escenario.BonusAtaquePct + "% ATK)");
            Registrar(Jugador.Nombre + " envía a " + Jugador.Activo.Nombre + ".");
            Registrar(Rival.Nombre + " envía a " + Rival.Activo.Nombre + ".");
            PrepararTurno();
        }

        public Escenario Escenario { get; private set; }
        public EquipoCombate Jugador { get; private set; }
        public EquipoCombate Rival { get; private set; }
        public EquipoCombate EquipoEnTurno { get; private set; }
        public EstadoCombate Estado { get; private set; }
        public int Semilla { get; private set; }

        /// <summary>Número de ronda. Sube cada vez que vuelve a jugar el Jugador.</summary>
        public int Turno { get; private set; }

        /// <summary>Cuánta vida le quitó el Terremoto al activo del Jugador en la última jugada (0 si no hubo).</summary>
        public int DanoTerremotoJugador { get; private set; }

        /// <summary>Cuánta vida le quitó el Terremoto al activo del Rival en la última jugada (0 si no hubo).</summary>
        public int DanoTerremotoRival { get; private set; }

        public bool Terminado
        {
            get { return Estado != EstadoCombate.EnCurso; }
        }

        public bool EsTurnoDelJugador
        {
            get { return !Terminado && EquipoEnTurno == Jugador; }
        }

        /// <summary>La pila con todo el historial. Al recorrerla con foreach sale primero lo más reciente.</summary>
        public Stack<string> Historial
        {
            get { return historial; }
        }

        /// <summary>La última jugada registrada (Peek de la pila).</summary>
        public string UltimaJugada
        {
            get
            {
                if (historial.Count == 0) return "";
                return historial.Peek();
            }
        }

        /// <summary>True si en la ronda actual hubo Terremoto.</summary>
        public bool TerremotoEstaRonda
        {
            get { return terremotoEstaRonda; }
        }

        /// <summary>El equipo contrario al que se indica.</summary>
        public EquipoCombate Oponente(EquipoCombate equipo)
        {
            if (equipo == Jugador) return Rival;
            return Jugador;
        }

        // ---------- Terremoto: vínculo con Cálculo 1 ----------
        //
        //   Función:   f(t)  = 2t² + 5t      (t = número de ronda)
        //   Derivada:  f'(t) = 4t + 5        (qué tan rápido crece f)
        //   Daño del Terremoto = Min(f'(t), límite), donde límite = 20% de la vida máxima.
        //
        // El Min() es un "límite" para que el daño no crezca sin control y desbalancee la partida.

        /// <summary>f(t) = 2t² + 5t</summary>
        public static int FuncionTerremoto(int t)
        {
            return 2 * t * t + 5 * t;
        }

        /// <summary>f'(t) = 4t + 5 (la derivada de f)</summary>
        public static int DerivadaTerremoto(int t)
        {
            return 4 * t + 5;
        }

        /// <summary>Daño del Terremoto en la ronda t para un personaje con esa vida máxima.</summary>
        public static int DanoDelTerremoto(int t, int hpMax)
        {
            int limite = hpMax * LimiteTerremotoPct / 100;
            return Math.Min(DerivadaTerremoto(t), limite);
        }

        // ---------- ¿Se puede hacer la acción? ----------

        /// <summary>Dice si el equipo en turno puede hacer la acción. Si no puede, explica el motivo.</summary>
        public bool PuedeEjecutar(AccionCombate accion, out string motivo)
        {
            motivo = "";

            if (accion == null)
            {
                motivo = "Acción nula.";
                return false;
            }
            if (Terminado)
            {
                motivo = "El combate ya terminó.";
                return false;
            }

            // Atacar y Pasar siempre se pueden.
            if (accion.Tipo == TipoAccion.Atacar || accion.Tipo == TipoAccion.Pasar)
                return true;

            if (accion.Tipo == TipoAccion.Habilidad)
                return PuedeUsarHabilidad(accion, out motivo);

            if (accion.Tipo == TipoAccion.Relevo)
                return PuedeRelevar(out motivo);

            motivo = "Acción desconocida.";
            return false;
        }

        private bool PuedeUsarHabilidad(AccionCombate accion, out string motivo)
        {
            motivo = "";
            EquipoCombate equipo = EquipoEnTurno;
            Personaje activo = equipo.Activo;

            if (accion.NumeroHabilidad < 0 || accion.NumeroHabilidad >= Personaje.CantidadHabilidades)
            {
                motivo = "Esa habilidad no existe.";
                return false;
            }

            Habilidad habilidad = activo.ObtenerHabilidad(accion.NumeroHabilidad);
            if (!activo.PuedeUsarHabilidad(accion.NumeroHabilidad))
            {
                if (habilidad.CooldownActual > 0)
                    motivo = habilidad.Nombre + " en recarga (" + habilidad.CooldownActual + " turno/s).";
                else
                    motivo = "Energía insuficiente (" + activo.Energia + "/" + habilidad.Costo + ").";
                return false;
            }

            // Si cura a un aliado, el aliado elegido tiene que existir y estar vivo.
            if (habilidad.RequiereAliado && accion.Indice >= 0)
            {
                if (accion.Indice >= equipo.Miembros.Count || !equipo.Miembros[accion.Indice].EstaVivo)
                {
                    motivo = "El aliado elegido no está disponible.";
                    return false;
                }
            }
            return true;
        }

        private bool PuedeRelevar(out string motivo)
        {
            motivo = "";
            EquipoCombate equipo = EquipoEnTurno;

            if (equipo.RelevoBloqueado)
            {
                motivo = "No se puede relevar dos turnos seguidos.";
                return false;
            }
            if (!equipo.HayRelevoDisponible)
            {
                motivo = "No queda nadie en la banca.";
                return false;
            }
            return true;
        }

        // ---------- Hacer la acción ----------

        /// <summary>Ejecuta la acción del equipo en turno y después pasa el turno al otro equipo.</summary>
        public void Ejecutar(AccionCombate accion)
        {
            string motivo;
            if (!PuedeEjecutar(accion, out motivo))
                throw new InvalidOperationException(motivo);

            DanoTerremotoJugador = 0;
            DanoTerremotoRival = 0;

            EquipoCombate propio = EquipoEnTurno;
            EquipoCombate contrario = Oponente(propio);

            if (accion.Tipo == TipoAccion.Atacar)
                EjecutarAtaque(propio, contrario);
            else if (accion.Tipo == TipoAccion.Habilidad)
                EjecutarHabilidad(accion, propio, contrario);
            else if (accion.Tipo == TipoAccion.Relevo)
                Registrar(propio.Relevar());
            else
                EjecutarPasar(propio);

            propio.RegistrarAccionDelTurno(accion.Tipo == TipoAccion.Relevo);

            ResolverDerrota(contrario);
            if (!Terminado) AvanzarTurno();
        }

        private void EjecutarAtaque(EquipoCombate propio, EquipoCombate contrario)
        {
            Personaje enemigo = contrario.Activo;
            int vidaAntes = enemigo.Hp;

            string texto = propio.Activo.Atacar(enemigo);

            propio.SumarDanoInfligido(vidaAntes - enemigo.Hp);
            Registrar(texto);
        }

        private void EjecutarHabilidad(AccionCombate accion, EquipoCombate propio, EquipoCombate contrario)
        {
            Personaje activo = propio.Activo;
            Personaje enemigo = contrario.Activo;
            Habilidad habilidad = activo.ObtenerHabilidad(accion.NumeroHabilidad);

            if (habilidad.EsOfensiva)
            {
                // Habilidad de ataque: va contra el enemigo que está en el campo.
                int vidaAntes = enemigo.Hp;
                string texto = activo.UsarHabilidad(accion.NumeroHabilidad, enemigo, propio.Miembros);
                propio.SumarDanoInfligido(vidaAntes - enemigo.Hp);
                Registrar(texto);
            }
            else
            {
                // Habilidad de utilidad: si cura a un aliado, el jugador ya eligió a cuál (si no, a sí mismo).
                Personaje aliado = activo;
                if (habilidad.RequiereAliado && accion.Indice >= 0)
                    aliado = propio.Miembros[accion.Indice];
                Registrar(activo.UsarHabilidad(accion.NumeroHabilidad, aliado, propio.Miembros));
            }
        }

        private void EjecutarPasar(EquipoCombate propio)
        {
            Personaje activo = propio.Activo;
            int ganada = activo.RecuperarEnergia(EnergiaAlDescansar);
            Registrar(activo.Nombre + " descansa y recupera " + ganada + " de energía.");
        }

        // ---------- Después de cada acción ----------

        /// <summary>Si el personaje activo del equipo contrario cayó, entra el siguiente de la cola (sin gastar turno).</summary>
        private void ResolverDerrota(EquipoCombate defensor)
        {
            Personaje caido = defensor.Activo;
            if (caido.EstaVivo) return;

            Registrar(caido.Nombre + " ha sido derrotado.");
            caido.AlSalirDelCampo();

            // Si ya no queda nadie vivo, se acaba el combate.
            if (defensor.TodosDerrotados)
            {
                if (defensor == Jugador)
                {
                    Estado = EstadoCombate.VictoriaRival;
                    Registrar("Has sido derrotado. ¡" + Rival.Nombre + " gana la partida!");
                }
                else
                {
                    Estado = EstadoCombate.VictoriaJugador;
                    Registrar("¡Victoria! " + Rival.Nombre + " ha sido derrotado.");
                }
                return;
            }

            Registrar(defensor.Relevar());
        }

        /// <summary>Pasa el turno al otro equipo. Cuando vuelve a jugar el Jugador, empieza una ronda nueva.</summary>
        private void AvanzarTurno()
        {
            EquipoEnTurno = Oponente(EquipoEnTurno);

            if (EquipoEnTurno == Jugador)
            {
                Turno++;
                AplicarMuerteSubita();

                // En cada ronda nueva hay un 25% de probabilidad de Terremoto.
                terremotoEstaRonda = azar.Next(100) < ProbabilidadTerremoto;
                if (terremotoEstaRonda) ProvocarTerremoto();
            }
            PrepararTurno();
        }

        /// <summary>
        /// Terremoto: los dos personajes que están en el campo pierden vida. El daño sale de la derivada
        /// f'(t) = 4t + 5 con el límite del 20% de su vida máxima. Nunca los deja en menos de 1 HP,
        /// así que el Terremoto no elimina a nadie. La fórmula queda escrita en el historial.
        /// </summary>
        private void ProvocarTerremoto()
        {
            Registrar("¡TERREMOTO! en " + Escenario.Nombre + ": f(t) = 2t² + 5t, f'(t) = 4t + 5, con t = " + Turno
                      + " => f'(" + Turno + ") = " + DerivadaTerremoto(Turno) + ".");
            DanoTerremotoJugador = AplicarDanoTerremoto(Jugador.Activo);
            DanoTerremotoRival = AplicarDanoTerremoto(Rival.Activo);
        }

        /// <summary>Le quita la vida del Terremoto a un personaje y devuelve cuánta perdió.</summary>
        private int AplicarDanoTerremoto(Personaje p)
        {
            int limite = p.HpMax * LimiteTerremotoPct / 100;
            int dano = DanoDelTerremoto(Turno, p.HpMax);

            if (dano < 1) dano = 1;
            if (dano > p.Hp - 1) dano = p.Hp - 1;   // siempre le queda al menos 1 HP

            int perdido = p.RecibirDano(dano);
            Registrar("   " + p.Nombre + ": Min(f'(t) = " + DerivadaTerremoto(Turno) + ", límite = " + limite
                      + ") => Daño: " + perdido + " (" + p.Hp + "/" + p.HpMax + " HP).");
            return perdido;
        }

        /// <summary>Pasada la ronda 30, el ataque de todos crece en cada ronda para que el combate termine.</summary>
        private void AplicarMuerteSubita()
        {
            if (Turno <= TurnoMuerteSubita) return;

            double aumento = 1.0 + IncrementoMuerteSubita * (Turno - TurnoMuerteSubita);
            foreach (Personaje p in Jugador.Miembros)
                p.MultiplicadorEscenario = Escenario.MultiplicadorPara(p.Elemento) * aumento;
            foreach (Personaje p in Rival.Miembros)
                p.MultiplicadorEscenario = Escenario.MultiplicadorPara(p.Elemento) * aumento;

            if (Turno == TurnoMuerteSubita + 1)
                Registrar("¡MUERTE SÚBITA! El ataque de todos aumenta en cada ronda.");
        }

        /// <summary>Al empezar el turno de un equipo, su personaje activo recupera energía y se actualizan sus efectos.</summary>
        private void PrepararTurno()
        {
            Personaje activo = EquipoEnTurno.Activo;
            activo.IniciarTurno();
            Registrar("— Turno " + Turno + ": " + EquipoEnTurno.Nombre + " (" + activo.Nombre + ") —");
        }

        /// <summary>Agrega una línea al historial (Push a la pila).</summary>
        private void Registrar(string mensaje)
        {
            historial.Push(mensaje);
        }
    }
}
