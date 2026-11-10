using System;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Modelos;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    // Pantalla para conectar dos PCs de la misma red:
    //  - Izquierda: ser el ANFITRIÓN (servidor). Se abre un puerto y se espera; se muestra la IP de este PC
    //    para dársela al otro jugador.
    //  - Derecha: UNIRSE por IP (cliente). Se escribe la IP del anfitrión y se conecta.
    // Cuando los dos PCs quedan conectados la ventana se cierra con DialogResult.OK y la conexión
    // queda disponible en la propiedad Red.
    public class FrmLan : FormularioModerno
    {
        private const int SegundosMaximosParaConectar = 10;

        private RedLan red;
        private Timer timer;
        private DateTime inicioConexion;
        private bool esperando;


        // Los controles se declaran como campos para que el diseñador de SharpDevelop los reconozca.
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Label lblEstado;
        private Button btnVolver;

        private Panel panelAnfitrion;
        private Label lblTituloAnfitrion;
        private Label lblTextoAnfitrion;
        private Label lblIp;
        private Label lblPuertoAnfitrion;
        private TextBox txtPuertoAnfitrion;
        private Button btnCrear;

        private Panel panelInvitado;
        private Label lblTituloInvitado;
        private Label lblTextoInvitado;
        private Label lblEtiquetaIp;
        private TextBox txtIp;
        private Label lblPuertoInvitado;
        private TextBox txtPuertoInvitado;
        private Button btnConectar;

        public FrmLan()
        {
            InitializeComponent();

            // Lo que cambia según el idioma (el diseñador solo muestra la versión en español).
            Text = Idioma.Texto("Arena Elemental - Partida LAN", "Arena Elemental - LAN Match");
            lblTitulo.Text = Idioma.Texto("PARTIDA LAN", "LAN MATCH");
            lblSubtitulo.Text = Idioma.Texto("Dos jugadores en dos PCs conectados a la misma red local (mismo Wi-Fi o cable).", "Two players on two PCs connected to the same local network (same Wi-Fi or cable).");
            lblEstado.Text = Idioma.Texto("Elige una opción.", "Choose an option.");

            lblTituloAnfitrion.Text = Idioma.Texto("ANFITRIÓN (SERVIDOR)", "HOST (SERVER)");
            lblTextoAnfitrion.Text = Idioma.Texto("Crea la partida y espera. Tú eliges el escenario y empiezas el combate.", "Create the match and wait. You choose the scenario and start the battle.");
            lblIp.Text = Idioma.Texto("Tu IP:  ", "Your IP:  ") + DireccionesDeEsteEquipo();
            lblPuertoAnfitrion.Text = Idioma.Texto("PUERTO", "PORT");
            txtPuertoAnfitrion.Text = RedLan.PuertoPorDefecto.ToString();

            lblTituloInvitado.Text = Idioma.Texto("UNIRSE POR IP (CLIENTE)", "JOIN BY IP (CLIENT)");
            lblTextoInvitado.Text = Idioma.Texto("Escribe la IP que le aparece al anfitrión en su pantalla.", "Type the IP shown on the host's screen.");
            lblEtiquetaIp.Text = Idioma.Texto("IP DEL ANFITRIÓN", "HOST IP");
            lblPuertoInvitado.Text = Idioma.Texto("PUERTO", "PORT");
            txtPuertoInvitado.Text = RedLan.PuertoPorDefecto.ToString();

            // Botones planos con esquinas redondeadas y paneles redondeados.
            Estilo.EstilizarBoton(btnVolver, Idioma.Texto("Volver", "Back"), Estilo.PanelClaro);
            Estilo.EstilizarBoton(btnCrear, Idioma.Texto("CREAR PARTIDA", "CREATE MATCH"), Color.FromArgb(46, 160, 96));
            btnCrear.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            Estilo.EstilizarBoton(btnConectar, Idioma.Texto("CONECTAR", "CONNECT"), Estilo.Acento);
            btnConectar.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            Estilo.Redondear(panelAnfitrion, 14);
            Estilo.Redondear(panelInvitado, 14);

            timer = new Timer();
            timer.Interval = 150;
            timer.Tick += Timer_Tick;
        }

        // La conexión ya establecida (solo es válida si la ventana se cerró con DialogResult.OK).
        public RedLan Red
        {
            get { return red; }
        }

        // Código del diseñador: aquí se crean y se colocan los controles de la ventana.
        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.lblEstado = new System.Windows.Forms.Label();
            this.btnVolver = new System.Windows.Forms.Button();
            this.panelAnfitrion = new System.Windows.Forms.Panel();
            this.lblTituloAnfitrion = new System.Windows.Forms.Label();
            this.lblTextoAnfitrion = new System.Windows.Forms.Label();
            this.lblIp = new System.Windows.Forms.Label();
            this.lblPuertoAnfitrion = new System.Windows.Forms.Label();
            this.txtPuertoAnfitrion = new System.Windows.Forms.TextBox();
            this.btnCrear = new System.Windows.Forms.Button();
            this.panelInvitado = new System.Windows.Forms.Panel();
            this.lblTituloInvitado = new System.Windows.Forms.Label();
            this.lblTextoInvitado = new System.Windows.Forms.Label();
            this.lblEtiquetaIp = new System.Windows.Forms.Label();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.lblPuertoInvitado = new System.Windows.Forms.Label();
            this.txtPuertoInvitado = new System.Windows.Forms.TextBox();
            this.btnConectar = new System.Windows.Forms.Button();
            this.panelAnfitrion.SuspendLayout();
            this.panelInvitado.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.Gold;
            this.lblTitulo.Location = new System.Drawing.Point(24, 12);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(500, 38);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "PARTIDA LAN";
            //
            // lblSubtitulo
            //
            this.lblSubtitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblSubtitulo.Location = new System.Drawing.Point(26, 50);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(740, 20);
            this.lblSubtitulo.TabIndex = 1;
            this.lblSubtitulo.Text = "Dos jugadores en dos PCs conectados a la misma red local (mismo Wi-Fi o cable).";
            //
            // panelAnfitrion
            //
            this.panelAnfitrion.BackColor = System.Drawing.Color.FromArgb(205, 10, 12, 18);
            this.panelAnfitrion.Controls.Add(this.lblTituloAnfitrion);
            this.panelAnfitrion.Controls.Add(this.lblTextoAnfitrion);
            this.panelAnfitrion.Controls.Add(this.lblIp);
            this.panelAnfitrion.Controls.Add(this.lblPuertoAnfitrion);
            this.panelAnfitrion.Controls.Add(this.txtPuertoAnfitrion);
            this.panelAnfitrion.Controls.Add(this.btnCrear);
            this.panelAnfitrion.Location = new System.Drawing.Point(24, 84);
            this.panelAnfitrion.Name = "panelAnfitrion";
            this.panelAnfitrion.Size = new System.Drawing.Size(356, 296);
            this.panelAnfitrion.TabIndex = 2;
            //
            // lblTituloAnfitrion
            //
            this.lblTituloAnfitrion.BackColor = System.Drawing.Color.Transparent;
            this.lblTituloAnfitrion.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTituloAnfitrion.ForeColor = System.Drawing.Color.Gold;
            this.lblTituloAnfitrion.Location = new System.Drawing.Point(16, 12);
            this.lblTituloAnfitrion.Name = "lblTituloAnfitrion";
            this.lblTituloAnfitrion.Size = new System.Drawing.Size(324, 24);
            this.lblTituloAnfitrion.TabIndex = 0;
            this.lblTituloAnfitrion.Text = "ANFITRIÓN (SERVIDOR)";
            //
            // lblTextoAnfitrion
            //
            this.lblTextoAnfitrion.BackColor = System.Drawing.Color.Transparent;
            this.lblTextoAnfitrion.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTextoAnfitrion.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblTextoAnfitrion.Location = new System.Drawing.Point(16, 40);
            this.lblTextoAnfitrion.Name = "lblTextoAnfitrion";
            this.lblTextoAnfitrion.Size = new System.Drawing.Size(324, 56);
            this.lblTextoAnfitrion.TabIndex = 1;
            this.lblTextoAnfitrion.Text = "Crea la partida y espera. Tú eliges el escenario y empiezas el combate.";
            //
            // lblIp
            //
            this.lblIp.BackColor = System.Drawing.Color.Transparent;
            this.lblIp.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblIp.ForeColor = System.Drawing.Color.LightGreen;
            this.lblIp.Location = new System.Drawing.Point(16, 104);
            this.lblIp.Name = "lblIp";
            this.lblIp.Size = new System.Drawing.Size(324, 44);
            this.lblIp.TabIndex = 2;
            this.lblIp.Text = "Tu IP:  192.168.1.20";
            //
            // lblPuertoAnfitrion
            //
            this.lblPuertoAnfitrion.BackColor = System.Drawing.Color.Transparent;
            this.lblPuertoAnfitrion.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblPuertoAnfitrion.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblPuertoAnfitrion.Location = new System.Drawing.Point(16, 160);
            this.lblPuertoAnfitrion.Name = "lblPuertoAnfitrion";
            this.lblPuertoAnfitrion.Size = new System.Drawing.Size(100, 18);
            this.lblPuertoAnfitrion.TabIndex = 3;
            this.lblPuertoAnfitrion.Text = "PUERTO";
            //
            // txtPuertoAnfitrion
            //
            this.txtPuertoAnfitrion.BackColor = System.Drawing.Color.FromArgb(16, 20, 32);
            this.txtPuertoAnfitrion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPuertoAnfitrion.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtPuertoAnfitrion.ForeColor = System.Drawing.Color.White;
            this.txtPuertoAnfitrion.Location = new System.Drawing.Point(16, 180);
            this.txtPuertoAnfitrion.Name = "txtPuertoAnfitrion";
            this.txtPuertoAnfitrion.Size = new System.Drawing.Size(120, 29);
            this.txtPuertoAnfitrion.TabIndex = 4;
            this.txtPuertoAnfitrion.Text = "8888";
            //
            // btnCrear
            //
            this.btnCrear.BackColor = System.Drawing.Color.FromArgb(46, 160, 96);
            this.btnCrear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCrear.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCrear.ForeColor = System.Drawing.Color.White;
            this.btnCrear.Location = new System.Drawing.Point(16, 232);
            this.btnCrear.Name = "btnCrear";
            this.btnCrear.Size = new System.Drawing.Size(324, 50);
            this.btnCrear.TabIndex = 5;
            this.btnCrear.Text = "CREAR PARTIDA";
            this.btnCrear.UseVisualStyleBackColor = false;
            this.btnCrear.Click += new System.EventHandler(this.BtnCrear_Click);
            //
            // panelInvitado
            //
            this.panelInvitado.BackColor = System.Drawing.Color.FromArgb(205, 10, 12, 18);
            this.panelInvitado.Controls.Add(this.lblTituloInvitado);
            this.panelInvitado.Controls.Add(this.lblTextoInvitado);
            this.panelInvitado.Controls.Add(this.lblEtiquetaIp);
            this.panelInvitado.Controls.Add(this.txtIp);
            this.panelInvitado.Controls.Add(this.lblPuertoInvitado);
            this.panelInvitado.Controls.Add(this.txtPuertoInvitado);
            this.panelInvitado.Controls.Add(this.btnConectar);
            this.panelInvitado.Location = new System.Drawing.Point(412, 84);
            this.panelInvitado.Name = "panelInvitado";
            this.panelInvitado.Size = new System.Drawing.Size(356, 296);
            this.panelInvitado.TabIndex = 3;
            //
            // lblTituloInvitado
            //
            this.lblTituloInvitado.BackColor = System.Drawing.Color.Transparent;
            this.lblTituloInvitado.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTituloInvitado.ForeColor = System.Drawing.Color.Gold;
            this.lblTituloInvitado.Location = new System.Drawing.Point(16, 12);
            this.lblTituloInvitado.Name = "lblTituloInvitado";
            this.lblTituloInvitado.Size = new System.Drawing.Size(324, 24);
            this.lblTituloInvitado.TabIndex = 0;
            this.lblTituloInvitado.Text = "UNIRSE POR IP (CLIENTE)";
            //
            // lblTextoInvitado
            //
            this.lblTextoInvitado.BackColor = System.Drawing.Color.Transparent;
            this.lblTextoInvitado.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTextoInvitado.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblTextoInvitado.Location = new System.Drawing.Point(16, 40);
            this.lblTextoInvitado.Name = "lblTextoInvitado";
            this.lblTextoInvitado.Size = new System.Drawing.Size(324, 56);
            this.lblTextoInvitado.TabIndex = 1;
            this.lblTextoInvitado.Text = "Escribe la IP que le aparece al anfitrión en su pantalla.";
            //
            // lblEtiquetaIp
            //
            this.lblEtiquetaIp.BackColor = System.Drawing.Color.Transparent;
            this.lblEtiquetaIp.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblEtiquetaIp.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblEtiquetaIp.Location = new System.Drawing.Point(16, 104);
            this.lblEtiquetaIp.Name = "lblEtiquetaIp";
            this.lblEtiquetaIp.Size = new System.Drawing.Size(200, 18);
            this.lblEtiquetaIp.TabIndex = 2;
            this.lblEtiquetaIp.Text = "IP DEL ANFITRIÓN";
            //
            // txtIp
            //
            this.txtIp.BackColor = System.Drawing.Color.FromArgb(16, 20, 32);
            this.txtIp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtIp.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtIp.ForeColor = System.Drawing.Color.White;
            this.txtIp.Location = new System.Drawing.Point(16, 124);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(200, 29);
            this.txtIp.TabIndex = 3;
            this.txtIp.Text = "192.168.1.";
            //
            // lblPuertoInvitado
            //
            this.lblPuertoInvitado.BackColor = System.Drawing.Color.Transparent;
            this.lblPuertoInvitado.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblPuertoInvitado.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblPuertoInvitado.Location = new System.Drawing.Point(232, 104);
            this.lblPuertoInvitado.Name = "lblPuertoInvitado";
            this.lblPuertoInvitado.Size = new System.Drawing.Size(100, 18);
            this.lblPuertoInvitado.TabIndex = 4;
            this.lblPuertoInvitado.Text = "PUERTO";
            //
            // txtPuertoInvitado
            //
            this.txtPuertoInvitado.BackColor = System.Drawing.Color.FromArgb(16, 20, 32);
            this.txtPuertoInvitado.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPuertoInvitado.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtPuertoInvitado.ForeColor = System.Drawing.Color.White;
            this.txtPuertoInvitado.Location = new System.Drawing.Point(232, 124);
            this.txtPuertoInvitado.Name = "txtPuertoInvitado";
            this.txtPuertoInvitado.Size = new System.Drawing.Size(108, 29);
            this.txtPuertoInvitado.TabIndex = 5;
            this.txtPuertoInvitado.Text = "8888";
            //
            // btnConectar
            //
            this.btnConectar.BackColor = System.Drawing.Color.FromArgb(96, 120, 220);
            this.btnConectar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConectar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnConectar.ForeColor = System.Drawing.Color.White;
            this.btnConectar.Location = new System.Drawing.Point(16, 232);
            this.btnConectar.Name = "btnConectar";
            this.btnConectar.Size = new System.Drawing.Size(324, 50);
            this.btnConectar.TabIndex = 6;
            this.btnConectar.Text = "CONECTAR";
            this.btnConectar.UseVisualStyleBackColor = false;
            this.btnConectar.Click += new System.EventHandler(this.BtnConectar_Click);
            //
            // lblEstado
            //
            this.lblEstado.BackColor = System.Drawing.Color.Transparent;
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblEstado.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblEstado.Location = new System.Drawing.Point(26, 392);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(600, 44);
            this.lblEstado.TabIndex = 4;
            this.lblEstado.Text = "Elige una opción.";
            //
            // btnVolver
            //
            this.btnVolver.BackColor = System.Drawing.Color.FromArgb(58, 62, 92);
            this.btnVolver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVolver.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVolver.ForeColor = System.Drawing.Color.White;
            this.btnVolver.Location = new System.Drawing.Point(644, 410);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(130, 38);
            this.btnVolver.TabIndex = 5;
            this.btnVolver.Text = "Volver";
            this.btnVolver.UseVisualStyleBackColor = false;
            this.btnVolver.Click += new System.EventHandler(this.BtnVolver_Click);
            //
            // FrmLan
            //
            this.BackColor = System.Drawing.Color.FromArgb(24, 26, 40);
            this.ClientSize = new System.Drawing.Size(800, 460);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblSubtitulo);
            this.Controls.Add(this.panelAnfitrion);
            this.Controls.Add(this.panelInvitado);
            this.Controls.Add(this.lblEstado);
            this.Controls.Add(this.btnVolver);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmLan";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Arena Elemental - Partida LAN";
            this.panelAnfitrion.ResumeLayout(false);
            this.panelAnfitrion.PerformLayout();
            this.panelInvitado.ResumeLayout(false);
            this.panelInvitado.PerformLayout();
            this.ResumeLayout(false);
        }
        private string DireccionesDeEsteEquipo()
        {
            System.Collections.Generic.List<string> direcciones = RedLan.DireccionesLocales();
            if (direcciones.Count == 0) return Idioma.Texto("(no se encontró una red)", "(no network found)");
            string texto = "";
            foreach (string direccion in direcciones)
            {
                if (texto != "") texto = texto + "   ";
                texto = texto + direccion;
            }
            return texto;
        }

        // ---------- Eventos ----------

        private void BtnCrear_Click(object sender, EventArgs e)
        {
            int puerto = LeerPuerto(txtPuertoAnfitrion.Text);
            if (puerto < 0) return;

            red = new RedLan();
            if (!red.IniciarAnfitrion(puerto))
            {
                MessageBox.Show(red.UltimoError, Idioma.Texto("No se pudo crear la partida", "Could not create the match"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                red = null;
                return;
            }

            EmpezarEspera(Idioma.Texto("Esperando a otro jugador en el puerto ", "Waiting for another player on port ") + puerto + Idioma.Texto("...  (Si Windows pregunta por el Firewall, permite el acceso.)", "...  (If Windows asks about the Firewall, allow access.)"));
        }

        private void BtnConectar_Click(object sender, EventArgs e)
        {
            int puerto = LeerPuerto(txtPuertoInvitado.Text);
            if (puerto < 0) return;

            red = new RedLan();
            if (!red.IniciarConexion(txtIp.Text, puerto))
            {
                MessageBox.Show(red.UltimoError, Idioma.Texto("No se pudo conectar", "Could not connect"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                red = null;
                return;
            }

            EmpezarEspera(Idioma.Texto("Conectando con ", "Connecting to ") + txtIp.Text.Trim() + ":" + puerto + "...");
        }

        private void BtnVolver_Click(object sender, EventArgs e)
        {
            if (esperando)
            {
                CancelarEspera(Idioma.Texto("Conexión cancelada.", "Connection cancelled."));
                return;
            }
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            bool listo;
            if (red.EsAnfitrion)
            {
                listo = red.HayClienteEsperando();
            }
            else
            {
                listo = red.ConexionTerminada();
                double segundos = (DateTime.Now - inicioConexion).TotalSeconds;
                if (!listo && (red.Desconectado || segundos > SegundosMaximosParaConectar))
                {
                    string motivo = Idioma.Texto("El anfitrión no respondió. Revisa la IP, el puerto y que la partida esté creada.", "The host did not respond. Check the IP, the port, and that the match has been created.");
                    if (red.Desconectado) motivo = red.UltimoError;
                    CancelarEspera(motivo);
                    return;
                }
            }

            if (listo)
            {
                timer.Stop();
                esperando = false;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer.Stop();
            // Si no se logró la conexión no debe quedar ningún puerto abierto.
            if (DialogResult != DialogResult.OK && red != null)
                red.Cerrar();
            base.OnFormClosed(e);
        }

        // ---------- Auxiliares ----------

        // Convierte el texto del puerto en número (1024 a 65535). Si no sirve avisa y devuelve -1.
        private int LeerPuerto(string texto)
        {
            int puerto;
            if (!int.TryParse(texto.Trim(), out puerto) || puerto < 1024 || puerto > 65535)
            {
                MessageBox.Show(Idioma.Texto("El puerto debe ser un número entre 1024 y 65535 (por ejemplo 8888).", "The port must be a number between 1024 and 65535 (for example 8888)."), Idioma.Texto("Puerto inválido", "Invalid port"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return -1;
            }
            return puerto;
        }

        private void EmpezarEspera(string mensaje)
        {
            esperando = true;
            inicioConexion = DateTime.Now;
            btnCrear.Enabled = false;
            btnConectar.Enabled = false;
            txtPuertoAnfitrion.Enabled = false;
            txtIp.Enabled = false;
            txtPuertoInvitado.Enabled = false;
            btnVolver.Text = Idioma.Texto("Cancelar", "Cancel");
            lblEstado.Text = mensaje;
            lblEstado.ForeColor = Color.Gold;
            timer.Start();
        }

        private void CancelarEspera(string mensaje)
        {
            timer.Stop();
            esperando = false;
            if (red != null) red.Cerrar();
            red = null;

            btnCrear.Enabled = true;
            btnConectar.Enabled = true;
            txtPuertoAnfitrion.Enabled = true;
            txtIp.Enabled = true;
            txtPuertoInvitado.Enabled = true;
            btnVolver.Text = Idioma.Texto("Volver", "Back");
            lblEstado.Text = mensaje;
            lblEstado.ForeColor = Color.IndianRed;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && timer != null) timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
