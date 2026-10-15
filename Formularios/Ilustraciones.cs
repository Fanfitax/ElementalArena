using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using ArenaElemental.Modelos;

namespace ArenaElemental.Formularios
{
    /// <summary>
    /// Prepara las ilustraciones de los personajes para las cartas.
    /// Las imágenes de la carpeta Imagenes son personajes de cuerpo completo sobre fondo blanco, así que
    /// aquí se les quita el fondo, se recortan al personaje y se pintan sobre un fondo del color de su elemento.
    /// El nombre del archivo es el nombre del personaje en minúsculas ("pyra.jpg"); sirve .png, .jpg o .jpeg
    /// y también con tildes ("gólem.jpg") o sin ellas ("golem.jpg").
    /// </summary>
    public static class Ilustraciones
    {
        private static readonly string[] Extensiones = new string[] { ".png", ".jpg", ".jpeg" };

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
            arte = recorte == null ? null : Componer(recorte, p.Elemento, ancho, alto, ancho >= 200 ? 10 : 7);
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
            imagen = null;
            if (recorte != null)
            {
                double escala = Math.Min((double)anchoMaximo / recorte.Width, (double)altoMaximo / recorte.Height);
                int ancho = Math.Max(1, (int)(recorte.Width * escala));
                int alto = Math.Max(1, (int)(recorte.Height * escala));
                Bitmap reducido = new Bitmap(ancho, alto, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(reducido))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(recorte, 0, 0, ancho, alto);
                }
                imagen = reducido;
            }
            compuestas[clave] = imagen;
            return imagen;
        }

        // ---------- Búsqueda y carga ----------

        private static string NombreArchivo(string nombrePersonaje)
        {
            return nombrePersonaje.Trim().ToLower().Replace(' ', '_');
        }

        private static Bitmap ObtenerRecorte(string nombrePersonaje)
        {
            string nombre = NombreArchivo(nombrePersonaje);
            Bitmap recorte;
            if (recortes.TryGetValue(nombre, out recorte))
                return recorte;

            // Prueba "gólem" y luego "golem", cada uno con cada extensión.
            string[] nombres = new string[] { nombre, QuitarTildes(nombre) };
            Image origen = null;
            foreach (string n in nombres)
            {
                foreach (string ext in Extensiones)
                {
                    origen = Estilo.CargarImagen(n + ext);
                    if (origen != null) break;
                }
                if (origen != null) break;
            }

            recorte = null;
            if (origen != null)
            {
                try
                {
                    recorte = QuitarFondoYRecortar((Bitmap)origen);
                }
                catch (Exception)
                {
                    recorte = null;   // si algo sale mal con la imagen, la carta usa el color sólido
                }
                origen.Dispose();
            }
            recortes[nombre] = recorte;
            return recorte;
        }

        private static string QuitarTildes(string texto)
        {
            string descompuesto = texto.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char letra in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letra) != UnicodeCategory.NonSpacingMark)
                    sb.Append(letra);
            }
            return sb.ToString();
        }

        // ---------- Quitar el fondo blanco ----------

        /// <summary>
        /// Convierte en transparente el fondo blanco que toca los bordes de la imagen (se rellena desde los bordes,
        /// así los blancos de adentro del personaje, como los ojos, no se tocan) y recorta al personaje.
        /// </summary>
        private static Bitmap QuitarFondoYRecortar(Bitmap origen)
        {
            int w = origen.Width;
            int h = origen.Height;

            Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(origen, new Rectangle(0, 0, w, h));
            }

            // Se copian los píxeles a un arreglo de bytes: cada píxel son 4 bytes (azul, verde, rojo, transparencia).
            BitmapData datos = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = datos.Stride;
            byte[] px = new byte[stride * h];
            Marshal.Copy(datos.Scan0, px, 0, px.Length);

            // Relleno desde los bordes: todo lo blanco conectado con el borde es fondo.
            bool[] esFondo = new bool[w * h];
            int[] pila = new int[w * h];
            int tope = 0;
            for (int x = 0; x < w; x++)
            {
                Empujar(x, w, stride, px, esFondo, pila, ref tope);
                Empujar((h - 1) * w + x, w, stride, px, esFondo, pila, ref tope);
            }
            for (int y = 0; y < h; y++)
            {
                Empujar(y * w, w, stride, px, esFondo, pila, ref tope);
                Empujar(y * w + w - 1, w, stride, px, esFondo, pila, ref tope);
            }
            while (tope > 0)
            {
                int i = pila[--tope];
                int x = i % w;
                int y = i / w;
                if (x > 0) Empujar(i - 1, w, stride, px, esFondo, pila, ref tope);
                if (x < w - 1) Empujar(i + 1, w, stride, px, esFondo, pila, ref tope);
                if (y > 0) Empujar(i - w, w, stride, px, esFondo, pila, ref tope);
                if (y < h - 1) Empujar(i + w, w, stride, px, esFondo, pila, ref tope);
            }

            // Zonas blancas grandes que quedaron encerradas dentro del personaje (por ejemplo entre las piernas)
            // también son fondo. Las zonas blancas chicas, como los ojos, se respetan.
            bool[] visitado = new bool[w * h];
            int[] lista = new int[w * h];
            int umbral = w * h / 600;
            for (int inicio = 0; inicio < w * h; inicio++)
            {
                if (visitado[inicio] || esFondo[inicio]) continue;
                if (!EsBlancoPuro(px, (inicio / w) * stride + (inicio % w) * 4)) continue;

                int cantidad = 0;
                tope = 0;
                pila[tope++] = inicio;
                visitado[inicio] = true;
                while (tope > 0)
                {
                    int i = pila[--tope];
                    lista[cantidad++] = i;
                    int x = i % w;
                    int y = i / w;
                    if (x > 0) EmpujarBlancoPuro(i - 1, w, stride, px, esFondo, visitado, pila, ref tope);
                    if (x < w - 1) EmpujarBlancoPuro(i + 1, w, stride, px, esFondo, visitado, pila, ref tope);
                    if (y > 0) EmpujarBlancoPuro(i - w, w, stride, px, esFondo, visitado, pila, ref tope);
                    if (y < h - 1) EmpujarBlancoPuro(i + w, w, stride, px, esFondo, visitado, pila, ref tope);
                }
                if (cantidad >= umbral)
                {
                    for (int k = 0; k < cantidad; k++) esFondo[lista[k]] = true;
                }
            }

            // Se hace transparente el fondo y se suaviza el borde del personaje.
            int minX = w, minY = h, maxX = -1, maxY = -1;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int k = y * w + x;
                    int p = y * stride + x * 4;
                    if (esFondo[k])
                    {
                        px[p] = 40; px[p + 1] = 30; px[p + 2] = 30; px[p + 3] = 0;
                        continue;
                    }

                    bool tocaFondo = (x > 0 && esFondo[k - 1]) || (x < w - 1 && esFondo[k + 1])
                                     || (y > 0 && esFondo[k - w]) || (y < h - 1 && esFondo[k + w]);
                    if (tocaFondo)
                    {
                        int claro = Math.Min(px[p], Math.Min(px[p + 1], px[p + 2]));
                        if (claro > 150)
                            px[p + 3] = (byte)Math.Max(0, Math.Min(255, (240 - claro) * 255 / 90));
                    }

                    if (px[p + 3] > 40)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            Marshal.Copy(px, 0, datos.Scan0, px.Length);
            bmp.UnlockBits(datos);

            if (maxX < 0) return bmp;   // no se encontró nada: se deja la imagen tal cual

            int margen = 4;
            int izq = Math.Max(0, minX - margen);
            int arr = Math.Max(0, minY - margen);
            int der = Math.Min(w - 1, maxX + margen);
            int aba = Math.Min(h - 1, maxY + margen);
            Bitmap recorte = bmp.Clone(new Rectangle(izq, arr, der - izq + 1, aba - arr + 1), PixelFormat.Format32bppArgb);
            bmp.Dispose();
            return recorte;
        }

        /// <summary>Agrega el píxel j a la pila si es blanco y todavía no se marcó como fondo.</summary>
        private static void Empujar(int j, int w, int stride, byte[] px, bool[] esFondo, int[] pila, ref int tope)
        {
            if (esFondo[j]) return;
            int p = (j / w) * stride + (j % w) * 4;
            bool transparente = px[p + 3] < 20;
            bool blanco = Math.Min(px[p], Math.Min(px[p + 1], px[p + 2])) >= 232;
            if (!transparente && !blanco) return;
            esFondo[j] = true;
            pila[tope++] = j;
        }

        private static bool EsBlancoPuro(byte[] px, int p)
        {
            return px[p + 3] >= 20 && Math.Min(px[p], Math.Min(px[p + 1], px[p + 2])) >= 246;
        }

        private static void EmpujarBlancoPuro(int j, int w, int stride, byte[] px, bool[] esFondo, bool[] visitado, int[] pila, ref int tope)
        {
            if (visitado[j] || esFondo[j]) return;
            if (!EsBlancoPuro(px, (j / w) * stride + (j % w) * 4)) return;
            visitado[j] = true;
            pila[tope++] = j;
        }

        // ---------- Armar la ilustración de la carta ----------

        /// <summary>Pinta un fondo con degradado y resplandor del color del elemento y encima al personaje.</summary>
        private static Image Componer(Bitmap personaje, Elemento elemento, int ancho, int alto, int radio)
        {
            Bitmap resultado = new Bitmap(ancho, alto, PixelFormat.Format32bppArgb);
            Color color = Estilo.ColorDe(elemento);
            Rectangle r = new Rectangle(0, 0, ancho, alto);

            using (Graphics g = Graphics.FromImage(resultado))
            using (GraphicsPath marco = Estilo.CaminoRedondeado(r, radio))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SetClip(marco);

                // Fondo: del color oscuro del elemento (arriba) a uno más vivo (abajo).
                using (LinearGradientBrush fondo = new LinearGradientBrush(r, Mezclar(color, Color.Black, 0.78), Mezclar(color, Color.Black, 0.30), 90f))
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
                int pw = (int)(personaje.Width * escala);
                int ph = (int)(personaje.Height * escala);
                int px = (ancho - pw) / 2;
                int py = alto - ph - (int)(alto * 0.03);

                // Sombra en el suelo.
                using (SolidBrush sombra = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                {
                    g.FillEllipse(sombra, ancho / 2 - (int)(pw * 0.38), alto - (int)(alto * 0.075), (int)(pw * 0.76), (int)(alto * 0.06));
                }
                g.DrawImage(personaje, px, py, pw, ph);

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
