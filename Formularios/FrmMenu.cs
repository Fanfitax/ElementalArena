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
    // Pantalla de inicio: título del juego, nombre del jugador y los botones principales.
    // A la derecha se muestran cuatro héroes, uno por elemento, sobre sus plataformas.
    public class FrmMenu : FormularioModerno
    {
        // Los héroes que se muestran en la portada (uno de cada elemento).
        private static string[] NombresHeroes = new string[] { "Hestia", "Tetis", "Atlas", "Atalanta" };
        private static string[] NombresHeroesIngles = new string[] { "Hestia", "Thetis", "Atlas", "Atalanta" };
        private static Elemento[] ElementosHeroes = new Elemento[] { Elemento.Fuego, Elemento.Agua, Elemento.Tierra, Elemento.Aire };

        private Image[] heroes = new Image[4];
        private Random azar = new Random();
        private TextBox txtJugador;

        // Controles cuyo texto cambia al elegir el idioma.
        private Label lblSubtitulo;
        private Label lblTriangulo;
        private Label lblEtiquetaNombre;
        private Label lblNota;
        private Button btnEspanol;
        private Button btnIngles;
        private Button btnJugar;
        private Button btnLan;
        private Button btnStats;
        private Button btnCodice;
        private Button btnRanking;
        private Button btnSalir;

        public FrmMenu()
        {
            Estilo.AplicarFormulario(this, "Arena Elemental", 1120, 640);
            CrearTitulo();
            CrearCajaDeNombre();
            CrearBotones();
            CrearBotonesDeIdioma();
            ActualizarTextos();
        }

        // ---------- Construcción de la interfaz ----------

        private void CrearTitulo()
        {
            Label arena = Estilo.CrearEtiqueta("ARENA", 46, 56, 400, 100, 60f, true);
            Controls.Add(arena);

            Label elemental = Estilo.CrearEtiqueta("ELEMENTAL", 52, 150, 400, 52, 30f, true);
            elemental.ForeColor = Color.Gold;
            Controls.Add(elemental);

            // Barrita con los cuatro colores de los elementos.
            for (int i = 0; i < 4; i++)
            {
                Panel franja = new Panel();
                franja.SetBounds(56 + i * 90, 212, 84, 6);
                franja.BackColor = Estilo.ColorDe(ElementosHeroes[i]);
                Controls.Add(franja);
            }

            lblSubtitulo = Estilo.CrearEtiqueta("", 54, 230, 380, 24, 11f, false);
            lblSubtitulo.ForeColor = Estilo.TextoSuave;
            Controls.Add(lblSubtitulo);

            lblTriangulo = Estilo.CrearEtiqueta("", 54, 256, 380, 20, 9f, true);
            lblTriangulo.ForeColor = Color.Gold;
            Controls.Add(lblTriangulo);
        }

        private void CrearCajaDeNombre()
        {
            lblEtiquetaNombre = Estilo.CrearEtiqueta("", 54, 318, 300, 20, 9.5f, true);
            lblEtiquetaNombre.ForeColor = Color.Gold;
            Controls.Add(lblEtiquetaNombre);

            txtJugador = new TextBox();
            txtJugador.SetBounds(54, 342, 340, 36);
            txtJugador.Font = new Font("Segoe UI", 14f);
            txtJugador.BackColor = Color.FromArgb(16, 20, 32);
            txtJugador.ForeColor = Color.White;
            txtJugador.BorderStyle = BorderStyle.FixedSingle;
            txtJugador.MaxLength = 30;
            txtJugador.Text = Environment.UserName;
            Controls.Add(txtJugador);
        }

        private void CrearBotones()
        {
            btnJugar = new Button();
            btnJugar.SetBounds(54, 392, 340, 50);
            Estilo.EstilizarBoton(btnJugar, "", Color.FromArgb(46, 160, 96));
            btnJugar.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            btnJugar.Click += BtnJugar_Click;
            Controls.Add(btnJugar);

            btnLan = new Button();
            btnLan.SetBounds(54, 448, 340, 42);
            Estilo.EstilizarBoton(btnLan, "", Color.FromArgb(196, 120, 24));
            btnLan.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnLan.Click += BtnLan_Click;
            Controls.Add(btnLan);

            btnStats = new Button();
            btnStats.SetBounds(54, 496, 166, 40);
            Estilo.EstilizarBoton(btnStats, "", Estilo.Acento);
            btnStats.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnStats.Click += BtnStats_Click;
            Controls.Add(btnStats);

            btnCodice = new Button();
            btnCodice.SetBounds(228, 496, 166, 40);
            Estilo.EstilizarBoton(btnCodice, "", Color.FromArgb(118, 78, 178));
            btnCodice.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnCodice.Click += BtnCodice_Click;
            Controls.Add(btnCodice);

            btnRanking = new Button();
            btnRanking.SetBounds(54, 544, 166, 36);
            Estilo.EstilizarBoton(btnRanking, "", Color.FromArgb(176, 132, 28));
            btnRanking.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnRanking.Click += BtnRanking_Click;
            Controls.Add(btnRanking);

            btnSalir = new Button();
            btnSalir.SetBounds(228, 544, 166, 36);
            Estilo.EstilizarBoton(btnSalir, "", Estilo.PanelClaro);
            btnSalir.Click += BtnSalir_Click;
            Controls.Add(btnSalir);

            lblNota = Estilo.CrearEtiqueta("", 54, 594, 400, 20, 8.5f, false);
            lblNota.ForeColor = Estilo.TextoSuave;
            Controls.Add(lblNota);
            AcceptButton = btnJugar;
        }

        // Los dos botones pequeños de arriba para elegir el idioma.
        private void CrearBotonesDeIdioma()
        {
            btnEspanol = new Button();
            btnEspanol.SetBounds(54, 16, 100, 28);
            btnEspanol.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnEspanol.Click += BtnEspanol_Click;
            Controls.Add(btnEspanol);

            btnIngles = new Button();
            btnIngles.SetBounds(162, 16, 100, 28);
            btnIngles.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnIngles.Click += BtnIngles_Click;
            Controls.Add(btnIngles);
        }

        private void BtnEspanol_Click(object sender, EventArgs e)
        {
            Idioma.Actual = Idioma.Espanol;
            ActualizarTextos();
        }

        private void BtnIngles_Click(object sender, EventArgs e)
        {
            Idioma.Actual = Idioma.Ingles;
            ActualizarTextos();
        }

        // Escribe todos los textos del menú en el idioma elegido y marca el botón del idioma activo.
        private void ActualizarTextos()
        {
            lblSubtitulo.Text = Idioma.Texto("Combate estratégico por turnos y relevos", "Turn-based strategic combat with swaps");
            lblTriangulo.Text = Estilo.Triangulo;
            lblEtiquetaNombre.Text = Idioma.Texto("TU NOMBRE", "YOUR NAME");
            lblNota.Text = Idioma.Texto("Las partidas se guardan en partidas.txt, junto al programa.", "Matches are saved in partidas.txt, next to the program.");

            btnJugar.Text = Idioma.Texto("NUEVA PARTIDA (VS CPU)", "NEW MATCH (VS CPU)");
            btnLan.Text = Idioma.Texto("PARTIDA LAN (2 JUGADORES)", "LAN MATCH (2 PLAYERS)");
            btnStats.Text = Idioma.Texto("ESTADÍSTICAS", "STATISTICS");
            btnCodice.Text = Idioma.Texto("CÓDICE ELEMENTAL", "ELEMENTAL CODEX");
            btnRanking.Text = "RANKING";
            btnSalir.Text = Idioma.Texto("SALIR", "EXIT");

            Estilo.EstilizarBoton(btnEspanol, "ESPAÑOL", Estilo.PanelClaro);
            Estilo.EstilizarBoton(btnIngles, "ENGLISH", Estilo.PanelClaro);
            if (Idioma.Actual == Idioma.Espanol) btnEspanol.BackColor = Color.FromArgb(200, 150, 30);
            else btnIngles.BackColor = Color.FromArgb(200, 150, 30);

            Invalidate();   // para que los nombres de los héroes dibujados en la portada también cambien
        }

        // ---------- Héroes de la portada ----------

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Primero se dibuja la ventana y después se preparan los héroes, para que el menú aparezca al instante.
            Refresh();
            CargarHeroes();
        }

        private void CargarHeroes()
        {
            for (int i = 0; i < heroes.Length; i++)
            {
                try
                {
                    heroes[i] = Ilustraciones.ObtenerPersonajeEncajado(NombresHeroes[i], 220, 400);
                }
                catch (Exception)
                {
                    heroes[i] = null;   // sin imagen solo se ve la plataforma
                }
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int piso = 540;             // altura de los pies de los héroes
            int separacion = 160;       // distancia entre un héroe y el siguiente
            int inicio = 500;           // centro del primero

            for (int i = 0; i < heroes.Length; i++)
            {
                int cx = inicio + i * separacion;
                Color color = Estilo.ColorDe(ElementosHeroes[i]);

                // Plataforma luminosa bajo cada héroe.
                using (GraphicsPath luz = new GraphicsPath())
                {
                    luz.AddEllipse(cx - 85, piso - 18, 170, 36);
                    using (PathGradientBrush brillo = new PathGradientBrush(luz))
                    {
                        brillo.CenterColor = Color.FromArgb(170, color);
                        brillo.SurroundColors = new Color[] { Color.FromArgb(0, color) };
                        g.FillPath(brillo, luz);
                    }
                }
                using (Pen aro = new Pen(Color.FromArgb(170, color), 2.5f))
                {
                    g.DrawEllipse(aro, cx - 60, piso - 11, 120, 22);
                }

                if (heroes[i] != null)
                    g.DrawImage(heroes[i], cx - heroes[i].Width / 2, piso - heroes[i].Height + 8);

                // Nombre y elemento.
                string nombre = Idioma.Texto(NombresHeroes[i], NombresHeroesIngles[i]).ToUpper();
                using (Font fuente = new Font("Segoe UI", 12f, FontStyle.Bold))
                using (Font pequena = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                using (SolidBrush blanco = new SolidBrush(Color.White))
                using (SolidBrush colorElemento = new SolidBrush(color))
                {
                    SizeF tam = g.MeasureString(nombre, fuente);
                    g.DrawString(nombre, fuente, blanco, cx - tam.Width / 2, piso + 22);
                    string rol = Idioma.NombreElemento(ElementosHeroes[i]).ToUpper() + " · " + Estilo.RolDe(ElementosHeroes[i]);
                    SizeF tam2 = g.MeasureString(rol, pequena);
                    g.DrawString(rol, pequena, colorElemento, cx - tam2.Width / 2, piso + 44);
                }
            }
        }

        // ---------- Eventos de los botones ----------

        private void BtnJugar_Click(object sender, EventArgs e)
        {
            NuevaPartida();
        }

        private void BtnLan_Click(object sender, EventArgs e)
        {
            PartidaLan();
        }

        private void BtnStats_Click(object sender, EventArgs e)
        {
            VerEstadisticas();
        }

        private void BtnRanking_Click(object sender, EventArgs e)
        {
            using (FrmRanking frm = new FrmRanking(txtJugador.Text.Trim()))
            {
                frm.ShowDialog(this);
            }
        }

        private void BtnCodice_Click(object sender, EventArgs e)
        {
            VerCodice();
        }

        private void BtnSalir_Click(object sender, EventArgs e)
        {
            Close();
        }

        // ---------- Acciones ----------

        // Devuelve el nombre escrito, o null si está vacío.
        private string LeerNombre()
        {
            string nombre = txtJugador.Text.Trim();
            if (nombre.Length == 0)
            {
                MessageBox.Show(Idioma.Texto("Escribe tu nombre de jugador.", "Type your player name."), Idioma.Texto("Falta el nombre", "Name missing"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtJugador.Focus();
                return null;
            }
            return nombre;
        }

        private void NuevaPartida()
        {
            string nombre = LeerNombre();
            if (nombre == null) return;

            using (FrmSeleccionEquipo seleccion = new FrmSeleccionEquipo(CatalogoBase.Personajes, CatalogoBase.Escenarios, nombre))
            {
                if (seleccion.ShowDialog(this) != DialogResult.OK) return;

                EquipoCombate propio = new EquipoCombate(seleccion.NombreEquipo, seleccion.EquipoElegido, true);
                EquipoCombate rival = new EquipoCombate("Rival CPU", SortearRival(), false);
                Combate combate = new Combate(seleccion.EscenarioElegido, propio, rival);

                using (FrmCombate frm = new FrmCombate(nombre, combate))
                {
                    frm.ShowDialog(this);
                }
            }
        }

        // Partida de dos jugadores en red. Pasos:
        //  1) FrmLan conecta los dos PCs (uno es anfitrión y el otro invitado).
        //  2) Cada jugador arma su equipo (el anfitrión también elige el escenario).
        //  3) Se intercambian los equipos con un mensaje CONFIG y los dos PCs crean el mismo combate:
        //     el equipo del anfitrión es el "Jugador" y el del invitado es el "Rival".
        //  4) FrmCombate se encarga de enviar y recibir las jugadas.
        private void PartidaLan()
        {
            string nombre = LeerNombre();
            if (nombre == null) return;

            RedLan red = null;
            try
            {
                using (FrmLan conexion = new FrmLan())
                {
                    if (conexion.ShowDialog(this) != DialogResult.OK) return;
                    red = conexion.Red;
                }
                bool soyAnfitrion = red.EsAnfitrion;

                // Cada uno arma su equipo. Si cierra la ventana sin terminar, se cancela la partida.
                ConfigPartida propia = ArmarConfigPropia(nombre, soyAnfitrion);
                if (propia == null) return;

                // Se anuncia el equipo propio y se espera el del otro jugador.
                red.EnviarTexto(propia.ATexto());
                ConfigPartida ajena = EsperarConfigDelOtroJugador(red);
                if (ajena == null) return;

                // Los dos PCs arman el mismo combate: primero el equipo del anfitrión, luego el del invitado.
                ConfigPartida delAnfitrion = ajena;
                ConfigPartida delInvitado = propia;
                if (soyAnfitrion)
                {
                    delAnfitrion = propia;
                    delInvitado = ajena;
                }
                if (delAnfitrion.IndiceEscenario < 0)
                {
                    MessageBox.Show(Idioma.Texto("El anfitrión no envió el escenario.", "The host did not send the scenario."), Idioma.Texto("Error de conexión", "Connection error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                EquipoCombate equipoAnfitrion = new EquipoCombate(delAnfitrion.NombreEquipo, delAnfitrion.Equipo, true);
                EquipoCombate equipoInvitado = new EquipoCombate(delInvitado.NombreEquipo, delInvitado.Equipo, false);
                Combate combate = new Combate(CatalogoBase.Escenarios[delAnfitrion.IndiceEscenario], equipoAnfitrion, equipoInvitado, delAnfitrion.Semilla);

                using (FrmCombate frm = new FrmCombate(nombre, combate, red, soyAnfitrion))
                {
                    frm.ShowDialog(this);
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(Idioma.Texto("El equipo recibido no es válido: ", "The received team is not valid: ") + ex.Message, Idioma.Texto("Error de conexión", "Connection error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (red != null) red.Cerrar();
            }
        }

        // El jugador arma su equipo (el invitado no elige escenario). Devuelve null si cerró la ventana sin terminar.
        private ConfigPartida ArmarConfigPropia(string nombre, bool soyAnfitrion)
        {
            using (FrmSeleccionEquipo seleccion = new FrmSeleccionEquipo(CatalogoBase.Personajes, CatalogoBase.Escenarios, nombre))
            {
                seleccion.PermitirElegirEscenario = soyAnfitrion;
                if (seleccion.ShowDialog(this) != DialogResult.OK) return null;

                ConfigPartida propia = new ConfigPartida();
                propia.NombreEquipo = seleccion.NombreEquipo;
                propia.Equipo = seleccion.EquipoElegido;
                if (soyAnfitrion)
                {
                    propia.IndiceEscenario = CatalogoBase.Escenarios.IndexOf(seleccion.EscenarioElegido);
                    propia.Semilla = azar.Next();
                }
                return propia;
            }
        }

        // Muestra una ventana de espera hasta que llega el CONFIG del otro jugador. Devuelve null si se canceló o llegó dañado.
        private ConfigPartida EsperarConfigDelOtroJugador(RedLan red)
        {
            using (FrmEsperaLan espera = new FrmEsperaLan(red, Idioma.Texto("Tu equipo ya fue enviado. Cuando el otro jugador termine de armar el suyo empezará el combate.", "Your team has been sent. When the other player finishes building theirs, the battle will begin.")))
            {
                if (espera.ShowDialog(this) != DialogResult.OK) return null;

                ConfigPartida config = ConfigPartida.DeTexto(espera.Mensaje);
                if (config == null)
                    MessageBox.Show(Idioma.Texto("El otro jugador envió un equipo que no es válido.", "The other player sent an invalid team."), Idioma.Texto("Error de conexión", "Connection error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return config;
            }
        }

        // Elige 3 personajes distintos al azar para el equipo de la CPU.
        private List<Personaje> SortearRival()
        {
            List<Personaje> disponibles = new List<Personaje>(CatalogoBase.Personajes);
            List<Personaje> elegidos = new List<Personaje>();
            for (int i = 0; i < EquipoCombate.TamanoEquipo; i++)
            {
                int k = azar.Next(disponibles.Count);
                elegidos.Add(disponibles[k]);
                disponibles.RemoveAt(k);
            }
            return elegidos;
        }

        // Abre el Códice Elemental: galería de personajes con su historia y su reflexión filosófica.
        private void VerCodice()
        {
            using (FrmCodice frm = new FrmCodice(CatalogoBase.Personajes))
            {
                frm.ShowDialog(this);
            }
        }

        private void VerEstadisticas()
        {
            string nombre = LeerNombre();
            if (nombre == null) return;

            using (FrmEstadisticas frm = new FrmEstadisticas(nombre, RegistroPartidas.DeJugador(nombre)))
            {
                frm.ShowDialog(this);
            }
        }
    }
}
