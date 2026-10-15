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
    /// Una habilidad especial de un personaje: nombre, costo de energía, recarga y efecto.
    /// Cada personaje tiene dos: una de ataque y una de utilidad.
    /// Se crean con los métodos estáticos (Golpe, Curacion, PotenciarAtaque...) para que sea fácil leerlas.
    /// </summary>
    public class Habilidad
    {
        private string nombre;
        private string descripcion;
        private EfectoHabilidad efecto;
        private int costo;
        private int cooldown;
        private int cooldownActual;
        private int usos;
        private double potencia;   // factor de daño, porcentaje de curación, de ataque o de reflejo
        private double extra;      // defensa ignorada, robo de vida o retroceso (entre 0 y 1)
        private int duracion;      // turnos que dura el efecto (o energía que se recupera)

        private Habilidad(string nombre, EfectoHabilidad efecto, int costo, int cooldown,
                          double potencia, double extra, int duracion)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("La habilidad debe tener nombre.", "nombre");
            if (costo < 0) throw new ArgumentOutOfRangeException("costo", costo, "El costo no puede ser negativo.");
            if (cooldown < 0) throw new ArgumentOutOfRangeException("cooldown", cooldown, "La recarga no puede ser negativa.");
            if (potencia < 0) throw new ArgumentOutOfRangeException("potencia", potencia, "La potencia no puede ser negativa.");

            this.nombre = nombre.Trim();
            this.efecto = efecto;
            this.costo = costo;
            this.cooldown = cooldown;
            this.potencia = potencia;
            this.extra = extra;
            this.duracion = duracion;
            this.descripcion = ConstruirDescripcion();
        }

        // ---------- Formas de crear habilidades ----------

        /// <summary>Un golpe de daño x"factor" que ignora una parte de la defensa del enemigo (0 a 1).</summary>
        public static Habilidad Golpe(string nombre, int costo, int cooldown, double factor, double defensaIgnorada)
        {
            return new Habilidad(nombre, EfectoHabilidad.GolpePotenciado, costo, cooldown, factor, defensaIgnorada, 0);
        }

        /// <summary>Dos golpes de daño x"factorPorGolpe" cada uno; después recupera algo de energía.</summary>
        public static Habilidad GolpeDoble(string nombre, int costo, int cooldown, double factorPorGolpe, int energiaRecuperada)
        {
            return new Habilidad(nombre, EfectoHabilidad.DobleGolpe, costo, cooldown, factorPorGolpe, 0, energiaRecuperada);
        }

        /// <summary>Un golpe que cura a quien lo usa una fracción (0 a 1) del daño causado.</summary>
        public static Habilidad GolpeVampirico(string nombre, int costo, int cooldown, double factor, double robo)
        {
            return new Habilidad(nombre, EfectoHabilidad.GolpeVampirico, costo, cooldown, factor, robo, 0);
        }

        /// <summary>Un golpe muy fuerte; quien lo usa pierde una fracción (0 a 1) de su vida máxima, sin llegar a caer.</summary>
        public static Habilidad GolpeConRetroceso(string nombre, int costo, int cooldown, double factor, double retroceso)
        {
            return new Habilidad(nombre, EfectoHabilidad.GolpeConRetroceso, costo, cooldown, factor, retroceso, 0);
        }

        /// <summary>Cura a un aliado esa fracción de la vida máxima de quien la usa, más 2 por punto de Empatía.</summary>
        public static Habilidad Curacion(string nombre, int costo, int cooldown, double fraccionDeVida)
        {
            return new Habilidad(nombre, EfectoHabilidad.Curar, costo, cooldown, fraccionDeVida, 0, 0);
        }

        /// <summary>Igual que Curacion pero cura a todos los aliados vivos.</summary>
        public static Habilidad CuracionEquipo(string nombre, int costo, int cooldown, double fraccionDeVida)
        {
            return new Habilidad(nombre, EfectoHabilidad.CurarEquipo, costo, cooldown, fraccionDeVida, 0, 0);
        }

        /// <summary>Sube el ataque ese porcentaje (0.3 = +30%) durante "turnos" turnos propios.</summary>
        public static Habilidad PotenciarAtaque(string nombre, int costo, int cooldown, double porcentaje, int turnos)
        {
            return new Habilidad(nombre, EfectoHabilidad.PotenciarAtaque, costo, cooldown, porcentaje, 0, turnos);
        }

        /// <summary>Sube la defensa durante "turnos" turnos del rival.</summary>
        public static Habilidad PotenciarDefensa(string nombre, int costo, int cooldown, int turnos)
        {
            return new Habilidad(nombre, EfectoHabilidad.PotenciarDefensa, costo, cooldown, 0, 0, turnos);
        }

        /// <summary>Devuelve ese porcentaje (0.5 = 50%) del daño recibido durante "turnos" turnos del rival.</summary>
        public static Habilidad Reflejo(string nombre, int costo, int cooldown, double porcentaje, int turnos)
        {
            return new Habilidad(nombre, EfectoHabilidad.Reflejo, costo, cooldown, porcentaje, 0, turnos);
        }

        /// <summary>Recupera energía.</summary>
        public static Habilidad RecargaEnergia(string nombre, int costo, int cooldown, int energia)
        {
            return new Habilidad(nombre, EfectoHabilidad.RecargarEnergia, costo, cooldown, 0, 0, energia);
        }

        // ---------- Datos de la habilidad ----------

        public string Nombre { get { return nombre; } }
        public string Descripcion { get { return descripcion; } }
        public EfectoHabilidad Efecto { get { return efecto; } }
        public int Costo { get { return costo; } }
        public int Cooldown { get { return cooldown; } }
        public double Potencia { get { return potencia; } }
        public double Extra { get { return extra; } }
        public int Duracion { get { return duracion; } }
        public int Usos { get { return usos; } }

        /// <summary>Turnos que faltan para poder usarla de nuevo, siempre entre 0 y Cooldown.</summary>
        public int CooldownActual
        {
            get { return cooldownActual; }
            set
            {
                if (value < 0) value = 0;
                if (value > cooldown) value = cooldown;
                cooldownActual = value;
            }
        }

        /// <summary>True si se usa contra el enemigo (ataque); false si es de utilidad.</summary>
        public bool EsOfensiva
        {
            get { return efecto <= EfectoHabilidad.GolpeConRetroceso; }
        }

        /// <summary>True si el jugador debe elegir a qué aliado se aplica.</summary>
        public bool RequiereAliado
        {
            get { return efecto == EfectoHabilidad.Curar; }
        }

        /// <summary>Tipo para mostrar en pantalla.</summary>
        public string Tipo
        {
            get { return EsOfensiva ? "Ataque" : "Utilidad"; }
        }

        // ---------- Uso ----------

        /// <summary>Activa la recarga y suma un uso. La energía la descuenta el personaje.</summary>
        internal void Registrar()
        {
            cooldownActual = cooldown;
            usos++;
        }

        /// <summary>Se llama al iniciar el turno del personaje: baja la recarga en 1.</summary>
        internal void AvanzarTurno()
        {
            if (cooldownActual > 0) cooldownActual--;
        }

        /// <summary>Copia de la habilidad lista para usar (sin recarga ni usos).</summary>
        public Habilidad Clonar()
        {
            Habilidad copia = (Habilidad)MemberwiseClone();
            copia.cooldownActual = 0;
            copia.usos = 0;
            return copia;
        }

        // ---------- Texto ----------

        private string ConstruirDescripcion()
        {
            string x = potencia.ToString("0.0#", CultureInfo.InvariantCulture);
            switch (efecto)
            {
                case EfectoHabilidad.GolpePotenciado:
                    return "Golpe x" + x + (extra > 0 ? " que ignora el " + Porcentaje(extra) + " de la defensa." : ".");
                case EfectoHabilidad.DobleGolpe:
                    return "Dos golpes de x" + x + " cada uno" + (duracion > 0 ? " y recupera " + duracion + " de energía." : ".");
                case EfectoHabilidad.GolpeVampirico:
                    return "Golpe x" + x + " que cura a quien lo usa el " + Porcentaje(extra) + " del daño causado.";
                case EfectoHabilidad.GolpeConRetroceso:
                    return "Golpe x" + x + ". Quien lo usa pierde el " + Porcentaje(extra) + " de su vida máxima (nunca cae por esto).";
                case EfectoHabilidad.Curar:
                    return "Cura a un aliado el " + Porcentaje(potencia) + " de la vida máxima de quien la usa + 2 por Empatía.";
                case EfectoHabilidad.CurarEquipo:
                    return "Cura a todo el equipo el " + Porcentaje(potencia) + " de la vida máxima de quien la usa + 2 por Empatía.";
                case EfectoHabilidad.PotenciarAtaque:
                    return "Sube el ataque +" + Porcentaje(potencia) + " durante " + duracion + " turnos propios.";
                case EfectoHabilidad.PotenciarDefensa:
                    return "Sube la defensa (mitad de su DEF + mitad de su Empatía) durante " + duracion + " turnos del rival.";
                case EfectoHabilidad.Reflejo:
                    return "Devuelve el " + Porcentaje(potencia) + " del daño que reciba durante " + duracion + " turnos del rival.";
                default:
                    return "Recupera " + duracion + " de energía.";
            }
        }

        private static string Porcentaje(double fraccion)
        {
            return (int)Math.Round(fraccion * 100) + "%";
        }

        public override string ToString()
        {
            return nombre + " (" + Tipo + ", " + costo + " ENG, recarga " + cooldown + ")";
        }
    }
}
