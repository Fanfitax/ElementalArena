using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ArenaElemental.Modelos;

namespace ArenaElemental.Formularios
{
    /// <summary>Los tres tamaños de carta: pequeña (banca), mediana (campo del combate) y grande (vista previa).</summary>
    public enum TamanoCarta
    {
        Pequena,
        Mediana,
        Grande
    }

    /// <summary>
    /// Representa un personaje como una carta física de un juego de cartas (TCG).
    /// Todo se arma por código con controles nativos: Panel (cabecera), PictureBox (ilustración),
    /// Label y ProgressBar. Hay dos tamaños: la carta grande (en el campo de duelo) y la pequeña (en la banca).
    /// Se conecta con la lógica llamando a ActualizarDatos(personaje).
    /// La ilustración sale de la carpeta Imagenes (ver la clase Ilustraciones); si no hay imagen,
    /// el PictureBox queda con un color sólido del elemento.
    /// </summary>
    public class VistaCarta : UserControl
    {
        // Pasos (de 30 ms) que duran los efectos de golpe.
        private const int PasosDestello = 12;
        private const int PasosSacudida = 10;

        private bool pequena;
        private bool mediana;
        private bool resaltada;
        private bool esSiguiente;
        private Elemento elemento = Elemento.Fuego;
        private string nombreMostrado = string.Empty;

        private Panel panelCabecera;
        private PictureBox pbImagen;
        private Label lblNombre, lblElemento;
        private Label lblHp, lblEng, lblStats, lblHabAtaque, lblHabUtilidad, lblEstado;
        private ProgressBar barHp, barEng;

        // Efectos de golpe (los maneja la propia carta con su reloj): destello de color y sacudida.
        private Timer timerEfecto;
        private Color colorDestello = Color.White;
        private int pasoDestello;
        private int pasoSacudida;
        private int xBaseSacudida;

        public VistaCarta() : this(TamanoCarta.Grande)
        {
        }

        public VistaCarta(bool pequena) : this(pequena ? TamanoCarta.Pequena : TamanoCarta.Grande)
        {
        }

        public VistaCarta(TamanoCarta tamano)
        {
            pequena = (tamano == TamanoCarta.Pequena);
            mediana = (tamano == TamanoCarta.Mediana);
            DoubleBuffered = true;
            BackColor = Estilo.Panel;

            if (pequena)
            {
                Size = new Size(92, 178);
                ConstruirPequena();
            }
            else if (mediana)
            {
                Size = new Size(200, 316);
                ConstruirMediana();
            }
            else
            {
                Size = new Size(232, 420);
                ConstruirGrande();
            }

            Estilo.Redondear(this, pequena ? 10 : 14);

            timerEfecto = new Timer();
            timerEfecto.Interval = 30;
            timerEfecto.Tick += TimerEfecto_Tick;
        }

        /// <summary>True para marcar la carta que entraría en el próximo relevo (se dibuja con borde dorado).</summary>
        public bool EsSiguiente
        {
            get { return esSiguiente; }
            set
            {
                if (esSiguiente == value) return;
                esSiguiente = value;
                Invalidate();
            }
        }

        // ---------- Construcción de los controles ----------

        private void ConstruirGrande()
        {
            panelCabecera = new Panel();
            panelCabecera.SetBounds(6, 6, 220, 36);
            lblNombre = CrearTexto(panelCabecera, 8, 4, 134, 28, 14f, true);
            lblElemento = CrearTexto(panelCabecera, 130, 4, 82, 28, 8.5f, true);
            lblElemento.TextAlign = ContentAlignment.MiddleRight;
            panelCabecera.Paint += Hijo_PintarFoil;
            Controls.Add(panelCabecera);

            pbImagen = CrearImagen(6, 46, 220, 200);

            lblHp = CrearTexto(this, 8, 250, 216, 18, 9.5f, true);
            barHp = CrearBarra(8, 268, 216, 14);
            lblEng = CrearTexto(this, 8, 286, 216, 18, 9.5f, true);
            barEng = CrearBarra(8, 304, 216, 12);

            lblStats = CrearTexto(this, 8, 322, 216, 20, 10f, true);
            lblHabAtaque = CrearTexto(this, 8, 344, 216, 18, 8.5f, false);
            lblHabUtilidad = CrearTexto(this, 8, 362, 216, 18, 8.5f, false);
            lblEstado = CrearTexto(this, 8, 388, 216, 22, 9.5f, true);
            lblEstado.TextAlign = ContentAlignment.MiddleCenter;
        }

        /// <summary>La carta del combate: como la grande pero sin las líneas de habilidades (están en los botones).</summary>
        private void ConstruirMediana()
        {
            panelCabecera = new Panel();
            panelCabecera.SetBounds(6, 6, 188, 32);
            lblNombre = CrearTexto(panelCabecera, 8, 3, 112, 26, 13f, true);
            lblElemento = CrearTexto(panelCabecera, 112, 3, 68, 26, 8f, true);
            lblElemento.TextAlign = ContentAlignment.MiddleRight;
            panelCabecera.Paint += Hijo_PintarFoil;
            Controls.Add(panelCabecera);

            pbImagen = CrearImagen(6, 42, 188, 148);

            lblHp = CrearTexto(this, 8, 194, 184, 18, 9f, true);
            barHp = CrearBarra(8, 212, 184, 12);
            lblEng = CrearTexto(this, 8, 228, 184, 18, 9f, true);
            barEng = CrearBarra(8, 246, 184, 10);

            lblStats = CrearTexto(this, 8, 262, 184, 20, 10f, true);
            lblEstado = CrearTexto(this, 8, 286, 184, 22, 9f, true);
            lblEstado.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void ConstruirPequena()
        {
            panelCabecera = new Panel();
            panelCabecera.SetBounds(3, 3, 86, 20);
            lblNombre = CrearTexto(panelCabecera, 3, 1, 80, 18, 8.5f, true);
            panelCabecera.Paint += Hijo_PintarFoil;
            Controls.Add(panelCabecera);

            pbImagen = CrearImagen(3, 26, 86, 72);

            lblHp = CrearTexto(this, 4, 100, 84, 14, 7.5f, true);
            barHp = CrearBarra(4, 115, 84, 7);
            lblStats = CrearTexto(this, 4, 125, 84, 14, 7.5f, false);
            lblEstado = CrearTexto(this, 3, 143, 86, 30, 7.5f, true);   // admite 2 líneas ("SIGUE EN ENTRAR")
            lblEstado.TextAlign = ContentAlignment.MiddleCenter;
        }

        private Label CrearTexto(Control padre, int x, int y, int ancho, int alto, float tamano, bool negrita)
        {
            Label l = Estilo.CrearEtiqueta(string.Empty, x, y, ancho, alto, tamano, negrita);
            padre.Controls.Add(l);
            return l;
        }

        private ProgressBar CrearBarra(int x, int y, int ancho, int alto)
        {
            ProgressBar b = new ProgressBar();
            b.SetBounds(x, y, ancho, alto);
            Controls.Add(b);
            return b;
        }

        private PictureBox CrearImagen(int x, int y, int ancho, int alto)
        {
            PictureBox pb = new PictureBox();
            pb.SetBounds(x, y, ancho, alto);
            pb.SizeMode = PictureBoxSizeMode.Normal;   // la ilustración ya se prepara exactamente de este tamaño
            pb.Paint += Hijo_PintarFoil;               // el brillo también debe verse sobre la imagen
            Controls.Add(pb);
            return pb;
        }

        // ---------- Conexión con la lógica ----------

        /// <summary>Muestra los datos del personaje (sin estado especial).</summary>
        public void ActualizarDatos(Personaje p)
        {
            ActualizarDatos(p, p.EstaVivo ? string.Empty : "DERROTADO", false);
        }

        /// <summary>
        /// Muestra los datos del personaje. "estado" es el texto de abajo ("EN CAMPO", "PRÓXIMO"...)
        /// y "resaltada" dibuja el borde más grueso (para la carta que está peleando).
        /// </summary>
        public void ActualizarDatos(Personaje p, string estado, bool resaltada)
        {
            if (p == null) return;

            bool cambioVisual = (p.Elemento != elemento) || (resaltada != this.resaltada);
            elemento = p.Elemento;
            this.resaltada = resaltada;

            // Colores según el elemento: fondo de la carta oscuro y cabecera de color.
            BackColor = Estilo.FondoDe(elemento);
            panelCabecera.BackColor = Estilo.ColorDe(elemento);
            Color colorTitulo = elemento == Elemento.Aire ? Color.FromArgb(20, 24, 40) : Color.White;
            lblNombre.ForeColor = colorTitulo;
            lblNombre.Text = p.Nombre;
            if (lblElemento != null)
            {
                lblElemento.ForeColor = colorTitulo;
                lblElemento.Text = elemento.ToString().ToUpper();
            }

            // Ilustración: solo se vuelve a preparar cuando cambia el personaje de esta carta.
            if (nombreMostrado != p.Nombre)
            {
                nombreMostrado = p.Nombre;
                MostrarIlustracion(p);
            }

            // Vida y energía (las barras no aceptan valores fuera de su rango).
            barHp.Maximum = Math.Max(1, p.HpMax);
            barHp.Value = Math.Max(0, Math.Min(barHp.Maximum, p.Hp));
            lblHp.Text = "HP  " + p.Hp + " / " + p.HpMax;

            if (barEng != null)
            {
                barEng.Maximum = Math.Max(1, p.EnergiaMax);
                barEng.Value = Math.Max(0, Math.Min(barEng.Maximum, p.Energia));
                lblEng.Text = "ENG  " + p.Energia + " / " + p.EnergiaMax;
            }

            // Ataque y defensa con sus bonos temporales.
            string atk = "ATK " + p.AtkEfectivo;
            if (p.AtkEfectivo > p.Atk) atk += " (+" + (p.AtkEfectivo - p.Atk) + ")";
            string def = "DEF " + p.DefensaEfectiva;
            if (p.DefensaEfectiva > p.Defensa) def += " (+" + (p.DefensaEfectiva - p.Defensa) + ")";
            lblStats.Text = atk + (pequena ? " · " : "    ") + def;

            if (lblHabAtaque != null)
            {
                MostrarHabilidad(lblHabAtaque, p.HabilidadAtaque);
                MostrarHabilidad(lblHabUtilidad, p.HabilidadUtilidad);
            }

            string texto = estado;
            if (p.ReflejoActual > 0) texto += "  ·  REFLEJO " + (int)Math.Round(p.ReflejoActual * 100) + "%";
            lblEstado.Text = texto;
            if (!p.EstaVivo) lblEstado.ForeColor = Color.IndianRed;
            else if (resaltada || esSiguiente) lblEstado.ForeColor = Color.Gold;
            else lblEstado.ForeColor = Estilo.TextoSuave;

            if (cambioVisual) Invalidate();
        }

        /// <summary>Una línea por habilidad: verde si está lista, gris con los turnos de recarga si no.</summary>
        private void MostrarHabilidad(Label etiqueta, Habilidad h)
        {
            if (h.CooldownActual > 0)
            {
                etiqueta.ForeColor = Estilo.TextoSuave;
                etiqueta.Text = "○ " + h.Nombre + " · recarga " + h.CooldownActual;
            }
            else
            {
                etiqueta.ForeColor = Color.White;
                etiqueta.Text = "● " + h.Nombre + " · " + h.Costo + " ENG";
            }
        }

        // ---------- Ilustración ----------

        /// <summary>Pone la ilustración del personaje; si no hay imagen deja un color sólido del elemento.</summary>
        private void MostrarIlustracion(Personaje p)
        {
            pbImagen.BackColor = Estilo.ColorDe(elemento);
            Image arte = null;
            try
            {
                arte = Ilustraciones.ObtenerArte(p, pbImagen.Width, pbImagen.Height);
            }
            catch (Exception)
            {
                arte = null;   // una imagen dañada nunca debe tumbar el programa
            }
            pbImagen.Image = arte;
        }

        // ---------- Dibujo ----------

        /// <summary>Borde redondeado con el color del elemento (más grueso en la carta del campo, dorado en la siguiente).</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color colorBorde = esSiguiente ? Color.Gold : Estilo.ColorDe(elemento);
            int grosor = (resaltada || esSiguiente) ? 4 : 2;
            Rectangle r = new Rectangle(grosor / 2, grosor / 2, Width - grosor - 1, Height - grosor - 1);
            using (GraphicsPath camino = Estilo.CaminoRedondeado(r, pequena ? 9 : 13))
            using (Pen lapiz = new Pen(colorBorde, grosor))
            {
                e.Graphics.DrawPath(lapiz, camino);
            }

            // El brillo holográfico va al final para quedar por encima del fondo y del borde.
            DibujarFoil(e.Graphics);
            DibujarDestello(e.Graphics, Width, Height);
        }

        // ---------- Efectos de golpe: destello de color y sacudida ----------

        /// <summary>La carta se tiñe un instante de este color y se apaga poco a poco (rojo = daño, verde = curación...).</summary>
        public void Destellar(Color color)
        {
            colorDestello = color;
            pasoDestello = PasosDestello;
            timerEfecto.Start();
        }

        /// <summary>La carta vibra de lado a lado como si recibiera un golpe.</summary>
        public void Sacudir()
        {
            if (pasoSacudida == 0) xBaseSacudida = Left;
            pasoSacudida = PasosSacudida;
            timerEfecto.Start();
        }

        private void TimerEfecto_Tick(object sender, EventArgs e)
        {
            if (pasoDestello > 0)
            {
                pasoDestello--;
                Invalidate();
                pbImagen.Invalidate();
                panelCabecera.Invalidate();
            }

            if (pasoSacudida > 0)
            {
                pasoSacudida--;
                int desplazamiento = 0;
                if (pasoSacudida > 0)
                {
                    desplazamiento = 7 * pasoSacudida / PasosSacudida + 2;
                    if (pasoSacudida % 2 == 0) desplazamiento = -desplazamiento;
                }
                Left = xBaseSacudida + desplazamiento;
            }

            if (pasoDestello == 0 && pasoSacudida == 0) timerEfecto.Stop();
        }

        /// <summary>Capa de color translúcida que se desvanece; la usan la carta y sus hijos (imagen y cabecera).</summary>
        private void DibujarDestello(Graphics g, int ancho, int alto)
        {
            if (pasoDestello <= 0) return;
            int opacidad = 120 * pasoDestello / PasosDestello;
            using (SolidBrush capa = new SolidBrush(Color.FromArgb(opacidad, colorDestello)))
            {
                g.FillRectangle(capa, 0, 0, ancho, alto);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && timerEfecto != null)
            {
                timerEfecto.Stop();
                timerEfecto.Dispose();
                timerEfecto = null;
            }
            base.Dispose(disposing);
        }

        // ---------- Efecto "foil" (carta holográfica) ----------

        /// <summary>
        /// Reflejo diagonal de brillo suave, como una carta plastificada o de edición rara: una franja
        /// de blanco semitransparente (con un leve tinte rosa y celeste en los bordes) que cruza la carta
        /// de la esquina inferior izquierda a la superior derecha. Se dibuja con coordenadas de la carta.
        /// </summary>
        private void DibujarFoil(Graphics g)
        {
            Color transparente = Color.FromArgb(0, Color.White);

            ColorBlend mezcla = new ColorBlend();
            mezcla.Positions = new float[] { 0f, 0.26f, 0.38f, 0.50f, 0.62f, 0.74f, 1f };
            mezcla.Colors = new Color[]
            {
                transparente,
                transparente,
                Color.FromArgb(14, 255, 140, 210),    // borde rosado
                Color.FromArgb(34, Color.White),      // centro del brillo
                Color.FromArgb(14, 120, 210, 255),    // borde celeste
                transparente,
                transparente
            };

            using (LinearGradientBrush brillo = new LinearGradientBrush(new Point(0, 0), new Point(Width, Height), transparente, transparente))
            {
                brillo.InterpolationColors = mezcla;
                g.FillRectangle(brillo, 0, 0, Width, Height);
            }
        }

        /// <summary>
        /// Los controles hijos (imagen, cabecera) tapan lo que dibuja la carta, así que ellos también pintan
        /// su parte del brillo. Se desplaza el dibujo para que la franja continúe sin cortes.
        /// </summary>
        private void Hijo_PintarFoil(object sender, PaintEventArgs e)
        {
            Control hijo = (Control)sender;
            e.Graphics.TranslateTransform(-hijo.Left, -hijo.Top);
            DibujarFoil(e.Graphics);
            e.Graphics.ResetTransform();
            DibujarDestello(e.Graphics, hijo.Width, hijo.Height);
        }
    }
}
