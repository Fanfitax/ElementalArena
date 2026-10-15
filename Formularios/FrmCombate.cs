using System;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Modelos;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    /// <summary>
    /// Pantalla de combate dibujada como una mesa de duelo:
    ///  - Centro: la carta del jugador frente a la carta del rival, con los botones de acción en medio.
    ///  - Arriba y abajo: la banca de cada equipo en un FlowLayoutPanel (la COLA de relevos, de izquierda a derecha).
    ///  - A la derecha: el registro de combate, que es la PILA del historial en un ListBox.
    /// </summary>
    public class FrmCombate : Form
    {
        private const int RetardoCpuMs = 900;

        private string nombreJugador;
        private Combate combate;
        private IaRival ia = new IaRival();

        // Cartas: una grande por equipo (en el campo) y cartas pequeñas para la banca.
        private VistaCarta cartaJugador, cartaRival;
        private VistaCarta[] bancaJugador = new VistaCarta[EquipoCombate.TamanoEquipo - 1];
        private VistaCarta[] bancaRival = new VistaCarta[EquipoCombate.TamanoEquipo - 1];

        private Panel mesa;
        private Label lblTitulo, lblInstruccion, lblTerremoto, lblUltimaJugada;
        private Label lblBancaJugador, lblBancaRival;
        private Button btnAtacar, btnHabilidad, btnRelevo, btnPasar, btnRendirse;
        private ListBox lstHistorial;
        private ContextMenuStrip menuAliados;

        private Timer timerCpu;
        private Timer timerTemblor;
        private int pasosTemblor;
        private int ultimaRondaConTemblor;
        private Point posicionOriginal;
        private bool partidaGuardada;

        public FrmCombate(string nombreJugador, Combate combate)
        {
            this.nombreJugador = nombreJugador;
            this.combate = combate;

            ConstruirInterfaz();

            timerCpu = new Timer();
            timerCpu.Interval = RetardoCpuMs;
            timerCpu.Tick += TimerCpu_Tick;

            // El temblor de la ventana cuando ocurre el Terremoto.
            timerTemblor = new Timer();
            timerTemblor.Interval = 40;
            timerTemblor.Tick += TimerTemblor_Tick;

            Refrescar();
        }

        // ---------- Construcción de la interfaz (todo por código) ----------

        private void ConstruirInterfaz()
        {
            Estilo.AplicarFormulario(this, "Arena Elemental - Combate", 1246, 670);

            lblTitulo = Estilo.CrearEtiqueta(string.Empty, 16, 8, 520, 28, 14f, true);
            Controls.Add(lblTitulo);

            Label lblReglas = Estilo.CrearEtiqueta(
                "Triángulo: " + Estilo.Triangulo + "   ·   Muerte súbita: ronda " + Combate.TurnoMuerteSubita,
                540, 6, 344, 34, 8.5f, false);
            lblReglas.ForeColor = Estilo.TextoSuave;
            lblReglas.TextAlign = ContentAlignment.MiddleRight;
            Controls.Add(lblReglas);

            // La mesa: un panel verde donde se juega el duelo.
            mesa = new Panel();
            mesa.SetBounds(16, 44, 868, 620);
            mesa.BackColor = Color.FromArgb(18, 64, 48);
            Controls.Add(mesa);

            // Banca del rival (arriba, a la derecha) y banca del jugador (abajo, a la izquierda).
            lblBancaRival = CrearEtiquetaMesa(494, 4, 270, 18);
            CrearBanca(494, 24, bancaRival);

            cartaJugador = new VistaCarta(false);
            cartaJugador.Location = new Point(104, 156);
            mesa.Controls.Add(cartaJugador);

            cartaRival = new VistaCarta(false);
            cartaRival.Location = new Point(534, 156);
            mesa.Controls.Add(cartaRival);

            lblBancaJugador = CrearEtiquetaMesa(104, 464, 330, 18);
            CrearBanca(104, 484, bancaJugador);

            // Zona central entre las dos cartas: VS, indicaciones y botones de acción.
            Label vs = Estilo.CrearEtiqueta("VS", 334, 156, 200, 34, 20f, true);
            vs.ForeColor = Color.Gold;
            vs.TextAlign = ContentAlignment.MiddleCenter;
            mesa.Controls.Add(vs);

            lblInstruccion = Estilo.CrearEtiqueta(string.Empty, 334, 192, 200, 44, 9.5f, false);
            lblInstruccion.ForeColor = Color.Gold;
            lblInstruccion.TextAlign = ContentAlignment.MiddleCenter;
            mesa.Controls.Add(lblInstruccion);

            btnAtacar = CrearBotonAccion("Atacar", 242, Color.FromArgb(200, 70, 60));
            btnAtacar.Click += delegate { EjecutarJugador(AccionCombate.Atacar()); };

            menuAliados = new ContextMenuStrip();
            btnHabilidad = CrearBotonAccion("Habilidad", 286, Color.FromArgb(150, 80, 200));
            btnHabilidad.Click += BtnHabilidad_Click;

            btnRelevo = CrearBotonAccion("Relevo", 330, Color.FromArgb(46, 120, 160));
            btnRelevo.Click += delegate { EjecutarJugador(AccionCombate.Relevar()); };

            btnPasar = CrearBotonAccion("Pasar (+" + Combate.EnergiaAlDescansar + " ENG)", 374, Estilo.PanelClaro);
            btnPasar.Click += delegate { EjecutarJugador(AccionCombate.Pasar()); };

            lblTerremoto = Estilo.CrearEtiqueta(string.Empty, 334, 420, 200, 34, 9.5f, true);
            lblTerremoto.TextAlign = ContentAlignment.MiddleCenter;
            mesa.Controls.Add(lblTerremoto);

            // Columna derecha: el historial (pila) con estilo de registro de combate.
            Controls.Add(Estilo.CrearEtiqueta("REGISTRO DE COMBATE (pila)", 900, 44, 330, 22, 10.5f, true));

            lstHistorial = new ListBox();
            lstHistorial.SetBounds(900, 68, 330, 420);
            lstHistorial.BackColor = Color.FromArgb(14, 15, 24);
            lstHistorial.ForeColor = Estilo.Texto;
            lstHistorial.BorderStyle = BorderStyle.None;
            lstHistorial.Font = new Font("Consolas", 9f);
            lstHistorial.SelectionMode = SelectionMode.None;
            lstHistorial.DrawMode = DrawMode.OwnerDrawVariable;
            lstHistorial.MeasureItem += Historial_MeasureItem;
            lstHistorial.DrawItem += Historial_DrawItem;
            Controls.Add(lstHistorial);

            Controls.Add(Estilo.CrearEtiqueta("Última jugada (Peek de la pila):", 900, 496, 330, 20, 9f, true));
            lblUltimaJugada = Estilo.CrearEtiqueta(string.Empty, 900, 518, 330, 70, 9.5f, false);
            lblUltimaJugada.ForeColor = Estilo.TextoSuave;
            Controls.Add(lblUltimaJugada);

            btnRendirse = Estilo.CrearBoton("Rendirse", 900, 616, 330, 38);
            btnRendirse.BackColor = Color.FromArgb(150, 50, 50);
            btnRendirse.Click += delegate { Close(); };
            Controls.Add(btnRendirse);
        }

        private Label CrearEtiquetaMesa(int x, int y, int ancho, int alto)
        {
            Label l = Estilo.CrearEtiqueta(string.Empty, x, y, ancho, alto, 9f, true);
            mesa.Controls.Add(l);
            return l;
        }

        /// <summary>Crea un FlowLayoutPanel con una carta pequeña por cada puesto de la banca.</summary>
        private void CrearBanca(int x, int y, VistaCarta[] cartas)
        {
            FlowLayoutPanel flujo = new FlowLayoutPanel();
            flujo.SetBounds(x, y, 276, 128);
            flujo.FlowDirection = FlowDirection.LeftToRight;
            flujo.WrapContents = false;
            flujo.BackColor = Color.FromArgb(10, 42, 32);
            flujo.Padding = new Padding(4);
            mesa.Controls.Add(flujo);

            for (int i = 0; i < cartas.Length; i++)
            {
                cartas[i] = new VistaCarta(true);
                flujo.Controls.Add(cartas[i]);
            }
        }

        private Button CrearBotonAccion(string texto, int y, Color color)
        {
            Button b = Estilo.CrearBoton(texto, 344, y, 180, 38);
            b.BackColor = color;
            mesa.Controls.Add(b);
            return b;
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
            if (disposing)
            {
                if (timerCpu != null) { timerCpu.Stop(); timerCpu.Dispose(); }
                if (timerTemblor != null) { timerTemblor.Stop(); timerTemblor.Dispose(); }
            }
            base.Dispose(disposing);
        }

        // ---------- Presentación: aquí se enlaza la lógica con las cartas ----------

        private void Refrescar()
        {
            // Las cartas grandes siempre muestran al personaje ACTIVO de cada equipo.
            cartaJugador.ActualizarDatos(combate.Jugador.Activo, "EN CAMPO", true);
            cartaRival.ActualizarDatos(combate.Rival.Activo, "EN CAMPO", true);

            // Las cartas pequeñas muestran la COLA (Queue) de cada banca, en orden.
            ActualizarBanca(combate.Jugador, bancaJugador, lblBancaJugador, "MI BANCA");
            ActualizarBanca(combate.Rival, bancaRival, lblBancaRival, "BANCA RIVAL");

            bool turnoJugador = combate.EsTurnoDelJugador;
            Personaje activo = combate.Jugador.Activo;
            string motivo;

            lblTitulo.Text = combate.Escenario.Nombre + "   ·   Ronda " + combate.Turno;

            if (combate.Terminado) lblInstruccion.Text = "El combate terminó.";
            else if (turnoJugador) lblInstruccion.Text = "Tu turno: " + activo.Nombre + ".\nElige una acción.";
            else lblInstruccion.Text = "Turno del rival...";

            btnAtacar.Enabled = turnoJugador;
            btnPasar.Enabled = turnoJugador;

            btnHabilidad.Text = activo.NombreHabilidad + "  (" + activo.CostoHabilidad + " ENG"
                                + (activo.CooldownActual > 0 ? ", recarga " + activo.CooldownActual : string.Empty) + ")";
            btnHabilidad.Enabled = turnoJugador && combate.PuedeEjecutar(AccionCombate.Habilidad(), out motivo);

            Personaje siguiente = combate.Jugador.ProximoRelevo;
            btnRelevo.Text = siguiente == null ? "Relevo (banca vacía)" : "Relevo: entra " + siguiente.Nombre;
            btnRelevo.Enabled = turnoJugador && combate.PuedeEjecutar(AccionCombate.Relevar(), out motivo);

            btnRendirse.Enabled = !combate.Terminado;

            ActualizarAvisoTerremoto();
            LlenarHistorial();
        }

        /// <summary>Muestra en las cartas pequeñas lo que hay en la cola: la primera es la que entra en el próximo relevo.</summary>
        private void ActualizarBanca(EquipoCombate equipo, VistaCarta[] cartas, Label titulo, string nombre)
        {
            Personaje[] cola = equipo.ObtenerBanca();
            for (int i = 0; i < cartas.Length; i++)
            {
                if (i < cola.Length)
                {
                    string estado = i == 0 ? "PRÓXIMO" : (i + 1) + ".º EN COLA";
                    cartas[i].ActualizarDatos(cola[i], estado, false);
                    cartas[i].Visible = true;
                }
                else
                {
                    cartas[i].Visible = false;
                }
            }
            int caidos = equipo.Miembros.Count - equipo.Vivos;
            titulo.Text = nombre + " (cola de relevo)   ·   Caídos: " + caidos;
        }

        /// <summary>Cuenta regresiva del Terremoto y, si ocurrió en esta ronda, la ventana tiembla.</summary>
        private void ActualizarAvisoTerremoto()
        {
            if (combate.Terminado)
            {
                lblTerremoto.Text = string.Empty;
                return;
            }
            if (combate.TerremotoEstaRonda)
            {
                lblTerremoto.Text = "¡TERREMOTO esta ronda!";
                lblTerremoto.ForeColor = Color.Orange;
                if (combate.Turno != ultimaRondaConTemblor)
                {
                    ultimaRondaConTemblor = combate.Turno;
                    IniciarTemblor();
                }
            }
            else
            {
                lblTerremoto.Text = "Terremoto en " + combate.RondasParaTerremoto + " ronda(s)";
                lblTerremoto.ForeColor = Estilo.TextoSuave;
            }
        }

        // ---------- Efecto de temblor ----------

        private void IniciarTemblor()
        {
            if (timerTemblor.Enabled) return;
            posicionOriginal = Location;
            pasosTemblor = 0;
            timerTemblor.Start();
        }

        private void TimerTemblor_Tick(object sender, EventArgs e)
        {
            pasosTemblor++;
            if (pasosTemblor > 12)
            {
                timerTemblor.Stop();
                Location = posicionOriginal;
                return;
            }
            int desplazamiento = (pasosTemblor % 2 == 0) ? 8 : -8;
            Location = new Point(posicionOriginal.X + desplazamiento, posicionOriginal.Y);
        }

        // ---------- Registro de combate (la PILA en un ListBox) ----------

        /// <summary>Copia la pila del historial al ListBox. Al recorrer un Stack sale primero lo más reciente.</summary>
        private void LlenarHistorial()
        {
            lstHistorial.BeginUpdate();
            lstHistorial.Items.Clear();
            foreach (string jugada in combate.Historial)
                lstHistorial.Items.Add(jugada);
            lstHistorial.EndUpdate();

            lblUltimaJugada.Text = combate.UltimaJugada;
        }

        // Las líneas se dibujan a mano para que se vean en varias líneas y con color.
        private void Historial_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            string texto = lstHistorial.Items[e.Index].ToString();
            SizeF tamano = e.Graphics.MeasureString(texto, lstHistorial.Font, lstHistorial.ClientSize.Width - 10);
            e.ItemHeight = (int)tamano.Height + 4;
        }

        private void Historial_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            string texto = lstHistorial.Items[e.Index].ToString();
            e.DrawBackground();
            RectangleF zona = new RectangleF(e.Bounds.X + 4, e.Bounds.Y + 2, e.Bounds.Width - 10, e.Bounds.Height - 2);
            using (SolidBrush pincel = new SolidBrush(ColorDeLinea(texto)))
            {
                e.Graphics.DrawString(texto, lstHistorial.Font, pincel, zona);
            }
        }

        private Color ColorDeLinea(string texto)
        {
            if (texto.StartsWith("—")) return Estilo.TextoSuave;
            if (texto.Contains("TERREMOTO") || texto.StartsWith("   ")) return Color.Orange;
            if (texto.Contains("derrotado") || texto.Contains("Has sido")) return Color.IndianRed;
            if (texto.Contains("Victoria") || texto.Contains("MUERTE")) return Color.Gold;
            if (texto.Contains("cura")) return Color.LightGreen;
            return Estilo.Texto;
        }
    }
}
