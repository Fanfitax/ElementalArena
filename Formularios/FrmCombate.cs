using System;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Modelos;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    /// <summary>Pantalla de combate por turnos y relevos contra el rival controlado por la CPU.</summary>
    public class FrmCombate : Form
    {
        private const int RetardoCpuMs = 900;

        private readonly IRepositorio repo;
        private readonly int idJugador;
        private readonly int idEquipo;
        private readonly Combate combate;
        private readonly IaRival ia = new IaRival();

        private readonly TarjetaPersonaje[] tarjetasJugador = new TarjetaPersonaje[EquipoCombate.TamanoEquipo];
        private readonly TarjetaPersonaje[] tarjetasRival = new TarjetaPersonaje[EquipoCombate.TamanoEquipo];
        private readonly Button[] botonesRelevo = new Button[EquipoCombate.TamanoEquipo];

        private Label lblTitulo, lblInstruccion;
        private Button btnAtacar, btnHabilidad, btnDescansar, btnRendirse;
        private RichTextBox txtLog;
        private ContextMenuStrip menuAliados;
        private Timer timerCpu;
        private bool partidaGuardada;

        public FrmCombate(IRepositorio repo, int idJugador, int idEquipo, Combate combate)
        {
            this.repo = repo;
            this.idJugador = idJugador;
            this.idEquipo = idEquipo;
            this.combate = combate;

            ConstruirInterfaz();

            foreach (string linea in combate.Registro) AgregarLog(linea);
            combate.MensajeRegistrado += AgregarLog;

            timerCpu = new Timer();
            timerCpu.Interval = RetardoCpuMs;
            timerCpu.Tick += TimerCpu_Tick;

            Refrescar();
            // Por si el rival empezara (no ocurre ahora, pero mantiene el flujo uniforme).
            if (!combate.EsTurnoDelJugador && !combate.Terminado) timerCpu.Start();
        }

        // ---------- Construcción de la interfaz ----------

        private void ConstruirInterfaz()
        {
            Estilo.AplicarFormulario(this, "Arena Elemental - Combate", 1010, 700);

            lblTitulo = Estilo.CrearEtiqueta(string.Empty, 16, 10, 978, 28, 14f, true);
            Controls.Add(lblTitulo);

            Controls.Add(Estilo.CrearEtiqueta(combate.Jugador.Nombre.ToUpper(), 16, 44, 330, 22, 10.5f, true));
            Controls.Add(Estilo.CrearEtiqueta(combate.Rival.Nombre.ToUpper(), 664, 44, 330, 22, 10.5f, true));

            for (int i = 0; i < EquipoCombate.TamanoEquipo; i++)
            {
                tarjetasJugador[i] = new TarjetaPersonaje();
                tarjetasJugador[i].Location = new Point(16, 70 + i * 120);
                Controls.Add(tarjetasJugador[i]);

                tarjetasRival[i] = new TarjetaPersonaje();
                tarjetasRival[i].Location = new Point(664, 70 + i * 120);
                Controls.Add(tarjetasRival[i]);
            }

            // Zona central: acciones
            Controls.Add(Estilo.CrearEtiqueta("ACCIONES", 362, 44, 290, 22, 10.5f, true));
            lblInstruccion = Estilo.CrearEtiqueta(string.Empty, 362, 68, 290, 44, 9.5f, false);
            lblInstruccion.ForeColor = Color.Gold;
            Controls.Add(lblInstruccion);

            btnAtacar = Estilo.CrearBoton("Atacar", 362, 116, 290, 40);
            btnAtacar.Click += delegate { EjecutarJugador(AccionCombate.Atacar()); };
            Controls.Add(btnAtacar);

            menuAliados = new ContextMenuStrip();
            btnHabilidad = Estilo.CrearBoton("Habilidad", 362, 162, 290, 40);
            btnHabilidad.BackColor = Color.FromArgb(150, 80, 200);
            btnHabilidad.Click += BtnHabilidad_Click;
            Controls.Add(btnHabilidad);

            btnDescansar = Estilo.CrearBoton("Descansar (+" + Combate.EnergiaAlDescansar + " ENG)", 362, 208, 290, 36);
            btnDescansar.BackColor = Estilo.PanelClaro;
            btnDescansar.Click += delegate { EjecutarJugador(AccionCombate.Pasar()); };
            Controls.Add(btnDescansar);

            Controls.Add(Estilo.CrearEtiqueta("RELEVO", 362, 256, 290, 22, 10.5f, true));
            for (int i = 0; i < EquipoCombate.TamanoEquipo; i++)
            {
                int indice = i;   // captura para el delegado
                Button b = Estilo.CrearBoton(string.Empty, 362, 282 + i * 42, 290, 36);
                b.BackColor = Color.FromArgb(46, 120, 160);
                b.Click += delegate { ClickRelevo(indice); };
                botonesRelevo[i] = b;
                Controls.Add(b);
            }

            btnRendirse = Estilo.CrearBoton("Rendirse", 362, 412, 290, 28);
            btnRendirse.BackColor = Color.FromArgb(150, 50, 50);
            btnRendirse.Click += delegate { Close(); };
            Controls.Add(btnRendirse);

            Label reglas = Estilo.CrearEtiqueta(
                "Triángulo: " + Estilo.Triangulo + "\nNo se puede relevar dos turnos seguidos.\nRonda " + Combate.TurnoMuerteSubita + ": muerte súbita.",
                362, 448, 290, 60, 8.5f, false);
            reglas.ForeColor = Estilo.TextoSuave;
            Controls.Add(reglas);

            txtLog = new RichTextBox();
            txtLog.SetBounds(16, 516, 978, 168);
            txtLog.ReadOnly = true;
            txtLog.BackColor = Color.FromArgb(16, 17, 28);
            txtLog.ForeColor = Estilo.Texto;
            txtLog.BorderStyle = BorderStyle.None;
            txtLog.Font = new Font("Consolas", 9.5f);
            txtLog.DetectUrls = false;
            Controls.Add(txtLog);
        }

        // ---------- Acciones del jugador ----------

        private void BtnHabilidad_Click(object sender, EventArgs e)
        {
            Personaje activo = combate.Jugador.Activo;
            if (activo.HabilidadEsOfensiva || !(activo is CartaAgua))
            {
                EjecutarJugador(AccionCombate.Habilidad());
                return;
            }

            // Habilidad de apoyo con objetivo: elegir a quién curar.
            menuAliados.Items.Clear();
            for (int i = 0; i < combate.Jugador.Miembros.Count; i++)
            {
                Personaje aliado = combate.Jugador.Miembros[i];
                if (!aliado.EstaVivo) continue;
                int indice = i;
                string texto = (aliado == activo ? "Curarse a sí misma" : "Curar a " + aliado.Nombre)
                               + "  (" + aliado.Hp + "/" + aliado.HpMax + ")";
                ToolStripMenuItem item = new ToolStripMenuItem(texto);
                item.Click += delegate { EjecutarJugador(AccionCombate.HabilidadSobreAliado(indice)); };
                menuAliados.Items.Add(item);
            }
            menuAliados.Show(btnHabilidad, new Point(0, btnHabilidad.Height));
        }

        private void ClickRelevo(int indice)
        {
            if (combate.RelevoForzadoPendiente)
            {
                try
                {
                    combate.ElegirRelevoForzado(indice);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Relevo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                Refrescar();
                ProcesarSiguiente();
                return;
            }
            EjecutarJugador(AccionCombate.Relevar(indice));
        }

        private void EjecutarJugador(AccionCombate accion)
        {
            string motivo = "No es tu turno.";
            if (!combate.EsTurnoDelJugador || !combate.PuedeEjecutar(accion, out motivo))
            {
                MessageBox.Show(motivo, "Acción no disponible", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            combate.Ejecutar(accion);
            Refrescar();
            ProcesarSiguiente();
        }

        // ---------- Flujo de turnos ----------

        private void ProcesarSiguiente()
        {
            if (combate.Terminado)
            {
                FinalizarPartida();
            }
            else if (!combate.EsTurnoDelJugador)
            {
                timerCpu.Start();
            }
        }

        private void TimerCpu_Tick(object sender, EventArgs e)
        {
            timerCpu.Stop();
            if (combate.Terminado || combate.EsTurnoDelJugador) return;
            combate.Ejecutar(ia.Decidir(combate));
            Refrescar();
            ProcesarSiguiente();
        }

        private void FinalizarPartida()
        {
            if (partidaGuardada) return;
            string resultado = combate.Estado == EstadoCombate.VictoriaJugador ? "Victoria" : "Derrota";
            GuardarPartida(resultado);

            MessageBox.Show(
                resultado == "Victoria"
                    ? "¡Victoria! Derrotaste al rival en " + combate.Turno + " rondas."
                    : "Derrota. Tu equipo cayó en la ronda " + combate.Turno + ".",
                "Fin del combate", MessageBoxButtons.OK,
                resultado == "Victoria" ? MessageBoxIcon.Information : MessageBoxIcon.Exclamation);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void GuardarPartida(string resultado)
        {
            if (partidaGuardada) return;
            partidaGuardada = true;
            try
            {
                repo.GuardarPartida(idJugador, idEquipo, combate.Escenario.Id, resultado,
                                    combate.Turno, combate.Jugador.DanoInfligido);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar la estadística de la partida:\n" + ex.Message,
                                "Base de datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!combate.Terminado && !partidaGuardada)
            {
                DialogResult r = MessageBox.Show("¿Abandonar el combate? Se registrará como abandono.",
                                                 "Rendirse", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                timerCpu.Stop();
                GuardarPartida("Abandono");
            }
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && timerCpu != null)
            {
                timerCpu.Stop();
                timerCpu.Dispose();
            }
            base.Dispose(disposing);
        }

        // ---------- Presentación ----------

        private void Refrescar()
        {
            for (int i = 0; i < EquipoCombate.TamanoEquipo; i++)
            {
                tarjetasJugador[i].Actualizar(combate.Jugador.Miembros[i], i == combate.Jugador.IndiceActivo);
                tarjetasRival[i].Actualizar(combate.Rival.Miembros[i], i == combate.Rival.IndiceActivo);
            }

            bool turnoJugador = combate.EsTurnoDelJugador;
            bool forzado = combate.RelevoForzadoPendiente;
            Personaje activo = combate.Jugador.Activo;
            bool libre = turnoJugador && !forzado;

            lblTitulo.Text = combate.Escenario.Nombre + "   ·   Ronda " + combate.Turno;

            if (combate.Terminado) lblInstruccion.Text = "El combate terminó.";
            else if (forzado) lblInstruccion.Text = "Tu personaje cayó. Elige quién entra al campo (no gasta turno).";
            else if (turnoJugador) lblInstruccion.Text = "Tu turno: " + activo.Nombre + ". Elige una acción.";
            else lblInstruccion.Text = "Turno del rival...";

            btnAtacar.Enabled = libre;
            btnDescansar.Enabled = libre;

            string motivo;
            btnHabilidad.Text = activo.NombreHabilidad + "  (" + activo.CostoHabilidad + " ENG"
                                + (activo.CooldownActual > 0 ? ", recarga " + activo.CooldownActual : string.Empty) + ")";
            btnHabilidad.Enabled = libre && combate.PuedeEjecutar(AccionCombate.Habilidad(), out motivo);

            for (int i = 0; i < EquipoCombate.TamanoEquipo; i++)
            {
                Personaje p = combate.Jugador.Miembros[i];
                Button b = botonesRelevo[i];
                if (i == combate.Jugador.IndiceActivo)
                {
                    b.Text = "En campo: " + p.Nombre;
                    b.Enabled = false;
                }
                else
                {
                    b.Text = p.EstaVivo ? "Relevo: " + p.Nombre + " (" + p.Hp + " HP)" : p.Nombre + " (derrotado)";
                    if (forzado) b.Enabled = combate.Jugador.PuedeRelevarA(i);
                    else b.Enabled = libre && combate.PuedeEjecutar(AccionCombate.Relevar(i), out motivo);
                }
            }

            btnRendirse.Enabled = !combate.Terminado;
        }

        private void AgregarLog(string mensaje)
        {
            Color color = Estilo.Texto;
            if (mensaje.StartsWith("—")) color = Estilo.TextoSuave;
            else if (mensaje.Contains("derrotado") || mensaje.Contains("Derrota")) color = Color.IndianRed;
            else if (mensaje.Contains("Victoria") || mensaje.Contains("MUERTE")) color = Color.Gold;
            else if (mensaje.Contains("cura")) color = Color.LightGreen;

            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;
            txtLog.SelectionColor = color;
            txtLog.AppendText(mensaje + "\n");
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }
    }
}
