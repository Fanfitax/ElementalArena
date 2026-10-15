using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ArenaElemental.Modelos;

namespace ArenaElemental.Formularios
{
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
        private bool pequena;
        private bool resaltada;
        private bool esSiguiente;
        private Elemento elemento = Elemento.Fuego;
        private string nombreMostrado = string.Empty;

        private Panel panelCabecera;
        private PictureBox pbImagen;
        private Label lblNombre, lblElemento;
        private Label lblHp, lblEng, lblStats, lblHabAtaque, lblHabUtilidad, lblEstado;
        private ProgressBar barHp, barEng;

        public VistaCarta() : this(false)
        {
        }

        public VistaCarta(bool pequena)
        {
            this.pequena = pequena;
            DoubleBuffered = true;
            Size = pequena ? new Size(128, 207) : new Size(232, 420);
            BackColor = Estilo.Panel;

            if (pequena) ConstruirPequena();
            else ConstruirGrande();

            Estilo.Redondear(this, pequena ? 10 : 14);
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

        private void ConstruirPequena()
        {
            panelCabecera = new Panel();
            panelCabecera.SetBounds(4, 4, 120, 22);
            lblNombre = CrearTexto(panelCabecera, 4, 1, 112, 20, 9f, true);
            Controls.Add(panelCabecera);

            pbImagen = CrearImagen(4, 30, 120, 100);

            lblHp = CrearTexto(this, 6, 134, 116, 14, 8f, true);
            barHp = CrearBarra(6, 148, 116, 8);
            lblStats = CrearTexto(this, 6, 160, 116, 14, 8f, false);
            lblEstado = CrearTexto(this, 6, 180, 116, 18, 8f, true);
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
        }
    }
}
