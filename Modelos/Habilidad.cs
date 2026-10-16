using System;
using System.Globalization;

namespace ArenaElemental.Modelos
{
    /// <summary>Qué hace una habilidad. Las cuatro primeras son de ATAQUE y las demás son de UTILIDAD.</summary>
    public enum EfectoHabilidad
    {
        // Ataque
        GolpePotenciado,     // un golpe fuerte (puede ignorar parte de la defensa)
        DobleGolpe,          // dos golpes seguidos
        GolpeVampirico,      // un golpe que cura a quien lo usa
        GolpeConRetroceso,   // un golpe muy fuerte que también lastima a quien lo usa

        // Utilidad
        Curar,               // cura a un aliado
        CurarEquipo,         // cura a todo el equipo
        PotenciarAtaque,     // sube el ataque unos turnos
        PotenciarDefensa,    // sube la defensa unos turnos
        Reflejo,             // devuelve parte del daño recibido
        RecargarEnergia      // recupera energía
    }

    /// <summary>
    /// Una habilidad especial de un personaje. Cada personaje tiene dos:
    /// una de ataque y una de utilidad.
    ///
    /// Todas las habilidades se crean con el mismo constructor; lo que cambia es el "efecto"
    /// y los tres números que lo acompañan (potencia, extra y duración), que significan:
    ///
    ///   Efecto             potencia                   extra                    duración
    ///   GolpePotenciado    factor de daño (2.0 = x2)  defensa que ignora (0-1) -
    ///   DobleGolpe         factor de cada golpe       -                        energía que recupera
    ///   GolpeVampirico     factor de daño             fracción del daño que cura -
    ///   GolpeConRetroceso  factor de daño             fracción de su vida que pierde -
    ///   Curar              fracción de vida que cura  -                        -
    ///   CurarEquipo        fracción de vida que cura  -                        -
    ///   PotenciarAtaque    aumento (0.3 = +30%)       -                        turnos
    ///   PotenciarDefensa   -                          -                        turnos
    ///   Reflejo            fracción que devuelve      -                        turnos
    ///   RecargarEnergia    -                          -                        energía que recupera
    /// </summary>
    public class Habilidad
    {
        public Habilidad(string nombre, EfectoHabilidad efecto, int costo, int cooldown,
                         double potencia, double extra, int duracion)
        {
            Nombre = nombre;
            Efecto = efecto;
            Costo = costo;
            Cooldown = cooldown;
            Potencia = potencia;
            Extra = extra;
            Duracion = duracion;
            CooldownActual = 0;
            Usos = 0;
        }

        public string Nombre { get; private set; }
        public EfectoHabilidad Efecto { get; private set; }
        public int Costo { get; private set; }            // energía que gasta
        public int Cooldown { get; private set; }         // turnos de recarga después de usarla
        public double Potencia { get; private set; }
        public double Extra { get; private set; }
        public int Duracion { get; private set; }

        /// <summary>Turnos que faltan para poder usarla de nuevo (0 = lista).</summary>
        public int CooldownActual { get; set; }

        /// <summary>Cuántas veces se ha usado.</summary>
        public int Usos { get; private set; }

        /// <summary>True si es una habilidad de ataque (se usa contra el enemigo).</summary>
        public bool EsOfensiva
        {
            get
            {
                return Efecto == EfectoHabilidad.GolpePotenciado
                    || Efecto == EfectoHabilidad.DobleGolpe
                    || Efecto == EfectoHabilidad.GolpeVampirico
                    || Efecto == EfectoHabilidad.GolpeConRetroceso;
            }
        }

        /// <summary>True si el jugador tiene que elegir a cuál aliado curar.</summary>
        public bool RequiereAliado
        {
            get { return Efecto == EfectoHabilidad.Curar; }
        }

        /// <summary>El tipo en texto, para mostrarlo en pantalla.</summary>
        public string Tipo
        {
            get
            {
                if (EsOfensiva) return "Ataque";
                return "Utilidad";
            }
        }

        /// <summary>Se llama cuando el personaje usa la habilidad: empieza la recarga y suma un uso.</summary>
        public void Usar()
        {
            CooldownActual = Cooldown;
            Usos++;
        }

        /// <summary>Se llama al iniciar el turno del personaje: la recarga baja 1.</summary>
        public void PasarTurno()
        {
            if (CooldownActual > 0) CooldownActual--;
        }

        /// <summary>Una copia nueva de la habilidad, lista para usar (sin recarga y sin usos).</summary>
        public Habilidad Clonar()
        {
            return new Habilidad(Nombre, Efecto, Costo, Cooldown, Potencia, Extra, Duracion);
        }

        /// <summary>Explica en una frase lo que hace la habilidad (se muestra en los tooltips).</summary>
        public string Descripcion
        {
            get
            {
                // Se usa la cultura invariante para que el decimal salga con punto (2.0) en cualquier PC.
                string x = Potencia.ToString("0.0#", CultureInfo.InvariantCulture);

                if (Efecto == EfectoHabilidad.GolpePotenciado)
                {
                    if (Extra > 0) return "Golpe x" + x + " que ignora el " + Porcentaje(Extra) + " de la defensa.";
                    return "Golpe x" + x + ".";
                }
                if (Efecto == EfectoHabilidad.DobleGolpe)
                {
                    if (Duracion > 0) return "Dos golpes de x" + x + " cada uno y recupera " + Duracion + " de energía.";
                    return "Dos golpes de x" + x + " cada uno.";
                }
                if (Efecto == EfectoHabilidad.GolpeVampirico)
                    return "Golpe x" + x + " que cura a quien lo usa el " + Porcentaje(Extra) + " del daño causado.";
                if (Efecto == EfectoHabilidad.GolpeConRetroceso)
                    return "Golpe x" + x + ". Quien lo usa pierde el " + Porcentaje(Extra) + " de su vida máxima (nunca cae por esto).";
                if (Efecto == EfectoHabilidad.Curar)
                    return "Cura a un aliado el " + Porcentaje(Potencia) + " de la vida máxima de quien la usa + 2 por Empatía.";
                if (Efecto == EfectoHabilidad.CurarEquipo)
                    return "Cura a todo el equipo el " + Porcentaje(Potencia) + " de la vida máxima de quien la usa + 2 por Empatía.";
                if (Efecto == EfectoHabilidad.PotenciarAtaque)
                    return "Sube el ataque +" + Porcentaje(Potencia) + " durante " + Duracion + " turnos propios.";
                if (Efecto == EfectoHabilidad.PotenciarDefensa)
                    return "Sube la defensa (mitad de su DEF + mitad de su Empatía) durante " + Duracion + " turnos del rival.";
                if (Efecto == EfectoHabilidad.Reflejo)
                    return "Devuelve el " + Porcentaje(Potencia) + " del daño que reciba durante " + Duracion + " turnos del rival.";

                return "Recupera " + Duracion + " de energía.";   // RecargarEnergia
            }
        }

        /// <summary>Convierte una fracción en porcentaje: 0.3 pasa a "30%".</summary>
        private string Porcentaje(double fraccion)
        {
            return (int)Math.Round(fraccion * 100) + "%";
        }

        public override string ToString()
        {
            return Nombre + " (" + Tipo + ", " + Costo + " ENG, recarga " + Cooldown + ")";
        }
    }
}
