using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Modelos;

namespace ArenaElemental.Formularios
{
    // "Códice Elemental": galería de los 12 personajes con su lore y su lectura de Antropología Filosófica.
    //  - Izquierda: la lista de personajes agrupada por elemento.
    //  - Centro: la carta grande del personaje elegido y, debajo, el dilema que plantea.
    //  - Derecha: un panel oscuro con tres secciones (pestañas): Historia &amp; Lore, Dimensión Antropológica y Ficha Técnica.
    // Los textos salen de la clase LorePersonajes.
    public class FrmCodice : FormularioModerno
    {
        private static Color PanelOscuro = Color.FromArgb(205, 10, 12, 18);
        private static Color FondoTexto = Color.FromArgb(12, 14, 22);

        private const int SeccionHistoria = 0;
        private const int SeccionAntropologia = 1;
        private const int SeccionFicha = 2;

        private List<Personaje> personajes = new List<Personaje>();
        private Personaje seleccionado;
        private int seccionActual = SeccionHistoria;

        private Button[] botonesPersonaje;
        private Button[] botonesSeccion;
        private string[] nombresSeccion = new string[] { Idioma.Texto("HISTORIA & LORE", "HISTORY & LORE"), Idioma.Texto("DIMENSIÓN ANTROPOLÓGICA", "ANTHROPOLOGICAL DIMENSION"), Idioma.Texto("FICHA TÉCNICA", "TECH SHEET") };
        private VistaCarta carta;
        private Label lblDilema, lblNombreLore, lblConcepto;
        private RichTextBox texto;

        // Fuentes del texto (se crean una sola vez y se liberan al cerrar).
        private Font fuenteSeccion = new Font("Segoe UI", 10f, FontStyle.Bold);
        private Font fuenteNormal = new Font("Segoe UI", 11f);
        private Font fuenteSuave = new Font("Segoe UI", 9.5f);
        private Font fuenteCursiva = new Font("Segoe UI", 11.5f, FontStyle.Italic);
        private Font fuenteNegrita = new Font("Segoe UI", 11f, FontStyle.Bold);
        private Font fuentePestana = new Font("Segoe UI", 9f, FontStyle.Bold);

        public FrmCodice(List<Personaje> personajes)
        {
            this.personajes = new List<Personaje>(personajes);

            Estilo.AplicarFormulario(this, Idioma.Texto("Arena Elemental - Códice Elemental", "Arena Elemental - Elemental Codex"), 1120, 640);
            ConstruirEncabezado();
            ConstruirGaleria();
            ConstruirCarta();
            ConstruirPanelTexto();

            if (this.personajes.Count > 0) Seleccionar(this.personajes[0]);
        }

        // ---------- Construcción de la interfaz ----------

        private void ConstruirEncabezado()
        {
            Label titulo = Estilo.CrearEtiqueta(Idioma.Texto("CÓDICE ELEMENTAL", "ELEMENTAL CODEX"), 20, 8, 600, 38, 22f, true);
            titulo.ForeColor = Color.Gold;
            Controls.Add(titulo);

            Label sub = Estilo.CrearEtiqueta(Idioma.Texto("Galería de personajes · Historia y reflexión de Antropología Filosófica", "Character gallery · History and reflection from Philosophical Anthropology"), 22, 46, 800, 20, 9.5f, false);
            sub.ForeColor = Estilo.TextoSuave;
            Controls.Add(sub);

            Button cerrar = new Button();
            cerrar.SetBounds(990, 14, 110, 32);
            Estilo.EstilizarBoton(cerrar, Idioma.Texto("Volver", "Back"), Estilo.PanelClaro);
            cerrar.Click += Volver_Click;
            Controls.Add(cerrar);
            CancelButton = cerrar;
        }

        // La lista de la izquierda: un encabezado por elemento y un botón por personaje.
        private void ConstruirGaleria()
        {
            Elemento[] elementos = new Elemento[] { Elemento.Fuego, Elemento.Agua, Elemento.Tierra, Elemento.Aire };
            botonesPersonaje = new Button[personajes.Count];

            for (int e = 0; e < elementos.Length; e++)
            {
                int y = 76 + e * 134;
                Label encabezado = Estilo.CrearEtiqueta(Idioma.NombreElemento(elementos[e]).ToUpper() + "  ·  " + Estilo.RolDe(elementos[e]),
                                                        20, y, 190, 18, 9f, true);
                encabezado.ForeColor = Estilo.ColorDe(elementos[e]);
                Controls.Add(encabezado);

                int fila = 0;
                for (int i = 0; i < personajes.Count; i++)
                {
                    if (personajes[i].Elemento != elementos[e]) continue;

                    Button b = new Button();
                    b.Tag = i;   // posición del personaje en la lista, para saber cuál botón se presionó
                    b.SetBounds(20, y + 22 + fila * 36, 190, 34);
                    b.Click += Personaje_Click;
                    Controls.Add(b);
                    botonesPersonaje[i] = b;
                    fila++;
                    EstilizarBotonPersonaje(i, false);
                }
            }
        }

        private void ConstruirCarta()
        {
            carta = new VistaCarta(TamanoCarta.Grande);
            carta.Location = new Point(232, 76);
            Controls.Add(carta);

            Panel marco = new Panel();
            marco.SetBounds(232, 504, 232, 108);
            marco.BackColor = PanelOscuro;
            Controls.Add(marco);
            Estilo.Redondear(marco, 12);

            Label etiqueta = Estilo.CrearEtiqueta(Idioma.Texto("EL DILEMA", "THE DILEMMA"), 10, 6, 210, 16, 8.5f, true);
            etiqueta.ForeColor = Color.Gold;
            marco.Controls.Add(etiqueta);

            lblDilema = Estilo.CrearEtiqueta(string.Empty, 10, 24, 212, 80, 9f, false);
            lblDilema.Font = new Font("Segoe UI", 9f, FontStyle.Italic);
            marco.Controls.Add(lblDilema);
        }

        // El panel de la derecha: las 3 pestañas, el nombre del personaje y el texto con barra de desplazamiento.
        private void ConstruirPanelTexto()
        {
            Panel panel = new Panel();
            panel.SetBounds(484, 76, 616, 536);
            panel.BackColor = PanelOscuro;
            Controls.Add(panel);
            Estilo.Redondear(panel, 14);

            int[] anchos = new int[] { 170, 230, 160 };
            botonesSeccion = new Button[nombresSeccion.Length];
            int x = 12;
            for (int i = 0; i < nombresSeccion.Length; i++)
            {
                Button b = new Button();
                b.Tag = i;   // número de la sección
                b.SetBounds(x, 12, anchos[i], 34);
                b.UseMnemonic = false;   // para que el "&" de Idioma.Texto("HISTORIA & LORE", "HISTORY & LORE") se vea como un símbolo y no como atajo de teclado
                b.Click += Seccion_Click;
                panel.Controls.Add(b);
                botonesSeccion[i] = b;
                x += anchos[i] + 8;
            }

            lblNombreLore = Estilo.CrearEtiqueta(string.Empty, 14, 56, 590, 32, 17f, true);
            lblNombreLore.ForeColor = Color.Gold;
            panel.Controls.Add(lblNombreLore);

            lblConcepto = Estilo.CrearEtiqueta(string.Empty, 16, 90, 590, 20, 9.5f, true);
            panel.Controls.Add(lblConcepto);

            texto = new RichTextBox();
            texto.SetBounds(12, 118, 592, 408);
            texto.ReadOnly = true;
            texto.BorderStyle = BorderStyle.None;
            texto.DetectUrls = false;
            texto.ScrollBars = RichTextBoxScrollBars.Vertical;
            texto.BackColor = FondoTexto;       // después de ReadOnly, que cambia el color de fondo por defecto
            texto.ForeColor = Color.WhiteSmoke;
            texto.Cursor = Cursors.Default;
            texto.TabStop = false;
            texto.MouseEnter += Texto_MouseEnter;   // así la rueda del mouse desplaza el texto
            panel.Controls.Add(texto);

            ActualizarPestanas();
        }

        // ---------- Eventos de los controles ----------

        private void Volver_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Personaje_Click(object sender, EventArgs e)
        {
            Button boton = (Button)sender;
            Seleccionar(personajes[(int)boton.Tag]);
        }

        private void Seccion_Click(object sender, EventArgs e)
        {
            Button boton = (Button)sender;
            MostrarSeccion((int)boton.Tag);
        }

        private void Texto_MouseEnter(object sender, EventArgs e)
        {
            texto.Focus();
        }

        // ---------- Selección ----------

        private void EstilizarBotonPersonaje(int indice, bool elegido)
        {
            Personaje p = personajes[indice];
            Button b = botonesPersonaje[indice];
            Color color = Estilo.FondoDe(p.Elemento);
            if (elegido) color = Estilo.ColorDe(p.Elemento);

            Estilo.EstilizarBoton(b, "   " + p.Nombre, color);
            b.TextAlign = ContentAlignment.MiddleLeft;
            b.ForeColor = Color.White;
            if (elegido && p.Elemento == Elemento.Aire) b.ForeColor = Color.FromArgb(20, 24, 40);
            b.FlatAppearance.MouseOverBackColor = Estilo.ColorDe(p.Elemento);

            // Miniatura de la ilustración a la izquierda (si no hay imagen, el botón queda solo con el nombre).
            if (b.Image == null)
            {
                try
                {
                    b.Image = Ilustraciones.ObtenerArte(p, 28, 28);
                    b.ImageAlign = ContentAlignment.MiddleLeft;
                    b.TextImageRelation = TextImageRelation.ImageBeforeText;
                }
                catch (Exception)
                {
                    b.Image = null;
                }
            }
        }

        private void Seleccionar(Personaje p)
        {
            seleccionado = p;
            for (int i = 0; i < personajes.Count; i++)
                EstilizarBotonPersonaje(i, personajes[i] == p);

            carta.ActualizarDatos(p, Estilo.RolDe(p.Elemento), false);
            lblDilema.Text = LorePersonajes.ObtenerDilema(p);
            lblDilema.ForeColor = Color.White;

            FichaLore ficha = LorePersonajes.ObtenerFicha(p);
            lblNombreLore.Text = p.Nombre + " — " + ficha.Titulo;
            lblConcepto.Text = Idioma.NombreElemento(p.Elemento).ToUpper() + "  ·  " + ficha.Concepto;
            lblConcepto.ForeColor = Estilo.ColorDe(p.Elemento);

            MostrarContenido();
        }

        private void MostrarSeccion(int seccion)
        {
            seccionActual = seccion;
            ActualizarPestanas();
            MostrarContenido();
        }

        // La pestaña activa se pinta dorada; las demás en gris azulado.
        private void ActualizarPestanas()
        {
            for (int i = 0; i < botonesSeccion.Length; i++)
            {
                bool activa = (i == seccionActual);
                Color colorFondo = Estilo.PanelClaro;
                Color colorTexto = Color.White;
                if (activa)
                {
                    colorFondo = Color.FromArgb(200, 150, 30);
                    colorTexto = Color.FromArgb(24, 18, 4);
                }
                Estilo.EstilizarBoton(botonesSeccion[i], nombresSeccion[i], colorFondo);
                botonesSeccion[i].Font = fuentePestana;
                botonesSeccion[i].ForeColor = colorTexto;
            }
        }

        // ---------- Contenido de cada sección ----------

        private void MostrarContenido()
        {
            if (seleccionado == null) return;

            texto.Clear();
            if (seccionActual == SeccionHistoria) EscribirHistoria();
            else if (seccionActual == SeccionAntropologia) EscribirAntropologia();
            else EscribirFicha();

            texto.SelectionStart = 0;
            texto.SelectionLength = 0;
            texto.ScrollToCaret();
        }

        private void EscribirHistoria()
        {
            Escribir(Idioma.Texto("HISTORIA", "HISTORY"), fuenteSeccion, Color.Gold);
            Escribir("\n", fuenteSuave, Color.White);
            Escribir(LorePersonajes.ObtenerHistoria(seleccionado) + "\n\n", fuenteNormal, Color.WhiteSmoke);

            Escribir(Idioma.Texto("EN LA ARENA", "IN THE ARENA"), fuenteSeccion, Color.Gold);
            Escribir("\n", fuenteSuave, Color.White);
            Escribir(seleccionado.Nombre + Idioma.Texto(" pelea como ", " fights in the role of ") + Estilo.RolDe(seleccionado.Elemento).ToLower() + Idioma.Texto(" de la facción ", " of the faction ") +
                     Idioma.NombreElemento(seleccionado.Elemento) + Idioma.Texto(". Su habilidad de ataque es ", ". Its attack ability is ") + seleccionado.HabilidadAtaque.Nombre +
                     Idioma.Texto(" y su habilidad de utilidad, ", " and its utility ability, ") + seleccionado.HabilidadUtilidad.Nombre + ".\n\n", fuenteNormal, Color.WhiteSmoke);

            Escribir(Idioma.Texto("Mira en la sección «Dimensión Antropológica» qué pregunta filosófica plantea este personaje.", "Look in the «Anthropological Dimension» section for the philosophical question this character raises."), fuenteSuave, Estilo.TextoSuave);
        }

        private void EscribirAntropologia()
        {
            Elemento elemento = seleccionado.Elemento;
            FichaLore ficha = LorePersonajes.ObtenerFicha(seleccionado);

            Escribir(Idioma.Texto("EJE DE LA FACCIÓN ", "AXIS OF THE FACTION ") + Idioma.NombreElemento(elemento).ToUpper(), fuenteSeccion, Estilo.ColorDe(elemento));
            Escribir("\n" + LorePersonajes.EjeDe(elemento) + "\n", fuenteCursiva, Color.WhiteSmoke);
            Escribir(LorePersonajes.ExplicacionDelEje(elemento) + "\n\n", fuenteSuave, Estilo.TextoSuave);

            Escribir(Idioma.Texto("REFLEXIÓN: ", "REFLECTION: ") + ficha.Concepto.ToUpper(), fuenteSeccion, Color.Gold);
            Escribir("\n", fuenteSuave, Color.White);
            Escribir(ficha.Reflexion + "\n\n", fuenteNormal, Color.WhiteSmoke);

            Escribir(Idioma.Texto("EL DILEMA", "THE DILEMMA"), fuenteSeccion, Color.Gold);
            Escribir("\n", fuenteSuave, Color.White);
            Escribir("«" + ficha.Dilema + "»\n\n", fuenteCursiva, Color.FromArgb(255, 200, 120));

            Escribir(Idioma.Texto("PARA PROFUNDIZAR", "GO DEEPER"), fuenteSeccion, Color.Gold);
            Escribir("\n", fuenteSuave, Color.White);
            foreach (string lectura in LorePersonajes.LecturasDe(elemento))
                Escribir("•  " + lectura + "\n", fuenteSuave, Color.WhiteSmoke);

            Escribir(Idioma.Texto("\nLectura propia del juego a partir de estos autores: consulta las obras originales para citar.", "\nThe game's own reading based on these authors: consult the original works when citing."),
                     fuenteSuave, Estilo.TextoSuave);
        }

        private void EscribirFicha()
        {
            Personaje p = seleccionado;

            Escribir(Idioma.Texto("ESTADÍSTICAS", "STATISTICS"), fuenteSeccion, Color.Gold);
            Escribir("\n", fuenteSuave, Color.White);
            Escribir(Idioma.Texto("Vida (HP):  ", "Health (HP):  ") + p.HpMax + Idioma.Texto("\nAtaque (ATK):  ", "\nAttack (ATK):  ") + p.Atk + Idioma.Texto("\nDefensa (DEF):  ", "\nDefense (DEF):  ") + p.Defensa +
                     Idioma.Texto("\nEnergía (ENG):  ", "\nEnergy (ENG):  ") + p.EnergiaMax + Idioma.Texto("\nEmpatía:  ", "\nEmpathy:  ") + p.Empatia + "\n\n", fuenteNormal, Color.WhiteSmoke);

            Escribir(Idioma.Texto("TRIÁNGULO ELEMENTAL", "ELEMENTAL TRIANGLE"), fuenteSeccion, Color.Gold);
            Escribir("\n", fuenteSuave, Color.White);
            Escribir(Estilo.DescripcionTriangulo(p) + "\n\n", fuenteNormal, Color.WhiteSmoke);

            Escribir(Idioma.Texto("HABILIDADES", "ABILITIES"), fuenteSeccion, Color.Gold);
            Escribir("\n", fuenteSuave, Color.White);
            EscribirHabilidad(p.HabilidadAtaque);
            EscribirHabilidad(p.HabilidadUtilidad);

            Escribir(Idioma.Texto("ROL", "ROLE"), fuenteSeccion, Color.Gold);
            Escribir("\n", fuenteSuave, Color.White);
            Escribir(Estilo.RolDe(p.Elemento) + Idioma.Texto(" de la facción ", " of the faction ") + Idioma.NombreElemento(p.Elemento) + ".", fuenteNormal, Color.WhiteSmoke);
        }

        private void EscribirHabilidad(Habilidad h)
        {
            Escribir(h.Tipo + ": " + h.Nombre + "\n", fuenteNegrita, Color.White);
            Escribir(Idioma.Texto("Cuesta ", "Costs ") + h.Costo + Idioma.Texto(" de energía y tiene ", " energy and has a ") + h.Cooldown + Idioma.Texto(" turnos de recarga.\n", " turn cooldown.\n"), fuenteSuave, Estilo.TextoSuave);
            Escribir(h.Descripcion + "\n\n", fuenteNormal, Color.WhiteSmoke);
        }

        // Agrega un fragmento de texto al final del cuadro, con su fuente y su color.
        private void Escribir(string fragmento, Font fuente, Color color)
        {
            texto.SelectionStart = texto.TextLength;
            texto.SelectionLength = 0;
            texto.SelectionFont = fuente;
            texto.SelectionColor = color;
            texto.SelectedText = fragmento;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                fuenteSeccion.Dispose();
                fuenteNormal.Dispose();
                fuenteSuave.Dispose();
                fuenteCursiva.Dispose();
                fuenteNegrita.Dispose();
                fuentePestana.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
