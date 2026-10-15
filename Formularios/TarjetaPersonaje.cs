using System;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Modelos;

namespace ArenaElemental.Formularios
{
    /// <summary>Control que muestra un personaje en combate: nombre, barras de HP y energía y estado.</summary>
    public class TarjetaPersonaje : Panel
    {
        private readonly Label lblNombre;
        private readonly Label lblDetalle;
        private readonly Label lblHp;
        private readonly Label lblEng;
        private readonly Label lblEstado;
        private readonly ProgressBar barHp;
        private readonly ProgressBar barEng;

        private Color colorBorde = Color.Gray;
        private int grosorBorde = 1;

        public TarjetaPersonaje()
        {
            DoubleBuffered = true;
            Size = new Size(330, 112);
            BackColor = Estilo.Panel;

            lblNombre = Estilo.CrearEtiqueta(string.Empty, 8, 6, 214, 22, 11f, true);
            lblEstado = Estilo.CrearEtiqueta(string.Empty, 222, 8, 100, 20, 8.5f, true);
            lblEstado.TextAlign = ContentAlignment.MiddleRight;

            lblHp = Estilo.CrearEtiqueta("HP", 8, 32, 80, 16, 8.5f, false);
            barHp = new ProgressBar();
            barHp.SetBounds(90, 33, 232, 14);

            lblEng = Estilo.CrearEtiqueta("ENG", 8, 52, 80, 16, 8.5f, false);
            barEng = new ProgressBar();
            barEng.SetBounds(90, 53, 232, 14);

            lblDetalle = Estilo.CrearEtiqueta(string.Empty, 8, 74, 314, 34, 8.5f, false);
            lblDetalle.ForeColor = Estilo.TextoSuave;

            Controls.Add(lblNombre);
            Controls.Add(lblEstado);
            Controls.Add(lblHp);
            Controls.Add(barHp);
            Controls.Add(lblEng);
            Controls.Add(barEng);
            Controls.Add(lblDetalle);
        }

        public void Actualizar(Personaje p, bool activo)
        {
            colorBorde = Estilo.ColorDe(p.Elemento);
            grosorBorde = activo ? 4 : 1;
            BackColor = activo ? Estilo.PanelClaro : Estilo.Panel;

            lblNombre.Text = p.Nombre + "  [" + p.Elemento + "]";
            lblNombre.ForeColor = p.EstaVivo ? Estilo.Texto : Estilo.TextoSuave;

            barHp.Maximum = Math.Max(1, p.HpMax);
            barHp.Value = Math.Min(barHp.Maximum, Math.Max(0, p.Hp));
            lblHp.Text = "HP " + p.Hp + "/" + p.HpMax;

            barEng.Maximum = Math.Max(1, p.EnergiaMax);
            barEng.Value = Math.Min(barEng.Maximum, Math.Max(0, p.Energia));
            lblEng.Text = "ENG " + p.Energia + "/" + p.EnergiaMax;

            string def = "DEF " + p.DefensaEfectiva;
            if (p.DefensaEfectiva != p.Defensa) def += " (+" + (p.DefensaEfectiva - p.Defensa) + ")";
            string hab = p.CooldownActual > 0 ? "recarga " + p.CooldownActual + "t" : "lista";
            lblDetalle.Text = "ATK " + p.Atk + "   " + def + "   EMP " + p.Empatia + "\n"
                              + p.NombreHabilidad + ": " + hab;

            if (!p.EstaVivo) lblEstado.Text = "DERROTADO";
            else lblEstado.Text = activo ? "EN CAMPO" : "EN ESPERA";
            lblEstado.ForeColor = !p.EstaVivo ? Color.IndianRed : (activo ? Color.Gold : Estilo.TextoSuave);

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen lapiz = new Pen(colorBorde, grosorBorde))
            {
                int mitad = grosorBorde / 2;
                e.Graphics.DrawRectangle(lapiz, mitad, mitad, Width - grosorBorde, Height - grosorBorde);
            }
        }
    }
}
