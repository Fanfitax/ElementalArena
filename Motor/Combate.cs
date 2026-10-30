using System;
using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    // El motor del combate por turnos entre dos equipos. No usa ninguna ventana: los formularios
    // solo le preguntan el estado y le envían la acción que eligió el jugador.
    //
    // Cada turno el equipo elige UNA acción: Atacar, usar una Habilidad, hacer un Relevo o Pasar.
    // Todo lo que ocurre se guarda en una PILA (Stack) de texto: el historial del combate.
    public class Combate
    {
        // ---------- Constantes de las reglas ----------

        // Energía que recupera el personaje activo al pasar el turno (descansar).
        public const int EnergiaAlDescansar = 3;

        // Después de esta ronda empieza la muerte súbita: el ataque de todos sube 15% por ronda.
        public const int TurnoMuerteSubita = 30;
        public const double IncrementoMuerteSubita = 0.15;

        // Probabilidad (en %) de que haya un Terremoto al empezar cada ronda.
        public const int ProbabilidadTerremoto = 25;

        // El daño del Terremoto nunca pasa de este % de la vida máxima del personaje (el "límite").
        public const int LimiteTerremotoPct = 20;

        // ---------- Datos del combate ----------

        // La PILA del historial: lo último que pasó queda arriba (Push para agregar, Peek para ver lo último).
        private Stack<string> historial = new Stack<string>();

        // Para decidir al azar si hay Terremoto. Se crea con una "semilla": dos combates con la misma
        // semilla y las mismas jugadas dan siempre el mismo resultado (así se sincronizan dos PCs en LAN).
        private Random azar;
        private bool terremotoEstaRonda;

        // Crea un combate con una semilla al azar.
        public Combate(Escenario escenario, EquipoCombate jugador, EquipoCombate rival)
            : this(escenario, jugador, rival, new Random().Next())
        {
        }

        // Crea un combate con una semilla elegida.
        public Combate(Escenario escenario, EquipoCombate jugador, EquipoCombate rival, int semilla)
        {
            if (escenario == null || jugador == null || rival == null)
                throw new ArgumentNullException(Idioma.Texto("Faltan datos para crear el combate.", "Data missing to create the battle."));

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

            Registrar(Idioma.Texto("¡Comienza el combate en ", "The battle begins in ") + escenario.Nombre + "! (" + Idioma.NombreElemento(escenario.ElementoAfin)
                      + " +" + escenario.BonusAtaquePct + "% ATK)");
            Registrar(Jugador.Nombre + Idioma.Texto(" envía a ", " sends ") + Jugador.Activo.Nombre + ".");
            Registrar(Rival.Nombre + Idioma.Texto(" envía a ", " sends ") + Rival.Activo.Nombre + ".");
            PrepararTurno();
        }

        public Escenario Escenario { get; private set; }
        public EquipoCombate Jugador { get; private set; }
        public EquipoCombate Rival { get; private set; }
        public EquipoCombate EquipoEnTurno { get; private set; }
        public EstadoCombate Estado { get; private set; }
        public int Semilla { get; private set; }

        // Número de ronda. Sube cada vez que vuelve a jugar el Jugador.
        public int Turno { get; private set; }

        // Cuánta vida le quitó el Terremoto al activo del Jugador en la última jugada (0 si no hubo).
        public int DanoTerremotoJugador { get; private set; }

        // Cuánta vida le quitó el Terremoto al activo del Rival en la última jugada (0 si no hubo).
        public int DanoTerremotoRival { get; private set; }

        public bool Terminado
        {
            get { return Estado != EstadoCombate.EnCurso; }
        }

        public bool EsTurnoDelJugador
        {
            get { return !Terminado && EquipoEnTurno == Jugador; }
        }

        // La pila con todo el historial. Al recorrerla con foreach sale primero lo más reciente.
        public Stack<string> Historial
        {
            get { return historial; }
        }

        // La última jugada registrada (Peek de la pila).
        public string UltimaJugada
        {
            get
            {
                if (historial.Count == 0) return "";
                return historial.Peek();
            }
        }

        // True si en la ronda actual hubo Terremoto.
        public bool TerremotoEstaRonda
        {
            get { return terremotoEstaRonda; }
        }

        // El equipo contrario al que se indica.
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

        // f(t) = 2t² + 5t
        public static int FuncionTerremoto(int t)
        {
            return 2 * t * t + 5 * t;
        }

        // f'(t) = 4t + 5 (la derivada de f)
        public static int DerivadaTerremoto(int t)
        {
            return 4 * t + 5;
        }

        // Daño del Terremoto en la ronda t para un personaje con esa vida máxima.
        public static int DanoDelTerremoto(int t, int hpMax)
        {
            int limite = hpMax * LimiteTerremotoPct / 100;
            return Math.Min(DerivadaTerremoto(t), limite);
        }

        // ---------- ¿Se puede hacer la acción? ----------

        // Dice si el equipo en turno puede hacer la acción.
        public bool PuedeEjecutar(AccionCombate accion)
        {
            return ObtenerMotivo(accion) == "";
        }

        // Si la acción no se puede hacer, devuelve la razón en texto. Si se puede, devuelve "".
        public string ObtenerMotivo(AccionCombate accion)
        {
            if (accion == null)
                return Idioma.Texto("Acción nula.", "Null action.");
            if (Terminado)
                return Idioma.Texto("El combate ya terminó.", "The battle is already over.");

            // Atacar y Pasar siempre se pueden.
            if (accion.Tipo == TipoAccion.Atacar || accion.Tipo == TipoAccion.Pasar)
                return "";

            if (accion.Tipo == TipoAccion.Habilidad)
                return ObtenerMotivoHabilidad(accion);

            if (accion.Tipo == TipoAccion.Relevo)
                return ObtenerMotivoRelevo();

            return Idioma.Texto("Acción desconocida.", "Unknown action.");
        }

        private string ObtenerMotivoHabilidad(AccionCombate accion)
        {
            EquipoCombate equipo = EquipoEnTurno;
            Personaje activo = equipo.Activo;

            if (accion.NumeroHabilidad < 0 || accion.NumeroHabilidad >= Personaje.CantidadHabilidades)
                return Idioma.Texto("Esa habilidad no existe.", "That ability does not exist.");

            Habilidad habilidad = activo.ObtenerHabilidad(accion.NumeroHabilidad);
            if (!activo.PuedeUsarHabilidad(accion.NumeroHabilidad))
            {
                if (habilidad.CooldownActual > 0)
                    return habilidad.Nombre + Idioma.Texto(" en recarga (", " on cooldown (") + habilidad.CooldownActual + Idioma.Texto(" turno/s).", " turn(s)).");
                return Idioma.Texto("Energía insuficiente (", "Not enough energy (") + activo.Energia + "/" + habilidad.Costo + ").";
            }

            // Si cura a un aliado, el aliado elegido tiene que existir y estar vivo.
            if (habilidad.RequiereAliado && accion.Indice >= 0)
            {
                if (accion.Indice >= equipo.Miembros.Count || !equipo.Miembros[accion.Indice].EstaVivo)
                    return Idioma.Texto("El aliado elegido no está disponible.", "The chosen ally is not available.");
            }
            return "";
        }

        private string ObtenerMotivoRelevo()
        {
            EquipoCombate equipo = EquipoEnTurno;

            if (equipo.RelevoBloqueado)
                return Idioma.Texto("No se puede relevar dos turnos seguidos.", "You cannot swap two turns in a row.");
            if (!equipo.HayRelevoDisponible)
                return Idioma.Texto("No queda nadie en la banca.", "Nobody is left on the bench.");
            return "";
        }

        // ---------- Hacer la acción ----------

        // Ejecuta la acción del equipo en turno y después pasa el turno al otro equipo.
        public void Ejecutar(AccionCombate accion)
        {
            string motivo = ObtenerMotivo(accion);
            if (motivo != "")
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
            Registrar(activo.Nombre + Idioma.Texto(" descansa y recupera ", " rests and recovers ") + ganada + Idioma.Texto(" de energía.", " energy."));
        }

        // ---------- Después de cada acción ----------

        // Si el personaje activo del equipo contrario cayó, entra el siguiente de la cola (sin gastar turno).
        private void ResolverDerrota(EquipoCombate defensor)
        {
            Personaje caido = defensor.Activo;
            if (caido.EstaVivo) return;

            Registrar(caido.Nombre + Idioma.Texto(" ha sido derrotado.", " has been defeated."));
            caido.AlSalirDelCampo();

            // Si ya no queda nadie vivo, se acaba el combate.
            if (defensor.TodosDerrotados)
            {
                if (defensor == Jugador)
                {
                    Estado = EstadoCombate.VictoriaRival;
                    Registrar(Idioma.Texto("Has sido derrotado. ¡", "You have been defeated.") + Rival.Nombre + Idioma.Texto(" gana la partida!", " wins the match!"));
                }
                else
                {
                    Estado = EstadoCombate.VictoriaJugador;
                    Registrar(Idioma.Texto("¡Victoria! ", "Victory! ") + Rival.Nombre + Idioma.Texto(" ha sido derrotado.", " has been defeated."));
                }
                return;
            }

            Registrar(defensor.Relevar());
        }

        // Pasa el turno al otro equipo. Cuando vuelve a jugar el Jugador, empieza una ronda nueva.
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

        // Terremoto: los dos personajes que están en el campo pierden vida. El daño sale de la derivada
        // f'(t) = 4t + 5 con el límite del 20% de su vida máxima. Nunca los deja en menos de 1 HP,
        // así que el Terremoto no elimina a nadie. La fórmula queda escrita en el historial.
        private void ProvocarTerremoto()
        {
            Registrar(Idioma.Texto("¡TERREMOTO! en ", "EARTHQUAKE! in ") + Escenario.Nombre + Idioma.Texto(": f(t) = 2t² + 5t, f'(t) = 4t + 5, con t = ", ": f(t) = 2t² + 5t, f'(t) = 4t + 5, with t = ") + Turno
                      + " => f'(" + Turno + ") = " + DerivadaTerremoto(Turno) + ".");
            DanoTerremotoJugador = AplicarDanoTerremoto(Jugador.Activo);
            DanoTerremotoRival = AplicarDanoTerremoto(Rival.Activo);
        }

        // Le quita la vida del Terremoto a un personaje y devuelve cuánta perdió.
        private int AplicarDanoTerremoto(Personaje p)
        {
            int limite = p.HpMax * LimiteTerremotoPct / 100;
            int dano = DanoDelTerremoto(Turno, p.HpMax);

            if (dano < 1) dano = 1;
            if (dano > p.Hp - 1) dano = p.Hp - 1;   // siempre le queda al menos 1 HP

            int perdido = p.RecibirDano(dano);
            Registrar("   " + p.Nombre + ": Min(f'(t) = " + DerivadaTerremoto(Turno) + Idioma.Texto(", límite = ", ", limit = ") + limite
                      + Idioma.Texto(") => Daño: ", ") => Damage: ") + perdido + " (" + p.Hp + "/" + p.HpMax + " HP).");
            return perdido;
        }

        // Pasada la ronda 30, el ataque de todos crece en cada ronda para que el combate termine.
        private void AplicarMuerteSubita()
        {
            if (Turno <= TurnoMuerteSubita) return;

            double aumento = 1.0 + IncrementoMuerteSubita * (Turno - TurnoMuerteSubita);
            foreach (Personaje p in Jugador.Miembros)
                p.MultiplicadorEscenario = Escenario.MultiplicadorPara(p.Elemento) * aumento;
            foreach (Personaje p in Rival.Miembros)
                p.MultiplicadorEscenario = Escenario.MultiplicadorPara(p.Elemento) * aumento;

            if (Turno == TurnoMuerteSubita + 1)
                Registrar(Idioma.Texto("¡MUERTE SÚBITA! El ataque de todos aumenta en cada ronda.", "SUDDEN DEATH! Everyone's attack increases every round."));
        }

        // Al empezar el turno de un equipo, su personaje activo recupera energía y se actualizan sus efectos.
        private void PrepararTurno()
        {
            Personaje activo = EquipoEnTurno.Activo;
            activo.IniciarTurno();
            Registrar(Idioma.Texto("— Turno ", "— Turn ") + Turno + ": " + EquipoEnTurno.Nombre + " (" + activo.Nombre + ") —");
        }

        // Agrega una línea al historial (Push a la pila).
        private void Registrar(string mensaje)
        {
            historial.Push(mensaje);
        }
    }
}
