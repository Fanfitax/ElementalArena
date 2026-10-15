using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ArenaElemental.Modelos;

namespace ArenaElemental.Formularios
{
    /// <summary>
    /// Representa un personaje como una carta física de un juego de cartas (TCG).
    /// Todo se dibuja por código con controles nativos: Panel (cabecera y dibujo), Label y ProgressBar.
    /// Hay dos tamaños: la carta grande (en el campo de duelo) y la carta pequeña (en la banca).
    /// Se conecta con la lógica llamando a ActualizarDatos(personaje).
    /// </summary>
    public class VistaCarta : UserControl
    {
        private bool pequena;
        private bool resaltada;
        private Elemento elemento = Elemento.Fuego;

        private Panel panelCabecera;
        private Panel panelArte;
        private Label lblNombre, lblElemento;
        private Label lblHp, lblEng, lblStats, lblHabilidad, lblEstado;
        private ProgressBar barHp, barEng;

        public VistaCarta() : this(false)
        {
        }

        public VistaCarta(bool pequena)
        {
            this.pequena = pequena;
            DoubleBuffered = true;
            Size = pequena ? new Size(124, 112) : new Size(230, 300);
            BackColor = Estilo.Panel;
            Padding = new Padding(0);

            if (pequena) ConstruirPequena();
            else ConstruirGrande();
        }

        // ---------- Construcción de los controles ----------

        private void ConstruirGrande()
        {
            panelCabecera = new Panel();
            panelCabecera.SetBounds(6, 6, 218, 34);
            lblNombre = CrearTexto(6, 4, 130, 26, 13f, true);
            lblElemento = CrearTexto(124, 4, 88, 26, 8.5f, true);
            lblElemento.TextAlign = ContentAlignment.MiddleRight;
            panelCabecera.Controls.Add(lblNombre);
            panelCabecera.Controls.Add(lblElemento);
            Controls.Add(panelCabecera);

            // Zona del dibujo (el emblema del elemento se pinta en el evento Paint).
            panelArte = new Panel();
            panelArte.SetBounds(6, 44, 218, 96);
            panelArte.Paint += PanelArte_Paint;
            Controls.Add(panelArte);

            lblHp = CrearTexto(8, 146, 214, 18, 9.5f, true);
            barHp = CrearBarra(8, 166, 214, 16);
            lblEng = CrearTexto(8, 188, 214, 18, 9.5f, true);
            barEng = CrearBarra(8, 208, 214, 14);

            lblStats = CrearTexto(8, 230, 214, 20, 10f, true);
            lblHabilidad = CrearTexto(8, 252, 214, 20, 9f, false);
            lblHabilidad.ForeColor = Estilo.TextoSuave;
            lblEstado = CrearTexto(8, 274, 214, 20, 10f, true);
            lblEstado.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void ConstruirPequena()
        {
            panelCabecera = new Panel();
            panelCabecera.SetBounds(4, 4, 116, 22);
            lblNombre = CrearTexto(4, 1, 108, 20, 9f, true);
            panelCabecera.Controls.Add(lblNombre);
            Controls.Add(panelCabecera);

            lblHp = CrearTexto(6, 30, 112, 14, 8f, true);
            barHp = CrearBarra(6, 44, 112, 10);
            lblEng = CrearTexto(6, 56, 112, 14, 8f, true);
            barEng = CrearBarra(6, 70, 112, 8);
            lblStats = CrearTexto(6, 80, 112, 14, 8f, false);
            lblEstado = CrearTexto(6, 94, 112, 14, 8f, true);
            lblEstado.TextAlign = ContentAlignment.MiddleCenter;
        }

        private Label CrearTexto(int x, int y, int ancho, int alto, float tamano, bool negrita)
        {
            Label l = Estilo.CrearEtiqueta(string.Empty, x, y, ancho, alto, tamano, negrita);
            Controls.Add(l);
            return l;
        }

        private ProgressBar CrearBarra(int x, int y, int ancho, int alto)
        {
            ProgressBar b = new ProgressBar();
            b.SetBounds(x, y, ancho, alto);
            Controls.Add(b);
            return b;
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
            if (lblElemento != null)
            {
                lblElemento.ForeColor = colorTitulo;
                lblElemento.Text = elemento.ToString().ToUpper();
            }
            lblNombre.Text = p.Nombre;

            // Vida y energía (las barras no aceptan valores fuera de su rango).
            barHp.Maximum = Math.Max(1, p.HpMax);
            barHp.Value = Math.Max(0, Math.Min(barHp.Maximum, p.Hp));
            lblHp.Text = "HP  " + p.Hp + " / " + p.HpMax;

            barEng.Maximum = Math.Max(1, p.EnergiaMax);
            barEng.Value = Math.Max(0, Math.Min(barEng.Maximum, p.Energia));
            lblEng.Text = "ENG  " + p.Energia + " / " + p.EnergiaMax;

            // Ataque y defensa (la defensa muestra el bono temporal, por ejemplo el Muro de Roca).
            string def = "DEF " + p.DefensaEfectiva;
            if (p.DefensaEfectiva > p.Defensa) def += " (+" + (p.DefensaEfectiva - p.Defensa) + ")";
            lblStats.Text = "ATK " + p.Atk + "    " + def;

            if (lblHabilidad != null)
            {
                string hab = p.CooldownActual > 0 ? "recarga " + p.CooldownActual + "t" : "lista";
                lblHabilidad.Text = p.NombreHabilidad + ": " + hab;
            }

            lblEstado.Text = estado;
            if (!p.EstaVivo) lblEstado.ForeColor = Color.IndianRed;
            else if (resaltada) lblEstado.ForeColor = Color.Gold;
            else lblEstado.ForeColor = Estilo.TextoSuave;

            if (cambioVisual)
            {
                if (panelArte != null) panelArte.Invalidate();
                Invalidate();
            }
        }

        // ---------- Dibujo ----------

        /// <summary>Borde de la carta con el color del elemento (más grueso si está en el campo).</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int grosor = resaltada ? 4 : 2;
            using (Pen lapiz = new Pen(Estilo.ColorDe(elemento), grosor))
            {
                e.Graphics.DrawRectangle(lapiz, grosor / 2, grosor / 2, Width - grosor, Height - grosor);
            }
        }

        /// <summary>Pinta el fondo degradado y un emblema sencillo según el elemento.</summary>
        private void PanelArte_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = panelArte.ClientRectangle;
            Color color = Estilo.ColorDe(elemento);

            using (LinearGradientBrush fondo = new LinearGradientBrush(r, Estilo.FondoDe(elemento), color, 90f))
            {
                g.FillRectangle(fondo, r);
            }

            // Emblema cuadrado centrado.
            int lado = Math.Min(r.Width, r.Height) - 20;
            Rectangle emb = new Rectangle((r.Width - lado) / 2, (r.Height - lado) / 2, lado, lado);
            Color claro = Color.FromArgb(215, 255, 255, 255);

            using (SolidBrush pincel = new SolidBrush(claro))
            using (Pen lapiz = new Pen(claro, 5f))
            {
                switch (elemento)
                {
                    case Elemento.Fuego:     // llama
                        g.FillPolygon(pincel, new Point[] {
                            Punto(emb, 0.55, 0.00), Punto(emb, 0.70, 0.30), Punto(emb, 0.88, 0.52),
                            Punto(emb, 0.94, 0.78), Punto(emb, 0.76, 1.00), Punto(emb, 0.24, 1.00),
                            Punto(emb, 0.06, 0.78), Punto(emb, 0.12, 0.54), Punto(emb, 0.30, 0.34),
                            Punto(emb, 0.38, 0.56), Punto(emb, 0.46, 0.24) });
                        break;
                    case Elemento.Agua:      // gota
                        g.FillEllipse(pincel, emb.X + emb.Width * 0.15f, emb.Y + emb.Height * 0.35f, emb.Width * 0.70f, emb.Height * 0.65f);
                        g.FillPolygon(pincel, new Point[] {
                            Punto(emb, 0.50, 0.00), Punto(emb, 0.22, 0.58), Punto(emb, 0.78, 0.58) });
                        break;
                    case Elemento.Tierra:    // roca
                        g.FillPolygon(pincel, new Point[] {
                            Punto(emb, 0.25, 0.10), Punto(emb, 0.75, 0.10), Punto(emb, 1.00, 0.55),
                            Punto(emb, 0.80, 1.00), Punto(emb, 0.20, 1.00), Punto(emb, 0.00, 0.55) });
                        break;
                    default:                 // Aire: tres rayas de viento
                        for (int i = 0; i < 3; i++)
                        {
                            int y = emb.Y + (int)(emb.Height * (0.15 + 0.35 * i));
                            g.DrawArc(lapiz, emb.X, y - emb.Height / 6, emb.Width, emb.Height / 3, 200, 140);
                        }
                        break;
                }
            }
        }

        /// <summary>Punto dentro de un rectángulo, dado como fracción (0 a 1) del ancho y del alto.</summary>
        private static Point Punto(Rectangle r, double fx, double fy)
        {
            return new Point(r.X + (int)(r.Width * fx), r.Y + (int)(r.Height * fy));
        }
    }
}
