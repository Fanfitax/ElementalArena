using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ArenaElemental.Formularios
{
    /// <summary>
    /// Banner grande que anuncia un evento del combate (TERREMOTO, MUERTE SÚBITA) sobre el tablero.
    /// Es una ventana sin borde con Opacity: así el fondo es de verdad translúcido y deja ver las cartas y
    /// los botones que quedan debajo (un Label con fondo semitransparente no puede hacerlo).
    /// Aparece suavemente, se queda un par de segundos y se cierra sola. Un clic lo cierra antes.
    /// </summary>
    public class AvisoEvento : Form
    {
        private const double OpacidadMaxima = 0.88;
        private const int TicksEntrada = 6;     // 6 x 30 ms: aparece
        private const int TicksVisible = 66;    // ~2 segundos en pantalla
        private const int TicksSalida = 10;     // se desvanece

        private string titulo;
        private string detalle;
        private Timer timer = new Timer();
        private int ticks;

        public AvisoEvento(string titulo, string detalle)
        {
            this.titulo = titulo;
            this.detalle = detalle;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            BackColor = Color.Black;
            Size = new Size(500, 126);
            Opacity = 0;
            Estilo.Redondear(this, 18);

            timer.Interval = 30;
            timer.Tick += Timer_Tick;
            Click += AvisoEvento_Click;
        }

        private void AvisoEvento_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>Se muestra sin quitarle el foco a la ventana del combate.</summary>
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ticks++;
            if (ticks <= TicksEntrada)
            {
                Opacity = OpacidadMaxima * ticks / TicksEntrada;
            }
            else if (ticks <= TicksEntrada + TicksVisible)
            {
                Opacity = OpacidadMaxima;
            }
            else if (ticks <= TicksEntrada + TicksVisible + TicksSalida)
            {
                int pasoSalida = ticks - TicksEntrada - TicksVisible;
                Opacity = OpacidadMaxima * (1.0 - (double)pasoSalida / TicksSalida);
            }
            else
            {
                Close();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            Rectangle area = new Rectangle(0, 0, Width, Height);
            using (LinearGradientBrush fondo = new LinearGradientBrush(area, Color.FromArgb(28, 16, 8), Color.FromArgb(6, 6, 12), 90f))
            {
                g.FillRectangle(fondo, area);
            }

            // Marco dorado.
            Rectangle marco = new Rectangle(2, 2, Width - 5, Height - 5);
            using (GraphicsPath camino = Estilo.CaminoRedondeado(marco, 16))
            using (Pen lapiz = new Pen(Color.FromArgb(230, 190, 60), 3f))
            {
                g.DrawPath(lapiz, camino);
            }

            StringFormat centrado = new StringFormat();
            centrado.Alignment = StringAlignment.Center;
            centrado.LineAlignment = StringAlignment.Center;

            // Título grande: sombra negra y letras con degradado dorado -> naranja.
            Rectangle zonaTitulo = new Rectangle(10, 8, Width - 20, 76);
            using (Font fuenteTitulo = new Font("Segoe UI", 40f, FontStyle.Bold))
            {
                Rectangle sombra = new Rectangle(zonaTitulo.X + 3, zonaTitulo.Y + 3, zonaTitulo.Width, zonaTitulo.Height);
                g.DrawString(titulo, fuenteTitulo, Brushes.Black, sombra, centrado);
                using (LinearGradientBrush letras = new LinearGradientBrush(zonaTitulo, Color.FromArgb(255, 226, 90), Color.FromArgb(255, 120, 20), 90f))
                {
                    g.DrawString(titulo, fuenteTitulo, letras, zonaTitulo, centrado);
                }
            }

            Rectangle zonaDetalle = new Rectangle(14, 82, Width - 28, 36);
            using (Font fuenteDetalle = new Font("Segoe UI", 11.5f, FontStyle.Bold))
            using (SolidBrush pincel = new SolidBrush(Color.FromArgb(255, 205, 140)))
            {
                g.DrawString(detalle, fuenteDetalle, pincel, zonaDetalle, centrado);
            }
            centrado.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
            base.Dispose(disposing);
        }
    }
}
