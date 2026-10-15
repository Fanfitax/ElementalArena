using System;
using System.Collections.Generic;

namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Clase base abstracta de todas las cartas/personajes de Arena Elemental.
    /// Encapsula las estadísticas y valida que nunca queden valores negativos:
    ///  - Estadísticas base (ATK, DEF, HP máx., ENG máx., Empatía...): lanzan
    ///    ArgumentOutOfRangeException si se intenta asignar un valor negativo.
    ///  - Estadísticas dinámicas (HP, ENG, recargas): se ajustan (clamp) al rango [0, máximo].
    /// Cada personaje tiene DOS habilidades: una de ataque y una de utilidad (ver la clase Habilidad).
    /// </summary>
    public abstract class Personaje
    {
        // ---------- Constantes del triángulo elemental y reglas generales ----------
        public const double MultiplicadorVentaja = 1.5;
        public const double MultiplicadorDesventaja = 0.5;
        public const double MultiplicadorNeutro = 1.0;

        /// <summary>Energía que regenera el personaje activo al iniciar su turno.</summary>
        public const int EnergiaPorTurno = 2;

        /// <summary>Cada personaje tiene exactamente dos habilidades: 0 = ataque, 1 = utilidad.</summary>
        public const int CantidadHabilidades = 2;

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
        private double multiplicadorEscenario = 1.0;

        private Habilidad[] habilidades = new Habilidad[CantidadHabilidades];

        // Efectos temporales que dejan las habilidades de utilidad.
        private double bonoAtaque;         // 0.3 = +30% de ataque
        private int turnosBonoAtaque;
        private int bonoDefensa;           // defensa extra
        private int turnosBonoDefensa;
        private double reflejo;            // 0.5 = devuelve el 50% del daño recibido
        private int turnosReflejo;

        // ---------- Constructor ----------
        protected Personaje(int id, string nombre, Elemento elemento,
                            int hpMax, int atk, int defensa,
                            int energiaMax, int empatia,
                            Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
        {
            if (habilidadAtaque == null || !habilidadAtaque.EsOfensiva)
                throw new ArgumentException("La primera habilidad debe ser de ataque.", "habilidadAtaque");
            if (habilidadUtilidad == null || habilidadUtilidad.EsOfensiva)
                throw new ArgumentException("La segunda habilidad debe ser de utilidad.", "habilidadUtilidad");

            this.elemento = elemento;
            Id = id;
            Nombre = nombre;
            HpMax = hpMax;
            Atk = atk;
            Defensa = defensa;
            EnergiaMax = energiaMax;
            Empatia = empatia;
            habilidades[0] = habilidadAtaque;
            habilidades[1] = habilidadUtilidad;

            // Estado inicial: vida y energía completas, habilidades listas.
            Hp = HpMax;
            Energia = EnergiaMax;
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

        // ---------- Habilidades ----------

        /// <summary>La habilidad de ataque (número 0).</summary>
        public Habilidad HabilidadAtaque
        {
            get { return habilidades[0]; }
        }

        /// <summary>La habilidad de utilidad (número 1).</summary>
        public Habilidad HabilidadUtilidad
        {
            get { return habilidades[1]; }
        }

        public Habilidad ObtenerHabilidad(int numero)
        {
            if (numero < 0 || numero >= CantidadHabilidades)
                throw new ArgumentOutOfRangeException("numero", numero, "Solo existen las habilidades 0 (ataque) y 1 (utilidad).");
            return habilidades[numero];
        }

        /// <summary>Veces que se han usado las habilidades (estadística).</summary>
        public int UsosHabilidad
        {
            get { return habilidades[0].Usos + habilidades[1].Usos; }
        }

        // ---------- Efectos temporales (los dejan las habilidades de utilidad) ----------

        /// <summary>Ataque real en combate, con el potenciador si está activo.</summary>
        public int AtkEfectivo
        {
            get { return (int)Math.Round(atk * (1.0 + bonoAtaque)); }
        }

        /// <summary>Porcentaje extra de ataque activo (0 si no hay).</summary>
        public double BonoAtaque
        {
            get { return bonoAtaque; }
        }

        public int TurnosBonoAtaque
        {
            get { return turnosBonoAtaque; }
        }

        /// <summary>Defensa extra activa ahora mismo (0 si no hay).</summary>
        public int BonoDefensaActual
        {
            get { return bonoDefensa; }
        }

        public int TurnosBonoDefensa
        {
            get { return turnosBonoDefensa; }
        }

        /// <summary>Fracción del daño que devuelve el reflejo activo (0 si no hay).</summary>
        public double ReflejoActual
        {
            get { return reflejo; }
        }

        public int TurnosReflejo
        {
            get { return turnosReflejo; }
        }

        // ---------- Propiedades derivadas ----------
        public bool EstaVivo
        {
            get { return hp > 0; }
        }

        /// <summary>Defensa real en combate: la base más el bonus temporal.</summary>
        public virtual int DefensaEfectiva
        {
            get { return defensa + bonoDefensa; }
        }

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
        /// El ATK incluye el potenciador de ataque si está activo.
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

            double bruto = Atk * (1.0 + bonoAtaque) * factor * ObtenerMultiplicador(objetivo.Elemento) * multiplicadorEscenario;
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
            return Nombre + " ataca a " + objetivo.Nombre + " y causa " + real + " de daño" + DescribirEfectividad(mult) + "."
                   + objetivo.Reflejar(this, real);
        }

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

        /// <summary>Si el personaje tiene el reflejo activo, devuelve parte del daño al atacante (nunca lo derrota).</summary>
        private string Reflejar(Personaje atacante, int danoRecibido)
        {
            if (turnosReflejo <= 0 || danoRecibido <= 0 || !atacante.EstaVivo) return string.Empty;

            int devuelto = (int)Math.Round(danoRecibido * reflejo);
            if (devuelto > atacante.hp - 1) devuelto = atacante.hp - 1;
            if (devuelto <= 0) return string.Empty;

            atacante.RecibirDano(devuelto);
            return " " + Nombre + " le devuelve " + devuelto + " de daño a " + atacante.Nombre + ".";
        }

        // ---------- Uso de habilidades ----------

        /// <summary>Vivo, sin recarga pendiente y con energía suficiente.</summary>
        public bool PuedeUsarHabilidad(int numero)
        {
            Habilidad h = ObtenerHabilidad(numero);
            return EstaVivo && h.CooldownActual == 0 && energia >= h.Costo;
        }

        /// <summary>
        /// Usa una de las dos habilidades (0 = ataque, 1 = utilidad) y devuelve el texto del evento.
        /// Si es de ataque, "objetivo" es el enemigo. Si es de utilidad, es el aliado a ayudar (null = el mismo personaje).
        /// "equipo" son los aliados (se usa para curar a todo el equipo). Si no se puede usar, no consume nada.
        /// </summary>
        public string UsarHabilidad(int numero, Personaje objetivo, List<Personaje> equipo)
        {
            Habilidad h = ObtenerHabilidad(numero);
            if (!PuedeUsarHabilidad(numero)) return MensajeHabilidadNoDisponible(h);

            if (h.EsOfensiva) return UsarHabilidadDeAtaque(h, objetivo);
            return UsarHabilidadDeUtilidad(h, objetivo, equipo);
        }

        /// <summary>Cantidad de vida que cura una habilidad de curación de este personaje.</summary>
        public int CuracionDe(Habilidad h)
        {
            return (int)Math.Round(HpMax * h.Potencia) + Empatia * 2;
        }

        private string UsarHabilidadDeAtaque(Habilidad h, Personaje objetivo)
        {
            ValidarObjetivo(objetivo);
            if (!objetivo.EstaVivo) return h.Nombre + " no puede dirigirse a un objetivo derrotado.";

            double mult = ObtenerMultiplicador(objetivo.Elemento);
            ConsumirHabilidad(h);

            int total = 0;
            string extra = string.Empty;
            switch (h.Efecto)
            {
                case EfectoHabilidad.GolpePotenciado:
                    total = objetivo.RecibirDano(CalcularDanoPotenciado(objetivo, h.Potencia, h.Extra));
                    break;

                case EfectoHabilidad.DobleGolpe:
                    for (int golpe = 0; golpe < 2; golpe++)
                    {
                        // Se recalcula en cada golpe por si el primer impacto cambia el estado del objetivo.
                        total += objetivo.RecibirDano(CalcularDanoPotenciado(objetivo, h.Potencia, 0.0));
                    }
                    int ganada = RecuperarEnergia(h.Duracion);
                    if (ganada > 0) extra = " y recupera " + ganada + " de energía";
                    break;

                case EfectoHabilidad.GolpeVampirico:
                    total = objetivo.RecibirDano(CalcularDanoPotenciado(objetivo, h.Potencia, 0.0));
                    int robado = Curar((int)Math.Round(total * h.Extra));
                    if (robado > 0) extra = " y se cura " + robado + " HP";
                    break;

                case EfectoHabilidad.GolpeConRetroceso:
                    total = objetivo.RecibirDano(CalcularDanoPotenciado(objetivo, h.Potencia, 0.0));
                    int retroceso = (int)Math.Round(HpMax * h.Extra);
                    if (retroceso > hp - 1) retroceso = hp - 1;   // el retroceso nunca lo derrota
                    if (retroceso > 0)
                    {
                        RecibirDano(retroceso);
                        extra = " pero pierde " + retroceso + " HP por el retroceso";
                    }
                    break;
            }

            return Nombre + " usa " + h.Nombre + " sobre " + objetivo.Nombre + " y causa " + total + " de daño"
                   + DescribirEfectividad(mult) + extra + "." + objetivo.Reflejar(this, total);
        }

        private string UsarHabilidadDeUtilidad(Habilidad h, Personaje objetivo, List<Personaje> equipo)
        {
            switch (h.Efecto)
            {
                case EfectoHabilidad.Curar:
                    {
                        Personaje destino = objetivo ?? this;
                        if (!destino.EstaVivo) return h.Nombre + " no puede revivir a " + destino.Nombre + ".";
                        ConsumirHabilidad(h);
                        int curado = destino.Curar(CuracionDe(h));
                        return Nombre + " usa " + h.Nombre + " y cura " + curado + " HP a "
                               + (destino == this ? "sí mismo" : destino.Nombre) + ".";
                    }
                case EfectoHabilidad.CurarEquipo:
                    {
                        ConsumirHabilidad(h);
                        int cantidad = CuracionDe(h);
                        int total = 0;
                        if (equipo != null)
                        {
                            foreach (Personaje aliado in equipo)
                                total += aliado.Curar(cantidad);
                        }
                        return Nombre + " usa " + h.Nombre + " y cura " + total + " HP en total a su equipo.";
                    }
                case EfectoHabilidad.PotenciarAtaque:
                    ConsumirHabilidad(h);
                    bonoAtaque = h.Potencia;
                    turnosBonoAtaque = h.Duracion + 1;   // +1 porque se descuenta al iniciar el próximo turno
                    return Nombre + " usa " + h.Nombre + ": +" + (int)Math.Round(h.Potencia * 100)
                           + "% de ataque durante " + h.Duracion + " turnos.";

                case EfectoHabilidad.PotenciarDefensa:
                    ConsumirHabilidad(h);
                    bonoDefensa = Math.Max(2, defensa / 2 + empatia / 2);
                    turnosBonoDefensa = h.Duracion;
                    return Nombre + " usa " + h.Nombre + ": +" + bonoDefensa + " de defensa durante "
                           + h.Duracion + " turnos del rival.";

                case EfectoHabilidad.Reflejo:
                    ConsumirHabilidad(h);
                    reflejo = h.Potencia;
                    turnosReflejo = h.Duracion;
                    return Nombre + " usa " + h.Nombre + ": devolverá el " + (int)Math.Round(h.Potencia * 100)
                           + "% del daño recibido durante " + h.Duracion + " turnos del rival.";

                default:   // RecargarEnergia
                    {
                        ConsumirHabilidad(h);
                        int ganada = RecuperarEnergia(h.Duracion);
                        return Nombre + " usa " + h.Nombre + " y recupera " + ganada + " de energía.";
                    }
            }
        }

        /// <summary>Descuenta la energía, activa la recarga y suma el uso.</summary>
        private void ConsumirHabilidad(Habilidad h)
        {
            if (energia < h.Costo || h.CooldownActual > 0)
                throw new InvalidOperationException("La habilidad no está disponible.");
            energia -= h.Costo;
            h.Registrar();
        }

        /// <summary>Explica por qué no se puede usar la habilidad (para el log).</summary>
        private string MensajeHabilidadNoDisponible(Habilidad h)
        {
            if (!EstaVivo) return Nombre + " está derrotado y no puede actuar.";
            if (h.CooldownActual > 0)
                return h.Nombre + " en recarga (" + h.CooldownActual + " turno(s)).";
            return Nombre + " no tiene energía suficiente para " + h.Nombre
                   + " (" + energia + "/" + h.Costo + ").";
        }

        // ---------- Ciclo de turnos y relevos ----------

        /// <summary>Se invoca al inicio del turno del personaje activo: avanza recargas y efectos, y regenera energía.</summary>
        public virtual void IniciarTurno()
        {
            if (!EstaVivo) return;
            foreach (Habilidad h in habilidades) h.AvanzarTurno();
            RecuperarEnergia(EnergiaPorTurno);

            if (turnosBonoAtaque > 0)
            {
                turnosBonoAtaque--;
                if (turnosBonoAtaque == 0) bonoAtaque = 0;
            }
            if (turnosBonoDefensa > 0)
            {
                turnosBonoDefensa--;
                if (turnosBonoDefensa == 0) bonoDefensa = 0;
            }
            if (turnosReflejo > 0)
            {
                turnosReflejo--;
                if (turnosReflejo == 0) reflejo = 0;
            }
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

        /// <summary>Se invoca cuando el personaje deja el campo (relevo o derrota). Limpia los efectos temporales.</summary>
        public virtual void AlSalirDelCampo()
        {
            bonoAtaque = 0;
            turnosBonoAtaque = 0;
            bonoDefensa = 0;
            turnosBonoDefensa = 0;
            reflejo = 0;
            turnosReflejo = 0;
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
                   + " ATK " + AtkEfectivo + " DEF " + DefensaEfectiva + " ENG " + Energia + "/" + EnergiaMax;
        }
    }
}
