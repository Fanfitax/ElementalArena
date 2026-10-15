using System;

namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Clase base abstracta de todas las cartas/personajes de Arena Elemental.
    /// Encapsula las estadísticas y valida que nunca queden valores negativos:
    ///  - Estadísticas base (ATK, DEF, HP máx., ENG máx., Empatía...): lanzan
    ///    ArgumentOutOfRangeException si se intenta asignar un valor negativo.
    ///  - Estadísticas dinámicas (HP, ENG, cooldown): se ajustan (clamp) al rango [0, máximo].
    /// </summary>
    public abstract class Personaje
    {
        // ---------- Constantes del triángulo elemental y reglas generales ----------
        public const double MultiplicadorVentaja = 1.5;
        public const double MultiplicadorDesventaja = 0.5;
        public const double MultiplicadorNeutro = 1.0;

        /// <summary>Energía que regenera el personaje activo al iniciar su turno.</summary>
        public const int EnergiaPorTurno = 2;

        // ---------- Campos privados ----------
        private int id;
        private string nombre;
        private readonly Elemento elemento;
        private int hpMax;
        private int hp;
        private int atk;
        private int defensa;
        private int energiaMax;
        private int energia;
        private int empatia;
        private string nombreHabilidad;
        private int costoHabilidad;
        private int cooldownHabilidad;
        private int cooldownActual;
        private int usosHabilidad;
        private double multiplicadorEscenario = 1.0;

        // ---------- Constructor ----------
        protected Personaje(int id, string nombre, Elemento elemento,
                            int hpMax, int atk, int defensa,
                            int energiaMax, int empatia,
                            string nombreHabilidad, int costoHabilidad, int cooldownHabilidad)
        {
            this.elemento = elemento;
            Id = id;
            Nombre = nombre;
            HpMax = hpMax;
            Atk = atk;
            Defensa = defensa;
            EnergiaMax = energiaMax;
            Empatia = empatia;
            NombreHabilidad = nombreHabilidad;
            CostoHabilidad = costoHabilidad;
            CooldownHabilidad = cooldownHabilidad;

            // Estado inicial: vida y energía completas, habilidad lista.
            Hp = HpMax;
            Energia = EnergiaMax;
            cooldownActual = 0;
            usosHabilidad = 0;
        }

        // ---------- Propiedades encapsuladas ----------
        public int Id
        {
            get { return id; }
            set { id = ValidarNoNegativo(value, "Id"); }
        }

        public string Nombre
        {
            get { return nombre; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre del personaje no puede estar vacío.", "Nombre");
                nombre = value.Trim();
            }
        }

        public Elemento Elemento
        {
            get { return elemento; }
        }

        /// <summary>Vida máxima (debe ser mayor que 0). Si se reduce, recorta el HP actual.</summary>
        public int HpMax
        {
            get { return hpMax; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("HpMax", value, "La vida máxima debe ser mayor que 0.");
                hpMax = value;
                if (hp > hpMax) hp = hpMax;
            }
        }

        /// <summary>Vida actual, siempre dentro de [0, HpMax].</summary>
        public int Hp
        {
            get { return hp; }
            set { hp = Limitar(value, 0, hpMax); }
        }

        public int Atk
        {
            get { return atk; }
            set { atk = ValidarNoNegativo(value, "Atk"); }
        }

        /// <summary>Defensa base (escudo). Ver DefensaEfectiva para incluir efectos temporales.</summary>
        public int Defensa
        {
            get { return defensa; }
            set { defensa = ValidarNoNegativo(value, "Defensa"); }
        }

        public int EnergiaMax
        {
            get { return energiaMax; }
            set
            {
                energiaMax = ValidarNoNegativo(value, "EnergiaMax");
                if (energia > energiaMax) energia = energiaMax;
            }
        }

        /// <summary>Energía actual, siempre dentro de [0, EnergiaMax].</summary>
        public int Energia
        {
            get { return energia; }
            set { energia = Limitar(value, 0, energiaMax); }
        }

        /// <summary>Empatía: potencia curaciones/efectos de apoyo y el bonus al entrar de relevo.</summary>
        public int Empatia
        {
            get { return empatia; }
            set { empatia = ValidarNoNegativo(value, "Empatia"); }
        }

        // ---------- Habilidad especial ----------
        public string NombreHabilidad
        {
            get { return nombreHabilidad; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La habilidad debe tener nombre.", "NombreHabilidad");
                nombreHabilidad = value.Trim();
            }
        }

        public int CostoHabilidad
        {
            get { return costoHabilidad; }
            set { costoHabilidad = ValidarNoNegativo(value, "CostoHabilidad"); }
        }

        /// <summary>Turnos de recarga fijos de la habilidad tras usarla.</summary>
        public int CooldownHabilidad
        {
            get { return cooldownHabilidad; }
            set
            {
                cooldownHabilidad = ValidarNoNegativo(value, "CooldownHabilidad");
                if (cooldownActual > cooldownHabilidad) cooldownActual = cooldownHabilidad;
            }
        }

        /// <summary>Turnos que faltan para poder usar de nuevo la habilidad, en [0, CooldownHabilidad].</summary>
        public int CooldownActual
        {
            get { return cooldownActual; }
            set { cooldownActual = Limitar(value, 0, cooldownHabilidad); }
        }

        /// <summary>Contador de veces que se ha usado la habilidad especial (estadística).</summary>
        public int UsosHabilidad
        {
            get { return usosHabilidad; }
        }

        /// <summary>
        /// Multiplicador de ataque aplicado por el escenario (1.0 = sin efecto).
        /// Lo configura el motor de combate al iniciar la partida.
        /// </summary>
        public double MultiplicadorEscenario
        {
            get { return multiplicadorEscenario; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("MultiplicadorEscenario", value, "Debe ser mayor que 0.");
                multiplicadorEscenario = value;
            }
        }

        // ---------- Propiedades derivadas ----------
        public bool EstaVivo
        {
            get { return hp > 0; }
        }

        /// <summary>Defensa real en combate. Las hijas la sobrescriben si tienen bonus temporales.</summary>
        public virtual int DefensaEfectiva
        {
            get { return defensa; }
        }

        /// <summary>True si la habilidad se usa contra un enemigo; false si es de apoyo/propia.</summary>
        public abstract bool HabilidadEsOfensiva { get; }

        /// <summary>Crea una copia nueva con las mismas estadísticas base y el estado inicial (vida/energía completas).</summary>
        public abstract Personaje CrearCopia();

        // ---------- Triángulo elemental y cálculo de daño ----------

        /// <summary>
        /// Multiplicador de daño de este personaje contra un elemento enemigo.
        /// Las clases hijas lo sobrescriben según el triángulo Agua > Fuego > Aire > Tierra > Agua.
        /// </summary>
        public virtual double ObtenerMultiplicador(Elemento enemigo)
        {
            return MultiplicadorNeutro;
        }

        /// <summary>Daño de un ataque básico contra el objetivo (mínimo 1).</summary>
        public virtual int CalcularDano(Personaje objetivo)
        {
            return CalcularDanoPotenciado(objetivo, 1.0, 0.0);
        }

        /// <summary>
        /// Fórmula común: max(1, ATK * factor * multiplicadorElemental * escenario - DEF * (1 - defensaIgnorada)).
        /// </summary>
        /// <param name="factor">Potencia del golpe (1.0 = ataque básico).</param>
        /// <param name="fraccionDefensaIgnorada">Parte de la defensa objetivo que se ignora, entre 0 y 1.</param>
        protected int CalcularDanoPotenciado(Personaje objetivo, double factor, double fraccionDefensaIgnorada)
        {
            ValidarObjetivo(objetivo);
            if (factor < 0)
                throw new ArgumentOutOfRangeException("factor", factor, "El factor de daño no puede ser negativo.");
            if (fraccionDefensaIgnorada < 0 || fraccionDefensaIgnorada > 1)
                throw new ArgumentOutOfRangeException("fraccionDefensaIgnorada", fraccionDefensaIgnorada, "Debe estar entre 0 y 1.");

            double bruto = Atk * factor * ObtenerMultiplicador(objetivo.Elemento) * multiplicadorEscenario;
            double escudo = objetivo.DefensaEfectiva * (1.0 - fraccionDefensaIgnorada);
            int dano = (int)Math.Round(bruto - escudo, MidpointRounding.AwayFromZero);
            return Math.Max(1, dano);
        }

        // ---------- Acciones de combate ----------

        /// <summary>Ataque básico. Devuelve el texto del evento para el registro de combate.</summary>
        public virtual string Atacar(Personaje objetivo)
        {
            ValidarObjetivo(objetivo);
            if (!EstaVivo)
                return Nombre + " está derrotado y no puede actuar.";
            if (!objetivo.EstaVivo)
                return Nombre + " no puede atacar a " + objetivo.Nombre + ": ya está derrotado.";

            double mult = ObtenerMultiplicador(objetivo.Elemento);
            int real = objetivo.RecibirDano(CalcularDano(objetivo));
            return Nombre + " ataca a " + objetivo.Nombre + " y causa " + real + " de daño" + DescribirEfectividad(mult) + ".";
        }

        /// <summary>
        /// Usa la habilidad especial. Si la habilidad es ofensiva, 'objetivo' es el enemigo;
        /// si es de apoyo, es el aliado a apoyar (null = el propio personaje).
        /// Devuelve el texto del evento. Si no se puede usar, no consume nada.
        /// </summary>
        public abstract string UsarHabilidadEspecial(Personaje objetivo);

        /// <summary>Resta vida (nunca por debajo de 0) y devuelve el daño realmente aplicado.</summary>
        public virtual int RecibirDano(int dano)
        {
            if (dano < 0)
                throw new ArgumentOutOfRangeException("dano", dano, "El daño no puede ser negativo.");
            int antes = hp;
            Hp = hp - dano;
            return antes - hp;
        }

        /// <summary>Recupera vida (sin pasar de HpMax ni revivir). Devuelve la vida realmente curada.</summary>
        public int Curar(int cantidad)
        {
            if (cantidad < 0)
                throw new ArgumentOutOfRangeException("cantidad", cantidad, "La curación no puede ser negativa.");
            if (!EstaVivo) return 0;
            int antes = hp;
            Hp = hp + cantidad;
            return hp - antes;
        }

        /// <summary>Gasta energía. Devuelve false (sin gastar nada) si no hay suficiente.</summary>
        public bool GastarEnergia(int cantidad)
        {
            if (cantidad < 0)
                throw new ArgumentOutOfRangeException("cantidad", cantidad, "El gasto de energía no puede ser negativo.");
            if (cantidad > energia) return false;
            energia -= cantidad;
            return true;
        }

        /// <summary>Recupera energía (sin pasar de EnergiaMax). Devuelve la energía realmente ganada.</summary>
        public int RecuperarEnergia(int cantidad)
        {
            if (cantidad < 0)
                throw new ArgumentOutOfRangeException("cantidad", cantidad, "La energía recuperada no puede ser negativa.");
            int antes = energia;
            Energia = energia + cantidad;
            return energia - antes;
        }

        /// <summary>Vivo, sin cooldown y con energía suficiente.</summary>
        public bool PuedeUsarHabilidad()
        {
            return EstaVivo && cooldownActual == 0 && energia >= costoHabilidad;
        }

        /// <summary>Descuenta energía, activa el cooldown y suma al contador de usos.</summary>
        protected void ConsumirHabilidad()
        {
            if (!PuedeUsarHabilidad())
                throw new InvalidOperationException("La habilidad no está disponible.");
            energia -= costoHabilidad;
            cooldownActual = cooldownHabilidad;
            usosHabilidad++;
        }

        /// <summary>Explica por qué no se puede usar la habilidad (para el log).</summary>
        protected string MensajeHabilidadNoDisponible()
        {
            if (!EstaVivo) return Nombre + " está derrotado y no puede actuar.";
            if (cooldownActual > 0)
                return NombreHabilidad + " en recarga (" + cooldownActual + " turno(s)).";
            return Nombre + " no tiene energía suficiente para " + NombreHabilidad
                   + " (" + energia + "/" + costoHabilidad + ").";
        }

        // ---------- Ciclo de turnos y relevos ----------

        /// <summary>Se invoca al inicio del turno del personaje activo: avanza cooldown y regenera energía.</summary>
        public virtual void IniciarTurno()
        {
            if (!EstaVivo) return;
            if (cooldownActual > 0) cooldownActual--;
            RecuperarEnergia(EnergiaPorTurno);
        }

        /// <summary>Se invoca al entrar de relevo; la Empatía otorga energía extra.</summary>
        public virtual string AlEntrarEnRelevo(Personaje saliente)
        {
            int bonus = RecuperarEnergia(Empatia / 2);
            string texto = Nombre + " entra al combate";
            if (saliente != null) texto += " relevando a " + saliente.Nombre;
            if (bonus > 0) texto += " y recupera " + bonus + " de energía por su empatía";
            return texto + ".";
        }

        /// <summary>Se invoca cuando el personaje deja el campo (relevo o derrota). Limpia efectos temporales.</summary>
        public virtual void AlSalirDelCampo()
        {
        }

        // ---------- Utilidades ----------
        protected static string DescribirEfectividad(double multiplicador)
        {
            if (multiplicador > MultiplicadorNeutro) return " (¡muy efectivo!)";
            if (multiplicador < MultiplicadorNeutro) return " (poco efectivo)";
            return string.Empty;
        }

        protected static void ValidarObjetivo(Personaje objetivo)
        {
            if (objetivo == null)
                throw new ArgumentNullException("objetivo");
        }

        private static int ValidarNoNegativo(int valor, string campo)
        {
            if (valor < 0)
                throw new ArgumentOutOfRangeException(campo, valor, campo + " no puede ser negativo.");
            return valor;
        }

        private static int Limitar(int valor, int minimo, int maximo)
        {
            if (valor < minimo) return minimo;
            if (valor > maximo) return maximo;
            return valor;
        }

        public override string ToString()
        {
            return Nombre + " [" + Elemento + "] HP " + Hp + "/" + HpMax
                   + " ATK " + Atk + " DEF " + DefensaEfectiva + " ENG " + Energia + "/" + EnergiaMax;
        }
    }
}
