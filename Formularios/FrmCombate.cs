using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Modelos;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    // Pantalla de combate dibujada como una mesa de duelo:
    //  - Fondo: la imagen del escenario elegido (volcan, lago, canon o cumbre). Si no existe, un color liso.
    //  - Centro: la carta del jugador frente a la del rival, con los botones de acción entre las dos.
    //  - A los lados: la banca de cada equipo como una "mano" de cartas en línea (FlowLayoutPanel horizontal).
    //    Es la COLA de relevos: la primera carta (con borde dorado) es la que entraría en el próximo relevo
    //    y siempre es la más cercana a la carta que está en el campo.
    //  - A la derecha: el registro de combate, que es la PILA del historial en un ListBox.
    //  - Efectos: números flotantes de daño/curación sobre las cartas, sonidos, temblor y banner del Terremoto.
    //
    // El formulario sirve para dos modos:
    //  - Contra la CPU: el rival juega con IaRival.
    //  - Partida LAN: el rival es otra persona en otro PC. Cada jugada propia se envía por la red (RedLan) y las
    //    jugadas del otro llegan por la misma conexión. En los dos PCs el Combate es el mismo (el anfitrión es el
    //    "Jugador" y el invitado el "Rival"); por eso aquí se usan "local" (mi equipo) y "remoto" (el otro).
    public class FrmCombate : FormularioModerno
    {
        private const int RetardoCpuMs = 900;

        // Tamaño de la ventana y posiciones del tablero. Las piezas son pocas y de tamaño contenido
        // para que la imagen del escenario se vea entre ellas: bancas a los lados, las dos cartas del campo,
        // los botones en el centro y el registro abajo a la izquierda. Abajo a la derecha queda libre.
        private const int AnchoVentana = 1120;
        private const int AltoVentana = 560;
        private const int YTitulos = 46;
        private const int YCampo = 74;
        private const int XBancaJugador = 14;
        private const int XCartaJugador = 222;
        private const int XCentro = 470;
        private const int AnchoCentro = 180;
        private const int XCartaRival = 682;
        private const int XBancaRival = 908;
        private const int AnchoBanca = 198;

        // Textos flotantes: cuánto viven y cuánto suben (en pasos de 30 ms).
        private const int VidaTextoFlotante = 50;
        private const int AlturaTextoFlotante = 100;
        private const int EsperaTerremoto = 24;

        // Un golpe que quita este % de la vida máxima (o más) hace temblar un poco la ventana.
        private const int GolpeFuertePct = 25;

        // Colores de los paneles oscuros que destacan sobre la imagen de fondo: son translúcidos para que
        // se siga viendo el escenario por detrás.
        private static Color PanelOscuro = Color.FromArgb(12, 12, 16);
        private static Color PanelOscuroTranslucido = Color.FromArgb(150, 8, 8, 12);
        private static Color PanelBancaTranslucido = Color.FromArgb(150, 8, 8, 12);

        private string nombreJugador;
        private Combate combate;
        private IaRival ia = new IaRival();

        // Modo de juego: mi equipo (local), el equipo contrario (remoto) y la conexión si es una partida LAN.
        private EquipoCombate local, remoto;
        private bool soyAnfitrion;
        private RedLan red;
        private Timer timerRed;

        // Cartas: una grande por equipo (en el campo) y cartas pequeñas para la banca.
        private VistaCarta cartaJugador, cartaRival;
        private VistaCarta[] bancaJugador = new VistaCarta[EquipoCombate.TamanoEquipo - 1];
        private VistaCarta[] bancaRival = new VistaCarta[EquipoCombate.TamanoEquipo - 1];

        private Label lblTitulo, lblTurno, lblInstruccion, lblTerremoto, lblUltimaJugada;
        private Label lblBancaJugador, lblBancaRival, lblEquipoJugador, lblEquipoRival;
        private Button btnAtacar, btnHabAtaque, btnHabUtilidad, btnRelevo, btnPasar, btnRendirse, btnReglas;
        private ListBox lstHistorial;
        private ContextMenuStrip menuAliados;
        private int numeroHabilidadPendiente;   // la habilidad que espera que se elija a qué aliado curar
        private ToolTip ayuda = new ToolTip();

        private Timer timerCpu;
        private Timer timerTemblor;
        private int pasosTemblor;
        private int pasosTotalesTemblor;
        private int amplitudTemblor;
        private int ultimaRondaConTemblor;
        private Point posicionOriginal;
        private bool partidaGuardada;

        // Números flotantes de daño y curación sobre las cartas.
        private Timer timerFlotantes;
        private List<TextoFlotante> flotantes = new List<TextoFlotante>();
        private Font fuenteFlotante = new Font("Segoe UI", 18f, FontStyle.Bold);
        private Font fuenteAviso = new Font("Segoe UI", 11f, FontStyle.Bold);

        // Partículas (chispas, brillos, escombros) sobre el tablero.
        private EfectosCombate efectos;

        // Anuncio de eventos: se muestra un instante después de la jugada para que no se pisen los efectos.
        private Timer timerEvento;
        private bool temblorPendiente;
        private string avisoTitulo, avisoDetalle;
        private bool muerteSubitaAnunciada;
        private AvisoEvento aviso;

        // Un número que sube sobre una carta y desaparece (daño en rojo, curación en verde, Terremoto en naranja).
        private class TextoFlotante
        {
            public Label Etiqueta;
            public int YInicial;
            public int Espera;   // pasos que faltan para que aparezca
            public int Edad;     // pasos que lleva visible
        }

        // Combate contra la CPU.
        public FrmCombate(string nombreJugador, Combate combate)
            : this(nombreJugador, combate, null, true)
        {
        }

        // Combate contra la CPU (red = null) o partida LAN (red = la conexión ya establecida).
        // "soyAnfitrion" indica si mi equipo es el Jugador del combate (true) o el Rival (false).
        public FrmCombate(string nombreJugador, Combate combate, RedLan red, bool soyAnfitrion)
        {
            this.nombreJugador = nombreJugador;
            this.combate = combate;
            this.red = red;
            this.soyAnfitrion = soyAnfitrion;

            if (soyAnfitrion)
            {
                local = combate.Jugador;
                remoto = combate.Rival;
            }
            else
            {
                local = combate.Rival;
                remoto = combate.Jugador;
            }

            ConstruirInterfaz();
            efectos = new EfectosCombate(this);

            timerCpu = new Timer();
            timerCpu.Interval = RetardoCpuMs;
            timerCpu.Tick += TimerCpu_Tick;

            // El temblor de la ventana cuando ocurre el Terremoto.
            timerTemblor = new Timer();
            timerTemblor.Interval = 40;
            timerTemblor.Tick += TimerTemblor_Tick;

            timerFlotantes = new Timer();
            timerFlotantes.Interval = 30;
            timerFlotantes.Tick += TimerFlotantes_Tick;

            timerEvento = new Timer();
            timerEvento.Interval = 600;
            timerEvento.Tick += TimerEvento_Tick;

            // En una partida LAN este reloj revisa varias veces por segundo si llegó algo del otro jugador.
            timerRed = new Timer();
            timerRed.Interval = 150;
            timerRed.Tick += TimerRed_Tick;

            Refrescar();
            if (EsPartidaLan) timerRed.Start();
        }

        private bool EsPartidaLan
        {
            get { return red != null; }
        }

        // True si la partida sigue y le toca jugar a MI equipo.
        private bool EsMiTurno
        {
            get { return !combate.Terminado && combate.EquipoEnTurno == local; }
        }

        private int DanoTerremotoLocal
        {
            get
            {
                if (soyAnfitrion) return combate.DanoTerremotoJugador;
                return combate.DanoTerremotoRival;
            }
        }

        private int DanoTerremotoRemoto
        {
            get
            {
                if (soyAnfitrion) return combate.DanoTerremotoRival;
                return combate.DanoTerremotoJugador;
            }
        }

        // ---------- Construcción de la interfaz (todo por código) ----------

        private void ConstruirInterfaz()
        {
            Estilo.AplicarFormulario(this, Idioma.Texto("Arena Elemental - Combate", "Arena Elemental - Battle"), AnchoVentana, AltoVentana);
            BackColor = Color.FromArgb(12, 18, 28);
            CargarFondoDelEscenario();

            ConstruirBarraSuperior();
            ConstruirCampoDeDuelo();
            ConstruirPanelRegistro();
        }

        // ---------- Fondo según el escenario ----------

        // Pone como fondo la imagen que corresponde al elemento del escenario elegido:
        // Fuego = volcan.jpg, Agua = lago.jpg, Tierra = canon.jpg, Aire = cumbre.jpg.
        // Si la imagen no existe o está dañada se usa un color liso del tema del elemento.
        private void CargarFondoDelEscenario()
        {
            Elemento elemento = combate.Escenario.ElementoAfin;
            Image original = Estilo.CargarImagen(NombreImagenDeFondo(elemento));

            if (original == null)
            {
                FondoModerno = false;
                BackColor = ColorDeRespaldo(elemento);
                return;
            }

            BackgroundImage = AjustarAlTablero(original, ClientSize.Width, ClientSize.Height);
            BackgroundImageLayout = ImageLayout.Stretch;
            original.Dispose();
        }

        private string NombreImagenDeFondo(Elemento elemento)
        {
            if (elemento == Elemento.Fuego) return "volcan.jpg";
            if (elemento == Elemento.Agua) return "lago.jpg";
            if (elemento == Elemento.Tierra) return "canon.jpg";
            return "cumbre.jpg";   // Aire
        }

        private Color ColorDeRespaldo(Elemento elemento)
        {
            if (elemento == Elemento.Fuego) return Color.FromArgb(52, 16, 10);
            if (elemento == Elemento.Agua) return Color.FromArgb(10, 32, 56);
            if (elemento == Elemento.Tierra) return Color.FromArgb(48, 34, 16);
            return Color.FromArgb(28, 52, 70);   // Aire
        }

        // Recorta la imagen por el centro para que tenga la misma forma que el tablero (sin deformarla) y la
        // oscurece un poco para que se lean bien los textos que van encima.
        private Bitmap AjustarAlTablero(Image original, int ancho, int alto)
        {
            double escala = Math.Max((double)ancho / original.Width, (double)alto / original.Height);
            int anchoOrigen = (int)(ancho / escala);
            int altoOrigen = (int)(alto / escala);
            Rectangle zona = new Rectangle((original.Width - anchoOrigen) / 2, (original.Height - altoOrigen) / 2, anchoOrigen, altoOrigen);

            Bitmap tablero = new Bitmap(ancho, alto);
            using (Graphics g = Graphics.FromImage(tablero))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, new Rectangle(0, 0, ancho, alto), zona, GraphicsUnit.Pixel);
                using (SolidBrush velo = new SolidBrush(Color.FromArgb(32, 0, 0, 0)))
                {
                    g.FillRectangle(velo, 0, 0, ancho, alto);
                }
            }
            return tablero;
        }

        // Franja oscura de arriba con el escenario, la ronda, el recordatorio del triángulo y los botones Reglas y Rendirse.
        private void ConstruirBarraSuperior()
        {
            Panel barra = new Panel();
            barra.SetBounds(0, 0, AnchoVentana, 36);
            barra.BackColor = Color.FromArgb(185, 10, 10, 14);
            Controls.Add(barra);

            lblTitulo = Estilo.CrearEtiqueta(string.Empty, 16, 4, 420, 28, 14f, true);
            lblTitulo.ForeColor = Color.Gold;
            barra.Controls.Add(lblTitulo);

            Label lblTriangulo = Estilo.CrearEtiqueta(Idioma.Texto("Triángulo: ", "Triangle: ") + Estilo.Triangulo, 440, 2, 470, 32, 9f, false);
            lblTriangulo.TextAlign = ContentAlignment.MiddleRight;
            barra.Controls.Add(lblTriangulo);

            btnReglas = new Button();
            btnReglas.SetBounds(AnchoVentana - 14 - 90 - 8 - 84, 5, 84, 26);
            Estilo.EstilizarBoton(btnReglas, Idioma.Texto("Reglas", "Rules"), Color.FromArgb(60, 70, 110));
            btnReglas.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnReglas.Click += BtnReglas_Click;
            barra.Controls.Add(btnReglas);

            btnRendirse = new Button();
            btnRendirse.SetBounds(AnchoVentana - 14 - 90, 5, 90, 26);
            Estilo.EstilizarBoton(btnRendirse, Idioma.Texto("Rendirse", "Give up"), Color.FromArgb(130, 30, 40));
            btnRendirse.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnRendirse.Click += BtnRendirse_Click;
            barra.Controls.Add(btnRendirse);
        }

        // Las dos cartas del campo, las dos bancas y la zona central con los botones.
        private void ConstruirCampoDeDuelo()
        {
            // Nombres de los equipos sobre sus cartas.
            lblEquipoJugador = CrearTitulo(string.Empty, XCartaJugador, 216);
            lblEquipoRival = CrearTitulo(string.Empty, XCartaRival, 216);

            cartaJugador = new VistaCarta(TamanoCarta.Mediana);
            cartaJugador.Location = new Point(XCartaJugador, YCampo);
            Controls.Add(cartaJugador);

            cartaRival = new VistaCarta(TamanoCarta.Mediana);
            cartaRival.Location = new Point(XCartaRival, YCampo);
            Controls.Add(cartaRival);

            // Bancas: mi banca a la izquierda, la del rival a la derecha. Cada una es una mano de cartas en línea
            // y la carta que entra primero queda siempre pegada a la carta del campo (la mía fluye de derecha a
            // izquierda y la del rival de izquierda a derecha).
            lblBancaJugador = CrearTitulo(string.Empty, XBancaJugador, AnchoBanca);
            CrearBanca(XBancaJugador, bancaJugador, FlowDirection.RightToLeft);
            lblBancaRival = CrearTitulo(string.Empty, XBancaRival, AnchoBanca);
            CrearBanca(XBancaRival, bancaRival, FlowDirection.LeftToRight);

            // Zona central entre las dos cartas.
            lblTurno = Estilo.CrearEtiqueta(string.Empty, XCentro, YCampo, AnchoCentro, 40, 14f, true);
            lblTurno.BackColor = PanelOscuroTranslucido;
            lblTurno.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(lblTurno);

            lblInstruccion = Estilo.CrearEtiqueta(string.Empty, XCentro, YCampo + 44, AnchoCentro, 32, 9f, false);
            lblInstruccion.BackColor = PanelOscuroTranslucido;
            lblInstruccion.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(lblInstruccion);

            btnAtacar = CrearBotonAccion(YCampo + 82, Color.Crimson);
            btnAtacar.Click += BtnAtacar_Click;

            menuAliados = new ContextMenuStrip();
            btnHabAtaque = CrearBotonAccion(YCampo + 126, Color.FromArgb(88, 40, 140));
            btnHabAtaque.Click += BtnHabAtaque_Click;

            btnHabUtilidad = CrearBotonAccion(YCampo + 170, Color.FromArgb(16, 122, 104));
            btnHabUtilidad.Click += BtnHabUtilidad_Click;

            btnRelevo = CrearBotonAccion(YCampo + 214, Color.FromArgb(24, 110, 160));
            btnRelevo.Click += BtnRelevo_Click;

            btnPasar = CrearBotonAccion(YCampo + 258, Color.FromArgb(70, 78, 100));
            btnPasar.Click += BtnPasar_Click;

            lblTerremoto = Estilo.CrearEtiqueta(string.Empty, XCentro, YCampo + 304, AnchoCentro, 24, 9f, true);
            lblTerremoto.BackColor = PanelOscuroTranslucido;
            lblTerremoto.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(lblTerremoto);

            Label vs = Estilo.CrearEtiqueta("VS", XCentro, YCampo + 336, AnchoCentro, 56, 28f, true);
            vs.ForeColor = Color.Gold;
            vs.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(vs);
        }

        // El historial (pila) con estilo de registro de combate. Es un panel pequeño y translúcido
        // debajo de la banca del jugador; el resto de esa zona queda libre para que se vea el escenario.
        private void ConstruirPanelRegistro()
        {
            Panel panel = new Panel();
            panel.SetBounds(XBancaJugador, 296, AnchoBanca, 244);
            panel.BackColor = PanelBancaTranslucido;
            Controls.Add(panel);
            Estilo.Redondear(panel, 12);

            Label titulo = Estilo.CrearEtiqueta(Idioma.Texto("REGISTRO (pila)", "LOG (stack)"), 10, 6, AnchoBanca - 20, 20, 9f, true);
            titulo.ForeColor = Color.Gold;
            panel.Controls.Add(titulo);

            lstHistorial = new ListBox();
            lstHistorial.SetBounds(8, 30, AnchoBanca - 16, 136);
            lstHistorial.BackColor = Color.FromArgb(6, 6, 9);
            lstHistorial.ForeColor = Color.White;
            lstHistorial.BorderStyle = BorderStyle.None;
            lstHistorial.Font = new Font("Consolas", 8.5f);
            lstHistorial.SelectionMode = SelectionMode.None;
            lstHistorial.DrawMode = DrawMode.OwnerDrawVariable;
            lstHistorial.MeasureItem += Historial_MeasureItem;
            lstHistorial.DrawItem += Historial_DrawItem;
            panel.Controls.Add(lstHistorial);

            Label ultima = Estilo.CrearEtiqueta(Idioma.Texto("Última jugada (Peek):", "Last move (Peek):"), 10, 172, AnchoBanca - 20, 18, 8f, true);
            ultima.ForeColor = Color.Gold;
            panel.Controls.Add(ultima);

            lblUltimaJugada = Estilo.CrearEtiqueta(string.Empty, 10, 190, AnchoBanca - 20, 48, 8f, false);
            lblUltimaJugada.ForeColor = Color.White;
            panel.Controls.Add(lblUltimaJugada);
        }

        private Label CrearTitulo(string texto, int x, int ancho)
        {
            Label l = Estilo.CrearEtiqueta(texto, x, YTitulos, ancho, 22, 10f, true);
            l.ForeColor = Color.Gold;
            l.BackColor = PanelOscuroTranslucido;
            l.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(l);
            return l;
        }

        // Crea la "mano" de la banca: un FlowLayoutPanel translúcido con las cartas pequeñas en una sola fila.
        // El orden de las cartas es el de la cola; la dirección del flujo decide de qué lado empieza la fila.
        private void CrearBanca(int x, VistaCarta[] cartas, FlowDirection direccion)
        {
            FlowLayoutPanel flujo = new FlowLayoutPanel();
            flujo.SetBounds(x, YCampo, AnchoBanca, 210);
            flujo.FlowDirection = direccion;
            flujo.WrapContents = false;
            flujo.AutoScroll = false;
            flujo.BackColor = PanelBancaTranslucido;
            flujo.ForeColor = Color.White;
            flujo.Padding = new Padding(3, 3, 3, 3);
            Controls.Add(flujo);
            Estilo.Redondear(flujo, 12);

            for (int i = 0; i < cartas.Length; i++)
            {
                cartas[i] = new VistaCarta(TamanoCarta.Pequena);
                cartas[i].Margin = new Padding(2);
                flujo.Controls.Add(cartas[i]);
            }
        }

        private Button CrearBotonAccion(int y, Color color)
        {
            Button b = new Button();
            b.SetBounds(XCentro, y, AnchoCentro, 40);
            Estilo.EstilizarBoton(b, string.Empty, color);
            b.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            Controls.Add(b);
            return b;
        }

        // El resumen de las reglas que antes ocupaba dos paneles fijos: ahora aparece solo cuando se pide.
        private void BtnReglas_Click(object sender, EventArgs e)
        {
            string texto =
                Idioma.Texto("COLA DE RELEVOS (Queue)\n", "SWAP QUEUE (Queue)\n") +
                Idioma.Texto("Entra primero la carta con borde dorado, la más cercana a tu carta en el campo. ", "The card with the golden border enters first, the one closest to your card on the field. ") +
                Idioma.Texto("Quien sale del campo se forma al final de la cola. ", "Whoever leaves the field goes to the end of the queue. ") +
                Idioma.Texto("No se puede relevar dos turnos seguidos. Si cae tu personaje, entra el siguiente gratis.\n\n", "You cannot swap two turns in a row. If your character falls, the next one enters for free.\n\n") +
                Idioma.Texto("TERREMOTO\n", "EARTHQUAKE\n") +
                Idioma.Texto("Cada ronda hay ", "Every round there is a ") + Combate.ProbabilidadTerremoto + Idioma.Texto("% de probabilidad. Daño = Min(f'(t), ", "% chance. Damage = Min(f'(t), ") +
                Combate.LimiteTerremotoPct + Idioma.Texto("% de la vida máxima), con f(t) = 2t² + 5t y f'(t) = 4t + 5 (t = ronda). ", "% of max HP), with f(t) = 2t² + 5t and f'(t) = 4t + 5 (t = round). ") +
                Idioma.Texto("Nunca baja de 1 HP.\n\n", "It never drops below 1 HP.\n\n") +
                Idioma.Texto("MUERTE SÚBITA\n", "SUDDEN DEATH\n") +
                Idioma.Texto("Pasada la ronda ", "After round ") + Combate.TurnoMuerteSubita + Idioma.Texto(", el ataque de todos crece ", ", everyone's attack grows by ") +
                (int)Math.Round(Combate.IncrementoMuerteSubita * 100) + Idioma.Texto("% en cada ronda.\n\n", "% each round.\n\n") +
                Idioma.Texto("TRIÁNGULO ELEMENTAL\n", "ELEMENTAL TRIANGLE\n") + Estilo.Triangulo + Idioma.Texto(" (ventaja x1.5, desventaja x0.5).", " (advantage x1.5, disadvantage x0.5).");
            MessageBox.Show(texto, Idioma.Texto("Reglas del combate", "Battle rules"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ---------- Eventos de los botones ----------

        private void BtnAtacar_Click(object sender, EventArgs e)
        {
            EjecutarJugador(new AccionCombate(TipoAccion.Atacar));
        }

        private void BtnHabAtaque_Click(object sender, EventArgs e)
        {
            UsarHabilidadDelJugador(0);
        }

        private void BtnHabUtilidad_Click(object sender, EventArgs e)
        {
            UsarHabilidadDelJugador(1);
        }

        private void BtnRelevo_Click(object sender, EventArgs e)
        {
            EjecutarJugador(new AccionCombate(TipoAccion.Relevo));
        }

        private void BtnPasar_Click(object sender, EventArgs e)
        {
            EjecutarJugador(new AccionCombate(TipoAccion.Pasar));
        }

        private void BtnRendirse_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Se eligió a qué aliado curar en el menú. El número del aliado está guardado en el Tag del elemento.
        private void ItemAliado_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            int indiceAliado = (int)item.Tag;
            EjecutarJugador(new AccionCombate(TipoAccion.Habilidad, numeroHabilidadPendiente, indiceAliado));
        }

        // ---------- Acciones del jugador ----------

        // Usa la habilidad 0 (ataque) o 1 (utilidad). Si cura a un aliado, primero se elige a cuál.
        private void UsarHabilidadDelJugador(int numero)
        {
            Personaje activo = local.Activo;
            Habilidad habilidad = activo.ObtenerHabilidad(numero);
            if (!habilidad.RequiereAliado)
            {
                EjecutarJugador(new AccionCombate(TipoAccion.Habilidad, numero));
                return;
            }

            numeroHabilidadPendiente = numero;
            menuAliados.Items.Clear();
            for (int i = 0; i < local.Miembros.Count; i++)
            {
                Personaje aliado = local.Miembros[i];
                if (!aliado.EstaVivo) continue;

                string texto;
                if (aliado == activo) texto = Idioma.Texto("Curarse a sí mismo", "Heal yourself");
                else texto = Idioma.Texto("Curar a ", "Heal ") + aliado.Nombre;
                texto += "  (" + aliado.Hp + "/" + aliado.HpMax + ")";

                ToolStripMenuItem item = new ToolStripMenuItem(texto);
                item.Tag = i;   // posición del aliado en el equipo
                item.Click += ItemAliado_Click;
                menuAliados.Items.Add(item);
            }

            Button boton = btnHabUtilidad;
            if (numero == 0) boton = btnHabAtaque;
            menuAliados.Show(boton, new Point(0, boton.Height));
        }

        private void EjecutarJugador(AccionCombate accion)
        {
            if (!EsMiTurno)
            {
                MessageBox.Show(Idioma.Texto("No es tu turno.", "It is not your turn."), Idioma.Texto("Acción no disponible", "Action not available"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!combate.PuedeEjecutar(accion))
            {
                MessageBox.Show(combate.ObtenerMotivo(accion), Idioma.Texto("Acción no disponible", "Action not available"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AplicarAccion(accion);
            ProcesarSiguiente();
        }

        // Ejecuta la acción (mía, de la CPU o del otro jugador en LAN) en el motor y la presenta: suena el efecto,
        // suben los números de daño o curación sobre las cartas y se actualiza toda la pantalla.
        // Si la jugada es mía y es una partida LAN, también se envía al otro PC.
        private void AplicarAccion(AccionCombate accion)
        {
            bool esMia = (combate.EquipoEnTurno == local);

            // Se anota la vida de los dos personajes en el campo para saber luego cuánto cambió.
            Personaje localAntes = local.Activo;
            Personaje remotoAntes = remoto.Activo;
            int hpLocalAntes = localAntes.Hp;
            int hpRemotoAntes = remotoAntes.Hp;

            ReproducirSonidoDe(accion);
            combate.Ejecutar(accion);

            if (EsPartidaLan && esMia) red.EnviarAccion(accion);

            MostrarCambioDeVida(cartaJugador, localAntes, hpLocalAntes, local.Activo, DanoTerremotoLocal);
            MostrarCambioDeVida(cartaRival, remotoAntes, hpRemotoAntes, remoto.Activo, DanoTerremotoRemoto);

            // Si fue un ataque, se avisa cuando el golpe fue muy efectivo o poco efectivo.
            if (esMia) MostrarEficacia(accion, localAntes, remotoAntes, CambioDeVida(remotoAntes, hpRemotoAntes, remoto.Activo, DanoTerremotoRemoto), cartaRival);
            else MostrarEficacia(accion, remotoAntes, localAntes, CambioDeVida(localAntes, hpLocalAntes, local.Activo, DanoTerremotoLocal), cartaJugador);

            Refrescar();
        }

        private void ReproducirSonidoDe(AccionCombate accion)
        {
            if (accion.Tipo == TipoAccion.Atacar) Sonidos.Ataque();
            else if (accion.Tipo == TipoAccion.Habilidad) Sonidos.Habilidad();
            else if (accion.Tipo == TipoAccion.Relevo) Sonidos.Relevo();
        }

        // ---------- Flujo de turnos ----------

        private void ProcesarSiguiente()
        {
            if (combate.Terminado)
            {
                FinalizarPartida();
                return;
            }

            // Contra la CPU, cuando no es mi turno la CPU piensa un momento y juega.
            // En LAN no hace falta: el reloj de la red espera la jugada del otro jugador.
            if (!EsMiTurno && !EsPartidaLan) timerCpu.Start();
        }

        private void TimerCpu_Tick(object sender, EventArgs e)
        {
            timerCpu.Stop();
            if (combate.Terminado || EsMiTurno || EsPartidaLan) return;
            AplicarAccion(ia.Decidir(combate));
            ProcesarSiguiente();
        }

        // ---------- Partida LAN: lo que llega del otro PC ----------

        private void TimerRed_Tick(object sender, EventArgs e)
        {
            string mensaje = red.LeerTexto();
            while (mensaje != null)
            {
                ProcesarMensajeDeRed(mensaje);
                if (IsDisposed || partidaGuardada || combate.Terminado) return;
                mensaje = red.LeerTexto();
            }

            // Se revisa la conexión al final, para no perder una jugada que llegó justo antes de cerrarse.
            if (red.Desconectado && !combate.Terminado && !partidaGuardada)
                TerminarPorDesconexion(Idioma.Texto("Se perdió la conexión con el otro jugador.", "The connection with the other player was lost."));
        }

        private void ProcesarMensajeDeRed(string mensaje)
        {
            if (mensaje == "RENDIRSE")
            {
                RivalSeRindio();
                return;
            }

            AccionCombate accion = RedLan.TextoAAccion(mensaje);
            if (accion == null) return;   // un mensaje que no es una jugada se ignora

            // Una jugada del otro jugador solo vale cuando es SU turno y la acción es válida en este combate.
            if (EsMiTurno)
            {
                TerminarPorDesconexion(Idioma.Texto("Las partidas de los dos PCs dejaron de coincidir (Llegó una jugada fuera de turno.).", "The matches on the two PCs no longer match (A move arrived out of turn.)."));
                return;
            }
            if (!combate.PuedeEjecutar(accion))
            {
                TerminarPorDesconexion(Idioma.Texto("Las partidas de los dos PCs dejaron de coincidir (", "The matches on the two PCs no longer match (") + combate.ObtenerMotivo(accion) + ").");
                return;
            }

            AplicarAccion(accion);
            ProcesarSiguiente();
        }

        private void RivalSeRindio()
        {
            timerRed.Stop();
            GuardarPartida("Victoria");
            MessageBox.Show(Idioma.Texto("El otro jugador se rindió. ¡Ganas la partida!", "The other player gave up. You win the match!"), Idioma.Texto("Fin del combate", "End of battle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void TerminarPorDesconexion(string texto)
        {
            timerRed.Stop();
            GuardarPartida("Abandono");
            MessageBox.Show(texto, Idioma.Texto("Conexión perdida", "Connection lost"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ---------- Fin de la partida ----------

        private void FinalizarPartida()
        {
            if (partidaGuardada) return;

            // VictoriaJugador es la victoria del anfitrión; si mi equipo es el invitado se invierte.
            bool gane = (combate.Estado == EstadoCombate.VictoriaJugador) == soyAnfitrion;
            string resultado = "Derrota";
            if (gane) resultado = "Victoria";
            GuardarPartida(resultado);

            if (gane)
                MessageBox.Show(Idioma.Texto("¡Victoria! Derrotaste al rival en ", "Victory! You defeated the rival in ") + combate.Turno + Idioma.Texto(" rondas.", " rounds."),
                                Idioma.Texto("Fin del combate", "End of battle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(Idioma.Texto("Derrota. Tu equipo cayó en la ronda ", "Defeat. Your team fell in round ") + combate.Turno + ".",
                                Idioma.Texto("Fin del combate", "End of battle"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void GuardarPartida(string resultado)
        {
            if (partidaGuardada) return;
            partidaGuardada = true;
            RegistroPartidas.Agregar(nombreJugador, local.Nombre, combate.Escenario.Nombre,
                                     resultado, combate.Turno, local.DanoInfligido);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!combate.Terminado && !partidaGuardada)
            {
                DialogResult r = MessageBox.Show(Idioma.Texto("¿Abandonar el combate? Se registrará como abandono.", "Forfeit the battle? It will be recorded as a forfeit."),
                                                 Idioma.Texto("Rendirse", "Give up"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                timerCpu.Stop();
                if (EsPartidaLan) red.EnviarRendicion();   // el otro jugador gana
                GuardarPartida("Abandono");
            }
            timerEvento.Stop();
            timerRed.Stop();
            CerrarAviso();
            efectos.Detener();
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (timerCpu != null) { timerCpu.Stop(); timerCpu.Dispose(); }
                if (timerTemblor != null) { timerTemblor.Stop(); timerTemblor.Dispose(); }
                if (timerFlotantes != null) { timerFlotantes.Stop(); timerFlotantes.Dispose(); }
                if (timerEvento != null) { timerEvento.Stop(); timerEvento.Dispose(); }
                if (timerRed != null) { timerRed.Stop(); timerRed.Dispose(); }
                if (BackgroundImage != null) BackgroundImage.Dispose();
                if (efectos != null) efectos.Dispose();
                fuenteFlotante.Dispose();
                fuenteAviso.Dispose();
                ayuda.Dispose();
            }
            base.Dispose(disposing);
        }

        // ---------- Presentación: aquí se enlaza la lógica con las cartas ----------

        private void Refrescar()
        {
            // Las cartas grandes siempre muestran al personaje ACTIVO de cada equipo.
            cartaJugador.ActualizarDatos(local.Activo, Idioma.Texto("EN CAMPO", "ON FIELD"), true);
            cartaRival.ActualizarDatos(remoto.Activo, Idioma.Texto("EN CAMPO", "ON FIELD"), true);

            // Las cartas pequeñas muestran la COLA (Queue) de cada banca, en orden.
            ActualizarBanca(local, bancaJugador, lblBancaJugador, Idioma.Texto("MI BANCA", "MY BENCH"));
            ActualizarBanca(remoto, bancaRival, lblBancaRival, Idioma.Texto("BANCA RIVAL", "RIVAL BENCH"));

            // Nombre de cada equipo con la cantidad de personajes caídos.
            lblEquipoJugador.Text = local.Nombre.ToUpper() + Idioma.Texto("  ·  Caídos: ", "  ·  Fallen: ") + (local.Miembros.Count - local.Vivos);
            lblEquipoRival.Text = remoto.Nombre.ToUpper() + Idioma.Texto("  ·  Caídos: ", "  ·  Fallen: ") + (remoto.Miembros.Count - remoto.Vivos);

            bool turnoJugador = EsMiTurno;
            Personaje activo = local.Activo;

            lblTitulo.Text = combate.Escenario.Nombre + Idioma.Texto("   ·   Ronda ", "   ·   Round ") + combate.Turno;
            if (EsPartidaLan) lblTitulo.Text += "   ·   LAN";

            if (combate.Terminado)
            {
                lblTurno.Text = Idioma.Texto("FIN", "END");
                lblTurno.ForeColor = Color.White;
                lblInstruccion.Text = Idioma.Texto("El combate terminó.", "The battle is over.");
            }
            else if (turnoJugador)
            {
                lblTurno.Text = Idioma.Texto("TU TURNO", "YOUR TURN");
                lblTurno.ForeColor = Color.Gold;
                lblInstruccion.Text = Idioma.Texto("Juega ", "Playing: ") + activo.Nombre + Idioma.Texto(". Elige una acción.", ". Choose an action.");
            }
            else
            {
                lblTurno.Text = Idioma.Texto("TURNO RIVAL", "RIVAL'S TURN");
                lblTurno.ForeColor = Color.IndianRed;
                if (EsPartidaLan) lblInstruccion.Text = Idioma.Texto("Esperando la jugada de ", "Waiting for the move of ") + remoto.Nombre + "...";
                else lblInstruccion.Text = remoto.Activo.Nombre + Idioma.Texto(" está pensando...", " is thinking...");
            }
            lblInstruccion.ForeColor = Color.White;

            btnAtacar.Text = Idioma.Texto("Atacar\nataque básico", "Attack\nbasic attack");
            btnAtacar.Enabled = turnoJugador;
            btnPasar.Text = Idioma.Texto("Pasar\n+", "Pass\n+") + Combate.EnergiaAlDescansar + " ENG";
            btnPasar.Enabled = turnoJugador;

            ActualizarBotonHabilidad(btnHabAtaque, activo, 0, turnoJugador);
            ActualizarBotonHabilidad(btnHabUtilidad, activo, 1, turnoJugador);

            Personaje siguiente = local.ProximoRelevo;
            if (siguiente == null) btnRelevo.Text = Idioma.Texto("Relevo\nbanca vacía", "Swap\nbench empty");
            else btnRelevo.Text = Idioma.Texto("Relevo\nentra ", "Swap\nenters ") + siguiente.Nombre;
            btnRelevo.Enabled = turnoJugador && combate.PuedeEjecutar(new AccionCombate(TipoAccion.Relevo));

            btnRendirse.Enabled = !combate.Terminado;

            ActualizarAvisoTerremoto();
            LlenarHistorial();
        }

        // Pone en el botón el nombre de la habilidad y su costo (o la recarga), y la activa solo si se puede usar.
        private void ActualizarBotonHabilidad(Button boton, Personaje activo, int numero, bool turnoJugador)
        {
            Habilidad h = activo.ObtenerHabilidad(numero);
            string detalle;
            if (h.CooldownActual > 0)
            {
                string turnos = Idioma.Texto("turnos", "turns");
                if (h.CooldownActual == 1) turnos = Idioma.Texto("turno", "turn");
                detalle = Idioma.Texto("recarga ", "cooldown ") + h.CooldownActual + " " + turnos;
            }
            else
            {
                detalle = h.Tipo + " · " + h.Costo + " ENG";
            }
            boton.Text = h.Nombre + "\n" + detalle;
            boton.Enabled = turnoJugador && combate.PuedeEjecutar(new AccionCombate(TipoAccion.Habilidad, numero));
            ayuda.SetToolTip(boton, h.Descripcion);
        }

        // Muestra en las cartas pequeñas lo que hay en la cola: la primera es la que entra en el próximo relevo.
        private void ActualizarBanca(EquipoCombate equipo, VistaCarta[] cartas, Label titulo, string nombre)
        {
            List<Personaje> cola = equipo.ObtenerBanca();
            for (int i = 0; i < cartas.Length; i++)
            {
                if (i < cola.Count)
                {
                    string estado = (i + 1) + Idioma.Texto(".º EN COLA", ". IN QUEUE");
                    if (i == 0) estado = Idioma.Texto("SIGUE EN ENTRAR", "NEXT IN");
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

        // Estado del Terremoto: la probabilidad por ronda y, si ocurrió en esta ronda, se programa el aviso y el temblor.
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
                lblTerremoto.Text = Idioma.Texto("¡TERREMOTO esta ronda!", "EARTHQUAKE this round!");
                lblTerremoto.ForeColor = Color.Orange;
                if (combate.Turno != ultimaRondaConTemblor)
                {
                    ultimaRondaConTemblor = combate.Turno;
                    int t = combate.Turno;
                    ProgramarEvento(true, Idioma.Texto("¡TERREMOTO!", "EARTHQUAKE!"),
                        Idioma.Texto("Ronda t = ", "Round t = ") + t + ":  f'(t) = 4t + 5 = " + Combate.DerivadaTerremoto(t) +
                        Idioma.Texto("  (tope: ", "  (cap: ") + Combate.LimiteTerremotoPct + Idioma.Texto("% de la vida máxima)", "% of max HP)"));
                }
            }
            else
            {
                lblTerremoto.Text = Idioma.Texto("Terremoto: ", "Earthquake: ") + Combate.ProbabilidadTerremoto + Idioma.Texto("% por ronda", "% per round");
                lblTerremoto.ForeColor = Color.White;
            }

            // La muerte súbita empieza la ronda siguiente a TurnoMuerteSubita (igual que en el historial).
            if (!muerteSubitaAnunciada && combate.Turno > Combate.TurnoMuerteSubita)
            {
                muerteSubitaAnunciada = true;
                ProgramarEvento(false, Idioma.Texto("¡MUERTE SÚBITA!", "SUDDEN DEATH!"),
                    Idioma.Texto("El ataque de todos aumenta ", "Everyone's attack increases by ") + (int)Math.Round(Combate.IncrementoMuerteSubita * 100) + Idioma.Texto("% en cada ronda", "% each round"));
            }
        }

        // ---------- Anuncio de eventos (Terremoto y Muerte Súbita) ----------

        // Deja el evento listo para mostrarse medio segundo después de la jugada. Así el sonido y el golpe
        // del ataque se alcanzan a ver y a oír antes de que el banner y el temblor los tapen.
        private void ProgramarEvento(bool conTemblor, string titulo, string detalle)
        {
            temblorPendiente = conTemblor;
            avisoTitulo = titulo;
            avisoDetalle = detalle;
            timerEvento.Stop();
            timerEvento.Start();
        }

        private void TimerEvento_Tick(object sender, EventArgs e)
        {
            timerEvento.Stop();
            if (IsDisposed || combate.Terminado || avisoTitulo == null) return;

            if (temblorPendiente)
            {
                Sonidos.Terremoto();
                IniciarTemblor();

                // Lluvia de escombros por todo el tablero y las dos cartas del campo se sacuden.
                efectos.Escombros(AnchoVentana, 70);
                cartaJugador.Sacudir();
                cartaRival.Sacudir();
                cartaJugador.Destellar(Color.Orange);
                cartaRival.Destellar(Color.Orange);
            }
            else
            {
                // Muerte súbita: estallido rojo en el centro y las cartas se tiñen de rojo.
                efectos.Estallido(new Point(XCentro + AnchoCentro / 2, YCampo + 150), Color.Crimson, 60);
                cartaJugador.Destellar(Color.Crimson);
                cartaRival.Destellar(Color.Crimson);
            }
            MostrarAviso(avisoTitulo, avisoDetalle);
            avisoTitulo = null;
            temblorPendiente = false;
        }

        // Muestra el banner translúcido en el centro del tablero (sobre la zona de los botones y las cartas).
        private void MostrarAviso(string titulo, string detalle)
        {
            if (!Visible) return;
            CerrarAviso();
            aviso = new AvisoEvento(titulo, detalle);
            int centroX = XCentro + AnchoCentro / 2;
            aviso.Location = PointToScreen(new Point(centroX - aviso.Width / 2, YCampo + 110));
            aviso.Show(this);
        }

        private void CerrarAviso()
        {
            if (aviso != null && !aviso.IsDisposed) aviso.Close();
            aviso = null;
        }

        // ---------- Textos flotantes de daño y curación ----------

        // Compara la vida del personaje antes y después de la jugada y hace subir el número sobre su carta:
        // rojo si perdió vida, verde si ganó. El daño del Terremoto sale aparte, en naranja y un poco después.
        private void MostrarCambioDeVida(VistaCarta carta, Personaje antes, int hpAntes, Personaje ahora, int danoTerremoto)
        {
            int cambio = CambioDeVida(antes, hpAntes, ahora, danoTerremoto);
            Point centro = new Point(carta.Left + carta.Width / 2, carta.Top + 110);

            if (cambio < 0)
            {
                int dano = -cambio;
                CrearTextoFlotante(carta, "-" + dano, Color.FromArgb(255, 110, 80), Color.FromArgb(34, 10, 10), 0);

                // Golpe: la carta se tiñe de rojo, vibra y suelta chispas (más si el golpe fue grande).
                carta.Destellar(Color.Red);
                carta.Sacudir();
                efectos.Chispas(centro, Color.FromArgb(255, 150, 60), 10 + Math.Min(20, dano / 2));

                if (dano * 100 >= antes.HpMax * GolpeFuertePct)
                    IniciarTemblor(5, 6);   // un golpe muy fuerte también mueve un poco la ventana
            }
            else if (cambio > 0)
            {
                CrearTextoFlotante(carta, "+" + cambio, Color.FromArgb(130, 255, 150), Color.FromArgb(10, 36, 18), 0);
                carta.Destellar(Color.LimeGreen);
                efectos.Brillos(centro, Color.FromArgb(110, 255, 140), 16);
            }

            if (antes != ahora)
            {
                if (!antes.EstaVivo)
                {
                    // Cayó el personaje: estalla en partículas del color de su elemento.
                    efectos.Estallido(centro, Estilo.ColorDe(antes.Elemento), 50);
                    CrearTextoFlotante(carta, Idioma.Texto("¡DERROTADO!", "DEFEATED!"), Color.FromArgb(255, 90, 90), Color.FromArgb(34, 10, 10), 6, fuenteAviso, 150, 70);
                }
                else
                {
                    // Relevo: un destello celeste y brillos de entrada.
                    carta.Destellar(Color.DeepSkyBlue);
                    efectos.Brillos(centro, Color.FromArgb(120, 200, 255), 18);
                }
            }

            if (danoTerremoto > 0)
                CrearTextoFlotante(carta, "-" + danoTerremoto, Color.FromArgb(255, 170, 40), Color.FromArgb(38, 24, 6), EsperaTerremoto);
        }

        // Cuánta vida cambió (negativo = daño, positivo = curación) sin contar el Terremoto, que se muestra aparte.
        // Si sigue siendo el mismo personaje, su HP ya incluye el golpe del Terremoto: se separa.
        private int CambioDeVida(Personaje antes, int hpAntes, Personaje ahora, int danoTerremoto)
        {
            int cambio = antes.Hp - hpAntes;
            if (antes == ahora) cambio += danoTerremoto;
            return cambio;
        }

        // Si la jugada fue un ataque que hizo daño, muestra Idioma.Texto("¡SUPEREFICAZ!", "SUPER EFFECTIVE!") o "POCO EFICAZ" sobre el que lo recibió.
        private void MostrarEficacia(AccionCombate accion, Personaje atacante, Personaje defensor, int cambioDelDefensor, VistaCarta cartaDefensor)
        {
            if (cambioDelDefensor >= 0) return;

            bool esAtaque = (accion.Tipo == TipoAccion.Atacar);
            if (accion.Tipo == TipoAccion.Habilidad) esAtaque = atacante.ObtenerHabilidad(accion.NumeroHabilidad).EsOfensiva;
            if (!esAtaque) return;

            double multiplicador = atacante.ObtenerMultiplicador(defensor.Elemento);
            if (multiplicador > Personaje.MultiplicadorNeutro)
                CrearTextoFlotante(cartaDefensor, Idioma.Texto("¡SUPEREFICAZ!", "SUPER EFFECTIVE!"), Color.Gold, Color.FromArgb(40, 30, 6), 4, fuenteAviso, 150, 70);
            else if (multiplicador < Personaje.MultiplicadorNeutro)
                CrearTextoFlotante(cartaDefensor, Idioma.Texto("poco eficaz", "not very effective"), Color.FromArgb(170, 190, 220), Color.FromArgb(16, 20, 32), 4, fuenteAviso, 150, 70);
        }

        private void CrearTextoFlotante(VistaCarta carta, string texto, Color colorTexto, Color colorFondo, int espera)
        {
            CrearTextoFlotante(carta, texto, colorTexto, colorFondo, espera, fuenteFlotante, 88, 130);
        }

        // Crea una etiqueta que sube sobre la carta: "y" es la altura (desde la parte de arriba de la carta) donde empieza.
        private void CrearTextoFlotante(VistaCarta carta, string texto, Color colorTexto, Color colorFondo, int espera, Font fuente, int ancho, int y)
        {
            int alto = 28;
            if (fuente == fuenteFlotante) alto = 40;

            Label etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.Font = fuente;
            etiqueta.ForeColor = colorTexto;
            etiqueta.BackColor = colorFondo;   // fondo sólido: un Label transparente dejaría ver el fondo del formulario y no la carta
            etiqueta.TextAlign = ContentAlignment.MiddleCenter;
            etiqueta.SetBounds(carta.Left + (carta.Width - ancho) / 2, carta.Top + y, ancho, alto);
            etiqueta.Visible = false;
            Controls.Add(etiqueta);
            Estilo.Redondear(etiqueta, alto / 2);

            TextoFlotante flotante = new TextoFlotante();
            flotante.Etiqueta = etiqueta;
            flotante.YInicial = etiqueta.Top;
            flotante.Espera = espera;
            flotantes.Add(flotante);
            timerFlotantes.Start();
        }

        // Cada 30 ms: hace subir los números (rápido al principio y más lento al final) y retira los que ya terminaron.
        private void TimerFlotantes_Tick(object sender, EventArgs e)
        {
            for (int i = flotantes.Count - 1; i >= 0; i--)
            {
                TextoFlotante f = flotantes[i];
                if (f.Espera > 0)
                {
                    f.Espera--;
                    continue;
                }

                if (!f.Etiqueta.Visible)
                {
                    f.Etiqueta.Visible = true;
                    f.Etiqueta.BringToFront();
                }

                f.Edad++;
                if (f.Edad >= VidaTextoFlotante)
                {
                    Controls.Remove(f.Etiqueta);
                    f.Etiqueta.Dispose();
                    flotantes.RemoveAt(i);
                    continue;
                }

                double avance = (double)f.Edad / VidaTextoFlotante;
                f.Etiqueta.Top = f.YInicial - (int)(AlturaTextoFlotante * (1 - (1 - avance) * (1 - avance)));
            }

            if (flotantes.Count == 0) timerFlotantes.Stop();
        }

        // ---------- Efecto de temblor ----------

        // Terremoto: temblor fuerte y largo.
        private void IniciarTemblor()
        {
            IniciarTemblor(8, 12);
        }

        // La ventana se mueve de lado a lado "amplitud" píxeles durante "pasos" cambios de 40 ms.
        private void IniciarTemblor(int amplitud, int pasos)
        {
            if (timerTemblor.Enabled) return;
            posicionOriginal = Location;
            amplitudTemblor = amplitud;
            pasosTotalesTemblor = pasos;
            pasosTemblor = 0;
            timerTemblor.Start();
        }

        private void TimerTemblor_Tick(object sender, EventArgs e)
        {
            pasosTemblor++;
            if (pasosTemblor > pasosTotalesTemblor)
            {
                timerTemblor.Stop();
                Location = posicionOriginal;
                return;
            }
            int desplazamiento = -amplitudTemblor;
            if (pasosTemblor % 2 == 0) desplazamiento = amplitudTemblor;
            Location = new Point(posicionOriginal.X + desplazamiento, posicionOriginal.Y);
        }

        // ---------- Registro de combate (la PILA en un ListBox) ----------

        // Copia la pila del historial al ListBox. Al recorrer un Stack sale primero lo más reciente.
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
            if (texto.Contains("TERREMOTO") || texto.Contains("EARTHQUAKE") || texto.StartsWith("   ")) return Color.Orange;
            if (texto.Contains("derrotado") || texto.Contains("Has sido") || texto.Contains("defeated")) return Color.IndianRed;
            if (texto.Contains("Victoria") || texto.Contains("MUERTE") || texto.Contains("Victory") || texto.Contains("SUDDEN")) return Color.Gold;
            if (texto.Contains("cura") || texto.Contains("heals")) return Color.LightGreen;
            return Color.White;
        }
    }
}
