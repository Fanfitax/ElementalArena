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
        private Label lblDilema;
        private Label lblNombreLore;
        private Label lblConcepto;
        private RichTextBox texto;

        // Fuentes del texto (se crean una sola vez y se liberan al cerrar).
        private Font fuenteSeccion = new Font("Segoe UI", 10f, FontStyle.Bold);
        private Font fuenteNormal = new Font("Segoe UI", 11f);
        private Font fuenteSuave = new Font("Segoe UI", 9.5f);
        private Font fuenteCursiva = new Font("Segoe UI", 11.5f, FontStyle.Italic);
        private Font fuenteNegrita = new Font("Segoe UI", 11f, FontStyle.Bold);
        private Font fuentePestana = new Font("Segoe UI", 9f, FontStyle.Bold);

        // Los controles se declaran como campos para que el diseñador de SharpDevelop los reconozca.
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Button btnVolver;
        private Label lblEncabezadoFuego;
        private Label lblEncabezadoAgua;
        private Label lblEncabezadoTierra;
        private Label lblEncabezadoAire;
        private Panel panelDilema;
        private Label lblEtiquetaDilema;
        private Panel panelTexto;
        private Button btnHistoria;
        private Button btnAntropologia;
        private Button btnFicha;

        public FrmCodice(List<Personaje> personajes)
        {
            this.personajes = new List<Personaje>(personajes);

            InitializeComponent();

            // Lo que cambia según el idioma (el diseñador solo muestra la versión en español).
            Text = Idioma.Texto("Arena Elemental - Códice Elemental", "Arena Elemental - Elemental Codex");
            lblTitulo.Text = Idioma.Texto("CÓDICE ELEMENTAL", "ELEMENTAL CODEX");
            lblSubtitulo.Text = Idioma.Texto("Galería de personajes · Historia y reflexión de Antropología Filosófica", "Character gallery · History and reflection from Philosophical Anthropology");
            lblEtiquetaDilema.Text = Idioma.Texto("EL DILEMA", "THE DILEMMA");
            Estilo.EstilizarBoton(btnVolver, Idioma.Texto("Volver", "Back"), Estilo.PanelClaro);
            Estilo.Redondear(panelDilema, 12);
            Estilo.Redondear(panelTexto, 14);
            texto.BackColor = FondoTexto;       // después de ReadOnly, que cambia el color de fondo por defecto

            // Las tres pestañas: cada botón guarda en Tag el número de su sección.
            botonesSeccion = new Button[] { btnHistoria, btnAntropologia, btnFicha };
            for (int i = 0; i < botonesSeccion.Length; i++)
                botonesSeccion[i].Tag = i;
            ActualizarPestanas();

            ConstruirGaleria();
            ConstruirCarta();

            if (this.personajes.Count > 0) Seleccionar(this.personajes[0]);
        }

        // ---------- Construcción de la interfaz ----------

        // La lista de la izquierda: un encabezado por elemento (ya creados en InitializeComponent) y un botón por personaje.
        private void ConstruirGaleria()
        {
            Elemento[] elementos = new Elemento[] { Elemento.Fuego, Elemento.Agua, Elemento.Tierra, Elemento.Aire };
            Label[] encabezados = new Label[] { lblEncabezadoFuego, lblEncabezadoAgua, lblEncabezadoTierra, lblEncabezadoAire };
            botonesPersonaje = new Button[personajes.Count];

            for (int e = 0; e < elementos.Length; e++)
            {
                int y = 76 + e * 134;
                encabezados[e].Text = Idioma.NombreElemento(elementos[e]).ToUpper() + "  ·  " + Estilo.RolDe(elementos[e]);

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

        // La carta grande se crea por código porque es un control propio que se llena con los datos del personaje.
        private void ConstruirCarta()
        {
            carta = new VistaCarta(TamanoCarta.Grande);
            carta.Location = new Point(232, 76);
            Controls.Add(carta);
        }

        // Código del diseñador: aquí se crean y se colocan los controles de la ventana.
        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.btnVolver = new System.Windows.Forms.Button();
            this.lblEncabezadoFuego = new System.Windows.Forms.Label();
            this.lblEncabezadoAgua = new System.Windows.Forms.Label();
            this.lblEncabezadoTierra = new System.Windows.Forms.Label();
            this.lblEncabezadoAire = new System.Windows.Forms.Label();
            this.panelDilema = new System.Windows.Forms.Panel();
            this.lblEtiquetaDilema = new System.Windows.Forms.Label();
            this.lblDilema = new System.Windows.Forms.Label();
            this.panelTexto = new System.Windows.Forms.Panel();
            this.btnHistoria = new System.Windows.Forms.Button();
            this.btnAntropologia = new System.Windows.Forms.Button();
            this.btnFicha = new System.Windows.Forms.Button();
            this.lblNombreLore = new System.Windows.Forms.Label();
            this.lblConcepto = new System.Windows.Forms.Label();
            this.texto = new System.Windows.Forms.RichTextBox();
            this.panelDilema.SuspendLayout();
            this.panelTexto.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.Gold;
            this.lblTitulo.Location = new System.Drawing.Point(20, 8);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(600, 38);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "CÓDICE ELEMENTAL";
            //
            // lblSubtitulo
            //
            this.lblSubtitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblSubtitulo.Location = new System.Drawing.Point(22, 46);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(800, 20);
            this.lblSubtitulo.TabIndex = 1;
            this.lblSubtitulo.Text = "Galería de personajes · Historia y reflexión de Antropología Filosófica";
            //
            // btnVolver
            //
            this.btnVolver.BackColor = System.Drawing.Color.FromArgb(58, 62, 92);
            this.btnVolver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVolver.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVolver.ForeColor = System.Drawing.Color.White;
            this.btnVolver.Location = new System.Drawing.Point(990, 14);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(110, 32);
            this.btnVolver.TabIndex = 2;
            this.btnVolver.Text = "Volver";
            this.btnVolver.UseVisualStyleBackColor = false;
            this.btnVolver.Click += new System.EventHandler(this.Volver_Click);
            //
            // lblEncabezadoFuego
            //
            this.lblEncabezadoFuego.BackColor = System.Drawing.Color.Transparent;
            this.lblEncabezadoFuego.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblEncabezadoFuego.ForeColor = System.Drawing.Color.FromArgb(226, 88, 52);
            this.lblEncabezadoFuego.Location = new System.Drawing.Point(20, 76);
            this.lblEncabezadoFuego.Name = "lblEncabezadoFuego";
            this.lblEncabezadoFuego.Size = new System.Drawing.Size(190, 18);
            this.lblEncabezadoFuego.TabIndex = 3;
            this.lblEncabezadoFuego.Text = "FUEGO  ·  ATACANTE";
            //
            // lblEncabezadoAgua
            //
            this.lblEncabezadoAgua.BackColor = System.Drawing.Color.Transparent;
            this.lblEncabezadoAgua.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblEncabezadoAgua.ForeColor = System.Drawing.Color.FromArgb(52, 136, 226);
            this.lblEncabezadoAgua.Location = new System.Drawing.Point(20, 210);
            this.lblEncabezadoAgua.Name = "lblEncabezadoAgua";
            this.lblEncabezadoAgua.Size = new System.Drawing.Size(190, 18);
            this.lblEncabezadoAgua.TabIndex = 4;
            this.lblEncabezadoAgua.Text = "AGUA  ·  APOYO";
            //
            // lblEncabezadoTierra
            //
            this.lblEncabezadoTierra.BackColor = System.Drawing.Color.Transparent;
            this.lblEncabezadoTierra.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblEncabezadoTierra.ForeColor = System.Drawing.Color.FromArgb(150, 108, 62);
            this.lblEncabezadoTierra.Location = new System.Drawing.Point(20, 344);
            this.lblEncabezadoTierra.Name = "lblEncabezadoTierra";
            this.lblEncabezadoTierra.Size = new System.Drawing.Size(190, 18);
            this.lblEncabezadoTierra.TabIndex = 5;
            this.lblEncabezadoTierra.Text = "TIERRA  ·  TANQUE";
            //
            // lblEncabezadoAire
            //
            this.lblEncabezadoAire.BackColor = System.Drawing.Color.Transparent;
            this.lblEncabezadoAire.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblEncabezadoAire.ForeColor = System.Drawing.Color.FromArgb(120, 196, 196);
            this.lblEncabezadoAire.Location = new System.Drawing.Point(20, 478);
            this.lblEncabezadoAire.Name = "lblEncabezadoAire";
            this.lblEncabezadoAire.Size = new System.Drawing.Size(190, 18);
            this.lblEncabezadoAire.TabIndex = 6;
            this.lblEncabezadoAire.Text = "AIRE  ·  VELOZ";
            //
            // panelDilema
            //
            this.panelDilema.BackColor = System.Drawing.Color.FromArgb(205, 10, 12, 18);
            this.panelDilema.Controls.Add(this.lblEtiquetaDilema);
            this.panelDilema.Controls.Add(this.lblDilema);
            this.panelDilema.Location = new System.Drawing.Point(232, 504);
            this.panelDilema.Name = "panelDilema";
            this.panelDilema.Size = new System.Drawing.Size(232, 108);
            this.panelDilema.TabIndex = 7;
            //
            //
            // lblEtiquetaDilema
            //
            this.lblEtiquetaDilema.BackColor = System.Drawing.Color.Transparent;
            this.lblEtiquetaDilema.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblEtiquetaDilema.ForeColor = System.Drawing.Color.Gold;
            this.lblEtiquetaDilema.Location = new System.Drawing.Point(10, 6);
            this.lblEtiquetaDilema.Name = "lblEtiquetaDilema";
            this.lblEtiquetaDilema.Size = new System.Drawing.Size(210, 16);
            this.lblEtiquetaDilema.TabIndex = 0;
            this.lblEtiquetaDilema.Text = "EL DILEMA";
            //
            // lblDilema
            //
            this.lblDilema.BackColor = System.Drawing.Color.Transparent;
            this.lblDilema.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblDilema.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblDilema.Location = new System.Drawing.Point(10, 24);
            this.lblDilema.Name = "lblDilema";
            this.lblDilema.Size = new System.Drawing.Size(212, 80);
            this.lblDilema.TabIndex = 1;
            //
            // panelTexto
            //
            this.panelTexto.BackColor = System.Drawing.Color.FromArgb(205, 10, 12, 18);
            this.panelTexto.Controls.Add(this.btnHistoria);
            this.panelTexto.Controls.Add(this.btnAntropologia);
            this.panelTexto.Controls.Add(this.btnFicha);
            this.panelTexto.Controls.Add(this.lblNombreLore);
            this.panelTexto.Controls.Add(this.lblConcepto);
            this.panelTexto.Controls.Add(this.texto);
            this.panelTexto.Location = new System.Drawing.Point(484, 76);
            this.panelTexto.Name = "panelTexto";
            this.panelTexto.Size = new System.Drawing.Size(616, 536);
            this.panelTexto.TabIndex = 8;            //
            // btnHistoria
            //
            this.btnHistoria.BackColor = System.Drawing.Color.FromArgb(58, 62, 92);
            this.btnHistoria.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHistoria.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnHistoria.ForeColor = System.Drawing.Color.White;
            this.btnHistoria.Location = new System.Drawing.Point(12, 12);
            this.btnHistoria.Name = "btnHistoria";
            this.btnHistoria.Size = new System.Drawing.Size(170, 34);
            this.btnHistoria.TabIndex = 0;
            this.btnHistoria.Text = "HISTORIA & LORE";
            this.btnHistoria.UseMnemonic = false;
            this.btnHistoria.UseVisualStyleBackColor = false;
            this.btnHistoria.Click += new System.EventHandler(this.Seccion_Click);
            //
            // btnAntropologia
            //
            this.btnAntropologia.BackColor = System.Drawing.Color.FromArgb(58, 62, 92);
            this.btnAntropologia.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAntropologia.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAntropologia.ForeColor = System.Drawing.Color.White;
            this.btnAntropologia.Location = new System.Drawing.Point(190, 12);
            this.btnAntropologia.Name = "btnAntropologia";
            this.btnAntropologia.Size = new System.Drawing.Size(230, 34);
            this.btnAntropologia.TabIndex = 1;
            this.btnAntropologia.Text = "DIMENSIÓN ANTROPOLÓGICA";
            this.btnAntropologia.UseMnemonic = false;
            this.btnAntropologia.UseVisualStyleBackColor = false;
            this.btnAntropologia.Click += new System.EventHandler(this.Seccion_Click);
            //
            // btnFicha
            //
            this.btnFicha.BackColor = System.Drawing.Color.FromArgb(58, 62, 92);
            this.btnFicha.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFicha.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnFicha.ForeColor = System.Drawing.Color.White;
            this.btnFicha.Location = new System.Drawing.Point(428, 12);
            this.btnFicha.Name = "btnFicha";
            this.btnFicha.Size = new System.Drawing.Size(160, 34);
            this.btnFicha.TabIndex = 2;
            this.btnFicha.Text = "FICHA TÉCNICA";
            this.btnFicha.UseMnemonic = false;
            this.btnFicha.UseVisualStyleBackColor = false;
            this.btnFicha.Click += new System.EventHandler(this.Seccion_Click);
            //
            // lblNombreLore
            //
            this.lblNombreLore.BackColor = System.Drawing.Color.Transparent;
            this.lblNombreLore.Font = new System.Drawing.Font("Segoe UI", 17F, System.Drawing.FontStyle.Bold);
            this.lblNombreLore.ForeColor = System.Drawing.Color.Gold;
            this.lblNombreLore.Location = new System.Drawing.Point(14, 56);
            this.lblNombreLore.Name = "lblNombreLore";
            this.lblNombreLore.Size = new System.Drawing.Size(590, 32);
            this.lblNombreLore.TabIndex = 3;
            this.lblNombreLore.Text = "";
            //
            // lblConcepto
            //
            this.lblConcepto.BackColor = System.Drawing.Color.Transparent;
            this.lblConcepto.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblConcepto.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblConcepto.Location = new System.Drawing.Point(16, 90);
            this.lblConcepto.Name = "lblConcepto";
            this.lblConcepto.Size = new System.Drawing.Size(590, 20);
            this.lblConcepto.TabIndex = 4;
            this.lblConcepto.Text = "";
            //
            // texto
            //
            this.texto.BackColor = System.Drawing.Color.FromArgb(12, 14, 22);
            this.texto.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.texto.DetectUrls = false;
            this.texto.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.texto.Location = new System.Drawing.Point(12, 118);
            this.texto.Name = "texto";
            this.texto.ReadOnly = true;
            this.texto.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.texto.Size = new System.Drawing.Size(592, 408);
            this.texto.TabIndex = 5;
            this.texto.TabStop = false;
            this.texto.Text = "";
            this.texto.MouseEnter += new System.EventHandler(this.Texto_MouseEnter);
            //
            // FrmCodice
            //
            this.BackColor = System.Drawing.Color.FromArgb(24, 26, 40);
            this.CancelButton = this.btnVolver;
            this.ClientSize = new System.Drawing.Size(1120, 640);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblSubtitulo);
            this.Controls.Add(this.btnVolver);
            this.Controls.Add(this.lblEncabezadoFuego);
            this.Controls.Add(this.lblEncabezadoAgua);
            this.Controls.Add(this.lblEncabezadoTierra);
            this.Controls.Add(this.lblEncabezadoAire);
            this.Controls.Add(this.panelDilema);
            this.Controls.Add(this.panelTexto);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmCodice";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Arena Elemental - Códice Elemental";
            this.panelDilema.ResumeLayout(false);
            this.panelTexto.ResumeLayout(false);
            this.ResumeLayout(false);
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
