using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using ArenaElemental.Modelos;

namespace ArenaElemental.Formularios
{
    /// <summary>
    /// Prepara las ilustraciones de los personajes para las cartas.
    /// Las imágenes de la carpeta Imagenes son personajes de cuerpo completo sobre fondo blanco, así que
    /// aquí se les quita el fondo, se recortan al personaje y se pintan sobre un fondo del color de su elemento.
    /// El nombre del archivo es el nombre del personaje en minúsculas ("pyra.jpg"); sirve .png, .jpg o .jpeg
    /// y también con tildes ("gólem.jpg") o sin ellas ("golem.jpg").
    ///
    /// Para quitar el fondo se usa un relleno por inundación (flood fill) con una Queue de puntos: se parte
    /// de los bordes de la imagen y se avanza por todos los píxeles blancos vecinos. Esos píxeles se pintan
    /// de magenta y se llama a Bitmap.MakeTransparent(Color.Magenta). Para que sea rápido, el trabajo se hace
    /// sobre una copia reducida de la imagen (TamanoTrabajo) y cada píxel se lee una sola vez: GetPixel es lento,
    /// así que primero se anota la "claridad" de cada píxel en una tabla y el relleno consulta esa tabla.
    /// </summary>
    public static class Ilustraciones
    {
        private static readonly string[] Extensiones = new string[] { ".png", ".jpg", ".jpeg" };

        // Lado más largo (en píxeles) de la copia sobre la que se quita el fondo.
        private const int TamanoTrabajo = 340;

        // Un píxel se considera "fondo blanco" si su canal más bajo (R, G o B) llega a este valor.
        private const int BlancoFondo = 232;
        private const int BlancoPuro = 246;
        private const int BordeClaro = 195;   // un píxel así de claro pegado al fondo se considera parte del contorno blanco

        // Personaje ya recortado (sin fondo). Si no hay imagen se guarda null para no volver a buscarla.
        private static Dictionary<string, Bitmap> recortes = new Dictionary<string, Bitmap>();

        // Arte ya compuesto para cada tamaño (clave = nombre + tamaño).
        private static Dictionary<string, Image> compuestas = new Dictionary<string, Image>();

        /// <summary>
        /// Devuelve la ilustración del personaje lista para mostrar en un PictureBox del tamaño indicado,
        /// o null si no hay imagen (en ese caso la carta queda con un color sólido).
        /// </summary>
        public static Image ObtenerArte(Personaje p, int ancho, int alto)
        {
            string clave = NombreArchivo(p.Nombre) + "|" + ancho + "x" + alto;
            Image arte;
            if (compuestas.TryGetValue(clave, out arte))
                return arte;

            Bitmap recorte = ObtenerRecorte(p.Nombre);
            if (recorte == null)
                arte = null;
            else
                arte = Componer(recorte, p.Elemento, ancho, alto);
            compuestas[clave] = arte;
            return arte;
        }

        /// <summary>
        /// El personaje sin fondo (transparente), reducido para que quepa en el tamaño indicado sin deformarse.
        /// Sirve para mostrarlo suelto sobre cualquier fondo (por ejemplo en el menú). Devuelve null si no hay imagen.
        /// </summary>
        public static Image ObtenerPersonajeEncajado(string nombrePersonaje, int anchoMaximo, int altoMaximo)
        {
            string clave = NombreArchivo(nombrePersonaje) + "|encajado|" + anchoMaximo + "x" + altoMaximo;
            Image imagen;
            if (compuestas.TryGetValue(clave, out imagen))
                return imagen;

            Bitmap recorte = ObtenerRecorte(nombrePersonaje);
            if (recorte == null)
            {
                imagen = null;
            }
            else
            {
                double escala = Math.Min((double)anchoMaximo / recorte.Width, (double)altoMaximo / recorte.Height);
                imagen = Reducir(recorte, (int)(recorte.Width * escala), (int)(recorte.Height * escala));
            }
            compuestas[clave] = imagen;
            return imagen;
        }

        // ---------- Búsqueda y carga ----------

        private static string NombreArchivo(string nombrePersonaje)
        {
            return nombrePersonaje.Trim().ToLower().Replace(' ', '_');
        }

        private static string QuitarTildes(string texto)
        {
            return texto.Replace('á', 'a').Replace('é', 'e').Replace('í', 'i')
                        .Replace('ó', 'o').Replace('ú', 'u').Replace('ñ', 'n');
        }

        /// <summary>Busca el archivo del personaje (con o sin tildes y con cada extensión) y lo carga. Devuelve null si no existe.</summary>
        private static Image CargarOriginal(string nombre)
        {
            string[] nombres = new string[] { nombre, QuitarTildes(nombre) };
            foreach (string n in nombres)
            {
                foreach (string extension in Extensiones)
                {
                    Image imagen = Estilo.CargarImagen(n + extension);
                    if (imagen != null) return imagen;
                }
            }
            return null;
        }

        private static Bitmap ObtenerRecorte(string nombrePersonaje)
        {
            string nombre = NombreArchivo(nombrePersonaje);
            Bitmap recorte;
            if (recortes.TryGetValue(nombre, out recorte))
                return recorte;

            recorte = null;
            Image original = CargarOriginal(nombre);
            if (original != null)
            {
                try
                {
                    recorte = QuitarFondoYRecortar(original);
                }
                catch (Exception)
                {
                    recorte = null;   // si algo sale mal con la imagen, la carta usa el color sólido
                }
                original.Dispose();
            }
            recortes[nombre] = recorte;
            return recorte;
        }

        // ---------- Quitar el fondo blanco ----------

        /// <summary>Copia la imagen en un Bitmap nuevo del tamaño indicado (dibujo directo con Graphics).</summary>
        private static Bitmap Reducir(Image origen, int ancho, int alto)
        {
            Bitmap copia = new Bitmap(Math.Max(1, ancho), Math.Max(1, alto));
            using (Graphics g = Graphics.FromImage(copia))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(origen, 0, 0, copia.Width, copia.Height);
            }
            return copia;
        }

        /// <summary>
        /// Quita el fondo blanco y recorta al personaje:
        ///  1) se reduce la imagen,
        ///  2) se marca como fondo todo el blanco conectado con los bordes,
        ///  3) se marcan también las zonas blancas grandes encerradas (entre las piernas, por ejemplo);
        ///     las chicas, como los ojos o la barba, se respetan,
        ///  4) lo marcado se pinta de magenta y se vuelve transparente con MakeTransparent,
        ///  5) se recorta alrededor de lo que quedó.
        /// </summary>
        private static Bitmap QuitarFondoYRecortar(Image origen)
        {
            double escala = Math.Min(1.0, (double)TamanoTrabajo / Math.Max(origen.Width, origen.Height));
            Bitmap imagen = Reducir(origen, (int)(origen.Width * escala), (int)(origen.Height * escala));
            int ancho = imagen.Width;
            int alto = imagen.Height;

            int[,] claridad = MedirClaridad(imagen);
            bool[,] esFondo = new bool[ancho, alto];
            bool[,] visitado = new bool[ancho, alto];

            MarcarFondoDeLosBordes(claridad, esFondo, visitado);
            MarcarZonasBlancasGrandes(claridad, esFondo, visitado);
            QuitarBordeClaro(claridad, esFondo);

            // Lo que es fondo se pinta de magenta y después se hace transparente de una sola vez.
            PintarFondoDeMagenta(imagen, esFondo);
            imagen.MakeTransparent(Color.Magenta);
            SuavizarBorde(imagen, claridad, esFondo);

            return Recortar(imagen, esFondo);
        }

        /// <summary>
        /// Anota la claridad de cada píxel: el menor de sus tres canales de color (rojo, verde y azul).
        /// Un píxel es blanco cuando los tres canales son altos, o sea, cuando ese menor valor es alto.
        /// </summary>
        private static int[,] MedirClaridad(Bitmap imagen)
        {
            int[,] claridad = new int[imagen.Width, imagen.Height];
            for (int y = 0; y < imagen.Height; y++)
            {
                for (int x = 0; x < imagen.Width; x++)
                {
                    Color c = imagen.GetPixel(x, y);
                    claridad[x, y] = Math.Min(c.R, Math.Min(c.G, c.B));
                }
            }
            return claridad;
        }

        /// <summary>
        /// Al borde del personaje le queda un contorno de píxeles casi blancos (por la compresión del JPG).
        /// Se hacen dos pasadas: en cada una, el píxel claro que toca el fondo pasa a ser fondo también.
        /// </summary>
        private static void QuitarBordeClaro(int[,] claridad, bool[,] esFondo)
        {
            int ancho = claridad.GetLength(0);
            int alto = claridad.GetLength(1);
            for (int pasada = 0; pasada < 2; pasada++)
            {
                List<Point> porQuitar = new List<Point>();
                for (int y = 1; y < alto - 1; y++)
                {
                    for (int x = 1; x < ancho - 1; x++)
                    {
                        if (esFondo[x, y] || claridad[x, y] < BordeClaro) continue;
                        bool tocaFondo = esFondo[x - 1, y] || esFondo[x + 1, y] || esFondo[x, y - 1] || esFondo[x, y + 1];
                        if (tocaFondo) porQuitar.Add(new Point(x, y));
                    }
                }
                foreach (Point p in porQuitar)
                    esFondo[p.X, p.Y] = true;
            }
        }

        /// <summary>
        /// Los píxeles del contorno que todavía son algo claros se hacen semitransparentes: cuanto más claros,
        /// más transparentes. Así el borde se funde con cualquier fondo en vez de verse un halo blanco.
        /// </summary>
        private static void SuavizarBorde(Bitmap imagen, int[,] claridad, bool[,] esFondo)
        {
            for (int y = 1; y < imagen.Height - 1; y++)
            {
                for (int x = 1; x < imagen.Width - 1; x++)
                {
                    if (esFondo[x, y] || claridad[x, y] < 120) continue;
                    bool tocaFondo = esFondo[x - 1, y] || esFondo[x + 1, y] || esFondo[x, y - 1] || esFondo[x, y + 1];
                    if (!tocaFondo) continue;

                    int opacidad = (235 - claridad[x, y]) * 255 / 115;
                    opacidad = Math.Max(40, Math.Min(255, opacidad));
                    Color c = imagen.GetPixel(x, y);
                    imagen.SetPixel(x, y, Color.FromArgb(opacidad, c.R, c.G, c.B));
                }
            }
        }

        /// <summary>
        /// Pinta de magenta los píxeles marcados como fondo. Se pintan por tramos horizontales con Graphics
        /// (mucho más rápido que llamar a SetPixel en cada píxel).
        /// </summary>
        private static void PintarFondoDeMagenta(Bitmap imagen, bool[,] esFondo)
        {
            using (Graphics g = Graphics.FromImage(imagen))
            {
                for (int y = 0; y < imagen.Height; y++)
                {
                    int inicioTramo = -1;
                    for (int x = 0; x <= imagen.Width; x++)
                    {
                        bool fondo = (x < imagen.Width) && esFondo[x, y];
                        if (fondo && inicioTramo < 0)
                        {
                            inicioTramo = x;
                        }
                        else if (!fondo && inicioTramo >= 0)
                        {
                            g.FillRectangle(Brushes.Magenta, inicioTramo, y, x - inicioTramo, 1);
                            inicioTramo = -1;
                        }
                    }
                }
            }
        }

        /// <summary>Todo el blanco que toca el borde de la imagen es fondo.</summary>
        private static void MarcarFondoDeLosBordes(int[,] claridad, bool[,] esFondo, bool[,] visitado)
        {
            int ancho = claridad.GetLength(0);
            int alto = claridad.GetLength(1);
            for (int x = 0; x < ancho; x++)
            {
                MarcarComoFondoSiEsBlanco(claridad, esFondo, visitado, x, 0);
                MarcarComoFondoSiEsBlanco(claridad, esFondo, visitado, x, alto - 1);
            }
            for (int y = 0; y < alto; y++)
            {
                MarcarComoFondoSiEsBlanco(claridad, esFondo, visitado, 0, y);
                MarcarComoFondoSiEsBlanco(claridad, esFondo, visitado, ancho - 1, y);
            }
        }

        private static void MarcarComoFondoSiEsBlanco(int[,] claridad, bool[,] esFondo, bool[,] visitado, int x, int y)
        {
            if (visitado[x, y] || claridad[x, y] < BlancoFondo) return;

            List<Point> zona = BuscarZonaBlanca(claridad, visitado, x, y, BlancoFondo);
            foreach (Point p in zona)
                esFondo[p.X, p.Y] = true;
        }

        /// <summary>Las zonas de blanco puro encerradas dentro del personaje: si son grandes, también son fondo.</summary>
        private static void MarcarZonasBlancasGrandes(int[,] claridad, bool[,] esFondo, bool[,] visitado)
        {
            int ancho = claridad.GetLength(0);
            int alto = claridad.GetLength(1);
            int minimoPixeles = ancho * alto / 600;
            for (int y = 0; y < alto; y++)
            {
                for (int x = 0; x < ancho; x++)
                {
                    if (visitado[x, y] || claridad[x, y] < BlancoPuro) continue;

                    List<Point> zona = BuscarZonaBlanca(claridad, visitado, x, y, BlancoPuro);
                    if (zona.Count < minimoPixeles) continue;   // una zona chica (un ojo, la barba) se conserva
                    foreach (Point p in zona)
                        esFondo[p.X, p.Y] = true;
                }
            }
        }

        /// <summary>
        /// Relleno por inundación: parte de un píxel blanco y, con una cola (Queue), visita a sus vecinos de arriba,
        /// abajo, izquierda y derecha que también sean blancos. Devuelve todos los píxeles de esa zona.
        /// </summary>
        private static List<Point> BuscarZonaBlanca(int[,] claridad, bool[,] visitado, int inicioX, int inicioY, int minimoBlanco)
        {
            List<Point> zona = new List<Point>();
            Queue<Point> pendientes = new Queue<Point>();
            pendientes.Enqueue(new Point(inicioX, inicioY));
            visitado[inicioX, inicioY] = true;

            int ancho = claridad.GetLength(0);
            int alto = claridad.GetLength(1);
            int[] difX = new int[] { 1, -1, 0, 0 };
            int[] difY = new int[] { 0, 0, 1, -1 };

            while (pendientes.Count > 0)
            {
                Point actual = pendientes.Dequeue();
                zona.Add(actual);

                for (int k = 0; k < 4; k++)
                {
                    int nx = actual.X + difX[k];
                    int ny = actual.Y + difY[k];
                    if (nx < 0 || ny < 0 || nx >= ancho || ny >= alto) continue;
                    if (visitado[nx, ny]) continue;
                    if (claridad[nx, ny] < minimoBlanco) continue;

                    visitado[nx, ny] = true;
                    pendientes.Enqueue(new Point(nx, ny));
                }
            }
            return zona;
        }


        /// <summary>Recorta la imagen al rectángulo que contiene al personaje (los píxeles que no son fondo).</summary>
        private static Bitmap Recortar(Bitmap imagen, bool[,] esFondo)
        {
            int minX = imagen.Width, minY = imagen.Height, maxX = -1, maxY = -1;
            for (int y = 0; y < imagen.Height; y++)
            {
                for (int x = 0; x < imagen.Width; x++)
                {
                    if (esFondo[x, y]) continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
            if (maxX < 0) return imagen;   // no se encontró nada: se deja la imagen tal cual

            int margen = 3;
            int izquierda = Math.Max(0, minX - margen);
            int arriba = Math.Max(0, minY - margen);
            int derecha = Math.Min(imagen.Width - 1, maxX + margen);
            int abajo = Math.Min(imagen.Height - 1, maxY + margen);

            Rectangle zona = new Rectangle(izquierda, arriba, derecha - izquierda + 1, abajo - arriba + 1);
            Bitmap recorte = new Bitmap(zona.Width, zona.Height);
            using (Graphics g = Graphics.FromImage(recorte))
            {
                g.DrawImage(imagen, new Rectangle(0, 0, zona.Width, zona.Height), zona, GraphicsUnit.Pixel);
            }
            imagen.Dispose();
            return recorte;
        }

        // ---------- Armar la ilustración de la carta ----------

        /// <summary>Pinta un fondo con degradado y resplandor del color del elemento y encima al personaje.</summary>
        private static Image Componer(Bitmap personaje, Elemento elemento, int ancho, int alto)
        {
            int radio = 7;
            if (ancho >= 200) radio = 10;

            Bitmap resultado = new Bitmap(ancho, alto);
            Color color = Estilo.ColorDe(elemento);
            Rectangle r = new Rectangle(0, 0, ancho, alto);

            using (Graphics g = Graphics.FromImage(resultado))
            using (GraphicsPath marco = Estilo.CaminoRedondeado(r, radio))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SetClip(marco);

                // Fondo: del color oscuro del elemento (arriba) a uno más vivo (abajo).
                Color arriba = Mezclar(color, Color.Black, 0.78);
                Color abajo = Mezclar(color, Color.Black, 0.30);
                using (LinearGradientBrush fondo = new LinearGradientBrush(r, arriba, abajo, 90f))
                {
                    g.FillRectangle(fondo, r);
                }

                // Resplandor detrás del personaje.
                using (GraphicsPath luz = new GraphicsPath())
                {
                    luz.AddEllipse(ancho * 0.04f, alto * 0.08f, ancho * 0.92f, alto * 0.90f);
                    using (PathGradientBrush brillo = new PathGradientBrush(luz))
                    {
                        brillo.CenterColor = Color.FromArgb(170, Mezclar(color, Color.White, 0.40));
                        brillo.SurroundColors = new Color[] { Color.FromArgb(0, color) };
                        g.FillPath(brillo, luz);
                    }
                }

                // El personaje, ajustado al tamaño sin deformarse y apoyado abajo.
                double escala = Math.Min(ancho * 0.94 / personaje.Width, alto * 0.93 / personaje.Height);
                int anchoPersonaje = (int)(personaje.Width * escala);
                int altoPersonaje = (int)(personaje.Height * escala);
                int x = (ancho - anchoPersonaje) / 2;
                int y = alto - altoPersonaje - (int)(alto * 0.03);

                // Sombra en el suelo.
                using (SolidBrush sombra = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                {
                    g.FillEllipse(sombra, ancho / 2 - (int)(anchoPersonaje * 0.38), alto - (int)(alto * 0.075),
                                  (int)(anchoPersonaje * 0.76), (int)(alto * 0.06));
                }
                g.DrawImage(personaje, x, y, anchoPersonaje, altoPersonaje);

                // Un leve brillo arriba, como el barniz de una carta.
                Rectangle superior = new Rectangle(0, 0, ancho, alto / 3);
                using (LinearGradientBrush barniz = new LinearGradientBrush(superior, Color.FromArgb(46, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f))
                {
                    g.FillRectangle(barniz, superior);
                }
            }
            return resultado;
        }

        private static Color Mezclar(Color a, Color b, double t)
        {
            return Color.FromArgb((int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
        }
    }
}
