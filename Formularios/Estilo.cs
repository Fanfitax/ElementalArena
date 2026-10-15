using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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

        /// <summary>
        /// Busca el archivo dentro de la carpeta "Imagenes": junto al .exe, en la carpeta actual
        /// o en la carpeta del proyecto (cuando se ejecuta desde bin\Debug). Devuelve null si no existe.
        /// </summary>
        public static string BuscarImagen(string archivo)
        {
            string[] carpetas = new string[]
            {
                Application.StartupPath,
                Directory.GetCurrentDirectory(),
                Path.Combine(Application.StartupPath, Path.Combine("..", "..")),
                Path.Combine(Application.StartupPath, Path.Combine("..", Path.Combine("..", "..")))
            };

            foreach (string carpeta in carpetas)
            {
                string ruta = Path.Combine(Path.Combine(carpeta, "Imagenes"), archivo);
                if (File.Exists(ruta)) return ruta;
            }
            return null;
        }

        /// <summary>
        /// Carga una imagen de la carpeta "Imagenes". Si no existe o está dañada devuelve null
        /// (el programa nunca se cae por una imagen que falta).
        /// </summary>
        public static Image CargarImagen(string archivo)
        {
            string ruta = BuscarImagen(archivo);
            if (ruta == null) return null;
            try
            {
                // Se copia a un Bitmap para no dejar el archivo bloqueado en el disco.
                using (Image original = Image.FromFile(ruta))
                {
                    return new Bitmap(original);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

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
            b.SetBounds(x, y, ancho, alto);
            EstilizarBoton(b, texto, Acento);
            return b;
        }

        /// <summary>
        /// Botón de diseño plano y moderno: sin borde, color sólido, esquinas redondeadas, letra en negrita
        /// y un tono más claro al pasar el mouse. Se debe llamar después de darle su tamaño al botón.
        /// </summary>
        public static void EstilizarBoton(Button b, string texto, Color color)
        {
            b.Text = texto;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = color;
            b.ForeColor = Color.White;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.25f);
            b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.1f);
            b.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
            Redondear(b, 8);
        }

        /// <summary>Le da esquinas redondeadas a cualquier control recortando su región.</summary>
        public static void Redondear(Control control, int radio)
        {
            if (control.Width <= 0 || control.Height <= 0) return;
            using (GraphicsPath camino = CaminoRedondeado(new Rectangle(0, 0, control.Width, control.Height), radio))
            {
                control.Region = new Region(camino);
            }
        }

        /// <summary>Rectángulo con las esquinas redondeadas (para recortar o dibujar bordes).</summary>
        public static GraphicsPath CaminoRedondeado(Rectangle r, int radio)
        {
            int d = radio * 2;
            GraphicsPath camino = new GraphicsPath();
            camino.AddArc(r.X, r.Y, d, d, 180, 90);
            camino.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            camino.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            camino.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            camino.CloseFigure();
            return camino;
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

        /// <summary>Rol de cada elemento, para mostrar en las cartas y en la galería.</summary>
        public static string RolDe(Elemento elemento)
        {
            switch (elemento)
            {
                case Elemento.Fuego: return "ATACANTE";
                case Elemento.Agua: return "APOYO";
                case Elemento.Tierra: return "TANQUE";
                default: return "VELOZ";
            }
        }

        /// <summary>
        /// Pinta el fondo moderno de las pantallas: degradado azul noche, un resplandor verde azulado,
        /// una cuadrícula de hexágonos muy suave y los bordes más oscuros (viñeta).
        /// </summary>
        public static void PintarFondo(Graphics g, Rectangle r)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (LinearGradientBrush fondo = new LinearGradientBrush(r, Color.FromArgb(6, 10, 22), Color.FromArgb(15, 32, 50), 90f))
            {
                g.FillRectangle(fondo, r);
            }

            using (GraphicsPath luz = new GraphicsPath())
            {
                luz.AddEllipse(r.X - r.Width * 0.15f, r.Y - r.Height * 0.35f, r.Width * 1.3f, r.Height * 1.7f);
                using (PathGradientBrush brillo = new PathGradientBrush(luz))
                {
                    brillo.CenterColor = Color.FromArgb(75, 40, 150, 170);
                    brillo.SurroundColors = new Color[] { Color.FromArgb(0, 40, 150, 170) };
                    g.FillPath(brillo, luz);
                }
            }

            // Cuadrícula de hexágonos.
            float radio = 30f;
            float ancho = (float)(Math.Sqrt(3) * radio);
            using (Pen lapiz = new Pen(Color.FromArgb(20, 120, 210, 230), 1.5f))
            {
                for (int fila = -1; fila < r.Height / (radio * 1.5f) + 2; fila++)
                {
                    for (int col = -1; col < r.Width / ancho + 2; col++)
                    {
                        float cx = r.X + col * ancho + (fila % 2 == 0 ? 0 : ancho / 2);
                        float cy = r.Y + fila * radio * 1.5f;
                        PointF[] puntos = new PointF[6];
                        for (int i = 0; i < 6; i++)
                        {
                            double angulo = Math.PI / 180 * (60 * i - 30);
                            puntos[i] = new PointF(cx + (float)(radio * Math.Cos(angulo)), cy + (float)(radio * Math.Sin(angulo)));
                        }
                        g.DrawPolygon(lapiz, puntos);
                    }
                }
            }

            // Viñeta: los bordes más oscuros.
            using (GraphicsPath marco = new GraphicsPath())
            {
                marco.AddEllipse(r.X - r.Width * 0.25f, r.Y - r.Height * 0.55f, r.Width * 1.5f, r.Height * 2.1f);
                using (PathGradientBrush vineta = new PathGradientBrush(marco))
                {
                    vineta.CenterColor = Color.FromArgb(0, 0, 0, 0);
                    vineta.SurroundColors = new Color[] { Color.FromArgb(190, 0, 0, 0) };
                    vineta.FocusScales = new PointF(0.62f, 0.62f);
                    g.FillRectangle(vineta, r);
                }
            }
        }

        /// <summary>Una línea con el nombre, el costo y lo que hace una habilidad.</summary>
        public static string DescripcionHabilidad(Habilidad h)
        {
            return h.Tipo + " · " + h.Nombre + " (" + h.Costo + " ENG, recarga " + h.Cooldown + "): " + h.Descripcion;
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
            sb.AppendLine(DescripcionHabilidad(p.HabilidadAtaque));
            sb.Append(DescripcionHabilidad(p.HabilidadUtilidad));
            return sb.ToString();
        }
    }
}
