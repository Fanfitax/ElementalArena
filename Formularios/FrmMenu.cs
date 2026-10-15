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
    /// <summary>
    /// Pantalla de inicio: título del juego, nombre del jugador y los botones principales.
    /// A la derecha se muestran cuatro héroes, uno por elemento, sobre sus plataformas.
    /// </summary>
    public class FrmMenu : FormularioModerno
    {
        // Los héroes que se muestran en la portada (uno de cada elemento).
        private static readonly string[] NombresHeroes = new string[] { "Pyra", "Marina", "Gaia", "Céfiro" };
        private static readonly Elemento[] ElementosHeroes = new Elemento[] { Elemento.Fuego, Elemento.Agua, Elemento.Tierra, Elemento.Aire };

        private Image[] heroes = new Image[4];
        private Random azar = new Random();
        private TextBox txtJugador;

        public FrmMenu()
        {
            Estilo.AplicarFormulario(this, "Arena Elemental", 1120, 640);

            // Título
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

            Label sub = Estilo.CrearEtiqueta("Combate estratégico por turnos y relevos", 54, 230, 380, 24, 11f, false);
            sub.ForeColor = Estilo.TextoSuave;
            Controls.Add(sub);

            Label triangulo = Estilo.CrearEtiqueta(Estilo.Triangulo, 54, 256, 380, 20, 9f, true);
            triangulo.ForeColor = Color.Gold;
            Controls.Add(triangulo);

            // Nombre del jugador
            Label etNombre = Estilo.CrearEtiqueta("TU NOMBRE", 54, 318, 300, 20, 9.5f, true);
            etNombre.ForeColor = Color.Gold;
            Controls.Add(etNombre);

            txtJugador = new TextBox();
            txtJugador.SetBounds(54, 342, 340, 36);
            txtJugador.Font = new Font("Segoe UI", 14f);
            txtJugador.BackColor = Color.FromArgb(16, 20, 32);
            txtJugador.ForeColor = Color.White;
            txtJugador.BorderStyle = BorderStyle.FixedSingle;
            txtJugador.MaxLength = 30;
            txtJugador.Text = Environment.UserName;
            Controls.Add(txtJugador);

            // Botones
            Button btnJugar = new Button();
            btnJugar.SetBounds(54, 392, 340, 50);
            Estilo.EstilizarBoton(btnJugar, "NUEVA PARTIDA (VS CPU)", Color.FromArgb(46, 160, 96));
            btnJugar.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            btnJugar.Click += BtnJugar_Click;
            Controls.Add(btnJugar);

            Button btnLan = new Button();
            btnLan.SetBounds(54, 448, 340, 42);
            Estilo.EstilizarBoton(btnLan, "PARTIDA LAN (2 JUGADORES)", Color.FromArgb(196, 120, 24));
            btnLan.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnLan.Click += BtnLan_Click;
            Controls.Add(btnLan);

            Button btnStats = new Button();
            btnStats.SetBounds(54, 496, 166, 40);
            Estilo.EstilizarBoton(btnStats, "ESTADÍSTICAS", Estilo.Acento);
            btnStats.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnStats.Click += BtnStats_Click;
            Controls.Add(btnStats);

            Button btnCodice = new Button();
            btnCodice.SetBounds(228, 496, 166, 40);
            Estilo.EstilizarBoton(btnCodice, "CÓDICE ELEMENTAL", Color.FromArgb(118, 78, 178));
            btnCodice.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnCodice.Click += BtnCodice_Click;
            Controls.Add(btnCodice);

            Button btnSalir = new Button();
            btnSalir.SetBounds(54, 544, 340, 36);
            Estilo.EstilizarBoton(btnSalir, "SALIR", Estilo.PanelClaro);
            btnSalir.Click += BtnSalir_Click;
            Controls.Add(btnSalir);

            Label nota = Estilo.CrearEtiqueta("Las estadísticas se guardan solo mientras el programa esté abierto.", 54, 594, 400, 20, 8.5f, false);
            nota.ForeColor = Estilo.TextoSuave;
            Controls.Add(nota);
            AcceptButton = btnJugar;
        }

        // ---------- Héroes de la portada ----------

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Se preparan después de mostrar la ventana para que aparezca al instante.
            BeginInvoke(new MethodInvoker(CargarHeroes));
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
                string nombre = NombresHeroes[i].ToUpper();
                using (Font fuente = new Font("Segoe UI", 12f, FontStyle.Bold))
                using (Font pequena = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                using (SolidBrush blanco = new SolidBrush(Color.White))
                using (SolidBrush colorElemento = new SolidBrush(color))
                {
                    SizeF tam = g.MeasureString(nombre, fuente);
                    g.DrawString(nombre, fuente, blanco, cx - tam.Width / 2, piso + 22);
                    string rol = ElementosHeroes[i].ToString().ToUpper() + " · " + Estilo.RolDe(ElementosHeroes[i]);
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

        private void BtnCodice_Click(object sender, EventArgs e)
        {
            VerCodice();
        }

        private void BtnSalir_Click(object sender, EventArgs e)
        {
            Close();
        }

        // ---------- Acciones ----------

        /// <summary>Devuelve el nombre escrito, o null si está vacío.</summary>
        private string LeerNombre()
        {
            string nombre = txtJugador.Text.Trim();
            if (nombre.Length == 0)
            {
                MessageBox.Show("Escribe tu nombre de jugador.", "Falta el nombre", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        /// <summary>
        /// Partida de dos jugadores en red. Pasos:
        ///  1) FrmLan conecta los dos PCs (uno es anfitrión y el otro invitado).
        ///  2) Cada jugador arma su equipo (el anfitrión también elige el escenario).
        ///  3) Se intercambian los equipos con un mensaje CONFIG y los dos PCs crean el mismo combate:
        ///     el equipo del anfitrión es el "Jugador" y el del invitado es el "Rival".
        ///  4) FrmCombate se encarga de enviar y recibir las jugadas.
        /// </summary>
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

                // Cada uno arma su equipo. El invitado no elige escenario.
                ConfigPartida propia;
                using (FrmSeleccionEquipo seleccion = new FrmSeleccionEquipo(CatalogoBase.Personajes, CatalogoBase.Escenarios, nombre))
                {
                    seleccion.PermitirElegirEscenario = soyAnfitrion;
                    if (seleccion.ShowDialog(this) != DialogResult.OK) return;

                    propia = new ConfigPartida();
                    propia.NombreEquipo = seleccion.NombreEquipo;
                    propia.Equipo = seleccion.EquipoElegido;
                    if (soyAnfitrion)
                    {
                        propia.IndiceEscenario = CatalogoBase.Escenarios.IndexOf(seleccion.EscenarioElegido);
                        propia.Semilla = azar.Next();
                    }
                }

                // Se anuncia el equipo propio y se espera el del otro jugador.
                red.EnviarTexto(propia.ATexto());
                ConfigPartida ajena = EsperarConfigDelOtroJugador(red);
                if (ajena == null) return;

                // Los dos PCs arman el mismo combate: primero el equipo del anfitrión, luego el del invitado.
                ConfigPartida delAnfitrion = soyAnfitrion ? propia : ajena;
                ConfigPartida delInvitado = soyAnfitrion ? ajena : propia;
                if (delAnfitrion.IndiceEscenario < 0)
                {
                    MessageBox.Show("El anfitrión no envió el escenario.", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("El equipo recibido no es válido: " + ex.Message, "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (red != null) red.Cerrar();
            }
        }

        /// <summary>Muestra una ventana de espera hasta que llega el CONFIG del otro jugador. Devuelve null si se canceló o llegó dañado.</summary>
        private ConfigPartida EsperarConfigDelOtroJugador(RedLan red)
        {
            using (FrmEsperaLan espera = new FrmEsperaLan(red, "Tu equipo ya fue enviado. Cuando el otro jugador termine de armar el suyo empezará el combate."))
            {
                if (espera.ShowDialog(this) != DialogResult.OK) return null;

                ConfigPartida config = ConfigPartida.DeTexto(espera.Mensaje);
                if (config == null)
                    MessageBox.Show("El otro jugador envió un equipo que no es válido.", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return config;
            }
        }

        /// <summary>Elige 3 personajes distintos al azar para el equipo de la CPU.</summary>
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

        /// <summary>Abre el Códice Elemental: galería de personajes con su historia y su reflexión filosófica.</summary>
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
