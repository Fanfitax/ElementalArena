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
    ///  - Fondo: la imagen Imagenes/tablero.jpg (si no existe, un fondo oscuro liso).
    ///  - Centro: la carta del jugador frente a la del rival, con los botones de acción entre las dos.
    ///  - A los lados: la banca de cada equipo en un FlowLayoutPanel. Es la COLA de relevos: la primera carta
    ///    (con borde dorado) es la que entraría en el próximo relevo.
    ///  - A la derecha: el registro de combate, que es la PILA del historial en un ListBox.
    /// </summary>
    public class FrmCombate : FormularioModerno
    {
        private const int RetardoCpuMs = 900;

        // Colores de los paneles oscuros (casi negros) que destacan sobre la imagen de fondo.
        private static Color PanelOscuro = Color.FromArgb(12, 12, 16);
        private static Color PanelOscuroTranslucido = Color.FromArgb(175, 8, 8, 12);

        private string nombreJugador;
        private Combate combate;
        private IaRival ia = new IaRival();

        // Cartas: una grande por equipo (en el campo) y cartas pequeñas para la banca.
        private VistaCarta cartaJugador, cartaRival;
        private VistaCarta[] bancaJugador = new VistaCarta[EquipoCombate.TamanoEquipo - 1];
        private VistaCarta[] bancaRival = new VistaCarta[EquipoCombate.TamanoEquipo - 1];

        private Label lblTitulo, lblTurno, lblInstruccion, lblTerremoto, lblUltimaJugada;
        private Label lblBancaJugador, lblBancaRival, lblEquipoJugador, lblEquipoRival;
        private Button btnAtacar, btnHabAtaque, btnHabUtilidad, btnRelevo, btnPasar, btnRendirse;
        private ListBox lstHistorial;
        private ContextMenuStrip menuAliados;
        private ToolTip ayuda = new ToolTip();

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
            Estilo.AplicarFormulario(this, "Arena Elemental - Combate", 1264, 500);
            BackColor = Color.FromArgb(12, 18, 28);
            CargarFondo();

            ConstruirBarraSuperior();
            ConstruirCampoDeDuelo();
            ConstruirPanelRegistro();
        }

        /// <summary>Intenta poner Imagenes/tablero.jpg como fondo. Si no existe se queda el color liso.</summary>
        private void CargarFondo()
        {
            Image fondo = Estilo.CargarImagen("tablero.jpg");
            if (fondo == null) return;

            BackgroundImage = fondo;
            BackgroundImageLayout = ImageLayout.Stretch;
        }

        /// <summary>Franja oscura de arriba con el escenario, la ronda y el recordatorio de reglas.</summary>
        private void ConstruirBarraSuperior()
        {
            Panel barra = new Panel();
            barra.SetBounds(0, 0, 1264, 36);
            barra.BackColor = PanelOscuro;
            Controls.Add(barra);

            lblTitulo = Estilo.CrearEtiqueta(string.Empty, 16, 4, 520, 28, 14f, true);
            lblTitulo.ForeColor = Color.Gold;
            barra.Controls.Add(lblTitulo);

            Label lblReglas = Estilo.CrearEtiqueta(
                "Triángulo: " + Estilo.Triangulo + "   ·   Muerte súbita: ronda " + Combate.TurnoMuerteSubita,
                540, 2, 710, 32, 9f, false);
            lblReglas.TextAlign = ContentAlignment.MiddleRight;
            barra.Controls.Add(lblReglas);
        }

        /// <summary>Las dos cartas del campo, las dos bancas y la zona central con los botones.</summary>
        private void ConstruirCampoDeDuelo()
        {
            // Nombres de los equipos sobre sus cartas.
            lblEquipoJugador = CrearTitulo(string.Empty, 144, 232);
            lblEquipoRival = CrearTitulo(string.Empty, 592, 232);

            cartaJugador = new VistaCarta(false);
            cartaJugador.Location = new Point(144, 70);
            Controls.Add(cartaJugador);

            cartaRival = new VistaCarta(false);
            cartaRival.Location = new Point(592, 70);
            Controls.Add(cartaRival);

            // Bancas: mi banca a la izquierda, la del rival a la derecha.
            lblBancaJugador = CrearTitulo(string.Empty, 4, 136);
            CrearBanca(4, bancaJugador);
            lblBancaRival = CrearTitulo(string.Empty, 828, 136);
            CrearBanca(828, bancaRival);

            // Zona central entre las dos cartas.
            lblTurno = Estilo.CrearEtiqueta(string.Empty, 384, 70, 200, 44, 14f, true);
            lblTurno.BackColor = PanelOscuroTranslucido;
            lblTurno.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(lblTurno);

            lblInstruccion = Estilo.CrearEtiqueta(string.Empty, 384, 118, 200, 36, 9f, false);
            lblInstruccion.BackColor = PanelOscuroTranslucido;
            lblInstruccion.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(lblInstruccion);

            btnAtacar = CrearBotonAccion(160, Color.Crimson);
            btnAtacar.Click += delegate { EjecutarJugador(AccionCombate.Atacar()); };

            menuAliados = new ContextMenuStrip();
            btnHabAtaque = CrearBotonAccion(206, Color.FromArgb(88, 40, 140));
            btnHabAtaque.Click += delegate { UsarHabilidadDelJugador(0); };

            btnHabUtilidad = CrearBotonAccion(252, Color.FromArgb(16, 122, 104));
            btnHabUtilidad.Click += delegate { UsarHabilidadDelJugador(1); };

            btnRelevo = CrearBotonAccion(298, Color.FromArgb(24, 110, 160));
            btnRelevo.Click += delegate { EjecutarJugador(AccionCombate.Relevar()); };

            btnPasar = CrearBotonAccion(344, Color.FromArgb(70, 78, 100));
            btnPasar.Click += delegate { EjecutarJugador(AccionCombate.Pasar()); };

            lblTerremoto = Estilo.CrearEtiqueta(string.Empty, 384, 394, 200, 28, 9.5f, true);
            lblTerremoto.BackColor = PanelOscuroTranslucido;
            lblTerremoto.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(lblTerremoto);

            Label vs = Estilo.CrearEtiqueta("VS", 384, 430, 200, 50, 28f, true);
            vs.ForeColor = Color.Gold;
            vs.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(vs);
        }

        /// <summary>Columna derecha: el historial (pila) con estilo de registro de combate.</summary>
        private void ConstruirPanelRegistro()
        {
            Panel panel = new Panel();
            panel.SetBounds(972, 44, 284, 448);
            panel.BackColor = PanelOscuro;
            Controls.Add(panel);
            Estilo.Redondear(panel, 12);

            Label titulo = Estilo.CrearEtiqueta("REGISTRO DE COMBATE (pila)", 12, 8, 260, 22, 10f, true);
            titulo.ForeColor = Color.Gold;
            panel.Controls.Add(titulo);

            lstHistorial = new ListBox();
            lstHistorial.SetBounds(10, 34, 264, 288);
            lstHistorial.BackColor = Color.FromArgb(6, 6, 9);
            lstHistorial.ForeColor = Color.White;
            lstHistorial.BorderStyle = BorderStyle.None;
            lstHistorial.Font = new Font("Consolas", 9f);
            lstHistorial.SelectionMode = SelectionMode.None;
            lstHistorial.DrawMode = DrawMode.OwnerDrawVariable;
            lstHistorial.MeasureItem += Historial_MeasureItem;
            lstHistorial.DrawItem += Historial_DrawItem;
            panel.Controls.Add(lstHistorial);

            Label ultima = Estilo.CrearEtiqueta("Última jugada (Peek de la pila):", 12, 328, 260, 18, 8.5f, true);
            ultima.ForeColor = Color.Gold;
            panel.Controls.Add(ultima);

            lblUltimaJugada = Estilo.CrearEtiqueta(string.Empty, 12, 348, 260, 46, 9f, false);
            lblUltimaJugada.ForeColor = Color.White;
            panel.Controls.Add(lblUltimaJugada);

            btnRendirse = new Button();
            btnRendirse.SetBounds(12, 398, 260, 38);
            Estilo.EstilizarBoton(btnRendirse, "Rendirse", Color.FromArgb(130, 30, 40));
            btnRendirse.Click += delegate { Close(); };
            panel.Controls.Add(btnRendirse);
        }

        private Label CrearTitulo(string texto, int x, int ancho)
        {
            Label l = Estilo.CrearEtiqueta(texto, x, 44, ancho, 22, 10.5f, true);
            l.ForeColor = Color.Gold;
            l.BackColor = PanelOscuroTranslucido;
            Controls.Add(l);
            return l;
        }

        /// <summary>
        /// Crea una columna (FlowLayoutPanel casi negro) con una carta pequeña por cada puesto de la banca.
        /// Las cartas se acomodan de arriba hacia abajo en el orden de la cola.
        /// </summary>
        private void CrearBanca(int x, VistaCarta[] cartas)
        {
            FlowLayoutPanel flujo = new FlowLayoutPanel();
            flujo.SetBounds(x, 68, 136, 426);
            flujo.FlowDirection = FlowDirection.TopDown;
            flujo.WrapContents = false;
            flujo.BackColor = PanelOscuro;
            flujo.ForeColor = Color.White;
            flujo.Padding = new Padding(2);
            Controls.Add(flujo);
            Estilo.Redondear(flujo, 12);

            for (int i = 0; i < cartas.Length; i++)
            {
                cartas[i] = new VistaCarta(true);
                cartas[i].Margin = new Padding(2);
                flujo.Controls.Add(cartas[i]);
            }
        }

        private Button CrearBotonAccion(int y, Color color)
        {
            Button b = new Button();
            b.SetBounds(392, y, 184, 40);
            Estilo.EstilizarBoton(b, string.Empty, color);
            Controls.Add(b);
            return b;
        }

        // ---------- Acciones del jugador ----------

        /// <summary>Usa la habilidad 0 (ataque) o 1 (utilidad). Si cura a un aliado, primero se elige a cuál.</summary>
        private void UsarHabilidadDelJugador(int numero)
        {
            Personaje activo = combate.Jugador.Activo;
            Habilidad habilidad = activo.ObtenerHabilidad(numero);
            if (!habilidad.RequiereAliado)
            {
                EjecutarJugador(AccionCombate.Habilidad(numero));
                return;
            }

            menuAliados.Items.Clear();
            for (int i = 0; i < combate.Jugador.Miembros.Count; i++)
            {
                Personaje aliado = combate.Jugador.Miembros[i];
                if (!aliado.EstaVivo) continue;
                int indice = i;
                string texto = (aliado == activo ? "Curarse a sí mismo" : "Curar a " + aliado.Nombre)
                               + "  (" + aliado.Hp + "/" + aliado.HpMax + ")";
                ToolStripMenuItem item = new ToolStripMenuItem(texto);
                item.Click += delegate { EjecutarJugador(AccionCombate.HabilidadSobreAliado(numero, indice)); };
                menuAliados.Items.Add(item);
            }
            Button boton = numero == 0 ? btnHabAtaque : btnHabUtilidad;
            menuAliados.Show(boton, new Point(0, boton.Height));
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
                if (BackgroundImage != null) BackgroundImage.Dispose();
                ayuda.Dispose();
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

            // Nombre de cada equipo con la cantidad de personajes caídos.
            lblEquipoJugador.Text = combate.Jugador.Nombre.ToUpper() + "  ·  Caídos: " + (combate.Jugador.Miembros.Count - combate.Jugador.Vivos);
            lblEquipoRival.Text = combate.Rival.Nombre.ToUpper() + "  ·  Caídos: " + (combate.Rival.Miembros.Count - combate.Rival.Vivos);

            bool turnoJugador = combate.EsTurnoDelJugador;
            Personaje activo = combate.Jugador.Activo;
            string motivo;

            lblTitulo.Text = combate.Escenario.Nombre + "   ·   Ronda " + combate.Turno;

            if (combate.Terminado)
            {
                lblTurno.Text = "FIN";
                lblTurno.ForeColor = Color.White;
                lblInstruccion.Text = "El combate terminó.";
            }
            else if (turnoJugador)
            {
                lblTurno.Text = "TU TURNO";
                lblTurno.ForeColor = Color.Gold;
                lblInstruccion.Text = "Juega " + activo.Nombre + ". Elige una acción.";
            }
            else
            {
                lblTurno.Text = "TURNO RIVAL";
                lblTurno.ForeColor = Color.IndianRed;
                lblInstruccion.Text = combate.Rival.Activo.Nombre + " está pensando...";
            }
            lblInstruccion.ForeColor = Color.White;

            btnAtacar.Text = "Atacar\nataque básico";
            btnAtacar.Enabled = turnoJugador;
            btnPasar.Text = "Pasar\n+" + Combate.EnergiaAlDescansar + " ENG";
            btnPasar.Enabled = turnoJugador;

            ActualizarBotonHabilidad(btnHabAtaque, activo, 0, turnoJugador);
            ActualizarBotonHabilidad(btnHabUtilidad, activo, 1, turnoJugador);

            Personaje siguiente = combate.Jugador.ProximoRelevo;
            btnRelevo.Text = siguiente == null ? "Relevo\nbanca vacía" : "Relevo\nentra " + siguiente.Nombre;
            btnRelevo.Enabled = turnoJugador && combate.PuedeEjecutar(AccionCombate.Relevar(), out motivo);

            btnRendirse.Enabled = !combate.Terminado;

            ActualizarAvisoTerremoto();
            LlenarHistorial();
        }

        /// <summary>Pone en el botón el nombre de la habilidad y su costo (o la recarga), y la activa solo si se puede usar.</summary>
        private void ActualizarBotonHabilidad(Button boton, Personaje activo, int numero, bool turnoJugador)
        {
            Habilidad h = activo.ObtenerHabilidad(numero);
            string detalle = h.CooldownActual > 0
                ? "recarga " + h.CooldownActual + (h.CooldownActual == 1 ? " turno" : " turnos")
                : h.Tipo + " · " + h.Costo + " ENG";
            boton.Text = h.Nombre + "\n" + detalle;
            boton.Enabled = turnoJugador && combate.PuedeEjecutar(AccionCombate.Habilidad(numero), out detalle);
            ayuda.SetToolTip(boton, h.Descripcion);
        }

        /// <summary>Muestra en las cartas pequeñas lo que hay en la cola: la primera es la que entra en el próximo relevo.</summary>
        private void ActualizarBanca(EquipoCombate equipo, VistaCarta[] cartas, Label titulo, string nombre)
        {
            Personaje[] cola = equipo.ObtenerBanca();
            for (int i = 0; i < cartas.Length; i++)
            {
                if (i < cola.Length)
                {
                    string estado = i == 0 ? "SIGUE EN ENTRAR" : (i + 1) + ".º EN COLA";
                    cartas[i].EsSiguiente = (i == 0);
                    cartas[i].ActualizarDatos(cola[i], estado, false);
                    cartas[i].Visible = true;
                }
                else
                {
                    cartas[i].Visible = false;
                }
            }
            titulo.Text = nombre;
        }

        /// <summary>Cuenta regresiva del Terremoto y, si ocurrió en esta ronda, la ventana tiembla.</summary>
        private void ActualizarAvisoTerremoto()
        {
            if (combate.Terminado)
            {
                lblTerremoto.Visible = false;
                return;
            }
            lblTerremoto.Visible = true;
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
                lblTerremoto.ForeColor = Color.White;
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
            if (texto.StartsWith("—")) return Color.Gold;
            if (texto.Contains("TERREMOTO") || texto.StartsWith("   ")) return Color.Orange;
            if (texto.Contains("derrotado") || texto.Contains("Has sido")) return Color.IndianRed;
            if (texto.Contains("Victoria") || texto.Contains("MUERTE")) return Color.Gold;
            if (texto.Contains("cura")) return Color.LightGreen;
            return Color.White;
        }
    }
}
