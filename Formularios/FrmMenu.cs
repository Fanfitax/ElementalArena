using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Modelos;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    // Pantalla de inicio: título del juego, nombre del jugador y los botones principales.
    // A la derecha se muestran cuatro héroes, uno por elemento, sobre sus plataformas.
    public class FrmMenu : FormularioModerno
    {
        // Los héroes que se muestran en la portada (uno de cada elemento).
        private static string[] NombresHeroes = new string[] { "Hestia", "Tetis", "Atlas", "Atalanta" };
        private static string[] NombresHeroesIngles = new string[] { "Hestia", "Thetis", "Atlas", "Atalanta" };
        private static Elemento[] ElementosHeroes = new Elemento[] { Elemento.Fuego, Elemento.Agua, Elemento.Tierra, Elemento.Aire };

        private Image[] heroes = new Image[4];
        private Random azar = new Random();
        private TextBox txtJugador;

        // Los controles se declaran como campos para que el diseñador de SharpDevelop los reconozca.
        private Label lblArena;
        private Label lblElemental;
        private Panel franjaFuego;
        private Panel franjaAgua;
        private Panel franjaTierra;
        private Panel franjaAire;
        private Label lblSubtitulo;
        private Label lblTriangulo;
        private Label lblEtiquetaNombre;
        private Label lblNota;
        private Button btnEspanol;
        private Button btnIngles;
        private Button btnJugar;
        private Button btnLan;
        private Button btnStats;
        private Button btnCodice;
        private Button btnRanking;
        private Button btnSalir;

        public FrmMenu()
        {
            InitializeComponent();

            // Lo que el diseñador no puede hacer: botones planos con esquinas redondeadas.
            Estilo.EstilizarBoton(btnJugar, "", Color.FromArgb(46, 160, 96));
            btnJugar.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            Estilo.EstilizarBoton(btnLan, "", Color.FromArgb(196, 120, 24));
            btnLan.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            Estilo.EstilizarBoton(btnStats, "", Estilo.Acento);
            Estilo.EstilizarBoton(btnCodice, "", Color.FromArgb(118, 78, 178));
            Estilo.EstilizarBoton(btnRanking, "", Color.FromArgb(176, 132, 28));
            Estilo.EstilizarBoton(btnSalir, "", Estilo.PanelClaro);
            ActualizarTextos();
            txtJugador.Text = Environment.UserName;
        }

        // Código del diseñador: aquí se crean y se colocan los controles de la ventana.
        private void InitializeComponent()
        {
            this.lblArena = new System.Windows.Forms.Label();
            this.lblElemental = new System.Windows.Forms.Label();
            this.franjaFuego = new System.Windows.Forms.Panel();
            this.franjaAgua = new System.Windows.Forms.Panel();
            this.franjaTierra = new System.Windows.Forms.Panel();
            this.franjaAire = new System.Windows.Forms.Panel();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.lblTriangulo = new System.Windows.Forms.Label();
            this.lblEtiquetaNombre = new System.Windows.Forms.Label();
            this.txtJugador = new System.Windows.Forms.TextBox();
            this.btnJugar = new System.Windows.Forms.Button();
            this.btnLan = new System.Windows.Forms.Button();
            this.btnStats = new System.Windows.Forms.Button();
            this.btnCodice = new System.Windows.Forms.Button();
            this.btnRanking = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();
            this.lblNota = new System.Windows.Forms.Label();
            this.btnEspanol = new System.Windows.Forms.Button();
            this.btnIngles = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblArena
            //
            this.lblArena.BackColor = System.Drawing.Color.Transparent;
            this.lblArena.Font = new System.Drawing.Font("Segoe UI", 60F, System.Drawing.FontStyle.Bold);
            this.lblArena.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblArena.Location = new System.Drawing.Point(46, 56);
            this.lblArena.Name = "lblArena";
            this.lblArena.Size = new System.Drawing.Size(400, 100);
            this.lblArena.TabIndex = 0;
            this.lblArena.Text = "ARENA";
            //
            // lblElemental
            //
            this.lblElemental.BackColor = System.Drawing.Color.Transparent;
            this.lblElemental.Font = new System.Drawing.Font("Segoe UI", 30F, System.Drawing.FontStyle.Bold);
            this.lblElemental.ForeColor = System.Drawing.Color.Gold;
            this.lblElemental.Location = new System.Drawing.Point(52, 150);
            this.lblElemental.Name = "lblElemental";
            this.lblElemental.Size = new System.Drawing.Size(400, 52);
            this.lblElemental.TabIndex = 1;
            this.lblElemental.Text = "ELEMENTAL";
            //
            // franjaFuego
            //
            this.franjaFuego.BackColor = System.Drawing.Color.FromArgb(226, 88, 52);
            this.franjaFuego.Location = new System.Drawing.Point(56, 212);
            this.franjaFuego.Name = "franjaFuego";
            this.franjaFuego.Size = new System.Drawing.Size(84, 6);
            this.franjaFuego.TabIndex = 2;
            //
            // franjaAgua
            //
            this.franjaAgua.BackColor = System.Drawing.Color.FromArgb(52, 136, 226);
            this.franjaAgua.Location = new System.Drawing.Point(146, 212);
            this.franjaAgua.Name = "franjaAgua";
            this.franjaAgua.Size = new System.Drawing.Size(84, 6);
            this.franjaAgua.TabIndex = 2;
            //
            // franjaTierra
            //
            this.franjaTierra.BackColor = System.Drawing.Color.FromArgb(150, 108, 62);
            this.franjaTierra.Location = new System.Drawing.Point(236, 212);
            this.franjaTierra.Name = "franjaTierra";
            this.franjaTierra.Size = new System.Drawing.Size(84, 6);
            this.franjaTierra.TabIndex = 2;
            //
            // franjaAire
            //
            this.franjaAire.BackColor = System.Drawing.Color.FromArgb(120, 196, 196);
            this.franjaAire.Location = new System.Drawing.Point(326, 212);
            this.franjaAire.Name = "franjaAire";
            this.franjaAire.Size = new System.Drawing.Size(84, 6);
            this.franjaAire.TabIndex = 2;
            //
            // lblSubtitulo
            //
            this.lblSubtitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblSubtitulo.Location = new System.Drawing.Point(54, 230);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(380, 24);
            this.lblSubtitulo.TabIndex = 3;
            this.lblSubtitulo.Text = "Combate estratégico por turnos y relevos";
            //
            // lblTriangulo
            //
            this.lblTriangulo.BackColor = System.Drawing.Color.Transparent;
            this.lblTriangulo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTriangulo.ForeColor = System.Drawing.Color.Gold;
            this.lblTriangulo.Location = new System.Drawing.Point(54, 256);
            this.lblTriangulo.Name = "lblTriangulo";
            this.lblTriangulo.Size = new System.Drawing.Size(380, 20);
            this.lblTriangulo.TabIndex = 4;
            this.lblTriangulo.Text = "Agua > Fuego > Aire > Tierra > Agua";
            //
            // lblEtiquetaNombre
            //
            this.lblEtiquetaNombre.BackColor = System.Drawing.Color.Transparent;
            this.lblEtiquetaNombre.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblEtiquetaNombre.ForeColor = System.Drawing.Color.Gold;
            this.lblEtiquetaNombre.Location = new System.Drawing.Point(54, 318);
            this.lblEtiquetaNombre.Name = "lblEtiquetaNombre";
            this.lblEtiquetaNombre.Size = new System.Drawing.Size(300, 20);
            this.lblEtiquetaNombre.TabIndex = 5;
            this.lblEtiquetaNombre.Text = "TU NOMBRE";
            //
            // txtJugador
            //
            this.txtJugador.BackColor = System.Drawing.Color.FromArgb(16, 20, 32);
            this.txtJugador.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtJugador.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.txtJugador.ForeColor = System.Drawing.Color.White;
            this.txtJugador.Location = new System.Drawing.Point(54, 342);
            this.txtJugador.MaxLength = 30;
            this.txtJugador.Name = "txtJugador";
            this.txtJugador.Size = new System.Drawing.Size(340, 29);
            this.txtJugador.TabIndex = 6;
            //
            // btnJugar
            //
            this.btnJugar.BackColor = System.Drawing.Color.FromArgb(46, 160, 96);
            this.btnJugar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnJugar.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnJugar.ForeColor = System.Drawing.Color.White;
            this.btnJugar.Location = new System.Drawing.Point(54, 392);
            this.btnJugar.Name = "btnJugar";
            this.btnJugar.Size = new System.Drawing.Size(340, 50);
            this.btnJugar.TabIndex = 7;
            this.btnJugar.Text = "NUEVA PARTIDA (VS CPU)";
            this.btnJugar.UseVisualStyleBackColor = false;
            this.btnJugar.Click += new System.EventHandler(this.BtnJugar_Click);
            //
            // btnLan
            //
            this.btnLan.BackColor = System.Drawing.Color.FromArgb(196, 120, 24);
            this.btnLan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLan.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnLan.ForeColor = System.Drawing.Color.White;
            this.btnLan.Location = new System.Drawing.Point(54, 448);
            this.btnLan.Name = "btnLan";
            this.btnLan.Size = new System.Drawing.Size(340, 42);
            this.btnLan.TabIndex = 8;
            this.btnLan.Text = "PARTIDA LAN (2 JUGADORES)";
            this.btnLan.UseVisualStyleBackColor = false;
            this.btnLan.Click += new System.EventHandler(this.BtnLan_Click);
            //
            // btnStats
            //
            this.btnStats.BackColor = System.Drawing.Color.FromArgb(96, 120, 220);
            this.btnStats.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStats.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStats.ForeColor = System.Drawing.Color.White;
            this.btnStats.Location = new System.Drawing.Point(54, 496);
            this.btnStats.Name = "btnStats";
            this.btnStats.Size = new System.Drawing.Size(166, 40);
            this.btnStats.TabIndex = 9;
            this.btnStats.Text = "ESTADÍSTICAS";
            this.btnStats.UseVisualStyleBackColor = false;
            this.btnStats.Click += new System.EventHandler(this.BtnStats_Click);
            //
            // btnCodice
            //
            this.btnCodice.BackColor = System.Drawing.Color.FromArgb(118, 78, 178);
            this.btnCodice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCodice.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCodice.ForeColor = System.Drawing.Color.White;
            this.btnCodice.Location = new System.Drawing.Point(228, 496);
            this.btnCodice.Name = "btnCodice";
            this.btnCodice.Size = new System.Drawing.Size(166, 40);
            this.btnCodice.TabIndex = 10;
            this.btnCodice.Text = "CÓDICE ELEMENTAL";
            this.btnCodice.UseVisualStyleBackColor = false;
            this.btnCodice.Click += new System.EventHandler(this.BtnCodice_Click);
            //
            // btnRanking
            //
            this.btnRanking.BackColor = System.Drawing.Color.FromArgb(176, 132, 28);
            this.btnRanking.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRanking.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRanking.ForeColor = System.Drawing.Color.White;
            this.btnRanking.Location = new System.Drawing.Point(54, 544);
            this.btnRanking.Name = "btnRanking";
            this.btnRanking.Size = new System.Drawing.Size(166, 36);
            this.btnRanking.TabIndex = 11;
            this.btnRanking.Text = "RANKING";
            this.btnRanking.UseVisualStyleBackColor = false;
            this.btnRanking.Click += new System.EventHandler(this.BtnRanking_Click);
            //
            // btnSalir
            //
            this.btnSalir.BackColor = System.Drawing.Color.FromArgb(58, 62, 92);
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSalir.ForeColor = System.Drawing.Color.White;
            this.btnSalir.Location = new System.Drawing.Point(228, 544);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(166, 36);
            this.btnSalir.TabIndex = 12;
            this.btnSalir.Text = "SALIR";
            this.btnSalir.UseVisualStyleBackColor = false;
            this.btnSalir.Click += new System.EventHandler(this.BtnSalir_Click);
            //
            // lblNota
            //
            this.lblNota.BackColor = System.Drawing.Color.Transparent;
            this.lblNota.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblNota.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblNota.Location = new System.Drawing.Point(54, 594);
            this.lblNota.Name = "lblNota";
            this.lblNota.Size = new System.Drawing.Size(400, 20);
            this.lblNota.TabIndex = 13;
            this.lblNota.Text = "Las partidas se guardan en partidas.txt, junto al programa.";
            //
            // btnEspanol
            //
            this.btnEspanol.BackColor = System.Drawing.Color.FromArgb(200, 150, 30);
            this.btnEspanol.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEspanol.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnEspanol.ForeColor = System.Drawing.Color.White;
            this.btnEspanol.Location = new System.Drawing.Point(54, 16);
            this.btnEspanol.Name = "btnEspanol";
            this.btnEspanol.Size = new System.Drawing.Size(100, 28);
            this.btnEspanol.TabIndex = 14;
            this.btnEspanol.Text = "ESPAÑOL";
            this.btnEspanol.UseVisualStyleBackColor = false;
            this.btnEspanol.Click += new System.EventHandler(this.BtnEspanol_Click);
            //
            // btnIngles
            //
            this.btnIngles.BackColor = System.Drawing.Color.FromArgb(58, 62, 92);
            this.btnIngles.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIngles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnIngles.ForeColor = System.Drawing.Color.White;
            this.btnIngles.Location = new System.Drawing.Point(162, 16);
            this.btnIngles.Name = "btnIngles";
            this.btnIngles.Size = new System.Drawing.Size(100, 28);
            this.btnIngles.TabIndex = 15;
            this.btnIngles.Text = "ENGLISH";
            this.btnIngles.UseVisualStyleBackColor = false;
            this.btnIngles.Click += new System.EventHandler(this.BtnIngles_Click);
            //
            // FrmMenu
            //
            this.AcceptButton = this.btnJugar;
            this.BackColor = System.Drawing.Color.FromArgb(24, 26, 40);
            this.ClientSize = new System.Drawing.Size(1120, 640);
            this.Controls.Add(this.lblArena);
            this.Controls.Add(this.lblElemental);
            this.Controls.Add(this.franjaFuego);
            this.Controls.Add(this.franjaAgua);
            this.Controls.Add(this.franjaTierra);
            this.Controls.Add(this.franjaAire);
            this.Controls.Add(this.lblSubtitulo);
            this.Controls.Add(this.lblTriangulo);
            this.Controls.Add(this.lblEtiquetaNombre);
            this.Controls.Add(this.txtJugador);
            this.Controls.Add(this.btnJugar);
            this.Controls.Add(this.btnLan);
            this.Controls.Add(this.btnStats);
            this.Controls.Add(this.btnCodice);
            this.Controls.Add(this.btnRanking);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.lblNota);
            this.Controls.Add(this.btnEspanol);
            this.Controls.Add(this.btnIngles);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmMenu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Arena Elemental";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Los dos botones de idioma cambian el idioma y vuelven a escribir los textos del menú.
        private void BtnEspanol_Click(object sender, EventArgs e)
        {
            Idioma.Actual = Idioma.Espanol;
            ActualizarTextos();
        }

        private void BtnIngles_Click(object sender, EventArgs e)
        {
            Idioma.Actual = Idioma.Ingles;
            ActualizarTextos();
        }

        // Escribe todos los textos del menú en el idioma elegido y marca el botón del idioma activo.
        private void ActualizarTextos()
        {
            lblSubtitulo.Text = Idioma.Texto("Combate estratégico por turnos y relevos", "Turn-based strategic combat with swaps");
            lblTriangulo.Text = Estilo.Triangulo;
            lblEtiquetaNombre.Text = Idioma.Texto("TU NOMBRE", "YOUR NAME");
            lblNota.Text = Idioma.Texto("Las partidas se guardan en partidas.txt, junto al programa.", "Matches are saved in partidas.txt, next to the program.");

            btnJugar.Text = Idioma.Texto("NUEVA PARTIDA (VS CPU)", "NEW MATCH (VS CPU)");
            btnLan.Text = Idioma.Texto("PARTIDA LAN (2 JUGADORES)", "LAN MATCH (2 PLAYERS)");
            btnStats.Text = Idioma.Texto("ESTADÍSTICAS", "STATISTICS");
            btnCodice.Text = Idioma.Texto("CÓDICE ELEMENTAL", "ELEMENTAL CODEX");
            btnRanking.Text = "RANKING";
            btnSalir.Text = Idioma.Texto("SALIR", "EXIT");

            Estilo.EstilizarBoton(btnEspanol, "ESPAÑOL", Estilo.PanelClaro);
            Estilo.EstilizarBoton(btnIngles, "ENGLISH", Estilo.PanelClaro);
            if (Idioma.Actual == Idioma.Espanol) btnEspanol.BackColor = Color.FromArgb(200, 150, 30);
            else btnIngles.BackColor = Color.FromArgb(200, 150, 30);

            Invalidate();   // para que los nombres de los héroes dibujados en la portada también cambien
        }

        // ---------- Héroes de la portada ----------

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Primero se dibuja la ventana y después se preparan los héroes, para que el menú aparezca al instante.
            Refresh();
            CargarHeroes();
        }

        private void CargarHeroes()
        {
            for (int i = 0; i < heroes.Length; i++)
            {
                try
                {
                    heroes[i] = Ilustraciones.ObtenerPersonajeEncajado(NombresHeroes[i], 220, 400);
                }
                catch (Exception)
                {
                    heroes[i] = null;   // sin imagen solo se ve la plataforma
                }
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int piso = 540;             // altura de los pies de los héroes
            int separacion = 160;       // distancia entre un héroe y el siguiente
            int inicio = 500;           // centro del primero

            for (int i = 0; i < heroes.Length; i++)
            {
                int cx = inicio + i * separacion;
                Color color = Estilo.ColorDe(ElementosHeroes[i]);

                // Plataforma luminosa bajo cada héroe.
                using (GraphicsPath luz = new GraphicsPath())
                {
                    luz.AddEllipse(cx - 85, piso - 18, 170, 36);
                    using (PathGradientBrush brillo = new PathGradientBrush(luz))
                    {
                        brillo.CenterColor = Color.FromArgb(170, color);
                        brillo.SurroundColors = new Color[] { Color.FromArgb(0, color) };
                        g.FillPath(brillo, luz);
                    }
                }
                using (Pen aro = new Pen(Color.FromArgb(170, color), 2.5f))
                {
                    g.DrawEllipse(aro, cx - 60, piso - 11, 120, 22);
                }

                if (heroes[i] != null)
                    g.DrawImage(heroes[i], cx - heroes[i].Width / 2, piso - heroes[i].Height + 8);

                // Nombre y elemento.
                string nombre = Idioma.Texto(NombresHeroes[i], NombresHeroesIngles[i]).ToUpper();
                using (Font fuente = new Font("Segoe UI", 12f, FontStyle.Bold))
                using (Font pequena = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                using (SolidBrush blanco = new SolidBrush(Color.White))
                using (SolidBrush colorElemento = new SolidBrush(color))
                {
                    SizeF tam = g.MeasureString(nombre, fuente);
                    g.DrawString(nombre, fuente, blanco, cx - tam.Width / 2, piso + 22);
                    string rol = Idioma.NombreElemento(ElementosHeroes[i]).ToUpper() + " · " + Estilo.RolDe(ElementosHeroes[i]);
                    SizeF tam2 = g.MeasureString(rol, pequena);
                    g.DrawString(rol, pequena, colorElemento, cx - tam2.Width / 2, piso + 44);
                }
            }
        }

        // ---------- Eventos de los botones ----------

        private void BtnJugar_Click(object sender, EventArgs e)
        {
            NuevaPartida();
        }

        private void BtnLan_Click(object sender, EventArgs e)
        {
            PartidaLan();
        }

        private void BtnStats_Click(object sender, EventArgs e)
        {
            VerEstadisticas();
        }

        private void BtnRanking_Click(object sender, EventArgs e)
        {
            using (FrmRanking frm = new FrmRanking(txtJugador.Text.Trim()))
            {
                frm.ShowDialog(this);
            }
        }

        private void BtnCodice_Click(object sender, EventArgs e)
        {
            VerCodice();
        }

        private void BtnSalir_Click(object sender, EventArgs e)
        {
            Close();
        }

        // ---------- Acciones ----------

        // Devuelve el nombre escrito, o null si está vacío.
        private string LeerNombre()
        {
            string nombre = txtJugador.Text.Trim();
            if (nombre.Length == 0)
            {
                MessageBox.Show(Idioma.Texto("Escribe tu nombre de jugador.", "Type your player name."), Idioma.Texto("Falta el nombre", "Name missing"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtJugador.Focus();
                return null;
            }
            return nombre;
        }

        private void NuevaPartida()
        {
            string nombre = LeerNombre();
            if (nombre == null) return;

            using (FrmSeleccionEquipo seleccion = new FrmSeleccionEquipo(CatalogoBase.Personajes, CatalogoBase.Escenarios, nombre))
            {
                if (seleccion.ShowDialog(this) != DialogResult.OK) return;

                EquipoCombate propio = new EquipoCombate(seleccion.NombreEquipo, seleccion.EquipoElegido, true);
                EquipoCombate rival = new EquipoCombate("Rival CPU", SortearRival(), false);
                Combate combate = new Combate(seleccion.EscenarioElegido, propio, rival);

                using (FrmCombate frm = new FrmCombate(nombre, combate))
                {
                    frm.ShowDialog(this);
                }
            }
        }

        // Partida de dos jugadores en red. Pasos:
        //  1) FrmLan conecta los dos PCs (uno es anfitrión y el otro invitado).
        //  2) Cada jugador arma su equipo (el anfitrión también elige el escenario).
        //  3) Se intercambian los equipos con un mensaje CONFIG y los dos PCs crean el mismo combate:
        //     el equipo del anfitrión es el "Jugador" y el del invitado es el "Rival".
        //  4) FrmCombate se encarga de enviar y recibir las jugadas.
        private void PartidaLan()
        {
            string nombre = LeerNombre();
            if (nombre == null) return;

            RedLan red = null;
            try
            {
                using (FrmLan conexion = new FrmLan())
                {
                    if (conexion.ShowDialog(this) != DialogResult.OK) return;
                    red = conexion.Red;
                }
                bool soyAnfitrion = red.EsAnfitrion;

                // Cada uno arma su equipo. Si cierra la ventana sin terminar, se cancela la partida.
                ConfigPartida propia = ArmarConfigPropia(nombre, soyAnfitrion);
                if (propia == null) return;

                // Se anuncia el equipo propio y se espera el del otro jugador.
                red.EnviarTexto(propia.ATexto());
                ConfigPartida ajena = EsperarConfigDelOtroJugador(red);
                if (ajena == null) return;

                // Los dos PCs arman el mismo combate: primero el equipo del anfitrión, luego el del invitado.
                ConfigPartida delAnfitrion = ajena;
                ConfigPartida delInvitado = propia;
                if (soyAnfitrion)
                {
                    delAnfitrion = propia;
                    delInvitado = ajena;
                }
                if (delAnfitrion.IndiceEscenario < 0)
                {
                    MessageBox.Show(Idioma.Texto("El anfitrión no envió el escenario.", "The host did not send the scenario."), Idioma.Texto("Error de conexión", "Connection error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                EquipoCombate equipoAnfitrion = new EquipoCombate(delAnfitrion.NombreEquipo, delAnfitrion.Equipo, true);
                EquipoCombate equipoInvitado = new EquipoCombate(delInvitado.NombreEquipo, delInvitado.Equipo, false);
                Combate combate = new Combate(CatalogoBase.Escenarios[delAnfitrion.IndiceEscenario], equipoAnfitrion, equipoInvitado, delAnfitrion.Semilla);

                using (FrmCombate frm = new FrmCombate(nombre, combate, red, soyAnfitrion))
                {
                    frm.ShowDialog(this);
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(Idioma.Texto("El equipo recibido no es válido: ", "The received team is not valid: ") + ex.Message, Idioma.Texto("Error de conexión", "Connection error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (red != null) red.Cerrar();
            }
        }

        // El jugador arma su equipo (el invitado no elige escenario). Devuelve null si cerró la ventana sin terminar.
        private ConfigPartida ArmarConfigPropia(string nombre, bool soyAnfitrion)
        {
            using (FrmSeleccionEquipo seleccion = new FrmSeleccionEquipo(CatalogoBase.Personajes, CatalogoBase.Escenarios, nombre))
            {
                seleccion.PermitirElegirEscenario = soyAnfitrion;
                if (seleccion.ShowDialog(this) != DialogResult.OK) return null;

                ConfigPartida propia = new ConfigPartida();
                propia.NombreEquipo = seleccion.NombreEquipo;
                propia.Equipo = seleccion.EquipoElegido;
                if (soyAnfitrion)
                {
                    propia.IndiceEscenario = CatalogoBase.Escenarios.IndexOf(seleccion.EscenarioElegido);
                    propia.Semilla = azar.Next();
                }
                return propia;
            }
        }

        // Muestra una ventana de espera hasta que llega el CONFIG del otro jugador. Devuelve null si se canceló o llegó dañado.
        private ConfigPartida EsperarConfigDelOtroJugador(RedLan red)
        {
            using (FrmEsperaLan espera = new FrmEsperaLan(red, Idioma.Texto("Tu equipo ya fue enviado. Cuando el otro jugador termine de armar el suyo empezará el combate.", "Your team has been sent. When the other player finishes building theirs, the battle will begin.")))
            {
                if (espera.ShowDialog(this) != DialogResult.OK) return null;

                ConfigPartida config = ConfigPartida.DeTexto(espera.Mensaje);
                if (config == null)
                    MessageBox.Show(Idioma.Texto("El otro jugador envió un equipo que no es válido.", "The other player sent an invalid team."), Idioma.Texto("Error de conexión", "Connection error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return config;
            }
        }

        // Elige 5 personajes distintos al azar para el equipo de la CPU.
        private List<Personaje> SortearRival()
        {
            List<Personaje> disponibles = new List<Personaje>(CatalogoBase.Personajes);
            List<Personaje> elegidos = new List<Personaje>();
            for (int i = 0; i < EquipoCombate.TamanoEquipo; i++)
            {
                int k = azar.Next(disponibles.Count);
                elegidos.Add(disponibles[k]);
                disponibles.RemoveAt(k);
            }
            return elegidos;
        }

        // Abre el Códice Elemental: galería de personajes con su historia y su reflexión filosófica.
        private void VerCodice()
        {
            using (FrmCodice frm = new FrmCodice(CatalogoBase.Personajes))
            {
                frm.ShowDialog(this);
            }
        }

        private void VerEstadisticas()
        {
            string nombre = LeerNombre();
            if (nombre == null) return;

            using (FrmEstadisticas frm = new FrmEstadisticas(nombre, RegistroPartidas.DeJugador(nombre)))
            {
                frm.ShowDialog(this);
            }
        }
    }
}
