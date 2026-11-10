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
        private const int AltoVentana = 600;
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

        // Los controles fijos se declaran como campos para que el diseñador de SharpDevelop los reconozca.
        private Panel barraSuperior;
        private Label lblTitulo;
        private Label lblTriangulo;
        private Button btnReglas;
        private Button btnRendirse;
        private Label lblEquipoJugador;
        private Label lblEquipoRival;
        private Label lblBancaJugador;
        private Label lblBancaRival;
        private FlowLayoutPanel flujoBancaJugador;
        private FlowLayoutPanel flujoBancaRival;
        private Label lblTurno;
        private Label lblInstruccion;
        private Button btnAtacar;
        private Button btnHabAtaque;
        private Button btnHabUtilidad;
        private Button btnRelevo;
        private Button btnPasar;
        private Label lblTerremoto;
        private Label lblVs;
        private Panel panelRegistro;
        private Label lblRegistro;
        private ListBox lstHistorial;
        private Label lblUltima;
        private Label lblUltimaJugada;
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

        // ---------- Construcción de la interfaz ----------

        // Los controles fijos están en InitializeComponent; aquí solo se pone lo que depende del idioma
        // o de los datos del combate (las cartas, el fondo del escenario).
        private void ConstruirInterfaz()
        {
            InitializeComponent();

            Text = Idioma.Texto("Arena Elemental - Combate", "Arena Elemental - Battle");
            lblTriangulo.Text = Idioma.Texto("Triángulo: ", "Triangle: ") + Estilo.Triangulo;
            Estilo.EstilizarBoton(btnReglas, Idioma.Texto("Reglas", "Rules"), Color.FromArgb(60, 70, 110));
            btnReglas.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            Estilo.EstilizarBoton(btnRendirse, Idioma.Texto("Rendirse", "Give up"), Color.FromArgb(130, 30, 40));
            btnRendirse.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            // Los botones de acción: color propio, letra de 9 y el texto lo pone Refrescar().
            Estilo.EstilizarBoton(btnAtacar, string.Empty, Color.Crimson);
            btnAtacar.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            Estilo.EstilizarBoton(btnHabAtaque, string.Empty, Color.FromArgb(88, 40, 140));
            btnHabAtaque.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            Estilo.EstilizarBoton(btnHabUtilidad, string.Empty, Color.FromArgb(16, 122, 104));
            btnHabUtilidad.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            Estilo.EstilizarBoton(btnRelevo, string.Empty, Color.FromArgb(24, 110, 160));
            btnRelevo.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            Estilo.EstilizarBoton(btnPasar, string.Empty, Color.FromArgb(70, 78, 100));
            btnPasar.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            menuAliados = new ContextMenuStrip();

            lblRegistro.Text = Idioma.Texto("REGISTRO (pila)", "LOG (stack)");
            lblUltima.Text = Idioma.Texto("Última jugada (Peek):", "Last move (Peek):");
            Estilo.Redondear(panelRegistro, 12);
            Estilo.Redondear(flujoBancaJugador, 12);
            Estilo.Redondear(flujoBancaRival, 12);

            CargarFondoDelEscenario();
            ConstruirCartas();
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

        // Las cartas son controles propios que se llenan con los datos de los personajes, así que se crean por código.
        // Las bancas (FlowLayoutPanel) ya están en InitializeComponent: aquí solo se les agregan las cartas pequeñas.
        private void ConstruirCartas()
        {
            cartaJugador = new VistaCarta(TamanoCarta.Mediana);
            cartaJugador.Location = new Point(XCartaJugador, YCampo);
            Controls.Add(cartaJugador);

            cartaRival = new VistaCarta(TamanoCarta.Mediana);
            cartaRival.Location = new Point(XCartaRival, YCampo);
            Controls.Add(cartaRival);

            // El orden de las cartas de la banca es el de la cola; la dirección del flujo de cada panel decide
            // de qué lado empieza la fila: la carta que entra primero queda siempre pegada a la carta del campo.
            LlenarBanca(flujoBancaJugador, bancaJugador);
            LlenarBanca(flujoBancaRival, bancaRival);
        }

        private void LlenarBanca(FlowLayoutPanel flujo, VistaCarta[] cartas)
        {
            for (int i = 0; i < cartas.Length; i++)
            {
                cartas[i] = new VistaCarta(TamanoCarta.Pequena);
                cartas[i].Margin = new Padding(2);
                flujo.Controls.Add(cartas[i]);
            }
        }

        // Código del diseñador: aquí se crean y se colocan los controles fijos de la ventana.
        private void InitializeComponent()
        {
            this.barraSuperior = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblTriangulo = new System.Windows.Forms.Label();
            this.btnReglas = new System.Windows.Forms.Button();
            this.btnRendirse = new System.Windows.Forms.Button();
            this.lblEquipoJugador = new System.Windows.Forms.Label();
            this.lblEquipoRival = new System.Windows.Forms.Label();
            this.lblBancaJugador = new System.Windows.Forms.Label();
            this.lblBancaRival = new System.Windows.Forms.Label();
            this.flujoBancaJugador = new System.Windows.Forms.FlowLayoutPanel();
            this.flujoBancaRival = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTurno = new System.Windows.Forms.Label();
            this.lblInstruccion = new System.Windows.Forms.Label();
            this.btnAtacar = new System.Windows.Forms.Button();
            this.btnHabAtaque = new System.Windows.Forms.Button();
            this.btnHabUtilidad = new System.Windows.Forms.Button();
            this.btnRelevo = new System.Windows.Forms.Button();
            this.btnPasar = new System.Windows.Forms.Button();
            this.lblTerremoto = new System.Windows.Forms.Label();
            this.lblVs = new System.Windows.Forms.Label();
            this.panelRegistro = new System.Windows.Forms.Panel();
            this.lblRegistro = new System.Windows.Forms.Label();
            this.lstHistorial = new System.Windows.Forms.ListBox();
            this.lblUltima = new System.Windows.Forms.Label();
            this.lblUltimaJugada = new System.Windows.Forms.Label();
            this.barraSuperior.SuspendLayout();
            this.panelRegistro.SuspendLayout();
            this.SuspendLayout();
            //
            // barraSuperior
            //
            this.barraSuperior.BackColor = System.Drawing.Color.FromArgb(185, 10, 10, 14);
            this.barraSuperior.Controls.Add(this.lblTitulo);
            this.barraSuperior.Controls.Add(this.lblTriangulo);
            this.barraSuperior.Controls.Add(this.btnReglas);
            this.barraSuperior.Controls.Add(this.btnRendirse);
            this.barraSuperior.Location = new System.Drawing.Point(0, 0);
            this.barraSuperior.Name = "barraSuperior";
            this.barraSuperior.Size = new System.Drawing.Size(1120, 36);
            this.barraSuperior.TabIndex = 0;
            //
            // lblTitulo
            //
            this.lblTitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.Gold;
            this.lblTitulo.Location = new System.Drawing.Point(16, 4);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(420, 28);
            this.lblTitulo.TabIndex = 0;
            //
            // lblTriangulo
            //
            this.lblTriangulo.BackColor = System.Drawing.Color.Transparent;
            this.lblTriangulo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTriangulo.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblTriangulo.Location = new System.Drawing.Point(440, 2);
            this.lblTriangulo.Name = "lblTriangulo";
            this.lblTriangulo.Size = new System.Drawing.Size(470, 32);
            this.lblTriangulo.TabIndex = 1;
            this.lblTriangulo.Text = "Triángulo: ";
            this.lblTriangulo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // btnReglas
            //
            this.btnReglas.BackColor = System.Drawing.Color.FromArgb(60, 70, 110);
            this.btnReglas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReglas.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnReglas.ForeColor = System.Drawing.Color.White;
            this.btnReglas.Location = new System.Drawing.Point(924, 5);
            this.btnReglas.Name = "btnReglas";
            this.btnReglas.Size = new System.Drawing.Size(84, 26);
            this.btnReglas.TabIndex = 2;
            this.btnReglas.Text = "Reglas";
            this.btnReglas.UseVisualStyleBackColor = false;
            this.btnReglas.Click += new System.EventHandler(this.BtnReglas_Click);
            //
            // btnRendirse
            //
            this.btnRendirse.BackColor = System.Drawing.Color.FromArgb(130, 30, 40);
            this.btnRendirse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRendirse.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRendirse.ForeColor = System.Drawing.Color.White;
            this.btnRendirse.Location = new System.Drawing.Point(1016, 5);
            this.btnRendirse.Name = "btnRendirse";
            this.btnRendirse.Size = new System.Drawing.Size(90, 26);
            this.btnRendirse.TabIndex = 3;
            this.btnRendirse.Text = "Rendirse";
            this.btnRendirse.UseVisualStyleBackColor = false;
            this.btnRendirse.Click += new System.EventHandler(this.BtnRendirse_Click);
            //
            // lblEquipoJugador
            //
            this.lblEquipoJugador.BackColor = System.Drawing.Color.FromArgb(150, 8, 8, 12);
            this.lblEquipoJugador.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblEquipoJugador.ForeColor = System.Drawing.Color.Gold;
            this.lblEquipoJugador.Location = new System.Drawing.Point(222, 46);
            this.lblEquipoJugador.Name = "lblEquipoJugador";
            this.lblEquipoJugador.Size = new System.Drawing.Size(216, 22);
            this.lblEquipoJugador.TabIndex = 1;
            this.lblEquipoJugador.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblEquipoRival
            //
            this.lblEquipoRival.BackColor = System.Drawing.Color.FromArgb(150, 8, 8, 12);
            this.lblEquipoRival.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblEquipoRival.ForeColor = System.Drawing.Color.Gold;
            this.lblEquipoRival.Location = new System.Drawing.Point(682, 46);
            this.lblEquipoRival.Name = "lblEquipoRival";
            this.lblEquipoRival.Size = new System.Drawing.Size(216, 22);
            this.lblEquipoRival.TabIndex = 2;
            this.lblEquipoRival.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblBancaJugador
            //
            this.lblBancaJugador.BackColor = System.Drawing.Color.FromArgb(150, 8, 8, 12);
            this.lblBancaJugador.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblBancaJugador.ForeColor = System.Drawing.Color.Gold;
            this.lblBancaJugador.Location = new System.Drawing.Point(14, 46);
            this.lblBancaJugador.Name = "lblBancaJugador";
            this.lblBancaJugador.Size = new System.Drawing.Size(198, 22);
            this.lblBancaJugador.TabIndex = 3;
            this.lblBancaJugador.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblBancaRival
            //
            this.lblBancaRival.BackColor = System.Drawing.Color.FromArgb(150, 8, 8, 12);
            this.lblBancaRival.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblBancaRival.ForeColor = System.Drawing.Color.Gold;
            this.lblBancaRival.Location = new System.Drawing.Point(908, 46);
            this.lblBancaRival.Name = "lblBancaRival";
            this.lblBancaRival.Size = new System.Drawing.Size(198, 22);
            this.lblBancaRival.TabIndex = 4;
            this.lblBancaRival.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // flujoBancaJugador
            //
            this.flujoBancaJugador.BackColor = System.Drawing.Color.FromArgb(150, 8, 8, 12);
            this.flujoBancaJugador.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flujoBancaJugador.ForeColor = System.Drawing.Color.White;
            this.flujoBancaJugador.Location = new System.Drawing.Point(14, 74);
            this.flujoBancaJugador.Name = "flujoBancaJugador";
            this.flujoBancaJugador.Padding = new System.Windows.Forms.Padding(3);
            this.flujoBancaJugador.Size = new System.Drawing.Size(198, 300);
            this.flujoBancaJugador.TabIndex = 5;
            //
            // flujoBancaRival
            //
            this.flujoBancaRival.BackColor = System.Drawing.Color.FromArgb(150, 8, 8, 12);
            this.flujoBancaRival.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flujoBancaRival.ForeColor = System.Drawing.Color.White;
            this.flujoBancaRival.Location = new System.Drawing.Point(908, 74);
            this.flujoBancaRival.Name = "flujoBancaRival";
            this.flujoBancaRival.Padding = new System.Windows.Forms.Padding(3);
            this.flujoBancaRival.Size = new System.Drawing.Size(198, 300);
            this.flujoBancaRival.TabIndex = 6;
            //
            // lblTurno
            //
            this.lblTurno.BackColor = System.Drawing.Color.FromArgb(150, 8, 8, 12);
            this.lblTurno.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTurno.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblTurno.Location = new System.Drawing.Point(470, 74);
            this.lblTurno.Name = "lblTurno";
            this.lblTurno.Size = new System.Drawing.Size(180, 40);
            this.lblTurno.TabIndex = 7;
            this.lblTurno.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblInstruccion
            //
            this.lblInstruccion.BackColor = System.Drawing.Color.FromArgb(150, 8, 8, 12);
            this.lblInstruccion.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblInstruccion.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblInstruccion.Location = new System.Drawing.Point(470, 118);
            this.lblInstruccion.Name = "lblInstruccion";
            this.lblInstruccion.Size = new System.Drawing.Size(180, 32);
            this.lblInstruccion.TabIndex = 8;
            this.lblInstruccion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnAtacar
            //
            this.btnAtacar.BackColor = System.Drawing.Color.Crimson;
            this.btnAtacar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAtacar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAtacar.ForeColor = System.Drawing.Color.White;
            this.btnAtacar.Location = new System.Drawing.Point(470, 156);
            this.btnAtacar.Name = "btnAtacar";
            this.btnAtacar.Size = new System.Drawing.Size(180, 40);
            this.btnAtacar.TabIndex = 9;
            this.btnAtacar.UseVisualStyleBackColor = false;
            this.btnAtacar.Click += new System.EventHandler(this.BtnAtacar_Click);
            //
            // btnHabAtaque
            //
            this.btnHabAtaque.BackColor = System.Drawing.Color.FromArgb(88, 40, 140);
            this.btnHabAtaque.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHabAtaque.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnHabAtaque.ForeColor = System.Drawing.Color.White;
            this.btnHabAtaque.Location = new System.Drawing.Point(470, 200);
            this.btnHabAtaque.Name = "btnHabAtaque";
            this.btnHabAtaque.Size = new System.Drawing.Size(180, 40);
            this.btnHabAtaque.TabIndex = 10;
            this.btnHabAtaque.UseVisualStyleBackColor = false;
            this.btnHabAtaque.Click += new System.EventHandler(this.BtnHabAtaque_Click);
            //
            // btnHabUtilidad
            //
            this.btnHabUtilidad.BackColor = System.Drawing.Color.FromArgb(16, 122, 104);
            this.btnHabUtilidad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHabUtilidad.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnHabUtilidad.ForeColor = System.Drawing.Color.White;
            this.btnHabUtilidad.Location = new System.Drawing.Point(470, 244);
            this.btnHabUtilidad.Name = "btnHabUtilidad";
            this.btnHabUtilidad.Size = new System.Drawing.Size(180, 40);
            this.btnHabUtilidad.TabIndex = 11;
            this.btnHabUtilidad.UseVisualStyleBackColor = false;
            this.btnHabUtilidad.Click += new System.EventHandler(this.BtnHabUtilidad_Click);
            //
            // btnRelevo
            //
            this.btnRelevo.BackColor = System.Drawing.Color.FromArgb(24, 110, 160);
            this.btnRelevo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRelevo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRelevo.ForeColor = System.Drawing.Color.White;
            this.btnRelevo.Location = new System.Drawing.Point(470, 288);
            this.btnRelevo.Name = "btnRelevo";
            this.btnRelevo.Size = new System.Drawing.Size(180, 40);
            this.btnRelevo.TabIndex = 12;
            this.btnRelevo.UseVisualStyleBackColor = false;
            this.btnRelevo.Click += new System.EventHandler(this.BtnRelevo_Click);
            //
            // btnPasar
            //
            this.btnPasar.BackColor = System.Drawing.Color.FromArgb(70, 78, 100);
            this.btnPasar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPasar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnPasar.ForeColor = System.Drawing.Color.White;
            this.btnPasar.Location = new System.Drawing.Point(470, 332);
            this.btnPasar.Name = "btnPasar";
            this.btnPasar.Size = new System.Drawing.Size(180, 40);
            this.btnPasar.TabIndex = 13;
            this.btnPasar.UseVisualStyleBackColor = false;
            this.btnPasar.Click += new System.EventHandler(this.BtnPasar_Click);
            //
            // lblTerremoto
            //
            this.lblTerremoto.BackColor = System.Drawing.Color.FromArgb(150, 8, 8, 12);
            this.lblTerremoto.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTerremoto.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblTerremoto.Location = new System.Drawing.Point(470, 378);
            this.lblTerremoto.Name = "lblTerremoto";
            this.lblTerremoto.Size = new System.Drawing.Size(180, 24);
            this.lblTerremoto.TabIndex = 14;
            this.lblTerremoto.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblVs
            //
            this.lblVs.BackColor = System.Drawing.Color.Transparent;
            this.lblVs.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            this.lblVs.ForeColor = System.Drawing.Color.Gold;
            this.lblVs.Location = new System.Drawing.Point(470, 410);
            this.lblVs.Name = "lblVs";
            this.lblVs.Size = new System.Drawing.Size(180, 56);
            this.lblVs.TabIndex = 15;
            this.lblVs.Text = "VS";
            this.lblVs.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // panelRegistro
            //
            this.panelRegistro.BackColor = System.Drawing.Color.FromArgb(150, 8, 8, 12);
            this.panelRegistro.Controls.Add(this.lblRegistro);
            this.panelRegistro.Controls.Add(this.lstHistorial);
            this.panelRegistro.Controls.Add(this.lblUltima);
            this.panelRegistro.Controls.Add(this.lblUltimaJugada);
            this.panelRegistro.Location = new System.Drawing.Point(14, 382);
            this.panelRegistro.Name = "panelRegistro";
            this.panelRegistro.Size = new System.Drawing.Size(198, 208);
            this.panelRegistro.TabIndex = 16;
            //
            // lblRegistro
            //
            this.lblRegistro.BackColor = System.Drawing.Color.Transparent;
            this.lblRegistro.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRegistro.ForeColor = System.Drawing.Color.Gold;
            this.lblRegistro.Location = new System.Drawing.Point(10, 6);
            this.lblRegistro.Name = "lblRegistro";
            this.lblRegistro.Size = new System.Drawing.Size(178, 20);
            this.lblRegistro.TabIndex = 0;
            this.lblRegistro.Text = "REGISTRO (pila)";
            //
            // lstHistorial
            //
            this.lstHistorial.BackColor = System.Drawing.Color.FromArgb(6, 6, 9);
            this.lstHistorial.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstHistorial.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lstHistorial.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.lstHistorial.ForeColor = System.Drawing.Color.White;
            this.lstHistorial.FormattingEnabled = true;
            this.lstHistorial.Location = new System.Drawing.Point(8, 28);
            this.lstHistorial.Name = "lstHistorial";
            this.lstHistorial.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lstHistorial.Size = new System.Drawing.Size(182, 112);
            this.lstHistorial.TabIndex = 1;
            this.lstHistorial.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.Historial_DrawItem);
            this.lstHistorial.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.Historial_MeasureItem);
            //
            // lblUltima
            //
            this.lblUltima.BackColor = System.Drawing.Color.Transparent;
            this.lblUltima.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblUltima.ForeColor = System.Drawing.Color.Gold;
            this.lblUltima.Location = new System.Drawing.Point(10, 146);
            this.lblUltima.Name = "lblUltima";
            this.lblUltima.Size = new System.Drawing.Size(178, 18);
            this.lblUltima.TabIndex = 2;
            this.lblUltima.Text = "Última jugada (Peek):";
            //
            // lblUltimaJugada
            //
            this.lblUltimaJugada.BackColor = System.Drawing.Color.Transparent;
            this.lblUltimaJugada.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblUltimaJugada.ForeColor = System.Drawing.Color.White;
            this.lblUltimaJugada.Location = new System.Drawing.Point(10, 164);
            this.lblUltimaJugada.Name = "lblUltimaJugada";
            this.lblUltimaJugada.Size = new System.Drawing.Size(178, 38);
            this.lblUltimaJugada.TabIndex = 3;
            //
            // FrmCombate
            //
            this.BackColor = System.Drawing.Color.FromArgb(12, 18, 28);
            this.ClientSize = new System.Drawing.Size(1120, 600);
            this.Controls.Add(this.barraSuperior);
            this.Controls.Add(this.lblEquipoJugador);
            this.Controls.Add(this.lblEquipoRival);
            this.Controls.Add(this.lblBancaJugador);
            this.Controls.Add(this.lblBancaRival);
            this.Controls.Add(this.flujoBancaJugador);
            this.Controls.Add(this.flujoBancaRival);
            this.Controls.Add(this.lblTurno);
            this.Controls.Add(this.lblInstruccion);
            this.Controls.Add(this.btnAtacar);
            this.Controls.Add(this.btnHabAtaque);
            this.Controls.Add(this.btnHabUtilidad);
            this.Controls.Add(this.btnRelevo);
            this.Controls.Add(this.btnPasar);
            this.Controls.Add(this.lblTerremoto);
            this.Controls.Add(this.lblVs);
            this.Controls.Add(this.panelRegistro);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmCombate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Arena Elemental - Combate";
            this.barraSuperior.ResumeLayout(false);
            this.panelRegistro.ResumeLayout(false);
            this.ResumeLayout(false);
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
