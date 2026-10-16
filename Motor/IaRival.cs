using System;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    /// <summary>
    /// La inteligencia del rival cuando juega la CPU. No es "inteligencia artificial" de verdad:
    /// son reglas simples que la CPU revisa en orden de prioridad (si se cumple una, hace esa acción y termina).
    /// Para decidir, la CPU CALCULA cuánto daño haría cada golpe (Personaje.DanoEstimado...), sin aplicarlo:
    ///   1) Rematar: si un golpe termina con el enemigo, lo da (el más barato que alcance).
    ///   2) Sobrevivir: si el enemigo la puede derrotar en su próximo turno, releva (si el que sigue aguanta)
    ///      o se cura (si la curación la salva).
    ///   3) Relevo táctico: calcula en cuántos turnos le gana cada uno al otro (la "carrera"). Si va perdiendo
    ///      y el que sigue en la cola la ganaría aunque el rival pegue primero, releva.
    ///   4) Habilidad de utilidad, si en este momento sirve (curar, potenciar, reflejar...).
    ///   5) Habilidad de ataque, si pega más fuerte que el ataque básico.
    ///   6) Descansa si vale la pena esperar a la energía; si no, ataque básico.
    /// Algunas reglas solo se cumplen con cierta probabilidad para que la CPU no sea siempre igual.
    /// </summary>
    public class IaRival
    {
        private Random azar = new Random();

        /// <summary>Decide una acción VÁLIDA para el equipo que tiene el turno.</summary>
        public AccionCombate Decidir(Combate combate)
        {
            EquipoCombate yo = combate.EquipoEnTurno;
            EquipoCombate otro = combate.Oponente(yo);
            Personaje miPersonaje = yo.Activo;
            Personaje enemigo = otro.Activo;

            // Puede relevar si no relevó en el turno anterior y hay alguien en la cola.
            bool puedeRelevar = !yo.RelevoBloqueado && yo.HayRelevoDisponible;

            int golpeBasico = miPersonaje.DanoEstimadoBasico(enemigo);
            int golpeHabilidad = miPersonaje.DanoEstimadoHabilidad(enemigo);
            bool habilidadLista = miPersonaje.PuedeUsarHabilidad(0);

            // 1) Rematar: el ataque básico no gasta nada, así que se prefiere si alcanza.
            if (golpeBasico >= enemigo.Hp) return AccionCombate.Atacar();
            if (habilidadLista && golpeHabilidad >= enemigo.Hp) return AccionCombate.Habilidad(0);

            // 2) Sobrevivir: el enemigo me puede derrotar en su próximo turno.
            int suGolpe = MayorGolpe(enemigo, miPersonaje);
            if (suGolpe >= miPersonaje.Hp)
            {
                AccionCombate salvacion = BuscarSalvacion(yo, miPersonaje, enemigo, suGolpe, puedeRelevar);
                if (salvacion != null) return salvacion;
            }

            // 3) Relevo táctico, si voy perdiendo la carrera y el que sigue la ganaría.
            if (puedeRelevar && DebeRelevarPorVentaja(yo, miPersonaje, enemigo) && Probabilidad(85))
                return AccionCombate.Relevar();

            // 4) Habilidad de utilidad, si sirve ahora.
            AccionCombate utilidad = DecidirUtilidad(yo, miPersonaje, suGolpe);
            if (utilidad != null) return utilidad;

            // 5) Habilidad de ataque, si pega más que el ataque básico y el retroceso no es peligroso.
            if (habilidadLista && golpeHabilidad > golpeBasico && Probabilidad(92))
            {
                bool esRetroceso = miPersonaje.HabilidadAtaque.Efecto == EfectoHabilidad.GolpeConRetroceso;
                if (!esRetroceso || miPersonaje.Hp > miPersonaje.HpMax * 0.35)
                    return AccionCombate.Habilidad(0);
            }

            // 6) Descansar si la habilidad ya no está en recarga, solo le falta energía y pega mucho más que el básico.
            Habilidad ataque = miPersonaje.HabilidadAtaque;
            if (ataque.CooldownActual == 0 && miPersonaje.Energia < ataque.Costo && golpeHabilidad >= golpeBasico * 2)
                return AccionCombate.Pasar();

            return AccionCombate.Atacar();
        }

        // ---------- Cálculos de daño ----------

        /// <summary>El golpe más fuerte que "atacante" le podría dar a "objetivo" en su próximo turno.</summary>
        private int MayorGolpe(Personaje atacante, Personaje objetivo)
        {
            int golpe = atacante.DanoEstimadoBasico(objetivo);

            // La habilidad cuenta si estará lista en su turno (la recarga baja y la energía sube al empezar).
            Habilidad h = atacante.HabilidadAtaque;
            bool estaraLista = h.CooldownActual <= 1 && atacante.Energia + Personaje.EnergiaPorTurno >= h.Costo;
            if (estaraLista)
                golpe = Math.Max(golpe, atacante.DanoEstimadoHabilidad(objetivo));
            return golpe;
        }

        /// <summary>Daño que hace en promedio por turno: la habilidad una vez cada "recarga" turnos y ataque básico el resto.</summary>
        private double DanoPorTurno(Personaje atacante, Personaje objetivo)
        {
            double basico = atacante.DanoEstimadoBasico(objetivo);
            double habilidad = atacante.DanoEstimadoHabilidad(objetivo);
            double periodo = Math.Max(1, atacante.HabilidadAtaque.Cooldown);
            if (habilidad < basico) return basico;
            return (habilidad + (periodo - 1) * basico) / periodo;
        }

        /// <summary>En cuántos turnos "atacante" derrota a "objetivo" si los dos siguen igual.</summary>
        private int TurnosParaVencer(Personaje atacante, Personaje objetivo)
        {
            return (int)Math.Ceiling(objetivo.Hp / DanoPorTurno(atacante, objetivo));
        }

        // ---------- Sobrevivir y relevo táctico ----------

        /// <summary>Una jugada que evita que me derroten el próximo turno: relevar o curarme. Devuelve null si no hay.</summary>
        private AccionCombate BuscarSalvacion(EquipoCombate yo, Personaje miPersonaje, Personaje enemigo, int suGolpe, bool puedeRelevar)
        {
            // Relevar sirve si el que entra aguanta el golpe del enemigo.
            if (puedeRelevar)
            {
                Personaje siguiente = yo.ProximoRelevo;
                if (siguiente.Hp > MayorGolpe(enemigo, siguiente) && Probabilidad(90))
                    return AccionCombate.Relevar();
            }

            // Curarme sirve si con la curación aguanto el golpe.
            if (miPersonaje.PuedeUsarHabilidad(1))
            {
                Habilidad h = miPersonaje.HabilidadUtilidad;
                int cura = miPersonaje.CuracionDe(h);
                if (miPersonaje.Hp + cura > suGolpe)
                {
                    if (h.Efecto == EfectoHabilidad.Curar)
                        return AccionCombate.HabilidadSobreAliado(1, yo.Miembros.IndexOf(miPersonaje));
                    if (h.Efecto == EfectoHabilidad.CurarEquipo)
                        return AccionCombate.Habilidad(1);
                }
            }
            return null;
        }

        /// <summary>
        /// True si voy perdiendo la carrera contra el enemigo (él me derrota antes de que yo lo derrote)
        /// y el que sigue en la cola la ganaría aunque el enemigo pegue primero (porque relevar gasta mi turno).
        /// </summary>
        private bool DebeRelevarPorVentaja(EquipoCombate yo, Personaje miPersonaje, Personaje enemigo)
        {
            int misTurnos = TurnosParaVencer(miPersonaje, enemigo);
            int susTurnos = TurnosParaVencer(enemigo, miPersonaje);
            if (misTurnos <= susTurnos) return false;   // voy ganando (y juego primero): me quedo

            Personaje siguiente = yo.ProximoRelevo;
            int turnosDelSiguiente = TurnosParaVencer(siguiente, enemigo);
            int turnosDelEnemigoContraSiguiente = TurnosParaVencer(enemigo, siguiente);
            return turnosDelSiguiente < turnosDelEnemigoContraSiguiente;
        }

        // ---------- Habilidad de utilidad ----------

        /// <summary>Devuelve la acción con la habilidad de utilidad si conviene usarla ahora, o null si no conviene.</summary>
        private AccionCombate DecidirUtilidad(EquipoCombate yo, Personaje miPersonaje, int suGolpe)
        {
            if (!miPersonaje.PuedeUsarHabilidad(1)) return null;

            Habilidad h = miPersonaje.HabilidadUtilidad;
            bool peligro = suGolpe >= miPersonaje.HpMax * 0.25;   // el enemigo pega fuerte

            // Curar a un aliado: solo si al más herido le falta bastante vida.
            if (h.Efecto == EfectoHabilidad.Curar)
            {
                int herido = IndiceDelMasHerido(yo);
                if (herido >= 0)
                {
                    Personaje p = yo.Miembros[herido];
                    int faltante = p.HpMax - p.Hp;
                    if (faltante >= miPersonaje.CuracionDe(h) * 0.7)
                        return AccionCombate.HabilidadSobreAliado(1, herido);
                }
                return null;
            }

            // Curar a todo el equipo: solo si entre todos les falta bastante vida.
            if (h.Efecto == EfectoHabilidad.CurarEquipo)
            {
                int faltanteTotal = 0;
                foreach (Personaje p in yo.Miembros)
                {
                    if (p.EstaVivo) faltanteTotal = faltanteTotal + (p.HpMax - p.Hp);
                }
                if (faltanteTotal >= miPersonaje.CuracionDe(h) * 1.5)
                    return AccionCombate.Habilidad(1);
                return null;
            }

            // Potenciar el ataque: mejor si la habilidad de ataque está por estar lista para aprovechar el bono.
            if (h.Efecto == EfectoHabilidad.PotenciarAtaque)
            {
                if (miPersonaje.BonoAtaque > 0) return null;
                int probabilidad = 50;
                if (miPersonaje.HabilidadAtaque.CooldownActual <= 1) probabilidad = 90;
                if (Probabilidad(probabilidad)) return AccionCombate.Habilidad(1);
                return null;
            }

            // Subir la defensa: si no la tiene subida y el enemigo pega fuerte.
            if (h.Efecto == EfectoHabilidad.PotenciarDefensa)
            {
                if (miPersonaje.BonoDefensaActual > 0) return null;
                int probabilidad = 40;
                if (peligro) probabilidad = 80;
                if (Probabilidad(probabilidad)) return AccionCombate.Habilidad(1);
                return null;
            }

            // Reflejo: si no lo tiene activo y el enemigo pega fuerte.
            if (h.Efecto == EfectoHabilidad.Reflejo)
            {
                if (miPersonaje.ReflejoActual > 0) return null;
                int probabilidad = 40;
                if (peligro) probabilidad = 80;
                if (Probabilidad(probabilidad)) return AccionCombate.Habilidad(1);
                return null;
            }

            // Lo que queda es recargar energía: si le queda la mitad o menos.
            if (miPersonaje.Energia <= miPersonaje.EnergiaMax / 2 && Probabilidad(60))
                return AccionCombate.Habilidad(1);
            return null;
        }

        /// <summary>Devuelve true con la probabilidad indicada (por ejemplo 55 = 55% de las veces).</summary>
        private bool Probabilidad(int porcentaje)
        {
            return azar.Next(100) < porcentaje;
        }

        /// <summary>Posición (en la lista del equipo) del aliado vivo al que más vida le falta. Devuelve -1 si nadie está herido.</summary>
        private int IndiceDelMasHerido(EquipoCombate equipo)
        {
            int indice = -1;
            int mayorFaltante = 0;
            for (int i = 0; i < equipo.Miembros.Count; i++)
            {
                Personaje p = equipo.Miembros[i];
                if (!p.EstaVivo) continue;

                int faltante = p.HpMax - p.Hp;
                if (faltante > mayorFaltante)
                {
                    mayorFaltante = faltante;
                    indice = i;
                }
            }
            return indice;
        }
    }
}
