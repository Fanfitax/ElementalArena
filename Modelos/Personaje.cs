using System;
using System.Collections.Generic;

namespace ArenaElemental.Modelos
{
    // Clase base de todos los personajes de Arena Elemental (CartaFuego, CartaAgua, CartaTierra y CartaAire
    // heredan de ella). Siempre se usa alguna de las cuatro clases hijas, nunca "new Personaje" directo.
    //
    // Guarda las estadísticas (vida, ataque, defensa, energía...), calcula el daño y sabe usar sus dos
    // habilidades: la 0 es de ataque y la 1 es de utilidad.
    // Lo único que cambia entre las cuatro clases hijas es el triángulo elemental
    // (ObtenerMultiplicador) y cómo hacer una copia (CrearCopia): se cambian con override.
    public class Personaje
    {
        // ---------- Constantes ----------
        public const double MultiplicadorVentaja = 1.5;     // el elemento le gana al del enemigo
        public const double MultiplicadorDesventaja = 0.5;  // el elemento pierde contra el del enemigo
        public const double MultiplicadorNeutro = 1.0;

        public const int EnergiaPorTurno = 2;        // energía que recupera al empezar su turno
        public const int CantidadHabilidades = 2;    // 0 = ataque, 1 = utilidad

        // ---------- Campos privados (encapsulamiento) ----------
        private string nombre;
        private string nombreIngles;
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
        public Personaje(int id, string nombre, string nombreIngles, Elemento elemento,
                            int hpMax, int atk, int defensa, int energiaMax, int empatia,
                            Habilidad habilidadAtaque, Habilidad habilidadUtilidad)
        {
            // Validaciones básicas: nada de estadísticas negativas ni nombres vacíos.
            if (nombre == null || nombre.Trim().Length == 0)
                throw new ArgumentException(Idioma.Texto("El personaje necesita nombre.", "The character needs a name."));
            if (hpMax <= 0)
                throw new ArgumentOutOfRangeException("hpMax", Idioma.Texto("La vida máxima debe ser mayor que 0.", "Max HP must be greater than 0."));
            if (atk < 0 || defensa < 0 || energiaMax < 0 || empatia < 0)
                throw new ArgumentOutOfRangeException("atk", Idioma.Texto("Las estadísticas no pueden ser negativas.", "Stats cannot be negative."));
            if (!habilidadAtaque.EsOfensiva || habilidadUtilidad.EsOfensiva)
                throw new ArgumentException(Idioma.Texto("La primera habilidad debe ser de ataque y la segunda de utilidad.", "The first ability must be an attack and the second a utility."));

            Id = id;
            this.nombre = nombre.Trim();
            this.nombreIngles = nombreIngles.Trim();
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

        // El nombre en español es la "clave" del personaje: con ella se buscan su imagen y su lore.
        // Nombre es el que se muestra en pantalla y cambia según el idioma elegido.
        public string Clave
        {
            get { return nombre; }
        }

        public string NombreIngles
        {
            get { return nombreIngles; }
        }

        public string Nombre
        {
            get { return Idioma.Texto(nombre, nombreIngles); }
        }

        public Elemento Elemento { get; private set; }
        public int HpMax { get; private set; }
        public int Atk { get; private set; }
        public int Defensa { get; private set; }
        public int EnergiaMax { get; private set; }

        // La empatía aumenta las curaciones y la energía que se gana al entrar de relevo.
        public int Empatia { get; private set; }

        // Multiplicador de ataque del escenario (1.0 = sin efecto). Lo cambia el combate.
        public double MultiplicadorEscenario { get; set; }

        // Vida actual. Siempre queda entre 0 y HpMax.
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

        // Energía actual. Siempre queda entre 0 y EnergiaMax.
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

        // Devuelve la habilidad 0 (ataque) o la 1 (utilidad).
        public Habilidad ObtenerHabilidad(int numero)
        {
            if (numero == 0) return habilidadAtaque;
            if (numero == 1) return habilidadUtilidad;
            throw new ArgumentOutOfRangeException("numero", Idioma.Texto("Solo existen la habilidad 0 (ataque) y la 1 (utilidad).", "Only ability 0 (attack) and 1 (utility) exist."));
        }

        // ---------- Efectos temporales ----------

        // El ataque real: el ataque base más el potenciador si está activo.
        public int AtkEfectivo
        {
            get { return (int)Math.Round(Atk * (1.0 + bonoAtaque)); }
        }

        // La defensa real: la base más la defensa extra si está activa.
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

        // Hace una copia nueva del personaje, con la vida y la energía llenas.
        public virtual Personaje CrearCopia()
        {
            return new Personaje(Id, Clave, NombreIngles, Elemento, HpMax, Atk, Defensa, EnergiaMax, Empatia,
                                 HabilidadAtaque.Clonar(), HabilidadUtilidad.Clonar());
        }

        // Multiplicador de daño contra el elemento del enemigo (1.5, 0.5 o 1.0).
        // Cada clase hija lo cambia con override según el triángulo: Agua > Fuego > Aire > Tierra > Agua.
        public virtual double ObtenerMultiplicador(Elemento enemigo)
        {
            return MultiplicadorNeutro;
        }

        // ---------- Daño ----------

        // Fórmula del daño: ATK * factor * multiplicador elemental * escenario - DEF * (1 - defensa ignorada).
        // El mínimo es 1. "factor" es la potencia del golpe (1.0 = ataque básico) y
        // "defensaIgnorada" es la parte de la defensa del enemigo que no cuenta (de 0 a 1).
        private int CalcularDano(Personaje objetivo, double factor, double defensaIgnorada)
        {
            double ataque = Atk * (1.0 + bonoAtaque) * factor * ObtenerMultiplicador(objetivo.Elemento) * MultiplicadorEscenario;
            double escudo = objetivo.DefensaEfectiva * (1.0 - defensaIgnorada);
            int dano = (int)Math.Round(ataque - escudo, MidpointRounding.AwayFromZero);
            if (dano < 1) dano = 1;
            return dano;
        }

        // Cuánto daño haría un ataque básico contra el objetivo, SIN aplicarlo.
        // Lo usa la CPU para decidir su jugada.
        public int DanoEstimadoBasico(Personaje objetivo)
        {
            return CalcularDano(objetivo, 1.0, 0.0);
        }

        // Cuánto daño haría en total la habilidad de ataque contra el objetivo, SIN aplicarla
        // (el doble golpe cuenta los dos golpes). Lo usa la CPU para decidir su jugada.
        public int DanoEstimadoHabilidad(Personaje objetivo)
        {
            Habilidad h = habilidadAtaque;
            if (h.Efecto == EfectoHabilidad.GolpePotenciado)
                return CalcularDano(objetivo, h.Potencia, h.Extra);
            if (h.Efecto == EfectoHabilidad.DobleGolpe)
                return 2 * CalcularDano(objetivo, h.Potencia, 0.0);
            return CalcularDano(objetivo, h.Potencia, 0.0);
        }

        // Le quita vida al personaje (nunca baja de 0) y devuelve el daño que realmente recibió.
        public int RecibirDano(int dano)
        {
            if (dano < 0) dano = 0;
            int vidaAntes = hp;
            Hp = hp - dano;
            return vidaAntes - hp;
        }

        // Recupera vida (sin pasar de HpMax). Devuelve cuánta vida se curó. Un derrotado no revive.
        public int Curar(int cantidad)
        {
            if (!EstaVivo) return 0;
            int vidaAntes = hp;
            Hp = hp + cantidad;
            return hp - vidaAntes;
        }

        // Recupera energía (sin pasar de EnergiaMax). Devuelve cuánta energía ganó.
        public int RecuperarEnergia(int cantidad)
        {
            int energiaAntes = energia;
            Energia = energia + cantidad;
            return energia - energiaAntes;
        }

        // ---------- Ataque básico ----------

        // Ataque básico contra el objetivo. Devuelve el texto que se escribe en el historial.
        public string Atacar(Personaje objetivo)
        {
            if (!EstaVivo)
                return Nombre + Idioma.Texto(" está derrotado y no puede actuar.", " is defeated and cannot act.");
            if (!objetivo.EstaVivo)
                return Nombre + Idioma.Texto(" no puede atacar a ", " cannot attack ") + objetivo.Nombre + Idioma.Texto(": ya está derrotado.", ": already defeated.");

            double multiplicador = ObtenerMultiplicador(objetivo.Elemento);
            int real = objetivo.RecibirDano(CalcularDano(objetivo, 1.0, 0.0));

            return Nombre + Idioma.Texto(" ataca a ", " attacks ") + objetivo.Nombre + Idioma.Texto(" y causa ", " and deals ") + real + Idioma.Texto(" de daño", " damage")
                   + DescribirEfectividad(multiplicador) + "." + objetivo.Reflejar(this, real);
        }

        // Si tiene el reflejo activo, le devuelve parte del daño al atacante (sin dejarlo en 0).
        private string Reflejar(Personaje atacante, int danoRecibido)
        {
            if (turnosReflejo <= 0 || danoRecibido <= 0 || !atacante.EstaVivo) return "";

            int devuelto = (int)Math.Round(danoRecibido * reflejo);
            if (devuelto > atacante.Hp - 1) devuelto = atacante.Hp - 1;   // no lo puede derrotar
            if (devuelto <= 0) return "";

            atacante.RecibirDano(devuelto);
            return " " + Nombre + Idioma.Texto(" le devuelve ", " reflects ") + devuelto + Idioma.Texto(" de daño a ", " damage to ") + atacante.Nombre + ".";
        }

        private string DescribirEfectividad(double multiplicador)
        {
            if (multiplicador > MultiplicadorNeutro) return Idioma.Texto(" (¡muy efectivo!)", " (very effective!)");
            if (multiplicador < MultiplicadorNeutro) return Idioma.Texto(" (poco efectivo)", " (not very effective)");
            return "";
        }

        // ---------- Habilidades ----------

        // Se puede usar si está vivo, no está en recarga y le alcanza la energía.
        public bool PuedeUsarHabilidad(int numero)
        {
            Habilidad h = ObtenerHabilidad(numero);
            return EstaVivo && h.CooldownActual == 0 && energia >= h.Costo;
        }

        // Cuánta vida cura una habilidad de curación de este personaje.
        public int CuracionDe(Habilidad h)
        {
            return (int)Math.Round(HpMax * h.Potencia) + Empatia * 2;
        }

        // Usa la habilidad 0 (ataque) o la 1 (utilidad) y devuelve el texto para el historial.
        // En una habilidad de ataque, "objetivo" es el enemigo. En una de utilidad es el aliado
        // al que se ayuda (null = él mismo). "equipo" son todos sus aliados (para curar a todos).
        public string UsarHabilidad(int numero, Personaje objetivo, List<Personaje> equipo)
        {
            Habilidad h = ObtenerHabilidad(numero);

            if (!PuedeUsarHabilidad(numero))
                return ExplicarPorQueNoPuede(h);

            if (h.EsOfensiva)
                return UsarHabilidadDeAtaque(h, objetivo);
            return UsarHabilidadDeUtilidad(h, objetivo, equipo);
        }

        // Gasta la energía de la habilidad y empieza su recarga.
        private void GastarHabilidad(Habilidad h)
        {
            energia = energia - h.Costo;
            h.Usar();
        }

        private string ExplicarPorQueNoPuede(Habilidad h)
        {
            if (!EstaVivo)
                return Nombre + Idioma.Texto(" está derrotado y no puede actuar.", " is defeated and cannot act.");
            if (h.CooldownActual > 0)
                return h.Nombre + Idioma.Texto(" en recarga (", " on cooldown (") + h.CooldownActual + Idioma.Texto(" turno(s)).", " turn(s)).");
            return Nombre + Idioma.Texto(" no tiene energía suficiente para ", " does not have enough energy for ") + h.Nombre + " (" + energia + "/" + h.Costo + ").";
        }

        // ----- Habilidades de ataque -----

        private string UsarHabilidadDeAtaque(Habilidad h, Personaje objetivo)
        {
            if (!objetivo.EstaVivo)
                return h.Nombre + Idioma.Texto(" no puede dirigirse a un objetivo derrotado.", " cannot target a defeated opponent.");

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
                if (ganada > 0) extra = Idioma.Texto(" y recupera ", " and recovers ") + ganada + Idioma.Texto(" de energía", " energy");
            }
            else if (h.Efecto == EfectoHabilidad.GolpeVampirico)
            {
                total = objetivo.RecibirDano(CalcularDano(objetivo, h.Potencia, 0.0));
                int robado = Curar((int)Math.Round(total * h.Extra));
                if (robado > 0) extra = Idioma.Texto(" y se cura ", " and heals ") + robado + " HP";
            }
            else if (h.Efecto == EfectoHabilidad.GolpeConRetroceso)
            {
                total = objetivo.RecibirDano(CalcularDano(objetivo, h.Potencia, 0.0));
                int retroceso = (int)Math.Round(HpMax * h.Extra);
                if (retroceso > hp - 1) retroceso = hp - 1;   // el retroceso nunca lo derrota
                if (retroceso > 0)
                {
                    RecibirDano(retroceso);
                    extra = Idioma.Texto(" pero pierde ", " but loses ") + retroceso + Idioma.Texto(" HP por el retroceso", " HP from recoil");
                }
            }

            return Nombre + Idioma.Texto(" usa ", " uses ") + h.Nombre + Idioma.Texto(" sobre ", " on ") + objetivo.Nombre + Idioma.Texto(" y causa ", " and deals ") + total + Idioma.Texto(" de daño", " damage")
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
                return Nombre + Idioma.Texto(" usa ", " uses ") + h.Nombre + ": +" + (int)Math.Round(h.Potencia * 100)
                       + Idioma.Texto("% de ataque durante ", "% attack for ") + h.Duracion + Idioma.Texto(" turnos.", " turns.");
            }
            if (h.Efecto == EfectoHabilidad.PotenciarDefensa)
            {
                bonoDefensa = Math.Max(2, Defensa / 2 + Empatia / 2);
                turnosBonoDefensa = h.Duracion;
                return Nombre + Idioma.Texto(" usa ", " uses ") + h.Nombre + ": +" + bonoDefensa + Idioma.Texto(" de defensa durante ", " defense for ")
                       + h.Duracion + Idioma.Texto(" turnos del rival.", " opponent turns.");
            }
            if (h.Efecto == EfectoHabilidad.Reflejo)
            {
                reflejo = h.Potencia;
                turnosReflejo = h.Duracion;
                return Nombre + Idioma.Texto(" usa ", " uses ") + h.Nombre + Idioma.Texto(": devolverá el ", ": will reflect ") + (int)Math.Round(h.Potencia * 100)
                       + Idioma.Texto("% del daño recibido durante ", "% of the damage received for ") + h.Duracion + Idioma.Texto(" turnos del rival.", " opponent turns.");
            }

            // Lo que queda es RecargarEnergia
            int ganada = RecuperarEnergia(h.Duracion);
            return Nombre + Idioma.Texto(" usa ", " uses ") + h.Nombre + Idioma.Texto(" y recupera ", " and recovers ") + ganada + Idioma.Texto(" de energía.", " energy.");
        }

        private string UsarCuracion(Habilidad h, Personaje objetivo)
        {
            Personaje destino = objetivo;
            if (destino == null) destino = this;   // si no se eligió aliado, se cura a sí mismo

            if (!destino.EstaVivo)
                return h.Nombre + Idioma.Texto(" no puede revivir a ", " cannot revive ") + destino.Nombre + ".";

            GastarHabilidad(h);
            int curado = destino.Curar(CuracionDe(h));

            string quien = destino.Nombre;
            if (destino == this) quien = Idioma.Texto("sí mismo", "self");
            return Nombre + Idioma.Texto(" usa ", " uses ") + h.Nombre + Idioma.Texto(" y cura ", " and heals ") + curado + Idioma.Texto(" HP a ", " HP to ") + quien + ".";
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
            return Nombre + Idioma.Texto(" usa ", " uses ") + h.Nombre + Idioma.Texto(" y cura ", " and heals ") + total + Idioma.Texto(" HP en total a su equipo.", " HP in total across the team.");
        }

        // ---------- Turnos y relevos ----------

        // Se llama cuando empieza el turno del personaje activo: baja las recargas, da energía y cuenta los efectos.
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

        // Se llama cuando entra de relevo. La empatía le da algo de energía. Devuelve el texto del historial.
        public string AlEntrarEnRelevo(Personaje saliente)
        {
            int bonus = RecuperarEnergia(Empatia / 2);

            string texto = Nombre + Idioma.Texto(" entra al combate", " enters the battle");
            if (saliente != null) texto = texto + Idioma.Texto(" relevando a ", " replacing ") + saliente.Nombre;
            if (bonus > 0) texto = texto + Idioma.Texto(" y recupera ", " and recovers ") + bonus + Idioma.Texto(" de energía por su empatía", " energy from empathy");
            return texto + ".";
        }

        // Se llama cuando sale del campo (relevo o derrota): se apagan los efectos temporales.
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
            return Nombre + " [" + Idioma.NombreElemento(Elemento) + "] HP " + Hp + "/" + HpMax
                   + " ATK " + AtkEfectivo + " DEF " + DefensaEfectiva + " ENG " + Energia + "/" + EnergiaMax;
        }
    }
}
