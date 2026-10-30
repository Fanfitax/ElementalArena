using System;
using System.Globalization;

namespace ArenaElemental.Modelos
{
    // Qué hace una habilidad. Las cuatro primeras son de ATAQUE y las demás son de UTILIDAD.
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

    // Una habilidad especial de un personaje. Cada personaje tiene dos:
    // una de ataque y una de utilidad.
    //
    // Todas las habilidades se crean con el mismo constructor; lo que cambia es el "efecto"
    // y los tres números que lo acompañan (potencia, extra y duración), que significan:
    //
    //   Efecto             potencia                   extra                    duración
    //   GolpePotenciado    factor de daño (2.0 = x2)  defensa que ignora (0-1) -
    //   DobleGolpe         factor de cada golpe       -                        energía que recupera
    //   GolpeVampirico     factor de daño             fracción del daño que cura -
    //   GolpeConRetroceso  factor de daño             fracción de su vida que pierde -
    //   Curar              fracción de vida que cura  -                        -
    //   CurarEquipo        fracción de vida que cura  -                        -
    //   PotenciarAtaque    aumento (0.3 = +30%)       -                        turnos
    //   PotenciarDefensa   -                          -                        turnos
    //   Reflejo            fracción que devuelve      -                        turnos
    //   RecargarEnergia    -                          -                        energía que recupera
    public class Habilidad
    {
        // El nombre se guarda en español y en inglés; la propiedad Nombre devuelve el del idioma elegido.
        private string nombre;
        private string nombreIngles;

        public Habilidad(string nombre, string nombreIngles, EfectoHabilidad efecto, int costo, int cooldown,
                         double potencia, double extra, int duracion)
        {
            this.nombre = nombre;
            this.nombreIngles = nombreIngles;
            Efecto = efecto;
            Costo = costo;
            Cooldown = cooldown;
            Potencia = potencia;
            Extra = extra;
            Duracion = duracion;
            CooldownActual = 0;
            Usos = 0;
        }

        public string Nombre
        {
            get { return Idioma.Texto(nombre, nombreIngles); }
        }

        public EfectoHabilidad Efecto { get; private set; }
        public int Costo { get; private set; }            // energía que gasta
        public int Cooldown { get; private set; }         // turnos de recarga después de usarla
        public double Potencia { get; private set; }
        public double Extra { get; private set; }
        public int Duracion { get; private set; }

        // Turnos que faltan para poder usarla de nuevo (0 = lista).
        public int CooldownActual { get; set; }

        // Cuántas veces se ha usado.
        public int Usos { get; private set; }

        // True si es una habilidad de ataque (se usa contra el enemigo).
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

        // True si el jugador tiene que elegir a cuál aliado curar.
        public bool RequiereAliado
        {
            get { return Efecto == EfectoHabilidad.Curar; }
        }

        // El tipo en texto, para mostrarlo en pantalla.
        public string Tipo
        {
            get
            {
                if (EsOfensiva) return Idioma.Texto("Ataque", "Attack");
                return Idioma.Texto("Utilidad", "Utility");
            }
        }

        // Se llama cuando el personaje usa la habilidad: empieza la recarga y suma un uso.
        public void Usar()
        {
            CooldownActual = Cooldown;
            Usos++;
        }

        // Se llama al iniciar el turno del personaje: la recarga baja 1.
        public void PasarTurno()
        {
            if (CooldownActual > 0) CooldownActual--;
        }

        // Una copia nueva de la habilidad, lista para usar (sin recarga y sin usos).
        public Habilidad Clonar()
        {
            return new Habilidad(nombre, nombreIngles, Efecto, Costo, Cooldown, Potencia, Extra, Duracion);
        }

        // Explica en una frase lo que hace la habilidad (se muestra en los tooltips).
        public string Descripcion
        {
            get
            {
                // Se usa la cultura invariante para que el decimal salga con punto (2.0) en cualquier PC.
                string x = Potencia.ToString("0.0#", CultureInfo.InvariantCulture);

                if (Efecto == EfectoHabilidad.GolpePotenciado)
                {
                    if (Extra > 0)
                        return Idioma.Texto("Golpe x" + x + " que ignora el " + Porcentaje(Extra) + " de la defensa.",
                                            "Hit x" + x + " that ignores " + Porcentaje(Extra) + " of the defense.");
                    return Idioma.Texto("Golpe x" + x + ".", "Hit x" + x + ".");
                }
                if (Efecto == EfectoHabilidad.DobleGolpe)
                {
                    if (Duracion > 0)
                        return Idioma.Texto("Dos golpes de x" + x + " cada uno y recupera " + Duracion + " de energía.",
                                            "Two hits of x" + x + " each and recovers " + Duracion + " energy.");
                    return Idioma.Texto("Dos golpes de x" + x + " cada uno.", "Two hits of x" + x + " each.");
                }
                if (Efecto == EfectoHabilidad.GolpeVampirico)
                    return Idioma.Texto("Golpe x" + x + " que cura a quien lo usa el " + Porcentaje(Extra) + " del daño causado.",
                                        "Hit x" + x + " that heals the user for " + Porcentaje(Extra) + " of the damage dealt.");
                if (Efecto == EfectoHabilidad.GolpeConRetroceso)
                    return Idioma.Texto("Golpe x" + x + ". Quien lo usa pierde el " + Porcentaje(Extra) + " de su vida máxima (nunca cae por esto).",
                                        "Hit x" + x + ". The user loses " + Porcentaje(Extra) + " of their max HP (never falls because of this).");
                if (Efecto == EfectoHabilidad.Curar)
                    return Idioma.Texto("Cura a un aliado el " + Porcentaje(Potencia) + " de la vida máxima de quien la usa + 2 por Empatía.",
                                        "Heals an ally for " + Porcentaje(Potencia) + " of the user's max HP + 2 per Empathy.");
                if (Efecto == EfectoHabilidad.CurarEquipo)
                    return Idioma.Texto("Cura a todo el equipo el " + Porcentaje(Potencia) + " de la vida máxima de quien la usa + 2 por Empatía.",
                                        "Heals the whole team for " + Porcentaje(Potencia) + " of the user's max HP + 2 per Empathy.");
                if (Efecto == EfectoHabilidad.PotenciarAtaque)
                    return Idioma.Texto("Sube el ataque +" + Porcentaje(Potencia) + " durante " + Duracion + " turnos propios.",
                                        "Raises attack +" + Porcentaje(Potencia) + " for " + Duracion + " of its own turns.");
                if (Efecto == EfectoHabilidad.PotenciarDefensa)
                    return Idioma.Texto("Sube la defensa (mitad de su DEF + mitad de su Empatía) durante " + Duracion + " turnos del rival.",
                                        "Raises defense (half of its DEF + half of its Empathy) for " + Duracion + " opponent turns.");
                if (Efecto == EfectoHabilidad.Reflejo)
                    return Idioma.Texto("Devuelve el " + Porcentaje(Potencia) + " del daño que reciba durante " + Duracion + " turnos del rival.",
                                        "Returns " + Porcentaje(Potencia) + " of the damage received for " + Duracion + " opponent turns.");

                // RecargarEnergia
                return Idioma.Texto("Recupera " + Duracion + " de energía.", "Recovers " + Duracion + " energy.");
            }
        }

        // Convierte una fracción en porcentaje: 0.3 pasa a "30%".
        private string Porcentaje(double fraccion)
        {
            return (int)Math.Round(fraccion * 100) + "%";
        }

        public override string ToString()
        {
            return Nombre + " (" + Tipo + ", " + Costo + " ENG, " + Idioma.Texto("recarga ", "cooldown ") + Cooldown + ")";
        }
    }
}
