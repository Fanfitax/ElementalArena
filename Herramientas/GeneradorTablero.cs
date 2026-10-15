using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

/// <summary>
/// HERRAMIENTA (no forma parte del juego ni se compila con el proyecto).
/// Dibuja con GDI+ el fondo del combate: Imagenes/tablero.jpg (1264 x 500, el tamaño de la ventana de combate).
/// Las posiciones coinciden con las del formulario FrmCombate: carta del jugador en x=144, carta del rival en x=592,
/// columna central de botones en x=384..584. Se compila aparte:  csc GeneradorTablero.cs  y se ejecuta con la carpeta de salida.
/// </summary>
class GeneradorTablero
{
    const int Ancho = 1264;
    const int Alto = 500;

    static void Main(string[] args)
    {
        string carpeta = args.Length > 0 ? args[0] : "Imagenes";
        Directory.CreateDirectory(carpeta);

        using (Bitmap bmp = new Bitmap(Ancho, Alto))
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle todo = new Rectangle(0, 0, Ancho, Alto);

            // Fondo: azul noche con un resplandor verde azulado en el centro.
            using (LinearGradientBrush fondo = new LinearGradientBrush(todo, Color.FromArgb(7, 12, 24), Color.FromArgb(14, 30, 46), 90f))
                g.FillRectangle(fondo, todo);
            using (GraphicsPath luz = new GraphicsPath())
            {
                luz.AddEllipse(-100, -140, Ancho + 200, Alto + 280);
                using (PathGradientBrush brillo = new PathGradientBrush(luz))
                {
                    brillo.CenterColor = Color.FromArgb(70, 40, 150, 160);
                    brillo.SurroundColors = new Color[] { Color.FromArgb(0, 40, 150, 160) };
                    g.FillPath(brillo, luz);
                }
            }

            DibujarHexagonos(g);

            // Haces de luz desde arriba sobre cada carta.
            HazDeLuz(g, 260, Color.FromArgb(70, 200, 255));
            HazDeLuz(g, 708, Color.FromArgb(255, 90, 110));

            // Plataformas bajo las cartas: azul para el jugador y rojo para el rival.
            Plataforma(g, 260, 484, Color.FromArgb(70, 200, 255));
            Plataforma(g, 708, 484, Color.FromArgb(255, 90, 110));

            // Columna central donde van los botones.
            Rectangle centro = new Rectangle(380, 62, 208, 430);
            using (GraphicsPath camino = Redondeado(centro, 16))
            using (SolidBrush relleno = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
            using (Pen borde = new Pen(Color.FromArgb(70, 120, 200, 255), 2f))
            {
                g.FillPath(relleno, camino);
                g.DrawPath(borde, camino);
            }

            // Estrellitas.
            Random azar = new Random(7);
            for (int i = 0; i < 140; i++)
            {
                int x = azar.Next(Ancho), y = azar.Next(Alto), t = azar.Next(1, 4);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(azar.Next(25, 90), 180, 230, 255)))
                    g.FillEllipse(b, x, y, t, t);
            }

            // Viñeta: los bordes más oscuros.
            using (GraphicsPath marco = new GraphicsPath())
            {
                marco.AddEllipse(-Ancho * 0.25f, -Alto * 0.55f, Ancho * 1.5f, Alto * 2.1f);
                using (PathGradientBrush vineta = new PathGradientBrush(marco))
                {
                    vineta.CenterColor = Color.FromArgb(0, 0, 0, 0);
                    vineta.SurroundColors = new Color[] { Color.FromArgb(190, 0, 0, 0) };
                    vineta.FocusScales = new PointF(0.62f, 0.62f);
                    g.FillRectangle(vineta, todo);
                }
            }

            ImageCodecInfo jpg = ImageCodecInfo.GetImageEncoders().First(e => e.MimeType == "image/jpeg");
            EncoderParameters calidad = new EncoderParameters(1);
            calidad.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 92L);
            bmp.Save(Path.Combine(carpeta, "tablero.jpg"), jpg, calidad);
        }
        Console.WriteLine("tablero.jpg generado en " + carpeta);
    }

    static void DibujarHexagonos(Graphics g)
    {
        float r = 30f;                       // radio del hexágono
        float ancho = (float)(Math.Sqrt(3) * r);
        using (Pen lapiz = new Pen(Color.FromArgb(22, 120, 210, 230), 1.5f))
        {
            for (int fila = -1; fila < Alto / (r * 1.5f) + 2; fila++)
            {
                for (int col = -1; col < Ancho / ancho + 2; col++)
                {
                    float cx = col * ancho + (fila % 2 == 0 ? 0 : ancho / 2);
                    float cy = fila * r * 1.5f;
                    PointF[] puntos = new PointF[6];
                    for (int i = 0; i < 6; i++)
                    {
                        double ang = Math.PI / 180 * (60 * i - 30);
                        puntos[i] = new PointF(cx + (float)(r * Math.Cos(ang)), cy + (float)(r * Math.Sin(ang)));
                    }
                    g.DrawPolygon(lapiz, puntos);
                }
            }
        }
    }

    static void HazDeLuz(Graphics g, int centroX, Color color)
    {
        PointF[] haz = new PointF[]
        {
            new PointF(centroX - 40, 36), new PointF(centroX + 40, 36),
            new PointF(centroX + 150, 490), new PointF(centroX - 150, 490)
        };
        RectangleF caja = new RectangleF(centroX - 150, 36, 300, 454);
        using (LinearGradientBrush pincel = new LinearGradientBrush(caja, Color.FromArgb(46, color), Color.FromArgb(0, color), 90f))
            g.FillPolygon(pincel, haz);
    }

    static void Plataforma(Graphics g, int cx, int cy, Color color)
    {
        Rectangle exterior = new Rectangle(cx - 150, cy - 24, 300, 48);
        using (GraphicsPath camino = new GraphicsPath())
        {
            camino.AddEllipse(exterior);
            using (PathGradientBrush brillo = new PathGradientBrush(camino))
            {
                brillo.CenterColor = Color.FromArgb(120, color);
                brillo.SurroundColors = new Color[] { Color.FromArgb(0, color) };
                g.FillPath(brillo, camino);
            }
        }
        using (Pen aro = new Pen(Color.FromArgb(150, color), 2.5f))
            g.DrawEllipse(aro, cx - 118, cy - 15, 236, 30);
        using (Pen aro2 = new Pen(Color.FromArgb(70, color), 1.5f))
            g.DrawEllipse(aro2, cx - 138, cy - 21, 276, 42);
    }

    static GraphicsPath Redondeado(Rectangle r, int radio)
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
}
