using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Modelos;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    // Pantalla para armar el equipo de 3 personajes y elegir el escenario.
    //  - Izquierda: la galería con los 12 personajes (una columna por elemento). Clic = ver la carta, doble clic = agregar.
    //  - Centro: la carta grande del personaje seleccionado.
    //  - Derecha: las 3 casillas del equipo. El orden importa: el 1.º sale al campo y los otros esperan en la COLA de relevos.
    public class FrmSeleccionEquipo : FormularioModerno
    {
        private static Color PanelOscuro = Color.FromArgb(205, 10, 12, 18);

        private List<Personaje> catalogo = new List<Personaje>();
        private List<Escenario> escenarios = new List<Escenario>();
        private List<Personaje> equipo = new List<Personaje>();
        private Personaje seleccionado;
        private int indiceEscenario;

        private MiniaturaPersonaje[] miniaturas;
        private VistaCarta vistaPrevia;
        private Label lblDetalle, lblEscenario, lblConsejo, etEscenario;
        private Button btnAgregar, btnCombatir, btnVolver;
        private TextBox txtNombreEquipo;
        private Button[] botonesEscenario;

        // Las 3 casillas del equipo
        private Panel[] casilla = new Panel[EquipoCombate.TamanoEquipo];
        private PictureBox[] casillaImagen = new PictureBox[EquipoCombate.TamanoEquipo];
        private Label[] casillaNombre = new Label[EquipoCombate.TamanoEquipo];
        private Label[] casillaRol = new Label[EquipoCombate.TamanoEquipo];
        private Button[] casillaSubir = new Button[EquipoCombate.TamanoEquipo];
        private Button[] casillaBajar = new Button[EquipoCombate.TamanoEquipo];
        private Button[] casillaQuitar = new Button[EquipoCombate.TamanoEquipo];

        public FrmSeleccionEquipo(List<Personaje> catalogo, List<Escenario> escenarios, string nombreJugador)
        {
            this.catalogo = new List<Personaje>(catalogo);
            this.escenarios = new List<Escenario>(escenarios);

            ConstruirInterfaz(nombreJugador);

            if (this.catalogo.Count > 0) Seleccionar(this.catalogo[0]);
            ElegirEscenario(0);
            RefrescarEquipo();
        }

        public List<Personaje> EquipoElegido { get { return new List<Personaje>(equipo); } }
        public Escenario EscenarioElegido { get { return escenarios[indiceEscenario]; } }
        public string NombreEquipo { get { return txtNombreEquipo.Text.Trim(); } }

        // Si es false se oculta la elección de escenario. Se usa en el modo LAN para el jugador que se une:
        // el escenario lo elige el anfitrión.
        public bool PermitirElegirEscenario
        {
            get { return etEscenario.Visible; }
            set
            {
                etEscenario.Visible = value;
                lblEscenario.Visible = value;
                foreach (Button b in botonesEscenario)
                    b.Visible = value;
                if (!value)
                {
                    lblConsejo.Text = Idioma.Texto("El escenario lo elige el anfitrión de la partida.", "The scenario is chosen by the match host.");
                }
            }
        }

        // ---------- Construcción de la interfaz ----------

        private void ConstruirInterfaz(string nombreJugador)
        {
            Estilo.AplicarFormulario(this, Idioma.Texto("Arena Elemental - Arma tu equipo", "Arena Elemental - Build your team"), 1260, 660);

            ConstruirEncabezado();
            ConstruirGaleria();
            ConstruirVistaPrevia();
            ConstruirPanelEquipo(nombreJugador);
        }

        private void ConstruirEncabezado()
        {
            Label titulo = Estilo.CrearEtiqueta(Idioma.Texto("ARMA TU EQUIPO", "BUILD YOUR TEAM"), 20, 8, 600, 36, 22f, true);
            titulo.ForeColor = Color.Gold;
            Controls.Add(titulo);

            Label sub = Estilo.CrearEtiqueta(Idioma.Texto("Elige 3 personajes. El 1.º sale al campo y los otros esperan en la cola de relevos, en el orden que pongas.", "Pick 3 characters. The 1st goes onto the field and the others wait in the swap queue, in the order you set."),
                                             22, 44, 900, 20, 9.5f, false);
            sub.ForeColor = Estilo.TextoSuave;
            Controls.Add(sub);

            btnVolver = new Button();
            btnVolver.SetBounds(1130, 14, 110, 32);
            Estilo.EstilizarBoton(btnVolver, Idioma.Texto("Volver", "Back"), Estilo.PanelClaro);
            btnVolver.Click += BtnVolver_Click;
            Controls.Add(btnVolver);
        }

        // La galería: 4 columnas (Fuego, Agua, Tierra, Aire) con 3 personajes cada una.
        private void ConstruirGaleria()
        {
            string[] columnas = new string[] { Idioma.Texto("FUEGO", "FIRE"), Idioma.Texto("AGUA", "WATER"), Idioma.Texto("TIERRA", "EARTH"), Idioma.Texto("AIRE", "AIR") };
            Elemento[] elementos = new Elemento[] { Elemento.Fuego, Elemento.Agua, Elemento.Tierra, Elemento.Aire };
            for (int c = 0; c < columnas.Length; c++)
            {
                Label l = Estilo.CrearEtiqueta(columnas[c] + "  ·  " + Estilo.RolDe(elementos[c]), 20 + c * 148, 72, 140, 18, 8.5f, true);
                l.ForeColor = Estilo.ColorDe(elementos[c]);
                Controls.Add(l);
            }

            miniaturas = new MiniaturaPersonaje[catalogo.Count];
            for (int i = 0; i < catalogo.Count; i++)
            {
                int columna = i / 3;
                int fila = i % 3;
                MiniaturaPersonaje m = new MiniaturaPersonaje(catalogo[i]);
                m.Location = new Point(20 + columna * 148, 94 + fila * 162);
                m.Click += Miniatura_Click;
                m.DoubleClick += Miniatura_DoubleClick;
                Controls.Add(m);
                miniaturas[i] = m;
            }

            // Panel con el triángulo y las habilidades del personaje seleccionado.
            Panel panel = new Panel();
            panel.SetBounds(20, 588, 584, 64);
            panel.BackColor = PanelOscuro;
            Controls.Add(panel);
            Estilo.Redondear(panel, 12);

            lblDetalle = Estilo.CrearEtiqueta(string.Empty, 12, 5, 562, 56, 9f, false);
            panel.Controls.Add(lblDetalle);
        }

        private void ConstruirVistaPrevia()
        {
            vistaPrevia = new VistaCarta(TamanoCarta.Seleccion);
            vistaPrevia.Location = new Point(624, 72);
            Controls.Add(vistaPrevia);

            btnAgregar = new Button();
            btnAgregar.SetBounds(624, 562, 232, 42);
            Estilo.EstilizarBoton(btnAgregar, Idioma.Texto("AGREGAR AL EQUIPO", "ADD TO TEAM"), Color.FromArgb(46, 160, 96));
            btnAgregar.Click += BtnAgregar_Click;
            Controls.Add(btnAgregar);

            lblConsejo = Estilo.CrearEtiqueta(Idioma.Texto("Clic: ver la carta\nDoble clic: agregar al equipo", "Click: view the card\nDouble click: add to the team"),
                                              624, 608, 232, 44, 8.5f, false);
            lblConsejo.ForeColor = Estilo.TextoSuave;
            Controls.Add(lblConsejo);
        }

        private void ConstruirPanelEquipo(string nombreJugador)
        {
            Label titulo = Estilo.CrearEtiqueta(Idioma.Texto("TU EQUIPO", "YOUR TEAM"), 876, 72, 364, 18, 9.5f, true);
            titulo.ForeColor = Color.Gold;
            Controls.Add(titulo);

            for (int i = 0; i < EquipoCombate.TamanoEquipo; i++)
                CrearCasilla(i);

            Label etNombre = Estilo.CrearEtiqueta(Idioma.Texto("NOMBRE DEL EQUIPO", "TEAM NAME"), 876, 316, 364, 18, 8.5f, true);
            etNombre.ForeColor = Color.Gold;
            Controls.Add(etNombre);

            txtNombreEquipo = new TextBox();
            txtNombreEquipo.SetBounds(876, 336, 364, 30);
            txtNombreEquipo.Font = new Font("Segoe UI", 11f);
            txtNombreEquipo.BackColor = Color.FromArgb(16, 20, 32);
            txtNombreEquipo.ForeColor = Color.White;
            txtNombreEquipo.BorderStyle = BorderStyle.FixedSingle;
            txtNombreEquipo.MaxLength = 40;
            txtNombreEquipo.Text = Idioma.Texto("Equipo de ", "Team of ") + nombreJugador;
            txtNombreEquipo.TextChanged += TxtNombreEquipo_TextChanged;
            Controls.Add(txtNombreEquipo);

            etEscenario = Estilo.CrearEtiqueta(Idioma.Texto("ESCENARIO", "SCENARIO"), 876, 380, 364, 18, 8.5f, true);
            etEscenario.ForeColor = Color.Gold;
            Controls.Add(etEscenario);

            botonesEscenario = new Button[escenarios.Count];
            for (int i = 0; i < escenarios.Count; i++)
            {
                Button b = new Button();
                b.Tag = i;   // el número del escenario, para saber cuál botón se presionó
                b.SetBounds(876 + (i % 2) * 186, 402 + (i / 2) * 42, 178, 36);
                Color color = Estilo.FondoDe(escenarios[i].ElementoAfin);
                Estilo.EstilizarBoton(b, escenarios[i].Nombre, Color.FromArgb(color.R + 30, color.G + 30, color.B + 30));
                b.Click += Escenario_Click;
                Controls.Add(b);
                botonesEscenario[i] = b;
            }

            lblEscenario = Estilo.CrearEtiqueta(string.Empty, 876, 490, 364, 50, 9f, false);
            lblEscenario.ForeColor = Estilo.TextoSuave;
            Controls.Add(lblEscenario);

            btnCombatir = new Button();
            btnCombatir.SetBounds(876, 560, 364, 62);
            Estilo.EstilizarBoton(btnCombatir, Idioma.Texto("¡A COMBATIR!", "FIGHT!"), Color.FromArgb(46, 160, 96));
            btnCombatir.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
            btnCombatir.Click += BtnCombatir_Click;
            Controls.Add(btnCombatir);
        }

        // Una casilla del equipo: imagen, nombre, su lugar en la cola y los botones para subir, bajar o quitar.
        private void CrearCasilla(int i)
        {
            Panel panel = new Panel();
            panel.SetBounds(876, 96 + i * 74, 364, 66);
            panel.BackColor = PanelOscuro;
            Controls.Add(panel);
            Estilo.Redondear(panel, 12);
            casilla[i] = panel;

            casillaImagen[i] = new PictureBox();
            casillaImagen[i].SetBounds(5, 5, 56, 56);
            panel.Controls.Add(casillaImagen[i]);

            casillaNombre[i] = Estilo.CrearEtiqueta(string.Empty, 68, 8, 190, 24, 12f, true);
            panel.Controls.Add(casillaNombre[i]);

            casillaRol[i] = Estilo.CrearEtiqueta(string.Empty, 68, 36, 200, 20, 8.5f, true);
            panel.Controls.Add(casillaRol[i]);

            // Cada botón guarda en Tag el número de su casilla.
            casillaSubir[i] = CrearBotonCasilla(panel, 270, "▲");
            casillaSubir[i].Tag = i;
            casillaSubir[i].Click += CasillaSubir_Click;
            casillaBajar[i] = CrearBotonCasilla(panel, 302, "▼");
            casillaBajar[i].Tag = i;
            casillaBajar[i].Click += CasillaBajar_Click;
            casillaQuitar[i] = CrearBotonCasilla(panel, 334, "×");
            casillaQuitar[i].Tag = i;
            casillaQuitar[i].Click += CasillaQuitar_Click;
            casillaQuitar[i].BackColor = Color.FromArgb(150, 50, 60);
        }

        private Button CrearBotonCasilla(Panel panel, int x, string texto)
        {
            Button b = new Button();
            b.SetBounds(x, 17, 28, 32);
            Estilo.EstilizarBoton(b, texto, Estilo.PanelClaro);
            b.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            b.Padding = new Padding(0);
            panel.Controls.Add(b);
            return b;
        }

        // ---------- Eventos de los controles ----------

        private void BtnVolver_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnCombatir_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            AgregarSeleccionado();
        }

        private void TxtNombreEquipo_TextChanged(object sender, EventArgs e)
        {
            ActualizarBotonCombatir();
        }

        private void Miniatura_Click(object sender, EventArgs e)
        {
            MiniaturaPersonaje miniatura = (MiniaturaPersonaje)sender;
            Seleccionar(miniatura.Personaje);
        }

        private void Miniatura_DoubleClick(object sender, EventArgs e)
        {
            MiniaturaPersonaje miniatura = (MiniaturaPersonaje)sender;
            Seleccionar(miniatura.Personaje);
            AgregarSeleccionado();
        }

        private void Escenario_Click(object sender, EventArgs e)
        {
            Button boton = (Button)sender;
            ElegirEscenario((int)boton.Tag);
        }

        private void CasillaSubir_Click(object sender, EventArgs e)
        {
            MoverEnEquipo((int)((Button)sender).Tag, -1);
        }

        private void CasillaBajar_Click(object sender, EventArgs e)
        {
            MoverEnEquipo((int)((Button)sender).Tag, 1);
        }

        private void CasillaQuitar_Click(object sender, EventArgs e)
        {
            QuitarDelEquipo((int)((Button)sender).Tag);
        }

        // ---------- Selección y equipo ----------

        // Muestra la carta del personaje y sus habilidades.
        private void Seleccionar(Personaje p)
        {
            seleccionado = p;
            vistaPrevia.ActualizarDatos(p, Estilo.RolDe(p.Elemento), false);
            lblDetalle.Text = Estilo.DescripcionTriangulo(p) + "\n" + Estilo.DescripcionHabilidad(p.HabilidadAtaque)
                              + "\n" + Estilo.DescripcionHabilidad(p.HabilidadUtilidad);

            foreach (MiniaturaPersonaje m in miniaturas)
                m.Seleccionada = (m.Personaje == p);
            ActualizarBotonAgregar();
        }

        private void AgregarSeleccionado()
        {
            if (seleccionado == null || equipo.Contains(seleccionado) || equipo.Count >= EquipoCombate.TamanoEquipo)
                return;
            equipo.Add(seleccionado);
            RefrescarEquipo();
        }

        private void QuitarDelEquipo(int indice)
        {
            if (indice < 0 || indice >= equipo.Count) return;
            equipo.RemoveAt(indice);
            RefrescarEquipo();
        }

        private void MoverEnEquipo(int indice, int delta)
        {
            int destino = indice + delta;
            if (indice < 0 || indice >= equipo.Count || destino < 0 || destino >= equipo.Count) return;
            Personaje temporal = equipo[indice];
            equipo[indice] = equipo[destino];
            equipo[destino] = temporal;
            RefrescarEquipo();
        }

        // Actualiza las 3 casillas, las marcas de la galería y los botones.
        private void RefrescarEquipo()
        {
            for (int i = 0; i < EquipoCombate.TamanoEquipo; i++)
            {
                bool ocupada = i < equipo.Count;
                if (ocupada)
                {
                    Personaje p = equipo[i];
                    casillaImagen[i].BackColor = Estilo.ColorDe(p.Elemento);
                    casillaImagen[i].Image = ObtenerImagenCasilla(p);
                    casillaNombre[i].Text = p.Nombre + "  ·  " + Idioma.NombreElemento(p.Elemento);
                    casillaNombre[i].ForeColor = Color.White;
                    if (i == 0)
                    {
                        casillaRol[i].Text = Idioma.Texto("1.º  ·  INICIA EN EL CAMPO", "1st  ·  STARTS ON THE FIELD");
                        casillaRol[i].ForeColor = Color.Gold;
                    }
                    else
                    {
                        casillaRol[i].Text = (i + 1) + Idioma.Texto(".º  ·  EN LA COLA DE RELEVO", ".  ·  IN THE SWAP QUEUE");
                        casillaRol[i].ForeColor = Estilo.TextoSuave;
                    }
                }
                else
                {
                    casillaImagen[i].BackColor = Color.FromArgb(28, 32, 46);
                    casillaImagen[i].Image = null;
                    casillaNombre[i].Text = Idioma.Texto("— vacío —", "— empty —");
                    casillaNombre[i].ForeColor = Estilo.TextoSuave;
                    if (i == 0) casillaRol[i].Text = Idioma.Texto("Elige quién inicia el combate", "Pick who starts the battle");
                    else casillaRol[i].Text = Idioma.Texto("Elige otro personaje", "Pick another character");
                    casillaRol[i].ForeColor = Estilo.TextoSuave;
                }
                casillaSubir[i].Visible = ocupada;
                casillaBajar[i].Visible = ocupada;
                casillaQuitar[i].Visible = ocupada;
                casillaSubir[i].Enabled = i > 0;
                casillaBajar[i].Enabled = i < equipo.Count - 1;
            }

            foreach (MiniaturaPersonaje m in miniaturas)
                m.PosicionEquipo = equipo.IndexOf(m.Personaje) + 1;

            ActualizarBotonAgregar();
            ActualizarBotonCombatir();
        }

        private Image ObtenerImagenCasilla(Personaje p)
        {
            try
            {
                return Ilustraciones.ObtenerArte(p, 56, 56);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ActualizarBotonAgregar()
        {
            bool yaEsta = seleccionado != null && equipo.Contains(seleccionado);
            bool completo = equipo.Count >= EquipoCombate.TamanoEquipo;
            btnAgregar.Enabled = seleccionado != null && !yaEsta && !completo;
            if (yaEsta) btnAgregar.Text = Idioma.Texto("YA ESTÁ EN TU EQUIPO", "ALREADY IN YOUR TEAM");
            else if (completo) btnAgregar.Text = Idioma.Texto("EQUIPO COMPLETO", "TEAM COMPLETE");
            else btnAgregar.Text = Idioma.Texto("AGREGAR AL EQUIPO", "ADD TO TEAM");
        }

        private void ActualizarBotonCombatir()
        {
            bool listo = equipo.Count == EquipoCombate.TamanoEquipo && NombreEquipo.Length > 0;
            btnCombatir.Enabled = listo;
            if (listo) btnCombatir.BackColor = Color.FromArgb(46, 160, 96);
            else btnCombatir.BackColor = Estilo.PanelClaro;
            if (equipo.Count < EquipoCombate.TamanoEquipo)
                btnCombatir.Text = Idioma.Texto("Elige ", "Pick ") + (EquipoCombate.TamanoEquipo - equipo.Count) + Idioma.Texto(" personaje(s) más", " more character(s)");
            else if (NombreEquipo.Length == 0)
                btnCombatir.Text = Idioma.Texto("Escribe un nombre para el equipo", "Type a name for the team");
            else
                btnCombatir.Text = Idioma.Texto("¡A COMBATIR!", "FIGHT!");
        }

        // ---------- Escenario ----------

        private void ElegirEscenario(int indice)
        {
            if (indice < 0 || indice >= escenarios.Count) return;
            indiceEscenario = indice;

            for (int i = 0; i < botonesEscenario.Length; i++)
            {
                bool elegido = (i == indice);
                if (elegido) botonesEscenario[i].FlatAppearance.BorderSize = 3;
                else botonesEscenario[i].FlatAppearance.BorderSize = 0;
                botonesEscenario[i].FlatAppearance.BorderColor = Color.Gold;
            }

            Escenario s = escenarios[indice];
            lblEscenario.Text = s.Descripcion + "\nBonus: +" + s.BonusAtaquePct + Idioma.Texto("% de ATK a las cartas de ", "% ATK to the cards of ") + Idioma.NombreElemento(s.ElementoAfin) + ".";
        }
    }
}
