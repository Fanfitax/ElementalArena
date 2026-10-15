using System;
using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    /// <summary>
    /// Motor de combate por turnos y relevos entre el equipo del jugador y el rival (CPU).
    /// No usa ninguna interfaz gráfica: los formularios solo consultan el estado y envían acciones.
    /// Cada turno el equipo elige UNA acción: Atacar, Habilidad, Relevo o Pasar (descansar).
    /// Todo lo que pasa se guarda en una PILA (Stack) de texto: el historial de jugadas.
    /// </summary>
    public class Combate
    {
        /// <summary>Energía que recupera el personaje activo al descansar.</summary>
        public const int EnergiaAlDescansar = 3;

        /// <summary>Ronda a partir de la cual empieza la muerte súbita (+15% de ataque por ronda).</summary>
        public const int TurnoMuerteSubita = 30;
        public const double IncrementoMuerteSubita = 0.15;

        /// <summary>Cada cuántas rondas ocurre el Terremoto (rondas 4, 8, 12...).</summary>
        public const int RondasEntreTerremotos = 4;

        /// <summary>El Terremoto quita este porcentaje de la vida máxima a los dos personajes en el campo.</summary>
        public const int PorcentajeDanoTerremoto = 10;

        // PILA del historial: lo último que pasó queda arriba (Push al agregar, Peek para ver la última).
        private Stack<string> historial = new Stack<string>();

        public Combate(Escenario escenario, EquipoCombate jugador, EquipoCombate rival)
        {
            if (escenario == null || jugador == null || rival == null)
                throw new ArgumentNullException("Faltan datos para crear el combate.");

            Escenario = escenario;
            Jugador = jugador;
            Rival = rival;

            foreach (Personaje p in jugador.Miembros) escenario.AplicarA(p);
            foreach (Personaje p in rival.Miembros) escenario.AplicarA(p);

            Estado = EstadoCombate.EnCurso;
            Turno = 1;
            EquipoEnTurno = Jugador;

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

        /// <summary>Número de ronda (aumenta cada vez que vuelve a jugar el jugador).</summary>
        public int Turno { get; private set; }

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

        /// <summary>Última jugada registrada (Peek de la pila).</summary>
        public string UltimaJugada
        {
            get { return historial.Count > 0 ? historial.Peek() : string.Empty; }
        }

        /// <summary>True si en la ronda actual ocurrió el Terremoto.</summary>
        public bool TerremotoEstaRonda
        {
            get { return Turno % RondasEntreTerremotos == 0; }
        }

        /// <summary>Cuántas rondas faltan para el próximo Terremoto (el de esta ronda no cuenta).</summary>
        public int RondasParaTerremoto
        {
            get { return RondasEntreTerremotos - (Turno % RondasEntreTerremotos); }
        }

        public EquipoCombate Oponente(EquipoCombate equipo)
        {
            return equipo == Jugador ? Rival : Jugador;
        }

        // ---------- Validación de acciones ----------

        public bool PuedeEjecutar(AccionCombate accion, out string motivo)
        {
            motivo = null;
            if (accion == null) { motivo = "Acción nula."; return false; }
            if (Terminado) { motivo = "El combate ya terminó."; return false; }

            EquipoCombate equipo = EquipoEnTurno;
            Personaje activo = equipo.Activo;

            switch (accion.Tipo)
            {
                case TipoAccion.Atacar:
                case TipoAccion.Pasar:
                    return true;

                case TipoAccion.Habilidad:
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
                    if (habilidad.RequiereAliado && accion.Indice >= 0)
                    {
                        if (accion.Indice >= equipo.Miembros.Count || !equipo.Miembros[accion.Indice].EstaVivo)
                        {
                            motivo = "El aliado elegido no está disponible.";
                            return false;
                        }
                    }
                    return true;

                case TipoAccion.Relevo:
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

                default:
                    motivo = "Acción desconocida.";
                    return false;
            }
        }

        // ---------- Ejecución ----------

        /// <summary>Ejecuta la acción del equipo en turno y pasa el turno.</summary>
        public void Ejecutar(AccionCombate accion)
        {
            string motivo;
            if (!PuedeEjecutar(accion, out motivo))
                throw new InvalidOperationException(motivo);

            EquipoCombate propio = EquipoEnTurno;
            EquipoCombate contrario = Oponente(propio);
            Personaje activo = propio.Activo;
            Personaje enemigo = contrario.Activo;

            switch (accion.Tipo)
            {
                case TipoAccion.Atacar:
                    {
                        int antes = enemigo.Hp;
                        string texto = activo.Atacar(enemigo);
                        propio.SumarDanoInfligido(antes - enemigo.Hp);
                        Registrar(texto);
                        break;
                    }
                case TipoAccion.Habilidad:
                    {
                        Habilidad habilidad = activo.ObtenerHabilidad(accion.NumeroHabilidad);
                        if (habilidad.EsOfensiva)
                        {
                            // Habilidad de ataque: va contra el enemigo activo.
                            int antes = enemigo.Hp;
                            string texto = activo.UsarHabilidad(accion.NumeroHabilidad, enemigo, propio.Miembros);
                            propio.SumarDanoInfligido(antes - enemigo.Hp);
                            Registrar(texto);
                        }
                        else
                        {
                            // Habilidad de utilidad: si cura a un aliado, el jugador eligió a cuál.
                            Personaje aliado = (habilidad.RequiereAliado && accion.Indice >= 0) ? propio.Miembros[accion.Indice] : activo;
                            Registrar(activo.UsarHabilidad(accion.NumeroHabilidad, aliado, propio.Miembros));
                        }
                        break;
                    }
                case TipoAccion.Relevo:
                    Registrar(propio.Relevar());
                    break;

                case TipoAccion.Pasar:
                    {
                        int ganada = activo.RecuperarEnergia(EnergiaAlDescansar);
                        Registrar(activo.Nombre + " descansa y recupera " + ganada + " de energía.");
                        break;
                    }
            }

            propio.RegistrarAccionDelTurno(accion.Tipo == TipoAccion.Relevo);
            ResolverDerrota(contrario);
            if (!Terminado) AvanzarTurno();
        }

        // ---------- Métodos internos ----------

        /// <summary>Si el personaje activo del defensor cayó, entra el siguiente de la cola (sin gastar turno).</summary>
        private void ResolverDerrota(EquipoCombate defensor)
        {
            Personaje caido = defensor.Activo;
            if (caido.EstaVivo) return;

            Registrar(caido.Nombre + " ha sido derrotado.");
            caido.AlSalirDelCampo();

            if (defensor.TodosDerrotados)
            {
                Estado = defensor == Jugador ? EstadoCombate.VictoriaRival : EstadoCombate.VictoriaJugador;
                Registrar(defensor == Jugador
                    ? "Has sido derrotado. ¡" + Rival.Nombre + " gana la partida!"
                    : "¡Victoria! " + Rival.Nombre + " ha sido derrotado.");
                return;
            }

            Registrar(defensor.Relevar());
        }

        private void AvanzarTurno()
        {
            EquipoEnTurno = Oponente(EquipoEnTurno);
            if (EquipoEnTurno == Jugador)
            {
                Turno++;
                AplicarMuerteSubita();
                if (TerremotoEstaRonda) ProvocarTerremoto();
            }
            PrepararTurno();
        }

        /// <summary>
        /// Evento automático: cada RondasEntreTerremotos rondas la tierra tiembla y los dos personajes
        /// que están en el campo pierden el 10% de su vida máxima. Nunca los deja por debajo de 1 HP
        /// (así el Terremoto no elimina a nadie). Cada efecto se guarda en la pila del historial.
        /// </summary>
        private void ProvocarTerremoto()
        {
            Registrar("¡TERREMOTO! El suelo tiembla en " + Escenario.Nombre + " (ronda " + Turno + ").");
            AplicarDanoTerremoto(Jugador.Activo);
            AplicarDanoTerremoto(Rival.Activo);
        }

        private void AplicarDanoTerremoto(Personaje p)
        {
            int dano = p.HpMax * PorcentajeDanoTerremoto / 100;
            if (dano < 1) dano = 1;
            if (dano > p.Hp - 1) dano = p.Hp - 1;   // siempre queda al menos 1 HP

            int aplicado = p.RecibirDano(dano);
            Registrar("   " + p.Nombre + " pierde " + aplicado + " HP (" + p.Hp + "/" + p.HpMax + ").");
        }

        /// <summary>Pasada la ronda TurnoMuerteSubita, el ataque de todos crece cada ronda para que el combate termine.</summary>
        private void AplicarMuerteSubita()
        {
            if (Turno <= TurnoMuerteSubita) return;
            double fatiga = 1.0 + IncrementoMuerteSubita * (Turno - TurnoMuerteSubita);
            foreach (Personaje p in Jugador.Miembros) p.MultiplicadorEscenario = Escenario.MultiplicadorPara(p.Elemento) * fatiga;
            foreach (Personaje p in Rival.Miembros) p.MultiplicadorEscenario = Escenario.MultiplicadorPara(p.Elemento) * fatiga;
            if (Turno == TurnoMuerteSubita + 1)
                Registrar("¡MUERTE SÚBITA! El ataque de todos aumenta en cada ronda.");
        }

        private void PrepararTurno()
        {
            Personaje activo = EquipoEnTurno.Activo;
            activo.IniciarTurno();
            Registrar("— Turno " + Turno + ": " + EquipoEnTurno.Nombre + " (" + activo.Nombre + ") —");
        }

        private void Registrar(string mensaje)
        {
            historial.Push(mensaje);
        }
    }
}
