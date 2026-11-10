using System;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Modelos;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    // Ventanita de espera: queda esperando el mensaje CONFIG del otro jugador (su equipo, y el escenario si es el anfitrión).
    // Se cierra con DialogResult.OK cuando el mensaje llega (queda en la propiedad Mensaje) o con Cancel si
    // se cancela o el otro PC se desconecta.
    public class FrmEsperaLan : FormularioModerno
    {
        private RedLan red;
        private Timer timer;
        private string mensaje;

        // Los controles se declaran como campos para que el diseñador de SharpDevelop los reconozca.
        private Label lblTitulo;
        private Label lblDetalle;
        private Button btnCancelar;

        public FrmEsperaLan(RedLan red, string texto)
        {
            this.red = red;

            InitializeComponent();

            // Lo que cambia según el idioma (el diseñador solo muestra la versión en español).
            Text = Idioma.Texto("Arena Elemental - Esperando al otro jugador", "Arena Elemental - Waiting for the other player");
            lblTitulo.Text = Idioma.Texto("ESPERANDO AL OTRO JUGADOR", "WAITING FOR THE OTHER PLAYER");
            lblDetalle.Text = texto;
            Estilo.EstilizarBoton(btnCancelar, Idioma.Texto("Cancelar", "Cancel"), Estilo.PanelClaro);

            timer = new Timer();
            timer.Interval = 150;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        // Código del diseñador: aquí se crean y se colocan los controles de la ventana.
        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblDetalle = new System.Windows.Forms.Label();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.Gold;
            this.lblTitulo.Location = new System.Drawing.Point(24, 18);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(500, 34);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "ESPERANDO AL OTRO JUGADOR";
            //
            // lblDetalle
            //
            this.lblDetalle.BackColor = System.Drawing.Color.Transparent;
            this.lblDetalle.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblDetalle.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblDetalle.Location = new System.Drawing.Point(26, 62);
            this.lblDetalle.Name = "lblDetalle";
            this.lblDetalle.Size = new System.Drawing.Size(508, 60);
            this.lblDetalle.TabIndex = 1;
            this.lblDetalle.Text = "Tu equipo ya fue enviado. Cuando el otro jugador termine de armar el suyo empezará el combate.";
            //
            // btnCancelar
            //
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(58, 62, 92);
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(404, 146);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(130, 38);
            this.btnCancelar.TabIndex = 2;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.Cancelar_Click);
            //
            // FrmEsperaLan
            //
            this.BackColor = System.Drawing.Color.FromArgb(24, 26, 40);
            this.ClientSize = new System.Drawing.Size(560, 210);
            this.ControlBox = false;
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblDetalle);
            this.Controls.Add(this.btnCancelar);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmEsperaLan";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Arena Elemental - Esperando al otro jugador";
            this.ResumeLayout(false);
        }
        // La línea CONFIG recibida del otro jugador.
        public string Mensaje
        {
            get { return mensaje; }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            string recibido = red.LeerTexto();
            if (recibido != null)
            {
                // Solo interesa el CONFIG; cualquier otra cosa se ignora.
                if (recibido.StartsWith("CONFIG|"))
                {
                    mensaje = recibido;
                    timer.Stop();
                    DialogResult = DialogResult.OK;
                    Close();
                }
                return;
            }

            if (red.Desconectado)
            {
                timer.Stop();
                MessageBox.Show(Idioma.Texto("El otro jugador se desconectó.", "The other player disconnected."), Idioma.Texto("Conexión perdida", "Connection lost"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void Cancelar_Click(object sender, EventArgs e)
        {
            timer.Stop();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && timer != null) timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
