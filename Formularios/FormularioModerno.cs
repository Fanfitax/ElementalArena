using System;
using System.Drawing;
using System.Windows.Forms;

namespace ArenaElemental.Formularios
{
    /// <summary>
    /// Formulario base con el fondo moderno del juego (ver Estilo.PintarFondo).
    /// El fondo se dibuja una sola vez en una imagen y después solo se copia, así la ventana no parpadea.
    /// Si el formulario tiene una BackgroundImage (como el tablero del combate), se usa esa en su lugar.
    /// </summary>
    public class FormularioModerno : Form
    {
        private Bitmap fondo;
        private bool fondoModerno = true;

        public FormularioModerno()
        {
            DoubleBuffered = true;
        }

        /// <summary>
        /// True (por defecto) pinta el fondo moderno del juego. Si se pone en false el formulario usa
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
                fondo = new Bitmap(ClientSize.Width, ClientSize.Height);
                using (Graphics g = Graphics.FromImage(fondo))
                {
                    Estilo.PintarFondo(g, new Rectangle(0, 0, fondo.Width, fondo.Height));
                }
            }
            e.Graphics.DrawImageUnscaled(fondo, 0, 0);
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
