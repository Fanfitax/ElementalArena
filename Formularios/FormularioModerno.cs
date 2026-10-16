using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ArenaElemental.Formularios
{
    /// <summary>
    /// Formulario base de todas las pantallas del juego. Su fondo es la imagen Imagenes\Inicio.jpg estirada
    /// al tamaño de la ventana (con un velo oscuro suave para que los textos se lean).
    /// El fondo se prepara una sola vez en una imagen y después solo se copia, así la ventana no parpadea.
    /// Si Inicio.jpg no existe, se pinta el fondo geométrico antiguo (ver Estilo.PintarFondo).
    /// La pantalla de combate es la excepción: pone su propia BackgroundImage (la del escenario) y entonces
    /// este fondo general se ignora.
    /// </summary>
    public class FormularioModerno : Form
    {
        private const string ArchivoFondo = "Inicio.jpg";
        // El velo oscuro va en degradado: más fuerte arriba (donde están los títulos y el cielo es claro) y suave abajo.
        private const int VeloArriba = 150;   // 0 = sin velo, 255 = negro total
        private const int VeloAbajo = 50;

        // La imagen original se carga una sola vez y la comparten todas las pantallas.
        private static Image imagenInicio;
        private static bool inicioIntentado;

        private Bitmap fondo;
        private bool fondoModerno = true;

        public FormularioModerno()
        {
            DoubleBuffered = true;
        }

        /// <summary>
        /// True (por defecto) pinta el fondo general del juego. Si se pone en false el formulario usa
        /// solo su BackColor (un color liso), que sirve de respaldo cuando falta una imagen de fondo.
        /// </summary>
        public bool FondoModerno
        {
            get { return fondoModerno; }
            set { fondoModerno = value; Invalidate(); }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!fondoModerno || BackgroundImage != null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                base.OnPaintBackground(e);
                return;
            }

            if (fondo == null || fondo.Width != ClientSize.Width || fondo.Height != ClientSize.Height)
            {
                if (fondo != null) fondo.Dispose();
                fondo = PrepararFondo(ClientSize.Width, ClientSize.Height);
            }
            e.Graphics.DrawImageUnscaled(fondo, 0, 0);
        }

        /// <summary>Estira Inicio.jpg al tamaño de la ventana y le pone el velo; si no hay imagen, el fondo geométrico.</summary>
        private Bitmap PrepararFondo(int ancho, int alto)
        {
            Bitmap resultado = new Bitmap(ancho, alto);
            using (Graphics g = Graphics.FromImage(resultado))
            {
                Image original = ObtenerImagenInicio();
                if (original == null)
                {
                    Estilo.PintarFondo(g, new Rectangle(0, 0, ancho, alto));
                }
                else
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(original, new Rectangle(0, 0, ancho, alto));
                    Rectangle zona = new Rectangle(0, 0, ancho, alto);
                    using (LinearGradientBrush velo = new LinearGradientBrush(zona, Color.FromArgb(VeloArriba, 0, 0, 0), Color.FromArgb(VeloAbajo, 0, 0, 0), 90f))
                    {
                        g.FillRectangle(velo, zona);
                    }
                }
            }
            return resultado;
        }

        private static Image ObtenerImagenInicio()
        {
            if (!inicioIntentado)
            {
                inicioIntentado = true;
                imagenInicio = Estilo.CargarImagen(ArchivoFondo);   // null si falta o está dañada
            }
            return imagenInicio;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && fondo != null)
            {
                fondo.Dispose();
                fondo = null;
            }
            base.Dispose(disposing);
        }
    }
}
