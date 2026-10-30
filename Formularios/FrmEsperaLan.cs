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

        public FrmEsperaLan(RedLan red, string texto)
        {
            this.red = red;

            Estilo.AplicarFormulario(this, Idioma.Texto("Arena Elemental - Esperando al otro jugador", "Arena Elemental - Waiting for the other player"), 560, 210);
            ControlBox = false;

            Label titulo = Estilo.CrearEtiqueta(Idioma.Texto("ESPERANDO AL OTRO JUGADOR", "WAITING FOR THE OTHER PLAYER"), 24, 18, 500, 34, 16f, true);
            titulo.ForeColor = Color.Gold;
            Controls.Add(titulo);

            Label detalle = Estilo.CrearEtiqueta(texto, 26, 62, 508, 60, 10.5f, false);
            detalle.ForeColor = Estilo.TextoSuave;
            Controls.Add(detalle);

            Button cancelar = new Button();
            cancelar.SetBounds(404, 146, 130, 38);
            Estilo.EstilizarBoton(cancelar, Idioma.Texto("Cancelar", "Cancel"), Estilo.PanelClaro);
            cancelar.Click += Cancelar_Click;
            Controls.Add(cancelar);

            timer = new Timer();
            timer.Interval = 150;
            timer.Tick += Timer_Tick;
            timer.Start();
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
