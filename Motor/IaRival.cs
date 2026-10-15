using System;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    /// <summary>
    /// Inteligencia simple del rival (CPU): releva si está en desventaja elemental, usa su
    /// habilidad cuando conviene, cura al aliado más herido y descansa si le falta energía.
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

            // 3) Habilidad especial.
            if (a.PuedeUsarHabilidad())
            {
                if (a.HabilidadEsOfensiva)
                {
                    if (Probabilidad(70)) return AccionCombate.Habilidad();
                }
                else if (a is CartaAgua)
                {
                    int herido = IndiceMasHerido(yo);
                    if (herido >= 0)
                    {
                        Personaje h = yo.Miembros[herido];
                        int faltante = h.HpMax - h.Hp;
                        if (faltante >= ((CartaAgua)a).CuracionDeHabilidad * 0.6)
                            return AccionCombate.HabilidadSobreAliado(herido);
                    }
                }
                else if (suMult >= 1.0 && Probabilidad(60))
                {
                    return AccionCombate.Habilidad();   // Muro de Roca
                }
            }

            // 4) Descansar si le falta energía para la habilidad.
            if (a.CooldownActual == 0 && a.Energia < a.CostoHabilidad && Probabilidad(25))
                return AccionCombate.Pasar();

            return AccionCombate.Atacar();
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
