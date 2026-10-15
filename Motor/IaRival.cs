using System;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    /// <summary>
    /// La inteligencia del rival cuando juega la CPU. No es "inteligencia artificial" de verdad:
    /// son reglas simples en orden de prioridad (si se cumple una, hace esa acción y termina):
    ///   1) Releva si está en desventaja elemental y el que sigue en la cola es mejor.
    ///   2) Releva si tiene muy poca vida.
    ///   3) Usa su habilidad de utilidad si en este momento sirve (curar, potenciar, reflejar...).
    ///   4) Usa su habilidad de ataque.
    ///   5) Descansa si no le alcanza la energía.
    ///   6) Si nada de lo anterior, ataca normal.
    /// Algunas reglas solo se cumplen con cierta probabilidad para que no sea siempre igual.
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

            double miVentaja = miPersonaje.ObtenerMultiplicador(enemigo.Elemento);
            double suVentaja = enemigo.ObtenerMultiplicador(miPersonaje.Elemento);

            // Puede relevar si no relevó en el turno anterior y hay alguien en la cola.
            bool puedeRelevar = !yo.RelevoBloqueado && yo.HayRelevoDisponible;

            // 1) Relevo táctico: el enemigo le gana y el que sigue en la cola le iría mejor.
            if (puedeRelevar && suVentaja > 1.0 && miVentaja <= 1.0)
            {
                double ventajaDelSiguiente = yo.ProximoRelevo.ObtenerMultiplicador(enemigo.Elemento);
                if (ventajaDelSiguiente > miVentaja && Probabilidad(55))
                    return AccionCombate.Relevar();
            }

            // 2) Relevo de emergencia: le queda menos del 25% de vida.
            if (puedeRelevar && miPersonaje.Hp < miPersonaje.HpMax * 0.25 && Probabilidad(35))
                return AccionCombate.Relevar();

            // 3) Habilidad de utilidad, si sirve ahora.
            AccionCombate utilidad = DecidirUtilidad(yo, miPersonaje, suVentaja);
            if (utilidad != null) return utilidad;

            // 4) Habilidad de ataque.
            if (miPersonaje.PuedeUsarHabilidad(0) && Probabilidad(75))
                return AccionCombate.Habilidad(0);

            // 5) Descansar, si la habilidad de ataque está lista pero le falta energía.
            Habilidad ataque = miPersonaje.HabilidadAtaque;
            if (ataque.CooldownActual == 0 && miPersonaje.Energia < ataque.Costo && Probabilidad(30))
                return AccionCombate.Pasar();

            // 6) Ataque básico.
            return AccionCombate.Atacar();
        }

        /// <summary>Devuelve la acción con la habilidad de utilidad si conviene usarla ahora, o null si no conviene.</summary>
        private AccionCombate DecidirUtilidad(EquipoCombate yo, Personaje miPersonaje, double suVentaja)
        {
            if (!miPersonaje.PuedeUsarHabilidad(1)) return null;

            Habilidad h = miPersonaje.HabilidadUtilidad;

            // Curar a un aliado: solo si al más herido le falta bastante vida.
            if (h.Efecto == EfectoHabilidad.Curar)
            {
                int herido = IndiceDelMasHerido(yo);
                if (herido >= 0)
                {
                    Personaje p = yo.Miembros[herido];
                    int faltante = p.HpMax - p.Hp;
                    if (faltante >= miPersonaje.CuracionDe(h) * 0.6)
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

            // Potenciar el ataque: si no tiene el potenciador activo.
            if (h.Efecto == EfectoHabilidad.PotenciarAtaque)
            {
                if (miPersonaje.BonoAtaque == 0 && Probabilidad(65)) return AccionCombate.Habilidad(1);
                return null;
            }

            // Subir la defensa: si no la tiene subida y el enemigo no le gana.
            if (h.Efecto == EfectoHabilidad.PotenciarDefensa)
            {
                if (miPersonaje.BonoDefensaActual == 0 && suVentaja >= 1.0 && Probabilidad(55)) return AccionCombate.Habilidad(1);
                return null;
            }

            // Reflejo: si no lo tiene activo.
            if (h.Efecto == EfectoHabilidad.Reflejo)
            {
                if (miPersonaje.ReflejoActual == 0 && Probabilidad(50)) return AccionCombate.Habilidad(1);
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
