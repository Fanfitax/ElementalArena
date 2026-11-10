using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Modelos;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    // Pantalla para armar el equipo de 5 personajes y elegir el escenario.
    //  - Izquierda: la galería con los 12 personajes (una columna por elemento). Clic = ver la carta, doble clic = agregar.
    //  - Centro: la carta grande del personaje seleccionado.
    //  - Derecha: las 3 casillas del equipo. El orden importa: el 1.º sale al campo y los otros esperan en la COLA de relevos.
    public class FrmSeleccionEquipo : FormularioModerno
    {
        private static Color PanelOscuro = Color.FromArgb(205, 10, 12, 18);

        private List<Personaje> catalogo = new List<Personaje>();
        private List<Escenario> escenarios = new List<Escenario>();
        private List<Personaje> equipo = new List<Personaje>();
        private Personaje seleccionado;
        private int indiceEscenario;

        // Los controles fijos se declaran como campos para que el diseñador de SharpDevelop los reconozca.
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Button btnVolver;
        private Label lblColumnaFuego;
        private Label lblColumnaAgua;
        private Label lblColumnaTierra;
        private Label lblColumnaAire;
        private Panel panelDetalle;
        private Label lblDetalle;
        private Button btnAgregar;
        private Label lblConsejo;
        private Label lblEquipo;
        private Label lblNombreEquipo;
        private TextBox txtNombreEquipo;
        private Label etEscenario;
        private Label lblEscenario;
        private Button btnCombatir;

        // Los controles que dependen de los datos se crean por código.
        private MiniaturaPersonaje[] miniaturas;
        private VistaCarta vistaPrevia;
        private Button[] botonesEscenario;

        // Las 5 casillas del equipo
        private Panel[] casilla = new Panel[EquipoCombate.TamanoEquipo];
        private PictureBox[] casillaImagen = new PictureBox[EquipoCombate.TamanoEquipo];
        private Label[] casillaNombre = new Label[EquipoCombate.TamanoEquipo];
        private Label[] casillaRol = new Label[EquipoCombate.TamanoEquipo];
        private Button[] casillaSubir = new Button[EquipoCombate.TamanoEquipo];
        private Button[] casillaBajar = new Button[EquipoCombate.TamanoEquipo];
        private Button[] casillaQuitar = new Button[EquipoCombate.TamanoEquipo];

        public FrmSeleccionEquipo(List<Personaje> catalogo, List<Escenario> escenarios, string nombreJugador)
        {
            this.catalogo = new List<Personaje>(catalogo);
            this.escenarios = new List<Escenario>(escenarios);

            InitializeComponent();

            // Lo que cambia según el idioma (el diseñador solo muestra la versión en español).
            Text = Idioma.Texto("Arena Elemental - Arma tu equipo", "Arena Elemental - Build your team");
            lblTitulo.Text = Idioma.Texto("ARMA TU EQUIPO", "BUILD YOUR TEAM");
            lblSubtitulo.Text = Idioma.Texto("Elige 5 personajes. El 1.º sale al campo y los otros esperan en la cola de relevos, en el orden que pongas.", "Pick 5 characters. The 1st goes onto the field and the others wait in the swap queue, in the order you set.");
            Estilo.EstilizarBoton(btnVolver, Idioma.Texto("Volver", "Back"), Estilo.PanelClaro);
            lblColumnaFuego.Text = Idioma.Texto("FUEGO", "FIRE") + "  ·  " + Estilo.RolDe(Elemento.Fuego);
            lblColumnaAgua.Text = Idioma.Texto("AGUA", "WATER") + "  ·  " + Estilo.RolDe(Elemento.Agua);
            lblColumnaTierra.Text = Idioma.Texto("TIERRA", "EARTH") + "  ·  " + Estilo.RolDe(Elemento.Tierra);
            lblColumnaAire.Text = Idioma.Texto("AIRE", "AIR") + "  ·  " + Estilo.RolDe(Elemento.Aire);
            Estilo.Redondear(panelDetalle, 12);
            Estilo.EstilizarBoton(btnAgregar, Idioma.Texto("AGREGAR AL EQUIPO", "ADD TO TEAM"), Color.FromArgb(46, 160, 96));
            lblConsejo.Text = Idioma.Texto("Clic: ver la carta\nDoble clic: agregar al equipo", "Click: view the card\nDouble click: add to the team");
            lblEquipo.Text = Idioma.Texto("TU EQUIPO", "YOUR TEAM");
            lblNombreEquipo.Text = Idioma.Texto("NOMBRE DEL EQUIPO", "TEAM NAME");
            txtNombreEquipo.Text = Idioma.Texto("Equipo de ", "Team of ") + nombreJugador;
            etEscenario.Text = Idioma.Texto("ESCENARIO", "SCENARIO");
            Estilo.EstilizarBoton(btnCombatir, Idioma.Texto("¡A COMBATIR!", "FIGHT!"), Color.FromArgb(46, 160, 96));
            btnCombatir.Font = new Font("Segoe UI", 15f, FontStyle.Bold);

            ConstruirGaleria();
            ConstruirVistaPrevia();
            ConstruirPanelEquipo();

            if (this.catalogo.Count > 0) Seleccionar(this.catalogo[0]);
            ElegirEscenario(0);
            RefrescarEquipo();
        }

        public List<Personaje> EquipoElegido { get { return new List<Personaje>(equipo); } }
        public Escenario EscenarioElegido { get { return escenarios[indiceEscenario]; } }
        public string NombreEquipo { get { return txtNombreEquipo.Text.Trim(); } }

        // Si es false se oculta la elección de escenario. Se usa en el modo LAN para el jugador que se une:
        // el escenario lo elige el anfitrión.
        public bool PermitirElegirEscenario
        {
            get { return etEscenario.Visible; }
            set
            {
                etEscenario.Visible = value;
                lblEscenario.Visible = value;
                foreach (Button b in botonesEscenario)
                    b.Visible = value;
                if (!value)
                {
                    lblConsejo.Text = Idioma.Texto("El escenario lo elige el anfitrión de la partida.", "The scenario is chosen by the match host.");
                }
            }
        }

        // ---------- Construcción de la interfaz ----------

        // La galería: 4 columnas (Fuego, Agua, Tierra, Aire) con 3 personajes cada una.
        // Los títulos de las columnas ya están en InitializeComponent.
        private void ConstruirGaleria()
        {
            miniaturas = new MiniaturaPersonaje[catalogo.Count];
            for (int i = 0; i < catalogo.Count; i++)
            {
                int columna = i / 3;
                int fila = i % 3;
                MiniaturaPersonaje m = new MiniaturaPersonaje(catalogo[i]);
                m.Location = new Point(20 + columna * 148, 94 + fila * 162);
                m.Click += Miniatura_Click;
                m.DoubleClick += Miniatura_DoubleClick;
                Controls.Add(m);
                miniaturas[i] = m;
            }
        }

        // La carta grande es un control propio que se llena con los datos del personaje.
        private void ConstruirVistaPrevia()
        {
            vistaPrevia = new VistaCarta(TamanoCarta.Seleccion);
            vistaPrevia.Location = new Point(624, 72);
            Controls.Add(vistaPrevia);
        }

        // Las casillas del equipo y los botones de escenario dependen de los datos, así que se crean aquí.
        private void ConstruirPanelEquipo()
        {
            for (int i = 0; i < EquipoCombate.TamanoEquipo; i++)
                CrearCasilla(i);

            botonesEscenario = new Button[escenarios.Count];
            for (int i = 0; i < escenarios.Count; i++)
            {
                Button b = new Button();
                b.Tag = i;   // el número del escenario, para saber cuál botón se presionó
                b.SetBounds(876 + (i % 2) * 186, 432 + (i / 2) * 40, 178, 34);
                Color color = Estilo.FondoDe(escenarios[i].ElementoAfin);
                Estilo.EstilizarBoton(b, escenarios[i].Nombre, Color.FromArgb(color.R + 30, color.G + 30, color.B + 30));
                b.Click += Escenario_Click;
                Controls.Add(b);
                botonesEscenario[i] = b;
            }
        }

        // Código del diseñador: aquí se crean y se colocan los controles fijos de la ventana.
        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.btnVolver = new System.Windows.Forms.Button();
            this.lblColumnaFuego = new System.Windows.Forms.Label();
            this.lblColumnaAgua = new System.Windows.Forms.Label();
            this.lblColumnaTierra = new System.Windows.Forms.Label();
            this.lblColumnaAire = new System.Windows.Forms.Label();
            this.panelDetalle = new System.Windows.Forms.Panel();
            this.lblDetalle = new System.Windows.Forms.Label();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.lblConsejo = new System.Windows.Forms.Label();
            this.lblEquipo = new System.Windows.Forms.Label();
            this.lblNombreEquipo = new System.Windows.Forms.Label();
            this.txtNombreEquipo = new System.Windows.Forms.TextBox();
            this.etEscenario = new System.Windows.Forms.Label();
            this.lblEscenario = new System.Windows.Forms.Label();
            this.btnCombatir = new System.Windows.Forms.Button();
            this.panelDetalle.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.Gold;
            this.lblTitulo.Location = new System.Drawing.Point(20, 8);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(600, 36);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "ARMA TU EQUIPO";
            //
            // lblSubtitulo
            //
            this.lblSubtitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblSubtitulo.Location = new System.Drawing.Point(22, 44);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(900, 20);
            this.lblSubtitulo.TabIndex = 1;
            this.lblSubtitulo.Text = "Elige 5 personajes. El 1.º sale al campo y los otros esperan en la cola de relevos, en el orden que pongas.";
            //
            // btnVolver
            //
            this.btnVolver.BackColor = System.Drawing.Color.FromArgb(58, 62, 92);
            this.btnVolver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVolver.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVolver.ForeColor = System.Drawing.Color.White;
            this.btnVolver.Location = new System.Drawing.Point(1130, 14);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(110, 32);
            this.btnVolver.TabIndex = 2;
            this.btnVolver.Text = "Volver";
            this.btnVolver.UseVisualStyleBackColor = false;
            this.btnVolver.Click += new System.EventHandler(this.BtnVolver_Click);
            //
            // lblColumnaFuego
            //
            this.lblColumnaFuego.BackColor = System.Drawing.Color.Transparent;
            this.lblColumnaFuego.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblColumnaFuego.ForeColor = System.Drawing.Color.FromArgb(226, 88, 52);
            this.lblColumnaFuego.Location = new System.Drawing.Point(20, 72);
            this.lblColumnaFuego.Name = "lblColumnaFuego";
            this.lblColumnaFuego.Size = new System.Drawing.Size(140, 18);
            this.lblColumnaFuego.TabIndex = 3;
            this.lblColumnaFuego.Text = "FUEGO  ·  ATACANTE";
            //
            // lblColumnaAgua
            //
            this.lblColumnaAgua.BackColor = System.Drawing.Color.Transparent;
            this.lblColumnaAgua.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblColumnaAgua.ForeColor = System.Drawing.Color.FromArgb(52, 136, 226);
            this.lblColumnaAgua.Location = new System.Drawing.Point(168, 72);
            this.lblColumnaAgua.Name = "lblColumnaAgua";
            this.lblColumnaAgua.Size = new System.Drawing.Size(140, 18);
            this.lblColumnaAgua.TabIndex = 4;
            this.lblColumnaAgua.Text = "AGUA  ·  APOYO";
            //
            // lblColumnaTierra
            //
            this.lblColumnaTierra.BackColor = System.Drawing.Color.Transparent;
            this.lblColumnaTierra.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblColumnaTierra.ForeColor = System.Drawing.Color.FromArgb(150, 108, 62);
            this.lblColumnaTierra.Location = new System.Drawing.Point(316, 72);
            this.lblColumnaTierra.Name = "lblColumnaTierra";
            this.lblColumnaTierra.Size = new System.Drawing.Size(140, 18);
            this.lblColumnaTierra.TabIndex = 5;
            this.lblColumnaTierra.Text = "TIERRA  ·  TANQUE";
            //
            // lblColumnaAire
            //
            this.lblColumnaAire.BackColor = System.Drawing.Color.Transparent;
            this.lblColumnaAire.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblColumnaAire.ForeColor = System.Drawing.Color.FromArgb(120, 196, 196);
            this.lblColumnaAire.Location = new System.Drawing.Point(464, 72);
            this.lblColumnaAire.Name = "lblColumnaAire";
            this.lblColumnaAire.Size = new System.Drawing.Size(140, 18);
            this.lblColumnaAire.TabIndex = 6;
            this.lblColumnaAire.Text = "AIRE  ·  VELOZ";
            //
            // panelDetalle
            //
            this.panelDetalle.BackColor = System.Drawing.Color.FromArgb(205, 10, 12, 18);
            this.panelDetalle.Controls.Add(this.lblDetalle);
            this.panelDetalle.Location = new System.Drawing.Point(20, 588);
            this.panelDetalle.Name = "panelDetalle";
            this.panelDetalle.Size = new System.Drawing.Size(584, 64);
            this.panelDetalle.TabIndex = 7;
            //
            // lblDetalle
            //
            this.lblDetalle.BackColor = System.Drawing.Color.Transparent;
            this.lblDetalle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDetalle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblDetalle.Location = new System.Drawing.Point(12, 5);
            this.lblDetalle.Name = "lblDetalle";
            this.lblDetalle.Size = new System.Drawing.Size(562, 56);
            this.lblDetalle.TabIndex = 0;
            //
            // btnAgregar
            //
            this.btnAgregar.BackColor = System.Drawing.Color.FromArgb(46, 160, 96);
            this.btnAgregar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAgregar.ForeColor = System.Drawing.Color.White;
            this.btnAgregar.Location = new System.Drawing.Point(624, 562);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(232, 42);
            this.btnAgregar.TabIndex = 8;
            this.btnAgregar.Text = "AGREGAR AL EQUIPO";
            this.btnAgregar.UseVisualStyleBackColor = false;
            this.btnAgregar.Click += new System.EventHandler(this.BtnAgregar_Click);
            //
            // lblConsejo
            //
            this.lblConsejo.BackColor = System.Drawing.Color.Transparent;
            this.lblConsejo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblConsejo.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblConsejo.Location = new System.Drawing.Point(624, 608);
            this.lblConsejo.Name = "lblConsejo";
            this.lblConsejo.Size = new System.Drawing.Size(232, 44);
            this.lblConsejo.TabIndex = 9;
            this.lblConsejo.Text = "Clic: ver la carta\nDoble clic: agregar al equipo";
            //
            // lblEquipo
            //
            this.lblEquipo.BackColor = System.Drawing.Color.Transparent;
            this.lblEquipo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblEquipo.ForeColor = System.Drawing.Color.Gold;
            this.lblEquipo.Location = new System.Drawing.Point(876, 72);
            this.lblEquipo.Name = "lblEquipo";
            this.lblEquipo.Size = new System.Drawing.Size(364, 18);
            this.lblEquipo.TabIndex = 10;
            this.lblEquipo.Text = "TU EQUIPO";
            //
            // lblNombreEquipo
            //
            this.lblNombreEquipo.BackColor = System.Drawing.Color.Transparent;
            this.lblNombreEquipo.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblNombreEquipo.ForeColor = System.Drawing.Color.Gold;
            this.lblNombreEquipo.Location = new System.Drawing.Point(876, 358);
            this.lblNombreEquipo.Name = "lblNombreEquipo";
            this.lblNombreEquipo.Size = new System.Drawing.Size(364, 18);
            this.lblNombreEquipo.TabIndex = 11;
            this.lblNombreEquipo.Text = "NOMBRE DEL EQUIPO";
            //
            // txtNombreEquipo
            //
            this.txtNombreEquipo.BackColor = System.Drawing.Color.FromArgb(16, 20, 32);
            this.txtNombreEquipo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNombreEquipo.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtNombreEquipo.ForeColor = System.Drawing.Color.White;
            this.txtNombreEquipo.Location = new System.Drawing.Point(876, 376);
            this.txtNombreEquipo.MaxLength = 40;
            this.txtNombreEquipo.Name = "txtNombreEquipo";
            this.txtNombreEquipo.Size = new System.Drawing.Size(364, 27);
            this.txtNombreEquipo.TabIndex = 12;
            this.txtNombreEquipo.TextChanged += new System.EventHandler(this.TxtNombreEquipo_TextChanged);
            //
            // etEscenario
            //
            this.etEscenario.BackColor = System.Drawing.Color.Transparent;
            this.etEscenario.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.etEscenario.ForeColor = System.Drawing.Color.Gold;
            this.etEscenario.Location = new System.Drawing.Point(876, 412);
            this.etEscenario.Name = "etEscenario";
            this.etEscenario.Size = new System.Drawing.Size(364, 18);
            this.etEscenario.TabIndex = 13;
            this.etEscenario.Text = "ESCENARIO";
            //
            // lblEscenario
            //
            this.lblEscenario.BackColor = System.Drawing.Color.Transparent;
            this.lblEscenario.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEscenario.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblEscenario.Location = new System.Drawing.Point(876, 508);
            this.lblEscenario.Name = "lblEscenario";
            this.lblEscenario.Size = new System.Drawing.Size(364, 46);
            this.lblEscenario.TabIndex = 14;
            this.lblEscenario.Text = "";
            //
            // btnCombatir
            //
            this.btnCombatir.BackColor = System.Drawing.Color.FromArgb(46, 160, 96);
            this.btnCombatir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCombatir.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.btnCombatir.ForeColor = System.Drawing.Color.White;
            this.btnCombatir.Location = new System.Drawing.Point(876, 564);
            this.btnCombatir.Name = "btnCombatir";
            this.btnCombatir.Size = new System.Drawing.Size(364, 58);
            this.btnCombatir.TabIndex = 15;
            this.btnCombatir.Text = "¡A COMBATIR!";
            this.btnCombatir.UseVisualStyleBackColor = false;
            this.btnCombatir.Click += new System.EventHandler(this.BtnCombatir_Click);
            //
            // FrmSeleccionEquipo
            //
            this.BackColor = System.Drawing.Color.FromArgb(24, 26, 40);
            this.ClientSize = new System.Drawing.Size(1260, 660);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblSubtitulo);
            this.Controls.Add(this.btnVolver);
            this.Controls.Add(this.lblColumnaFuego);
            this.Controls.Add(this.lblColumnaAgua);
            this.Controls.Add(this.lblColumnaTierra);
            this.Controls.Add(this.lblColumnaAire);
            this.Controls.Add(this.panelDetalle);
            this.Controls.Add(this.btnAgregar);
            this.Controls.Add(this.lblConsejo);
            this.Controls.Add(this.lblEquipo);
            this.Controls.Add(this.lblNombreEquipo);
            this.Controls.Add(this.txtNombreEquipo);
            this.Controls.Add(this.etEscenario);
            this.Controls.Add(this.lblEscenario);
            this.Controls.Add(this.btnCombatir);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmSeleccionEquipo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Arena Elemental - Arma tu equipo";
            this.panelDetalle.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        // Una casilla del equipo: imagen, nombre, su lugar en la cola y los botones para subir, bajar o quitar.
        private void CrearCasilla(int i)
        {
            Panel panel = new Panel();
            panel.SetBounds(876, 96 + i * 52, 364, 46);
            panel.BackColor = PanelOscuro;
            Controls.Add(panel);
            Estilo.Redondear(panel, 12);
            casilla[i] = panel;

            casillaImagen[i] = new PictureBox();
            casillaImagen[i].SetBounds(4, 4, 38, 38);
            panel.Controls.Add(casillaImagen[i]);

            casillaNombre[i] = Estilo.CrearEtiqueta(string.Empty, 50, 3, 214, 22, 11f, true);
            panel.Controls.Add(casillaNombre[i]);

            casillaRol[i] = Estilo.CrearEtiqueta(string.Empty, 50, 25, 214, 18, 8f, true);
            panel.Controls.Add(casillaRol[i]);

            // Cada botón guarda en Tag el número de su casilla.
            casillaSubir[i] = CrearBotonCasilla(panel, 270, "▲");
            casillaSubir[i].Tag = i;
            casillaSubir[i].Click += CasillaSubir_Click;
            casillaBajar[i] = CrearBotonCasilla(panel, 302, "▼");
            casillaBajar[i].Tag = i;
            casillaBajar[i].Click += CasillaBajar_Click;
            casillaQuitar[i] = CrearBotonCasilla(panel, 334, "×");
            casillaQuitar[i].Tag = i;
            casillaQuitar[i].Click += CasillaQuitar_Click;
            casillaQuitar[i].BackColor = Color.FromArgb(150, 50, 60);
        }

        private Button CrearBotonCasilla(Panel panel, int x, string texto)
        {
            Button b = new Button();
            b.SetBounds(x, 7, 28, 32);
            Estilo.EstilizarBoton(b, texto, Estilo.PanelClaro);
            b.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            b.Padding = new Padding(0);
            panel.Controls.Add(b);
            return b;
        }

        // ---------- Eventos de los controles ----------

        private void BtnVolver_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnCombatir_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            AgregarSeleccionado();
        }

        private void TxtNombreEquipo_TextChanged(object sender, EventArgs e)
        {
            ActualizarBotonCombatir();
        }

        private void Miniatura_Click(object sender, EventArgs e)
        {
            MiniaturaPersonaje miniatura = (MiniaturaPersonaje)sender;
            Seleccionar(miniatura.Personaje);
        }

        private void Miniatura_DoubleClick(object sender, EventArgs e)
        {
            MiniaturaPersonaje miniatura = (MiniaturaPersonaje)sender;
            Seleccionar(miniatura.Personaje);
            AgregarSeleccionado();
        }

        private void Escenario_Click(object sender, EventArgs e)
        {
            Button boton = (Button)sender;
            ElegirEscenario((int)boton.Tag);
        }

        private void CasillaSubir_Click(object sender, EventArgs e)
        {
            MoverEnEquipo((int)((Button)sender).Tag, -1);
        }

        private void CasillaBajar_Click(object sender, EventArgs e)
        {
            MoverEnEquipo((int)((Button)sender).Tag, 1);
        }

        private void CasillaQuitar_Click(object sender, EventArgs e)
        {
            QuitarDelEquipo((int)((Button)sender).Tag);
        }

        // ---------- Selección y equipo ----------

        // Muestra la carta del personaje y sus habilidades.
        private void Seleccionar(Personaje p)
        {
            seleccionado = p;
            vistaPrevia.ActualizarDatos(p, Estilo.RolDe(p.Elemento), false);
            lblDetalle.Text = Estilo.DescripcionTriangulo(p) + "\n" + Estilo.DescripcionHabilidad(p.HabilidadAtaque)
                              + "\n" + Estilo.DescripcionHabilidad(p.HabilidadUtilidad);

            foreach (MiniaturaPersonaje m in miniaturas)
                m.Seleccionada = (m.Personaje == p);
            ActualizarBotonAgregar();
        }

        private void AgregarSeleccionado()
        {
            if (seleccionado == null || equipo.Contains(seleccionado) || equipo.Count >= EquipoCombate.TamanoEquipo)
                return;
            equipo.Add(seleccionado);
            RefrescarEquipo();
        }

        private void QuitarDelEquipo(int indice)
        {
            if (indice < 0 || indice >= equipo.Count) return;
            equipo.RemoveAt(indice);
            RefrescarEquipo();
        }

        private void MoverEnEquipo(int indice, int delta)
        {
            int destino = indice + delta;
            if (indice < 0 || indice >= equipo.Count || destino < 0 || destino >= equipo.Count) return;
            Personaje temporal = equipo[indice];
            equipo[indice] = equipo[destino];
            equipo[destino] = temporal;
            RefrescarEquipo();
        }

        // Actualiza las 3 casillas, las marcas de la galería y los botones.
        private void RefrescarEquipo()
        {
            for (int i = 0; i < EquipoCombate.TamanoEquipo; i++)
            {
                bool ocupada = i < equipo.Count;
                if (ocupada)
                {
                    Personaje p = equipo[i];
                    casillaImagen[i].BackColor = Estilo.ColorDe(p.Elemento);
                    casillaImagen[i].Image = ObtenerImagenCasilla(p);
                    casillaNombre[i].Text = p.Nombre + "  ·  " + Idioma.NombreElemento(p.Elemento);
                    casillaNombre[i].ForeColor = Color.White;
                    if (i == 0)
                    {
                        casillaRol[i].Text = Idioma.Texto("1.º  ·  INICIA EN EL CAMPO", "1st  ·  STARTS ON THE FIELD");
                        casillaRol[i].ForeColor = Color.Gold;
                    }
                    else
                    {
                        casillaRol[i].Text = (i + 1) + Idioma.Texto(".º  ·  EN LA COLA DE RELEVO", ".  ·  IN THE SWAP QUEUE");
                        casillaRol[i].ForeColor = Estilo.TextoSuave;
                    }
                }
                else
                {
                    casillaImagen[i].BackColor = Color.FromArgb(28, 32, 46);
                    casillaImagen[i].Image = null;
                    casillaNombre[i].Text = Idioma.Texto("— vacío —", "— empty —");
                    casillaNombre[i].ForeColor = Estilo.TextoSuave;
                    if (i == 0) casillaRol[i].Text = Idioma.Texto("Elige quién inicia el combate", "Pick who starts the battle");
                    else casillaRol[i].Text = Idioma.Texto("Elige otro personaje", "Pick another character");
                    casillaRol[i].ForeColor = Estilo.TextoSuave;
                }
                casillaSubir[i].Visible = ocupada;
                casillaBajar[i].Visible = ocupada;
                casillaQuitar[i].Visible = ocupada;
                casillaSubir[i].Enabled = i > 0;
                casillaBajar[i].Enabled = i < equipo.Count - 1;
            }

            foreach (MiniaturaPersonaje m in miniaturas)
                m.PosicionEquipo = equipo.IndexOf(m.Personaje) + 1;

            ActualizarBotonAgregar();
            ActualizarBotonCombatir();
        }

        private Image ObtenerImagenCasilla(Personaje p)
        {
            try
            {
                return Ilustraciones.ObtenerArte(p, 56, 56);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ActualizarBotonAgregar()
        {
            bool yaEsta = seleccionado != null && equipo.Contains(seleccionado);
            bool completo = equipo.Count >= EquipoCombate.TamanoEquipo;
            btnAgregar.Enabled = seleccionado != null && !yaEsta && !completo;
            if (yaEsta) btnAgregar.Text = Idioma.Texto("YA ESTÁ EN TU EQUIPO", "ALREADY IN YOUR TEAM");
            else if (completo) btnAgregar.Text = Idioma.Texto("EQUIPO COMPLETO", "TEAM COMPLETE");
            else btnAgregar.Text = Idioma.Texto("AGREGAR AL EQUIPO", "ADD TO TEAM");
        }

        private void ActualizarBotonCombatir()
        {
            bool listo = equipo.Count == EquipoCombate.TamanoEquipo && NombreEquipo.Length > 0;
            btnCombatir.Enabled = listo;
            if (listo) btnCombatir.BackColor = Color.FromArgb(46, 160, 96);
            else btnCombatir.BackColor = Estilo.PanelClaro;
            if (equipo.Count < EquipoCombate.TamanoEquipo)
                btnCombatir.Text = Idioma.Texto("Elige ", "Pick ") + (EquipoCombate.TamanoEquipo - equipo.Count) + Idioma.Texto(" personaje(s) más", " more character(s)");
            else if (NombreEquipo.Length == 0)
                btnCombatir.Text = Idioma.Texto("Escribe un nombre para el equipo", "Type a name for the team");
            else
                btnCombatir.Text = Idioma.Texto("¡A COMBATIR!", "FIGHT!");
        }

        // ---------- Escenario ----------

        private void ElegirEscenario(int indice)
        {
            if (indice < 0 || indice >= escenarios.Count) return;
            indiceEscenario = indice;

            for (int i = 0; i < botonesEscenario.Length; i++)
            {
                bool elegido = (i == indice);
                if (elegido) botonesEscenario[i].FlatAppearance.BorderSize = 3;
                else botonesEscenario[i].FlatAppearance.BorderSize = 0;
                botonesEscenario[i].FlatAppearance.BorderColor = Color.Gold;
            }

            Escenario s = escenarios[indice];
            lblEscenario.Text = s.Descripcion + "\nBonus: +" + s.BonusAtaquePct + Idioma.Texto("% de ATK a las cartas de ", "% ATK to the cards of ") + Idioma.NombreElemento(s.ElementoAfin) + ".";
        }
    }
}
