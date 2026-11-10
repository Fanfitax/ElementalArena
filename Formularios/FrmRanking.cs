using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Modelos;

namespace ArenaElemental.Formularios
{
    // El ranking de todos los jugadores: un podio con los tres primeros y la tabla completa debajo.
    // El jugador actual aparece resaltado en la tabla. Los puntos son: victoria 3, derrota 1, abandono 0.
    public class FrmRanking : FormularioModerno
    {
        private static Color Oro = Color.FromArgb(255, 205, 60);
        private static Color Plata = Color.FromArgb(205, 212, 225);
        private static Color Bronce = Color.FromArgb(214, 140, 80);

        private string nombreJugador;
        private Font fuenteNegrita = new Font("Segoe UI", 10f, FontStyle.Bold);
        private DataGridView tabla;

        // Los controles se declaran como campos para que el diseñador de SharpDevelop los reconozca.
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Panel panelTabla;
        private Button btnCerrar;

        public FrmRanking(string nombreJugador)
        {
            this.nombreJugador = nombreJugador;

            InitializeComponent();

            // Lo que cambia según el idioma (el diseñador solo muestra la versión en español).
            lblSubtitulo.Text = Idioma.Texto("Puntos: victoria = ", "Points: win = ") + PuestoRanking.PuntosPorVictoria + Idioma.Texto("  ·  derrota = ", "  ·  loss = ") + PuestoRanking.PuntosPorDerrota + Idioma.Texto("  ·  abandono = 0", "  ·  forfeit = 0");
            Estilo.EstilizarBoton(btnCerrar, Idioma.Texto("Cerrar", "Close"), Estilo.Acento);
            Estilo.Redondear(panelTabla, 12);

            List<PuestoRanking> ranking = RegistroPartidas.ObtenerRanking();

            // Podio: los tres primeros.
            Color[] colores = new Color[] { Oro, Plata, Bronce };
            for (int i = 0; i < 3; i++)
            {
                PuestoRanking puesto = null;
                if (i < ranking.Count) puesto = ranking[i];
                CrearTarjetaPodio(i, puesto, colores[i]);
            }

            // Tabla completa (se llena con datos, por eso se crea por código).
            DataGridView dgv = CrearTabla(ranking);
            tabla = dgv;
            dgv.SetBounds(8, 8, 860, 314);
            panelTabla.Controls.Add(dgv);

            if (ranking.Count == 0)
            {
                Label vacio = Estilo.CrearEtiqueta(Idioma.Texto("Todavía no hay partidas. ¡Juega una y aparece en el ranking!", "There are no matches yet. Play one and appear in the ranking!"), 0, 150, 876, 30, 12f, true);
                vacio.ForeColor = Estilo.TextoSuave;
                vacio.TextAlign = ContentAlignment.MiddleCenter;
                panelTabla.Controls.Add(vacio);
                vacio.BringToFront();
            }
        }

        // Código del diseñador: aquí se crean y se colocan los controles de la ventana.
        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.panelTabla = new System.Windows.Forms.Panel();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.Gold;
            this.lblTitulo.Location = new System.Drawing.Point(24, 12);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(600, 38);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "RANKING";
            //
            // lblSubtitulo
            //
            this.lblSubtitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblSubtitulo.Location = new System.Drawing.Point(26, 50);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(700, 20);
            this.lblSubtitulo.TabIndex = 1;
            this.lblSubtitulo.Text = "Puntos: victoria = 3  ·  derrota = 1  ·  abandono = 0";
            //
            // panelTabla
            //
            this.panelTabla.BackColor = System.Drawing.Color.FromArgb(205, 10, 12, 18);
            this.panelTabla.Location = new System.Drawing.Point(24, 190);
            this.panelTabla.Name = "panelTabla";
            this.panelTabla.Size = new System.Drawing.Size(876, 330);
            this.panelTabla.TabIndex = 2;
            //
            // btnCerrar
            //
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(96, 120, 220);
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.Location = new System.Drawing.Point(780, 540);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(120, 40);
            this.btnCerrar.TabIndex = 3;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.Cerrar_Click);
            //
            // FrmRanking
            //
            this.BackColor = System.Drawing.Color.FromArgb(24, 26, 40);
            this.ClientSize = new System.Drawing.Size(924, 600);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblSubtitulo);
            this.Controls.Add(this.panelTabla);
            this.Controls.Add(this.btnCerrar);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmRanking";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Arena Elemental - Ranking";
            this.ResumeLayout(false);
        }
        private void Cerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Al mostrarse la ventana el DataGridView selecciona la primera fila: se quita para que solo se vea el resaltado del jugador.
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            tabla.ClearSelection();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) fuenteNegrita.Dispose();
            base.Dispose(disposing);
        }

        // Una tarjeta del podio con el puesto, el nombre y los puntos. Si no hay jugador queda vacía.
        private void CrearTarjetaPodio(int posicion, PuestoRanking puesto, Color color)
        {
            Panel tarjeta = new Panel();
            tarjeta.SetBounds(24 + posicion * 296, 84, 284, 92);
            tarjeta.BackColor = Color.FromArgb(205, 10, 12, 18);
            Controls.Add(tarjeta);
            Estilo.Redondear(tarjeta, 12);

            Panel franja = new Panel();
            franja.SetBounds(0, 0, 6, 92);
            franja.BackColor = color;
            tarjeta.Controls.Add(franja);

            Label lugar = Estilo.CrearEtiqueta((posicion + 1) + Idioma.Texto(".º", "."), 16, 10, 70, 70, 28f, true);
            lugar.ForeColor = color;
            lugar.TextAlign = ContentAlignment.MiddleCenter;
            tarjeta.Controls.Add(lugar);

            string nombre = "—";
            string detalle = Idioma.Texto("Puesto libre", "Free spot");
            if (puesto != null)
            {
                nombre = puesto.Jugador;
                detalle = puesto.Puntos + " pts  ·  " + puesto.Victorias + Idioma.Texto(" V / ", " W / ") + puesto.Derrotas + Idioma.Texto(" D", " L");
            }

            Label lblNombre = Estilo.CrearEtiqueta(nombre, 92, 16, 184, 30, 15f, true);
            lblNombre.AutoEllipsis = true;
            tarjeta.Controls.Add(lblNombre);

            Label lblDetalle = Estilo.CrearEtiqueta(detalle, 94, 50, 182, 22, 10f, false);
            lblDetalle.ForeColor = Estilo.TextoSuave;
            tarjeta.Controls.Add(lblDetalle);
        }

        private DataGridView CrearTabla(List<PuestoRanking> ranking)
        {
            DataGridView dgv = new DataGridView();
            Estilo.EstilizarTabla(dgv);

            Estilo.AgregarColumna(dgv, "Posicion", "#", 40, null);
            Estilo.AgregarColumna(dgv, "Jugador", Idioma.Texto("Jugador", "Player"), 170, null);
            Estilo.AgregarColumna(dgv, "Puntos", Idioma.Texto("Puntos", "Points"), 70, null);
            Estilo.AgregarColumna(dgv, "Partidas", Idioma.Texto("Partidas", "Matches"), 70, null);
            Estilo.AgregarColumna(dgv, "Victorias", Idioma.Texto("Victorias", "Victories"), 70, null);
            Estilo.AgregarColumna(dgv, "Derrotas", Idioma.Texto("Derrotas", "Defeats"), 70, null);
            Estilo.AgregarColumna(dgv, "Efectividad", Idioma.Texto("% victorias", "% wins"), 85, null);
            Estilo.AgregarColumna(dgv, "DanoTotal", Idioma.Texto("Daño total", "Total damage"), 85, null);
            dgv.DataSource = ranking;

            // Los colores se deciden al dibujar cada celda (las filas aún no existen al asignar los datos).
            dgv.CellFormatting += Tabla_CellFormatting;
            dgv.ClearSelection();
            return dgv;
        }

        // Medalla de color para los tres primeros y fila resaltada para el jugador actual.
        private void Tabla_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            PuestoRanking puesto = (PuestoRanking)dgv.Rows[e.RowIndex].DataBoundItem;
            if (puesto == null) return;

            if (puesto.Jugador.ToLower() == nombreJugador.Trim().ToLower())
            {
                e.CellStyle.BackColor = Color.FromArgb(36, 52, 88);
                e.CellStyle.Font = fuenteNegrita;
            }

            string columna = dgv.Columns[e.ColumnIndex].Name;
            if (columna == "Posicion" || columna == "Puntos")
            {
                e.CellStyle.Font = fuenteNegrita;
                if (columna == "Puntos") e.CellStyle.ForeColor = Color.Gold;
                else if (puesto.Posicion == 1) e.CellStyle.ForeColor = Oro;
                else if (puesto.Posicion == 2) e.CellStyle.ForeColor = Plata;
                else if (puesto.Posicion == 3) e.CellStyle.ForeColor = Bronce;
            }
        }
    }
}
