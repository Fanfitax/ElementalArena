using System;
using System.Collections.Generic;

namespace ArenaElemental.Modelos
{
    /// <summary>
    /// Clase base de todos los personajes de Arena Elemental (CartaFuego, CartaAgua, CartaTierra y CartaAire
    /// heredan de ella). Es abstracta: no se puede crear un "Personaje" solo, siempre es de un elemento.
    ///
    /// Guarda las estadísticas (vida, ataque, defensa, energía...), calcula el daño y sabe usar sus dos
    /// habilidades: la 0 es de ataque y la 1 es de utilidad.
    /// Lo único que cambia entre las cuatro clases hijas es el triángulo elemental
    /// (ObtenerMultiplicador) y cómo hacer una copia (CrearCopia): eso es el polimorfismo.
    /// </summary>
    public abstract class Personaje
    {
        // ---------- Constantes ----------
        public const double MultiplicadorVentaja = 1.5;     // el elemento le gana al del enemigo
        public const double MultiplicadorDesventaja = 0.5;  // el elemento pierde contra el del enemigo
        public const double MultiplicadorNeutro = 1.0;

        public const int EnergiaPorTurno = 2;        // energía que recupera al empezar su turno
        public const int CantidadHabilidades = 2;    // 0 = ataque, 1 = utilidad

        // ---------- Campos privados (encapsulamiento) ----------
        private int hp;
        private int energia;
        private Habilidad habilidadAtaque;
        private Habilidad habilidadUtilidad;

        // Efectos temporales que dejan las habilidades de utilidad
        private double bonoAtaque;       // 0.3 = +30% de ataque
        private int turnosBonoAtaque;
        private int bonoDefensa;         // puntos de defensa extra
        private int turnosBonoDefensa;
        private double reflejo;          // 0.5 = devuelve el 50% del daño recibido
        private int turnosReflejo;

        // ---------- Constructor ----------
        protected Personaje(int id, string nombre, Elemento elemento,
                            int hpMax, int atk, int defensa, int energiaMax, int empatia,
                            Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
        {
            // Validaciones básicas: nada de estadísticas negativas ni nombres vacíos.
            if (nombre == null || nombre.Trim().Length == 0)
                throw new ArgumentException("El personaje necesita nombre.");
            if (hpMax <= 0)
                throw new ArgumentOutOfRangeException("hpMax", "La vida máxima debe ser mayor que 0.");
            if (atk < 0 || defensa < 0 || energiaMax < 0 || empatia < 0)
                throw new ArgumentOutOfRangeException("atk", "Las estadísticas no pueden ser negativas.");
            if (!habilidadAtaque.EsOfensiva || habilidadUtilidad.EsOfensiva)
                throw new ArgumentException("La primera habilidad debe ser de ataque y la segunda de utilidad.");

            Id = id;
            Nombre = nombre.Trim();
            Elemento = elemento;
            HpMax = hpMax;
            Atk = atk;
            Defensa = defensa;
            EnergiaMax = energiaMax;
            Empatia = empatia;
            MultiplicadorEscenario = 1.0;
            this.habilidadAtaque = habilidadAtaque;
            this.habilidadUtilidad = habilidadUtilidad;

            // Empieza con la vida y la energía llenas.
            hp = hpMax;
            energia = energiaMax;
        }

        // ---------- Estadísticas ----------
        public int Id { get; private set; }
        public string Nombre { get; private set; }
        public Elemento Elemento { get; private set; }
        public int HpMax { get; private set; }
        public int Atk { get; private set; }
        public int Defensa { get; private set; }
        public int EnergiaMax { get; private set; }

        /// <summary>La empatía aumenta las curaciones y la energía que se gana al entrar de relevo.</summary>
        public int Empatia { get; private set; }

        /// <summary>Multiplicador de ataque del escenario (1.0 = sin efecto). Lo cambia el combate.</summary>
        public double MultiplicadorEscenario { get; set; }

        /// <summary>Vida actual. Siempre queda entre 0 y HpMax.</summary>
        public int Hp
        {
            get { return hp; }
            set
            {
                if (value < 0) value = 0;
                if (value > HpMax) value = HpMax;
                hp = value;
            }
        }

        /// <summary>Energía actual. Siempre queda entre 0 y EnergiaMax.</summary>
        public int Energia
        {
            get { return energia; }
            set
            {
                if (value < 0) value = 0;
                if (value > EnergiaMax) value = EnergiaMax;
                energia = value;
            }
        }

        public bool EstaVivo
        {
            get { return hp > 0; }
        }

        // ---------- Habilidades ----------
        public Habilidad HabilidadAtaque
        {
            get { return habilidadAtaque; }
        }

        public Habilidad HabilidadUtilidad
        {
            get { return habilidadUtilidad; }
        }

        /// <summary>Devuelve la habilidad 0 (ataque) o la 1 (utilidad).</summary>
        public Habilidad ObtenerHabilidad(int numero)
        {
            if (numero == 0) return habilidadAtaque;
            if (numero == 1) return habilidadUtilidad;
            throw new ArgumentOutOfRangeException("numero", "Solo existen la habilidad 0 (ataque) y la 1 (utilidad).");
        }

        // ---------- Efectos temporales ----------

        /// <summary>El ataque real: el ataque base más el potenciador si está activo.</summary>
        public int AtkEfectivo
        {
            get { return (int)Math.Round(Atk * (1.0 + bonoAtaque)); }
        }

        /// <summary>La defensa real: la base más la defensa extra si está activa.</summary>
        public int DefensaEfectiva
        {
            get { return Defensa + bonoDefensa; }
        }

        public double BonoAtaque
        {
            get { return bonoAtaque; }
        }

        public int BonoDefensaActual
        {
            get { return bonoDefensa; }
        }

        public double ReflejoActual
        {
            get { return reflejo; }
        }

        // ---------- Métodos que cambian en cada elemento ----------

        /// <summary>Hace una copia nueva del personaje, con la vida y la energía llenas.</summary>
        public abstract Personaje CrearCopia();

        /// <summary>
        /// Multiplicador de daño contra el elemento del enemigo (1.5, 0.5 o 1.0).
        /// Cada clase hija lo escribe según el triángulo: Agua > Fuego > Aire > Tierra > Agua.
        /// </summary>
        public abstract double ObtenerMultiplicador(Elemento enemigo);

        // ---------- Daño ----------

        /// <summary>
        /// Fórmula del daño: ATK * factor * multiplicador elemental * escenario - DEF * (1 - defensa ignorada).
        /// El mínimo es 1. "factor" es la potencia del golpe (1.0 = ataque básico) y
        /// "defensaIgnorada" es la parte de la defensa del enemigo que no cuenta (de 0 a 1).
        /// </summary>
        private int CalcularDano(Personaje objetivo, double factor, double defensaIgnorada)
        {
            double ataque = Atk * (1.0 + bonoAtaque) * factor * ObtenerMultiplicador(objetivo.Elemento) * MultiplicadorEscenario;
            double escudo = objetivo.DefensaEfectiva * (1.0 - defensaIgnorada);
            int dano = (int)Math.Round(ataque - escudo, MidpointRounding.AwayFromZero);
            if (dano < 1) dano = 1;
            return dano;
        }

        /// <summary>Le quita vida al personaje (nunca baja de 0) y devuelve el daño que realmente recibió.</summary>
        public int RecibirDano(int dano)
        {
            if (dano < 0) dano = 0;
            int vidaAntes = hp;
            Hp = hp - dano;
            return vidaAntes - hp;
        }

        /// <summary>Recupera vida (sin pasar de HpMax). Devuelve cuánta vida se curó. Un derrotado no revive.</summary>
        public int Curar(int cantidad)
        {
            if (!EstaVivo) return 0;
            int vidaAntes = hp;
            Hp = hp + cantidad;
            return hp - vidaAntes;
        }

        /// <summary>Recupera energía (sin pasar de EnergiaMax). Devuelve cuánta energía ganó.</summary>
        public int RecuperarEnergia(int cantidad)
        {
            int energiaAntes = energia;
            Energia = energia + cantidad;
            return energia - energiaAntes;
        }

        // ---------- Ataque básico ----------

        /// <summary>Ataque básico contra el objetivo. Devuelve el texto que se escribe en el historial.</summary>
        public string Atacar(Personaje objetivo)
        {
            if (!EstaVivo)
                return Nombre + " está derrotado y no puede actuar.";
            if (!objetivo.EstaVivo)
                return Nombre + " no puede atacar a " + objetivo.Nombre + ": ya está derrotado.";

            double multiplicador = ObtenerMultiplicador(objetivo.Elemento);
            int real = objetivo.RecibirDano(CalcularDano(objetivo, 1.0, 0.0));

            return Nombre + " ataca a " + objetivo.Nombre + " y causa " + real + " de daño"
                   + DescribirEfectividad(multiplicador) + "." + objetivo.Reflejar(this, real);
        }

        /// <summary>Si tiene el reflejo activo, le devuelve parte del daño al atacante (sin dejarlo en 0).</summary>
        private string Reflejar(Personaje atacante, int danoRecibido)
        {
            if (turnosReflejo <= 0 || danoRecibido <= 0 || !atacante.EstaVivo) return "";

            int devuelto = (int)Math.Round(danoRecibido * reflejo);
            if (devuelto > atacante.Hp - 1) devuelto = atacante.Hp - 1;   // no lo puede derrotar
            if (devuelto <= 0) return "";

            atacante.RecibirDano(devuelto);
            return " " + Nombre + " le devuelve " + devuelto + " de daño a " + atacante.Nombre + ".";
        }

        private string DescribirEfectividad(double multiplicador)
        {
            if (multiplicador > MultiplicadorNeutro) return " (¡muy efectivo!)";
            if (multiplicador < MultiplicadorNeutro) return " (poco efectivo)";
            return "";
        }

        // ---------- Habilidades ----------

        /// <summary>Se puede usar si está vivo, no está en recarga y le alcanza la energía.</summary>
        public bool PuedeUsarHabilidad(int numero)
        {
            Habilidad h = ObtenerHabilidad(numero);
            return EstaVivo && h.CooldownActual == 0 && energia >= h.Costo;
        }

        /// <summary>Cuánta vida cura una habilidad de curación de este personaje.</summary>
        public int CuracionDe(Habilidad h)
        {
            return (int)Math.Round(HpMax * h.Potencia) + Empatia * 2;
        }

        /// <summary>
        /// Usa la habilidad 0 (ataque) o la 1 (utilidad) y devuelve el texto para el historial.
        /// En una habilidad de ataque, "objetivo" es el enemigo. En una de utilidad es el aliado
        /// al que se ayuda (null = él mismo). "equipo" son todos sus aliados (para curar a todos).
        /// </summary>
        public string UsarHabilidad(int numero, Personaje objetivo, List<Personaje> equipo)
        {
            Habilidad h = ObtenerHabilidad(numero);

            if (!PuedeUsarHabilidad(numero))
                return ExplicarPorQueNoPuede(h);

            if (h.EsOfensiva)
                return UsarHabilidadDeAtaque(h, objetivo);
            return UsarHabilidadDeUtilidad(h, objetivo, equipo);
        }

        /// <summary>Gasta la energía de la habilidad y empieza su recarga.</summary>
        private void GastarHabilidad(Habilidad h)
        {
            energia = energia - h.Costo;
            h.Usar();
        }

        private string ExplicarPorQueNoPuede(Habilidad h)
        {
            if (!EstaVivo)
                return Nombre + " está derrotado y no puede actuar.";
            if (h.CooldownActual > 0)
                return h.Nombre + " en recarga (" + h.CooldownActual + " turno(s)).";
            return Nombre + " no tiene energía suficiente para " + h.Nombre + " (" + energia + "/" + h.Costo + ").";
        }

        // ----- Habilidades de ataque -----

        private string UsarHabilidadDeAtaque(Habilidad h, Personaje objetivo)
        {
            if (!objetivo.EstaVivo)
                return h.Nombre + " no puede dirigirse a un objetivo derrotado.";

            double multiplicador = ObtenerMultiplicador(objetivo.Elemento);
            GastarHabilidad(h);

            int total = 0;       // daño total causado
            string extra = "";   // texto adicional según el efecto

            if (h.Efecto == EfectoHabilidad.GolpePotenciado)
            {
                total = objetivo.RecibirDano(CalcularDano(objetivo, h.Potencia, h.Extra));
            }
            else if (h.Efecto == EfectoHabilidad.DobleGolpe)
            {
                // Se recalcula el daño en cada golpe por si el primero cambia algo en el objetivo.
                total = objetivo.RecibirDano(CalcularDano(objetivo, h.Potencia, 0.0));
                total = total + objetivo.RecibirDano(CalcularDano(objetivo, h.Potencia, 0.0));

                int ganada = RecuperarEnergia(h.Duracion);
                if (ganada > 0) extra = " y recupera " + ganada + " de energía";
            }
            else if (h.Efecto == EfectoHabilidad.GolpeVampirico)
            {
                total = objetivo.RecibirDano(CalcularDano(objetivo, h.Potencia, 0.0));
                int robado = Curar((int)Math.Round(total * h.Extra));
                if (robado > 0) extra = " y se cura " + robado + " HP";
            }
            else if (h.Efecto == EfectoHabilidad.GolpeConRetroceso)
            {
                total = objetivo.RecibirDano(CalcularDano(objetivo, h.Potencia, 0.0));
                int retroceso = (int)Math.Round(HpMax * h.Extra);
                if (retroceso > hp - 1) retroceso = hp - 1;   // el retroceso nunca lo derrota
                if (retroceso > 0)
                {
                    RecibirDano(retroceso);
                    extra = " pero pierde " + retroceso + " HP por el retroceso";
                }
            }

            return Nombre + " usa " + h.Nombre + " sobre " + objetivo.Nombre + " y causa " + total + " de daño"
                   + DescribirEfectividad(multiplicador) + extra + "." + objetivo.Reflejar(this, total);
        }

        // ----- Habilidades de utilidad -----

        private string UsarHabilidadDeUtilidad(Habilidad h, Personaje objetivo, List<Personaje> equipo)
        {
            if (h.Efecto == EfectoHabilidad.Curar)
                return UsarCuracion(h, objetivo);
            if (h.Efecto == EfectoHabilidad.CurarEquipo)
                return UsarCuracionDeEquipo(h, equipo);

            GastarHabilidad(h);

            if (h.Efecto == EfectoHabilidad.PotenciarAtaque)
            {
                bonoAtaque = h.Potencia;
                turnosBonoAtaque = h.Duracion + 1;   // +1 porque se descuenta al empezar el siguiente turno
                return Nombre + " usa " + h.Nombre + ": +" + (int)Math.Round(h.Potencia * 100)
                       + "% de ataque durante " + h.Duracion + " turnos.";
            }
            if (h.Efecto == EfectoHabilidad.PotenciarDefensa)
            {
                bonoDefensa = Math.Max(2, Defensa / 2 + Empatia / 2);
                turnosBonoDefensa = h.Duracion;
                return Nombre + " usa " + h.Nombre + ": +" + bonoDefensa + " de defensa durante "
                       + h.Duracion + " turnos del rival.";
            }
            if (h.Efecto == EfectoHabilidad.Reflejo)
            {
                reflejo = h.Potencia;
                turnosReflejo = h.Duracion;
                return Nombre + " usa " + h.Nombre + ": devolverá el " + (int)Math.Round(h.Potencia * 100)
                       + "% del daño recibido durante " + h.Duracion + " turnos del rival.";
            }

            // Lo que queda es RecargarEnergia
            int ganada = RecuperarEnergia(h.Duracion);
            return Nombre + " usa " + h.Nombre + " y recupera " + ganada + " de energía.";
        }

        private string UsarCuracion(Habilidad h, Personaje objetivo)
        {
            Personaje destino = objetivo;
            if (destino == null) destino = this;   // si no se eligió aliado, se cura a sí mismo

            if (!destino.EstaVivo)
                return h.Nombre + " no puede revivir a " + destino.Nombre + ".";

            GastarHabilidad(h);
            int curado = destino.Curar(CuracionDe(h));

            string quien = destino.Nombre;
            if (destino == this) quien = "sí mismo";
            return Nombre + " usa " + h.Nombre + " y cura " + curado + " HP a " + quien + ".";
        }

        private string UsarCuracionDeEquipo(Habilidad h, List<Personaje> equipo)
        {
            GastarHabilidad(h);
            int cantidad = CuracionDe(h);
            int total = 0;
            if (equipo != null)
            {
                foreach (Personaje aliado in equipo)
                    total = total + aliado.Curar(cantidad);
            }
            return Nombre + " usa " + h.Nombre + " y cura " + total + " HP en total a su equipo.";
        }

        // ---------- Turnos y relevos ----------

        /// <summary>Se llama cuando empieza el turno del personaje activo: baja las recargas, da energía y cuenta los efectos.</summary>
        public void IniciarTurno()
        {
            if (!EstaVivo) return;

            habilidadAtaque.PasarTurno();
            habilidadUtilidad.PasarTurno();
            RecuperarEnergia(EnergiaPorTurno);

            // Cada efecto temporal dura unos turnos; al llegar a 0 se apaga.
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

        /// <summary>Se llama cuando entra de relevo. La empatía le da algo de energía. Devuelve el texto del historial.</summary>
        public string AlEntrarEnRelevo(Personaje saliente)
        {
            int bonus = RecuperarEnergia(Empatia / 2);

            string texto = Nombre + " entra al combate";
            if (saliente != null) texto = texto + " relevando a " + saliente.Nombre;
            if (bonus > 0) texto = texto + " y recupera " + bonus + " de energía por su empatía";
            return texto + ".";
        }

        /// <summary>Se llama cuando sale del campo (relevo o derrota): se apagan los efectos temporales.</summary>
        public void AlSalirDelCampo()
        {
            bonoAtaque = 0;
            turnosBonoAtaque = 0;
            bonoDefensa = 0;
            turnosBonoDefensa = 0;
            reflejo = 0;
            turnosReflejo = 0;
        }

        public override string ToString()
        {
            return Nombre + " [" + Elemento + "] HP " + Hp + "/" + HpMax
                   + " ATK " + AtkEfectivo + " DEF " + DefensaEfectiva + " ENG " + Energia + "/" + EnergiaMax;
        }
    }
}
