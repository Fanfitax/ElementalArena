using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ArenaElemental.Formularios
{
    /// <summary>
    /// Efectos de partículas del combate: chispas cuando se recibe un golpe, brillos que suben al curar,
    /// un estallido cuando cae un personaje y escombros que caen durante el Terremoto.
    ///
    /// Es una ventana sin borde que se coloca justo encima del tablero. Su fondo es de un color "clave"
    /// (TransparencyKey), así que solo se ven las partículas y todo lo demás es transparente, incluso al
    /// hacer clic (los clics pasan al combate). Así se dibujan decenas de partículas en una sola pasada,
    /// en lugar de crear un control por partícula, que haría la pantalla lenta.
    /// Todas las posiciones se dan en coordenadas del formulario del combate.
    /// La ventana solo está visible (y el reloj solo corre) mientras quedan partículas.
    /// </summary>
    public class EfectosCombate : Form
    {
        private const int MaximoParticulas = 400;
        private static Color ColorClave = Color.FromArgb(255, 0, 255);   // el color que se vuelve transparente

        private class Particula
        {
            public double X, Y;               // centro de la partícula
            public double VelocidadX, VelocidadY;
            public double Gravedad;           // positivo = cae, negativo = sube
            public int Tamano;                // tamaño inicial
            public int Vida, Edad;            // en pasos de 30 ms
            public Color Color;
        }

        private Form duenio;
        private Timer timer;
        private List<Particula> particulas = new List<Particula>();
        private Random azar = new Random();

        public EfectosCombate(Form duenio)
        {
            this.duenio = duenio;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            BackColor = ColorClave;
            TransparencyKey = ColorClave;
            Size = duenio.ClientSize;

            timer = new Timer();
            timer.Interval = 30;
            timer.Tick += Timer_Tick;

            // Si el combate se mueve (por ejemplo con el temblor), la capa lo sigue.
            duenio.LocationChanged += Duenio_Cambio;
            duenio.SizeChanged += Duenio_Cambio;
        }

        /// <summary>Se muestra sin quitarle el foco al combate.</summary>
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        /// <summary>Sin foco, fuera de la barra de tareas y sin capturar el ratón: los clics pasan al combate.</summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle = cp.ExStyle | 0x08000000 | 0x00000020 | 0x00000080;   // NOACTIVATE | TRANSPARENT | TOOLWINDOW
                return cp;
            }
        }

        // ---------- Los efectos que usa el combate ----------

        /// <summary>Chispas que salen disparadas hacia todos lados (golpe recibido).</summary>
        public void Chispas(Point centro, Color color, int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                double angulo = azar.NextDouble() * Math.PI * 2;
                double fuerza = 3 + azar.NextDouble() * 8;
                Agregar(centro.X, centro.Y, Math.Cos(angulo) * fuerza, Math.Sin(angulo) * fuerza - 2, 0.5, 6 + azar.Next(7), 16 + azar.Next(12), Variar(color));
            }
        }

        /// <summary>Brillos que suben despacio con un vaivén (curación).</summary>
        public void Brillos(Point centro, Color color, int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                double x = centro.X + (azar.NextDouble() - 0.5) * 140;
                double y = centro.Y + (azar.NextDouble() - 0.2) * 70;
                Agregar(x, y, (azar.NextDouble() - 0.5) * 1.2, -1.5 - azar.NextDouble() * 2, -0.04, 5 + azar.Next(6), 26 + azar.Next(14), Variar(color));
            }
        }

        /// <summary>Una explosión grande de partículas (cae un personaje).</summary>
        public void Estallido(Point centro, Color color, int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                double angulo = azar.NextDouble() * Math.PI * 2;
                double fuerza = 2 + azar.NextDouble() * 12;
                Color tono = Variar(color);
                if (i % 4 == 0) tono = Color.White;
                Agregar(centro.X, centro.Y, Math.Cos(angulo) * fuerza, Math.Sin(angulo) * fuerza - 3, 0.35, 7 + azar.Next(9), 28 + azar.Next(16), tono);
            }
        }

        /// <summary>Escombros y polvo que caen desde arriba por todo el ancho (Terremoto).</summary>
        public void Escombros(int ancho, int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                double x = azar.NextDouble() * ancho;
                Color tono = Color.FromArgb(120 + azar.Next(70), 90 + azar.Next(50), 50 + azar.Next(40));
                if (i % 3 == 0) tono = Color.FromArgb(150, 140, 130);   // polvo gris
                Agregar(x, 30 - azar.Next(60), (azar.NextDouble() - 0.5) * 1.5, 1 + azar.NextDouble() * 3, 0.55, 7 + azar.Next(10), 34 + azar.Next(16), tono);
            }
        }

        /// <summary>Quita todas las partículas y oculta la capa.</summary>
        public void Detener()
        {
            timer.Stop();
            particulas.Clear();
            if (Visible) Hide();
        }

        // ---------- Movimiento ----------

        private void Agregar(double x, double y, double velX, double velY, double gravedad, int tamano, int vida, Color color)
        {
            if (particulas.Count >= MaximoParticulas) return;

            Particula p = new Particula();
            p.X = x;
            p.Y = y;
            p.VelocidadX = velX;
            p.VelocidadY = velY;
            p.Gravedad = gravedad;
            p.Tamano = tamano;
            p.Vida = vida;
            p.Color = color;
            particulas.Add(p);

            Mostrar();
        }

        /// <summary>Pone la capa sobre el tablero y arranca el reloj (si no estaba ya en marcha).</summary>
        private void Mostrar()
        {
            if (IsDisposed || duenio.IsDisposed || !duenio.Visible) return;

            Ubicar();
            if (!Visible) Show(duenio);
            timer.Start();
        }

        private void Ubicar()
        {
            Size = duenio.ClientSize;
            Location = duenio.PointToScreen(Point.Empty);
        }

        private void Duenio_Cambio(object sender, EventArgs e)
        {
            if (Visible) Ubicar();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            for (int i = particulas.Count - 1; i >= 0; i--)
            {
                Particula p = particulas[i];
                p.Edad++;
                if (p.Edad >= p.Vida)
                {
                    particulas.RemoveAt(i);
                    continue;
                }

                p.VelocidadY += p.Gravedad;
                p.VelocidadX *= 0.96;
                p.X += p.VelocidadX;
                p.Y += p.VelocidadY;
            }

            if (particulas.Count == 0)
            {
                timer.Stop();
                Hide();
                return;
            }
            Invalidate();
        }

        // ---------- Dibujo ----------

        /// <summary>
        /// Dibuja todas las partículas: cuadritos que se encogen al envejecer. Se dibuja sin suavizado porque
        /// los bordes suavizados se mezclarían con el color clave de la transparencia.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.None;

            using (SolidBrush pincel = new SolidBrush(Color.White))
            {
                foreach (Particula p in particulas)
                {
                    // Se mantiene grande casi toda su vida y se encoge al final.
                    double progreso = (double)p.Edad / p.Vida;
                    int tamano = (int)Math.Round(p.Tamano * (1.0 - progreso * progreso));
                    if (tamano < 2) tamano = 2;

                    pincel.Color = p.Color;
                    e.Graphics.FillRectangle(pincel, (int)p.X - tamano / 2, (int)p.Y - tamano / 2, tamano, tamano);
                }
            }
        }

        // ---------- Colores ----------

        /// <summary>El mismo color pero un poco más claro o más oscuro, para que las partículas no sean todas iguales.</summary>
        private Color Variar(Color color)
        {
            int cambio = azar.Next(-30, 60);
            return Color.FromArgb(Limitar(color.R + cambio), Limitar(color.G + cambio), Limitar(color.B + cambio));
        }

        private int Limitar(int valor)
        {
            // El 255,0,255 es el color transparente: ningún tono debe parecerse a él.
            if (valor < 1) return 1;
            if (valor > 255) return 255;
            return valor;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                duenio.LocationChanged -= Duenio_Cambio;
                duenio.SizeChanged -= Duenio_Cambio;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    timer = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
