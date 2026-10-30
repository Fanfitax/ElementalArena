using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ArenaElemental.Modelos;

namespace ArenaElemental.Formularios
{
    // Ficha pequeña de un personaje para la galería de la pantalla de selección:
    // su imagen, el nombre y sus estadísticas principales. Se puede seleccionar (borde dorado)
    // y marcar como "EN EQUIPO" con su posición en la cola de relevos.
    // Para saber cuándo la eligen se usan los eventos Click (la selecciona) y DoubleClick (la agrega).
    public class MiniaturaPersonaje : UserControl
    {
        private Personaje personaje;
        private bool seleccionada;
        private int posicionEquipo;   // 0 = no está en el equipo; 1, 2, 3 = su lugar

        private PictureBox pbArte;
        private Label lblNombre, lblDatos, lblEstado;

        public MiniaturaPersonaje(Personaje personaje)
        {
            this.personaje = personaje;
            DoubleBuffered = true;
            Size = new Size(140, 158);
            BackColor = Estilo.FondoDe(personaje.Elemento);
            Cursor = Cursors.Hand;

            pbArte = new PictureBox();
            pbArte.SetBounds(4, 4, 132, 104);
            pbArte.BackColor = Estilo.ColorDe(personaje.Elemento);
            try
            {
                pbArte.Image = Ilustraciones.ObtenerArte(personaje, pbArte.Width, pbArte.Height);
            }
            catch (Exception)
            {
                pbArte.Image = null;   // sin imagen queda el color sólido del elemento
            }
            Controls.Add(pbArte);

            lblNombre = Estilo.CrearEtiqueta(personaje.Nombre, 6, 108, 128, 20, 10f, true);
            Controls.Add(lblNombre);

            lblDatos = Estilo.CrearEtiqueta("HP " + personaje.HpMax + " · ATK " + personaje.Atk + " · DEF " + personaje.Defensa,
                                            6, 127, 130, 16, 8f, false);
            lblDatos.ForeColor = Estilo.TextoSuave;
            Controls.Add(lblDatos);

            lblEstado = Estilo.CrearEtiqueta(string.Empty, 6, 143, 128, 14, 7.5f, true);
            Controls.Add(lblEstado);

            // Los controles de adentro tapan a la ficha: se reenvían sus clics para que la ficha los reciba.
            foreach (Control hijo in Controls)
            {
                hijo.Cursor = Cursors.Hand;
                hijo.Click += Hijo_Click;
                hijo.DoubleClick += Hijo_DoubleClick;
            }

            Estilo.Redondear(this, 10);
            ActualizarTextoEstado();
        }

        public Personaje Personaje
        {
            get { return personaje; }
        }

        private void Hijo_Click(object sender, EventArgs e)
        {
            OnClick(EventArgs.Empty);
        }

        private void Hijo_DoubleClick(object sender, EventArgs e)
        {
            OnDoubleClick(EventArgs.Empty);
        }

        // True si es el personaje que se está viendo en la vista previa (borde dorado).
        public bool Seleccionada
        {
            get { return seleccionada; }
            set
            {
                if (seleccionada == value) return;
                seleccionada = value;
                Invalidate();
            }
        }

        // Lugar que ocupa en el equipo (1, 2 o 3) o 0 si no está en el equipo.
        public int PosicionEquipo
        {
            get { return posicionEquipo; }
            set
            {
                if (posicionEquipo == value) return;
                posicionEquipo = value;
                ActualizarTextoEstado();
                Invalidate();
            }
        }

        private void ActualizarTextoEstado()
        {
            if (posicionEquipo > 0)
            {
                if (posicionEquipo == 1)
                    lblEstado.Text = Idioma.Texto("EN EQUIPO · INICIA", "IN TEAM · STARTS");
                else
                    lblEstado.Text = Idioma.Texto("EN EQUIPO · ", "IN TEAM · ") + posicionEquipo + Idioma.Texto(".º EN COLA", ". IN QUEUE");
                lblEstado.ForeColor = Color.Gold;
            }
            else
            {
                lblEstado.Text = Estilo.RolDe(personaje.Elemento);
                lblEstado.ForeColor = Estilo.ColorDe(personaje.Elemento);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color color = Estilo.ColorDe(personaje.Elemento);
            int grosor = 2;
            if (posicionEquipo > 0) { color = Color.FromArgb(110, 210, 140); grosor = 3; }
            if (seleccionada) { color = Color.Gold; grosor = 4; }

            Rectangle r = new Rectangle(grosor / 2, grosor / 2, Width - grosor - 1, Height - grosor - 1);
            using (GraphicsPath camino = Estilo.CaminoRedondeado(r, 9))
            using (Pen lapiz = new Pen(color, grosor))
            {
                e.Graphics.DrawPath(lapiz, camino);
            }
        }
    }
}
