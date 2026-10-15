using System;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    /// <summary>
    /// Inteligencia simple del rival (CPU): releva si está en desventaja elemental, usa sus habilidades
    /// de utilidad cuando conviene (curar, potenciar, reflejar...), ataca con su habilidad y descansa si le falta energía.
    /// </summary>
    public class IaRival
    {
        private Random azar = new Random();

        /// <summary>Decide una acción VÁLIDA para el equipo que tiene el turno en el combate.</summary>
        public AccionCombate Decidir(Combate combate)
        {
            EquipoCombate yo = combate.EquipoEnTurno;
            EquipoCombate otro = combate.Oponente(yo);
            Personaje a = yo.Activo;
            Personaje e = otro.Activo;

            double miMult = a.ObtenerMultiplicador(e.Elemento);
            double suMult = e.ObtenerMultiplicador(a.Elemento);

            // El relevo es posible si no relevó en el turno anterior y hay alguien en la cola.
            bool puedeRelevar = !yo.RelevoBloqueado && yo.HayRelevoDisponible;

            // 1) Relevo táctico: está en desventaja y el que sigue en la cola es mejor contra el enemigo.
            if (puedeRelevar && suMult > 1.0 && miMult <= 1.0
                && yo.ProximoRelevo.ObtenerMultiplicador(e.Elemento) > miMult && Probabilidad(55))
                return AccionCombate.Relevar();

            // 2) Relevo de emergencia con poca vida.
            if (puedeRelevar && a.Hp < a.HpMax * 0.25 && Probabilidad(35))
                return AccionCombate.Relevar();

            // 3) Habilidad de utilidad, si en este momento sirve de algo.
            AccionCombate utilidad = DecidirUtilidad(yo, a, suMult);
            if (utilidad != null) return utilidad;

            // 4) Habilidad de ataque.
            if (a.PuedeUsarHabilidad(0) && Probabilidad(75))
                return AccionCombate.Habilidad(0);

            // 5) Descansar si le falta energía para la habilidad de ataque.
            if (a.HabilidadAtaque.CooldownActual == 0 && a.Energia < a.HabilidadAtaque.Costo && Probabilidad(30))
                return AccionCombate.Pasar();

            return AccionCombate.Atacar();
        }

        /// <summary>Devuelve la acción con la habilidad de utilidad si conviene usarla ahora, o null si no.</summary>
        private AccionCombate DecidirUtilidad(EquipoCombate yo, Personaje a, double suMult)
        {
            if (!a.PuedeUsarHabilidad(1)) return null;
            Habilidad h = a.HabilidadUtilidad;

            switch (h.Efecto)
            {
                case EfectoHabilidad.Curar:
                    {
                        int herido = IndiceMasHerido(yo);
                        if (herido >= 0)
                        {
                            Personaje p = yo.Miembros[herido];
                            if (p.HpMax - p.Hp >= a.CuracionDe(h) * 0.6)
                                return AccionCombate.HabilidadSobreAliado(1, herido);
                        }
                        return null;
                    }
                case EfectoHabilidad.CurarEquipo:
                    {
                        int faltante = 0;
                        foreach (Personaje p in yo.Miembros)
                        {
                            if (p.EstaVivo) faltante += p.HpMax - p.Hp;
                        }
                        return faltante >= a.CuracionDe(h) * 1.5 ? AccionCombate.Habilidad(1) : null;
                    }
                case EfectoHabilidad.PotenciarAtaque:
                    return (a.BonoAtaque == 0 && Probabilidad(65)) ? AccionCombate.Habilidad(1) : null;

                case EfectoHabilidad.PotenciarDefensa:
                    return (a.BonoDefensaActual == 0 && suMult >= 1.0 && Probabilidad(55)) ? AccionCombate.Habilidad(1) : null;

                case EfectoHabilidad.Reflejo:
                    return (a.ReflejoActual == 0 && Probabilidad(50)) ? AccionCombate.Habilidad(1) : null;

                default:   // RecargarEnergia
                    return (a.Energia <= a.EnergiaMax / 2 && Probabilidad(60)) ? AccionCombate.Habilidad(1) : null;
            }
        }

        private bool Probabilidad(int porcentaje)
        {
            return azar.Next(100) < porcentaje;
        }

        /// <summary>Posición (en la lista de miembros) del aliado vivo al que más vida le falta. -1 si nadie está herido.</summary>
        private int IndiceMasHerido(EquipoCombate equipo)
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
