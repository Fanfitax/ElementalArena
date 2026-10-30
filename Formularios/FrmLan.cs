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
        private static Color PanelOscuro = Color.FromArgb(205, 10, 12, 18);
        private const int SegundosMaximosParaConectar = 10;

        private RedLan red;
        private Timer timer;
        private DateTime inicioConexion;
        private bool esperando;

        private TextBox txtPuertoAnfitrion, txtIp, txtPuertoInvitado;
        private Button btnCrear, btnConectar, btnVolver;
        private Label lblEstado;

        public FrmLan()
        {
            Estilo.AplicarFormulario(this, Idioma.Texto("Arena Elemental - Partida LAN", "Arena Elemental - LAN Match"), 800, 460);

            Label titulo = Estilo.CrearEtiqueta(Idioma.Texto("PARTIDA LAN", "LAN MATCH"), 24, 12, 500, 38, 22f, true);
            titulo.ForeColor = Color.Gold;
            Controls.Add(titulo);

            Label sub = Estilo.CrearEtiqueta(Idioma.Texto("Dos jugadores en dos PCs conectados a la misma red local (mismo Wi-Fi o cable).", "Two players on two PCs connected to the same local network (same Wi-Fi or cable)."), 26, 50, 740, 20, 9.5f, false);
            sub.ForeColor = Estilo.TextoSuave;
            Controls.Add(sub);

            ConstruirPanelAnfitrion();
            ConstruirPanelInvitado();

            lblEstado = Estilo.CrearEtiqueta(Idioma.Texto("Elige una opción.", "Choose an option."), 26, 392, 600, 44, 10f, true);
            lblEstado.ForeColor = Estilo.TextoSuave;
            Controls.Add(lblEstado);

            btnVolver = new Button();
            btnVolver.SetBounds(644, 410, 130, 38);
            Estilo.EstilizarBoton(btnVolver, Idioma.Texto("Volver", "Back"), Estilo.PanelClaro);
            btnVolver.Click += BtnVolver_Click;
            Controls.Add(btnVolver);

            timer = new Timer();
            timer.Interval = 150;
            timer.Tick += Timer_Tick;
        }

        // La conexión ya establecida (solo es válida si la ventana se cerró con DialogResult.OK).
        public RedLan Red
        {
            get { return red; }
        }

        // ---------- Construcción de la interfaz ----------

        private void ConstruirPanelAnfitrion()
        {
            Panel panel = CrearPanel(24);

            panel.Controls.Add(CrearTitulo(Idioma.Texto("ANFITRIÓN (SERVIDOR)", "HOST (SERVER)")));
            panel.Controls.Add(CrearTexto(Idioma.Texto("Crea la partida y espera. Tú eliges el escenario y empiezas el combate.", "Create the match and wait. You choose the scenario and start the battle.")));

            string ips = Idioma.Texto("Tu IP:  ", "Your IP:  ") + DireccionesDeEsteEquipo();
            Label lblIp = Estilo.CrearEtiqueta(ips, 16, 104, 324, 44, 10.5f, true);
            lblIp.ForeColor = Color.LightGreen;
            panel.Controls.Add(lblIp);

            panel.Controls.Add(Estilo.CrearEtiqueta(Idioma.Texto("PUERTO", "PORT"), 16, 160, 100, 18, 8.5f, true));
            txtPuertoAnfitrion = CrearCaja(16, 180, 120);
            txtPuertoAnfitrion.Text = RedLan.PuertoPorDefecto.ToString();
            panel.Controls.Add(txtPuertoAnfitrion);

            btnCrear = new Button();
            btnCrear.SetBounds(16, 232, 324, 50);
            Estilo.EstilizarBoton(btnCrear, Idioma.Texto("CREAR PARTIDA", "CREATE MATCH"), Color.FromArgb(46, 160, 96));
            btnCrear.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnCrear.Click += BtnCrear_Click;
            panel.Controls.Add(btnCrear);
        }

        private void ConstruirPanelInvitado()
        {
            Panel panel = CrearPanel(412);

            panel.Controls.Add(CrearTitulo(Idioma.Texto("UNIRSE POR IP (CLIENTE)", "JOIN BY IP (CLIENT)")));
            panel.Controls.Add(CrearTexto(Idioma.Texto("Escribe la IP que le aparece al anfitrión en su pantalla.", "Type the IP shown on the host's screen.")));

            panel.Controls.Add(Estilo.CrearEtiqueta(Idioma.Texto("IP DEL ANFITRIÓN", "HOST IP"), 16, 104, 200, 18, 8.5f, true));
            txtIp = CrearCaja(16, 124, 200);
            txtIp.Text = "192.168.1.";
            panel.Controls.Add(txtIp);

            panel.Controls.Add(Estilo.CrearEtiqueta(Idioma.Texto("PUERTO", "PORT"), 232, 104, 100, 18, 8.5f, true));
            txtPuertoInvitado = CrearCaja(232, 124, 108);
            txtPuertoInvitado.Text = RedLan.PuertoPorDefecto.ToString();
            panel.Controls.Add(txtPuertoInvitado);

            btnConectar = new Button();
            btnConectar.SetBounds(16, 232, 324, 50);
            Estilo.EstilizarBoton(btnConectar, Idioma.Texto("CONECTAR", "CONNECT"), Estilo.Acento);
            btnConectar.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnConectar.Click += BtnConectar_Click;
            panel.Controls.Add(btnConectar);
        }

        private Panel CrearPanel(int x)
        {
            Panel panel = new Panel();
            panel.SetBounds(x, 84, 356, 296);
            panel.BackColor = PanelOscuro;
            Controls.Add(panel);
            Estilo.Redondear(panel, 14);
            return panel;
        }

        private Label CrearTitulo(string texto)
        {
            Label l = Estilo.CrearEtiqueta(texto, 16, 12, 324, 24, 12f, true);
            l.ForeColor = Color.Gold;
            return l;
        }

        private Label CrearTexto(string texto)
        {
            Label l = Estilo.CrearEtiqueta(texto, 16, 40, 324, 56, 9f, false);
            l.ForeColor = Estilo.TextoSuave;
            return l;
        }

        private TextBox CrearCaja(int x, int y, int ancho)
        {
            TextBox caja = new TextBox();
            caja.SetBounds(x, y, ancho, 30);
            caja.Font = new Font("Segoe UI", 12f);
            caja.BackColor = Color.FromArgb(16, 20, 32);
            caja.ForeColor = Color.White;
            caja.BorderStyle = BorderStyle.FixedSingle;
            return caja;
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
