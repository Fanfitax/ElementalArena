using System;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Modelos;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    /// <summary>
    /// Pantalla de combate. Muestra la COLA (Queue) de relevos de cada equipo y el
    /// historial de jugadas guardado en una PILA (Stack), cada uno en un ListBox.
    /// </summary>
    public class FrmCombate : Form
    {
        private const int RetardoCpuMs = 900;

        private string nombreJugador;
        private Combate combate;
        private IaRival ia = new IaRival();

        private TarjetaPersonaje[] tarjetasJugador = new TarjetaPersonaje[EquipoCombate.TamanoEquipo];
        private TarjetaPersonaje[] tarjetasRival = new TarjetaPersonaje[EquipoCombate.TamanoEquipo];

        private Label lblTitulo, lblInstruccion, lblUltimaJugada;
        private Button btnAtacar, btnHabilidad, btnDescansar, btnRelevo, btnRendirse;
        private ListBox lstBancaJugador, lstBancaRival, lstHistorial;
        private ContextMenuStrip menuAliados;
        private Timer timerCpu;
        private bool partidaGuardada;

        public FrmCombate(string nombreJugador, Combate combate)
        {
            this.nombreJugador = nombreJugador;
            this.combate = combate;

            ConstruirInterfaz();

            timerCpu = new Timer();
            timerCpu.Interval = RetardoCpuMs;
            timerCpu.Tick += TimerCpu_Tick;

            Refrescar();
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

            btnRelevo = Estilo.CrearBoton("Relevo", 362, 252, 290, 40);
            btnRelevo.BackColor = Color.FromArgb(46, 120, 160);
            btnRelevo.Click += delegate { EjecutarJugador(AccionCombate.Relevar()); };
            Controls.Add(btnRelevo);

            Controls.Add(Estilo.CrearEtiqueta("Última jugada (Peek de la pila):", 362, 304, 290, 20, 9f, true));
            lblUltimaJugada = Estilo.CrearEtiqueta(string.Empty, 362, 324, 290, 66, 9.5f, false);
            lblUltimaJugada.ForeColor = Estilo.TextoSuave;
            Controls.Add(lblUltimaJugada);

            btnRendirse = Estilo.CrearBoton("Rendirse", 362, 400, 290, 28);
            btnRendirse.BackColor = Color.FromArgb(150, 50, 50);
            btnRendirse.Click += delegate { Close(); };
            Controls.Add(btnRendirse);

            Label reglas = Estilo.CrearEtiqueta(
                "Triángulo: " + Estilo.Triangulo + "\nNo se puede relevar dos turnos seguidos.\nRonda " + Combate.TurnoMuerteSubita + ": muerte súbita.",
                362, 440, 290, 60, 8.5f, false);
            reglas.ForeColor = Estilo.TextoSuave;
            Controls.Add(reglas);

            // Parte de abajo: dos colas (banca de cada equipo) y la pila del historial.
            Controls.Add(Estilo.CrearEtiqueta("MI BANCA (cola de relevo)", 16, 512, 230, 22, 9.5f, true));
            lstBancaJugador = CrearLista(16, 536, 230, 148);

            Controls.Add(Estilo.CrearEtiqueta("BANCA RIVAL (cola)", 256, 512, 230, 22, 9.5f, true));
            lstBancaRival = CrearLista(256, 536, 230, 148);

            Controls.Add(Estilo.CrearEtiqueta("HISTORIAL DE JUGADAS (pila: lo último arriba)", 496, 512, 498, 22, 9.5f, true));
            lstHistorial = CrearLista(496, 536, 498, 148);
        }

        private ListBox CrearLista(int x, int y, int ancho, int alto)
        {
            ListBox lista = new ListBox();
            lista.SetBounds(x, y, ancho, alto);
            lista.BackColor = Color.FromArgb(16, 17, 28);
            lista.ForeColor = Estilo.Texto;
            lista.BorderStyle = BorderStyle.None;
            lista.Font = new Font("Consolas", 9.5f);
            lista.HorizontalScrollbar = true;
            Controls.Add(lista);
            return lista;
        }

        // ---------- Acciones del jugador ----------

        private void BtnHabilidad_Click(object sender, EventArgs e)
        {
            Personaje activo = combate.Jugador.Activo;
            if (!(activo is CartaAgua))
            {
                EjecutarJugador(AccionCombate.Habilidad());
                return;
            }

            // La habilidad del Agua cura: se elige a qué aliado.
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
                FinalizarPartida();
            else if (!combate.EsTurnoDelJugador)
                timerCpu.Start();
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
            RegistroPartidas.Agregar(nombreJugador, combate.Jugador.Nombre, combate.Escenario.Nombre,
                                     resultado, combate.Turno, combate.Jugador.DanoInfligido);
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
                Personaje propio = combate.Jugador.Miembros[i];
                Personaje rival = combate.Rival.Miembros[i];
                tarjetasJugador[i].Actualizar(propio, propio == combate.Jugador.Activo);
                tarjetasRival[i].Actualizar(rival, rival == combate.Rival.Activo);
            }

            bool turnoJugador = combate.EsTurnoDelJugador;
            Personaje activo = combate.Jugador.Activo;

            lblTitulo.Text = combate.Escenario.Nombre + "   ·   Ronda " + combate.Turno;

            if (combate.Terminado) lblInstruccion.Text = "El combate terminó.";
            else if (turnoJugador) lblInstruccion.Text = "Tu turno: " + activo.Nombre + ". Elige una acción.";
            else lblInstruccion.Text = "Turno del rival...";

            string motivo;
            btnAtacar.Enabled = turnoJugador;
            btnDescansar.Enabled = turnoJugador;

            btnHabilidad.Text = activo.NombreHabilidad + "  (" + activo.CostoHabilidad + " ENG"
                                + (activo.CooldownActual > 0 ? ", recarga " + activo.CooldownActual : string.Empty) + ")";
            btnHabilidad.Enabled = turnoJugador && combate.PuedeEjecutar(AccionCombate.Habilidad(), out motivo);

            Personaje siguiente = combate.Jugador.ProximoRelevo;
            btnRelevo.Text = siguiente == null ? "Relevo (banca vacía)" : "Relevo: entra " + siguiente.Nombre;
            btnRelevo.Enabled = turnoJugador && combate.PuedeEjecutar(AccionCombate.Relevar(), out motivo);

            btnRendirse.Enabled = !combate.Terminado;

            LlenarBanca(lstBancaJugador, combate.Jugador);
            LlenarBanca(lstBancaRival, combate.Rival);
            LlenarHistorial();
        }

        /// <summary>Muestra la cola de un equipo en un ListBox: el primero de la lista es el próximo en entrar.</summary>
        private void LlenarBanca(ListBox lista, EquipoCombate equipo)
        {
            lista.Items.Clear();
            int posicion = 1;
            foreach (Personaje p in equipo.ObtenerBanca())
            {
                lista.Items.Add(posicion + ". " + p.Nombre + " [" + p.Elemento + "] " + p.Hp + "/" + p.HpMax + " HP");
                posicion++;
            }
            if (lista.Items.Count == 0) lista.Items.Add("(banca vacía)");
        }

        /// <summary>Muestra la pila del historial en un ListBox (la jugada más reciente queda arriba).</summary>
        private void LlenarHistorial()
        {
            lstHistorial.BeginUpdate();
            lstHistorial.Items.Clear();
            foreach (string jugada in combate.Historial)
                lstHistorial.Items.Add(jugada);
            lstHistorial.EndUpdate();

            lblUltimaJugada.Text = combate.UltimaJugada;
        }
    }
}
