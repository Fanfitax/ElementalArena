using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ArenaElemental.Modelos;

namespace ArenaElemental.Formularios
{
    // Colores, textos y fábricas de controles compartidos por todos los formularios.
    public static class Estilo
    {
        public static Color Fondo = Color.FromArgb(24, 26, 40);
        public static Color Panel = Color.FromArgb(38, 41, 62);
        public static Color PanelClaro = Color.FromArgb(58, 62, 92);
        public static Color Texto = Color.WhiteSmoke;
        public static Color TextoSuave = Color.FromArgb(165, 170, 200);
        public static Color Acento = Color.FromArgb(96, 120, 220);

        // El triángulo elemental escrito en el idioma elegido.
        public static string Triangulo
        {
            get { return Idioma.Texto("Agua > Fuego > Aire > Tierra > Agua", "Water > Fire > Air > Earth > Water"); }
        }

        // Busca el archivo dentro de la carpeta "Imagenes": junto al .exe, en la carpeta actual
        // o en la carpeta del proyecto (cuando se ejecuta desde bin\Debug). Devuelve null si no existe.
        public static string BuscarImagen(string archivo)
        {
            return BuscarEnCarpeta("Imagenes", archivo);
        }

        // Igual que BuscarImagen pero para cualquier carpeta del juego (por ejemplo "Sonidos").
        // Devuelve la ruta completa del archivo o null si no existe.
        public static string BuscarEnCarpeta(string nombreCarpeta, string archivo)
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
                string ruta = Path.Combine(Path.Combine(carpeta, nombreCarpeta), archivo);
                if (File.Exists(ruta)) return ruta;
            }
            return null;
        }

        // Carga una imagen de la carpeta "Imagenes". Si no existe o está dañada devuelve null
        // (el programa nunca se cae por una imagen que falta).
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

        // Versión oscura del color del elemento, para fondos de filas.
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

        // Botón de diseño plano y moderno: sin borde, color sólido, esquinas redondeadas, letra en negrita
        // y un tono más claro al pasar el mouse. Se debe llamar después de darle su tamaño al botón.
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

        // Le da esquinas redondeadas a cualquier control recortando su región.
        public static void Redondear(Control control, int radio)
        {
            if (control.Width <= 0 || control.Height <= 0) return;
            using (GraphicsPath camino = CaminoRedondeado(new Rectangle(0, 0, control.Width, control.Height), radio))
            {
                control.Region = new Region(camino);
            }
        }

        // Rectángulo con las esquinas redondeadas (para recortar o dibujar bordes).
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
            FontStyle estilo = FontStyle.Regular;
            if (negrita) estilo = FontStyle.Bold;
            l.Font = new Font("Segoe UI", tamano, estilo);
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

        // Rol de cada elemento, para mostrar en las cartas y en la galería.
        public static string RolDe(Elemento elemento)
        {
            switch (elemento)
            {
                case Elemento.Fuego: return Idioma.Texto("ATACANTE", "ATTACKER");
                case Elemento.Agua: return Idioma.Texto("APOYO", "SUPPORT");
                case Elemento.Tierra: return Idioma.Texto("TANQUE", "TANK");
                default: return Idioma.Texto("VELOZ", "FAST");
            }
        }

        // Pinta el fondo moderno de las pantallas: degradado azul noche, un resplandor verde azulado,
        // una cuadrícula de hexágonos muy suave y los bordes más oscuros (viñeta).
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
                        float cx = r.X + col * ancho;
                        if (fila % 2 != 0) cx = cx + ancho / 2;   // las filas impares van corridas media celda
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

        // Da a una tabla el aspecto oscuro del juego (la usan las estadísticas y el ranking).
        public static void EstilizarTabla(DataGridView dgv)
        {
            dgv.AutoGenerateColumns = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.BorderStyle = BorderStyle.None;
            dgv.BackgroundColor = Color.FromArgb(10, 12, 18);
            dgv.GridColor = Color.FromArgb(36, 42, 60);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersHeight = 36;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.RowTemplate.Height = 30;
            dgv.Font = new Font("Segoe UI", 10f);
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 46);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gold;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(24, 30, 46);
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(10, 12, 18);
            dgv.DefaultCellStyle.ForeColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 56, 96);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(16, 20, 30);
        }

        // Agrega una columna de texto a la tabla. "peso" es su ancho relativo y "formato" es opcional (fechas, números).
        public static void AgregarColumna(DataGridView dgv, string propiedad, string titulo, int peso, string formato)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.Name = propiedad;
            col.DataPropertyName = propiedad;
            col.HeaderText = titulo;
            col.FillWeight = peso;
            if (formato != null) col.DefaultCellStyle.Format = formato;
            dgv.Columns.Add(col);
        }

        // Una línea con el nombre, el costo y lo que hace una habilidad.
        public static string DescripcionHabilidad(Habilidad h)
        {
            return h.Tipo + " · " + h.Nombre + " (" + h.Costo + Idioma.Texto(" ENG, recarga ", " ENG, cooldown ") + h.Cooldown + "): " + h.Descripcion;
        }

        // Texto "Fuerte contra X · Débil contra Y" según el triángulo elemental.
        public static string DescripcionTriangulo(Personaje p)
        {
            string fuerte = string.Empty, debil = string.Empty;
            foreach (Elemento e in Enum.GetValues(typeof(Elemento)))
            {
                double m = p.ObtenerMultiplicador(e);
                if (m > Personaje.MultiplicadorNeutro) fuerte = Idioma.NombreElemento(e);
                else if (m < Personaje.MultiplicadorNeutro) debil = Idioma.NombreElemento(e);
            }
            return Idioma.Texto("Fuerte contra ", "Strong against ") + fuerte + Idioma.Texto("  ·  Débil contra ", "  ·  Weak against ") + debil;
        }

        public static string DescripcionCompleta(Personaje p)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(p.Nombre + "  [" + Idioma.NombreElemento(p.Elemento) + "]");
            sb.AppendLine("HP " + p.HpMax + "   ATK " + p.Atk + "   DEF " + p.Defensa + "   ENG " + p.EnergiaMax + "   EMP " + p.Empatia);
            sb.AppendLine(DescripcionTriangulo(p));
            sb.AppendLine(DescripcionHabilidad(p.HabilidadAtaque));
            sb.Append(DescripcionHabilidad(p.HabilidadUtilidad));
            return sb.ToString();
        }
    }
}
