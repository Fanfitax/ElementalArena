using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ArenaElemental.Modelos;

namespace ArenaElemental.Formularios
{
    /// <summary>Colores, textos y fábricas de controles compartidos por todos los formularios.</summary>
    public static class Estilo
    {
        public static readonly Color Fondo = Color.FromArgb(24, 26, 40);
        public static readonly Color Panel = Color.FromArgb(38, 41, 62);
        public static readonly Color PanelClaro = Color.FromArgb(58, 62, 92);
        public static readonly Color Texto = Color.WhiteSmoke;
        public static readonly Color TextoSuave = Color.FromArgb(165, 170, 200);
        public static readonly Color Acento = Color.FromArgb(96, 120, 220);

        public const string Triangulo = "Agua > Fuego > Aire > Tierra > Agua";

        public static Color ColorDe(Elemento elemento)
        {
            switch (elemento)
            {
                case Elemento.Fuego: return Color.FromArgb(226, 88, 52);
                case Elemento.Agua: return Color.FromArgb(52, 136, 226);
                case Elemento.Tierra: return Color.FromArgb(150, 108, 62);
                default: return Color.FromArgb(120, 196, 196);   // Aire
            }
        }

        /// <summary>Versión oscura del color del elemento, para fondos de filas.</summary>
        public static Color FondoDe(Elemento elemento)
        {
            Color c = ColorDe(elemento);
            return Color.FromArgb(c.R / 3 + 14, c.G / 3 + 14, c.B / 3 + 14);
        }

        public static Button CrearBoton(string texto, int x, int y, int ancho, int alto)
        {
            Button b = new Button();
            b.Text = texto;
            b.SetBounds(x, y, ancho, alto);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Acento;
            b.ForeColor = Color.White;
            b.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
            return b;
        }

        public static Label CrearEtiqueta(string texto, int x, int y, int ancho, int alto, float tamano, bool negrita)
        {
            Label l = new Label();
            l.Text = texto;
            l.SetBounds(x, y, ancho, alto);
            l.ForeColor = Texto;
            l.BackColor = Color.Transparent;
            l.Font = new Font("Segoe UI", tamano, negrita ? FontStyle.Bold : FontStyle.Regular);
            return l;
        }

        public static void AplicarFormulario(Form f, string titulo, int ancho, int alto)
        {
            f.Text = titulo;
            f.ClientSize = new Size(ancho, alto);
            f.BackColor = Fondo;
            f.ForeColor = Texto;
            f.Font = new Font("Segoe UI", 9.5f);
            f.StartPosition = FormStartPosition.CenterScreen;
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
        }

        public static string DescripcionHabilidad(Personaje p)
        {
            switch (p.Elemento)
            {
                case Elemento.Fuego: return "Llamarada: daño x2 que ignora el 50% de la defensa del rival.";
                case Elemento.Agua: return "Marea Curativa: cura al aliado elegido (25% de su HP máx. + 2 por Empatía).";
                case Elemento.Tierra: return "Muro de Roca: sube la defensa durante 2 turnos del rival.";
                default: return "Ráfaga Veloz: dos golpes al 75% de potencia y recupera 2 de energía.";
            }
        }

        /// <summary>Texto "Fuerte contra X · Débil contra Y" según el triángulo elemental.</summary>
        public static string DescripcionTriangulo(Personaje p)
        {
            string fuerte = string.Empty, debil = string.Empty;
            foreach (Elemento e in Enum.GetValues(typeof(Elemento)))
            {
                double m = p.ObtenerMultiplicador(e);
                if (m > Personaje.MultiplicadorNeutro) fuerte = e.ToString();
                else if (m < Personaje.MultiplicadorNeutro) debil = e.ToString();
            }
            return "Fuerte contra " + fuerte + "  ·  Débil contra " + debil;
        }

        public static string DescripcionCompleta(Personaje p)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(p.Nombre + "  [" + p.Elemento + "]");
            sb.AppendLine("HP " + p.HpMax + "   ATK " + p.Atk + "   DEF " + p.Defensa + "   ENG " + p.EnergiaMax + "   EMP " + p.Empatia);
            sb.AppendLine(DescripcionTriangulo(p));
            sb.Append(DescripcionHabilidad(p) + "  (costo " + p.CostoHabilidad + " ENG, recarga " + p.CooldownHabilidad + " turnos)");
            return sb.ToString();
        }
    }
}
