using System;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    /// <summary>
    /// Pantalla para conectar dos PCs de la misma red:
    ///  - Izquierda: ser el ANFITRIÓN (servidor). Se abre un puerto y se espera; se muestra la IP de este PC
    ///    para dársela al otro jugador.
    ///  - Derecha: UNIRSE por IP (cliente). Se escribe la IP del anfitrión y se conecta.
    /// Cuando los dos PCs quedan conectados la ventana se cierra con DialogResult.OK y la conexión
    /// queda disponible en la propiedad Red.
    /// </summary>
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
            Estilo.AplicarFormulario(this, "Arena Elemental - Partida LAN", 800, 460);

            Label titulo = Estilo.CrearEtiqueta("PARTIDA LAN", 24, 12, 500, 38, 22f, true);
            titulo.ForeColor = Color.Gold;
            Controls.Add(titulo);

            Label sub = Estilo.CrearEtiqueta("Dos jugadores en dos PCs conectados a la misma red local (mismo Wi-Fi o cable).", 26, 50, 740, 20, 9.5f, false);
            sub.ForeColor = Estilo.TextoSuave;
            Controls.Add(sub);

            ConstruirPanelAnfitrion();
            ConstruirPanelInvitado();

            lblEstado = Estilo.CrearEtiqueta("Elige una opción.", 26, 392, 600, 44, 10f, true);
            lblEstado.ForeColor = Estilo.TextoSuave;
            Controls.Add(lblEstado);

            btnVolver = new Button();
            btnVolver.SetBounds(644, 410, 130, 38);
            Estilo.EstilizarBoton(btnVolver, "Volver", Estilo.PanelClaro);
            btnVolver.Click += BtnVolver_Click;
            Controls.Add(btnVolver);

            timer = new Timer();
            timer.Interval = 150;
            timer.Tick += Timer_Tick;
        }

        /// <summary>La conexión ya establecida (solo es válida si la ventana se cerró con DialogResult.OK).</summary>
        public RedLan Red
        {
            get { return red; }
        }

        // ---------- Construcción de la interfaz ----------

        private void ConstruirPanelAnfitrion()
        {
            Panel panel = CrearPanel(24);

            panel.Controls.Add(CrearTitulo("ANFITRIÓN (SERVIDOR)"));
            panel.Controls.Add(CrearTexto("Crea la partida y espera. Tú eliges el escenario y empiezas el combate."));

            string ips = "Tu IP:  " + DireccionesDeEsteEquipo();
            Label lblIp = Estilo.CrearEtiqueta(ips, 16, 104, 324, 44, 10.5f, true);
            lblIp.ForeColor = Color.LightGreen;
            panel.Controls.Add(lblIp);

            panel.Controls.Add(Estilo.CrearEtiqueta("PUERTO", 16, 160, 100, 18, 8.5f, true));
            txtPuertoAnfitrion = CrearCaja(16, 180, 120);
            txtPuertoAnfitrion.Text = RedLan.PuertoPorDefecto.ToString();
            panel.Controls.Add(txtPuertoAnfitrion);

            btnCrear = new Button();
            btnCrear.SetBounds(16, 232, 324, 50);
            Estilo.EstilizarBoton(btnCrear, "CREAR PARTIDA", Color.FromArgb(46, 160, 96));
            btnCrear.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnCrear.Click += BtnCrear_Click;
            panel.Controls.Add(btnCrear);
        }

        private void ConstruirPanelInvitado()
        {
            Panel panel = CrearPanel(412);

            panel.Controls.Add(CrearTitulo("UNIRSE POR IP (CLIENTE)"));
            panel.Controls.Add(CrearTexto("Escribe la IP que le aparece al anfitrión en su pantalla."));

            panel.Controls.Add(Estilo.CrearEtiqueta("IP DEL ANFITRIÓN", 16, 104, 200, 18, 8.5f, true));
            txtIp = CrearCaja(16, 124, 200);
            txtIp.Text = "192.168.1.";
            panel.Controls.Add(txtIp);

            panel.Controls.Add(Estilo.CrearEtiqueta("PUERTO", 232, 104, 100, 18, 8.5f, true));
            txtPuertoInvitado = CrearCaja(232, 124, 108);
            txtPuertoInvitado.Text = RedLan.PuertoPorDefecto.ToString();
            panel.Controls.Add(txtPuertoInvitado);

            btnConectar = new Button();
            btnConectar.SetBounds(16, 232, 324, 50);
            Estilo.EstilizarBoton(btnConectar, "CONECTAR", Estilo.Acento);
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
            if (direcciones.Count == 0) return "(no se encontró una red)";
            return string.Join("   ", direcciones.ToArray());
        }

        // ---------- Eventos ----------

        private void BtnCrear_Click(object sender, EventArgs e)
        {
            int puerto = LeerPuerto(txtPuertoAnfitrion.Text);
            if (puerto < 0) return;

            red = new RedLan();
            if (!red.IniciarAnfitrion(puerto))
            {
                MessageBox.Show(red.UltimoError, "No se pudo crear la partida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                red = null;
                return;
            }

            EmpezarEspera("Esperando a otro jugador en el puerto " + puerto + "...  (Si Windows pregunta por el Firewall, permite el acceso.)");
        }

        private void BtnConectar_Click(object sender, EventArgs e)
        {
            int puerto = LeerPuerto(txtPuertoInvitado.Text);
            if (puerto < 0) return;

            red = new RedLan();
            if (!red.IniciarConexion(txtIp.Text, puerto))
            {
                MessageBox.Show(red.UltimoError, "No se pudo conectar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                red = null;
                return;
            }

            EmpezarEspera("Conectando con " + txtIp.Text.Trim() + ":" + puerto + "...");
        }

        private void BtnVolver_Click(object sender, EventArgs e)
        {
            if (esperando)
            {
                CancelarEspera("Conexión cancelada.");
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
                    string motivo = red.Desconectado ? red.UltimoError : "El anfitrión no respondió. Revisa la IP, el puerto y que la partida esté creada.";
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

        /// <summary>Convierte el texto del puerto en número (1024 a 65535). Si no sirve avisa y devuelve -1.</summary>
        private int LeerPuerto(string texto)
        {
            int puerto;
            if (!int.TryParse(texto.Trim(), out puerto) || puerto < 1024 || puerto > 65535)
            {
                MessageBox.Show("El puerto debe ser un número entre 1024 y 65535 (por ejemplo 8888).", "Puerto inválido",
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
            btnVolver.Text = "Cancelar";
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
            btnVolver.Text = "Volver";
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
