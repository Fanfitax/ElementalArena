using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Modelos;

namespace ArenaElemental.Formularios
{
    // Historial de partidas de un jugador, con tarjetas de resumen arriba y la tabla de partidas abajo.
    public class FrmEstadisticas : FormularioModerno
    {
        private Font fuenteResultado = new Font("Segoe UI", 10f, FontStyle.Bold);

        // Los controles se declaran como campos para que el diseñador de SharpDevelop los reconozca.
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Panel panelTabla;
        private Button btnCerrar;

        public FrmEstadisticas(string nombreJugador, List<EstadisticaPartida> partidas)
        {
            InitializeComponent();

            // Lo que cambia según el idioma (el diseñador solo muestra la versión en español).
            Text = Idioma.Texto("Arena Elemental - Estadísticas", "Arena Elemental - Statistics");
            lblTitulo.Text = Idioma.Texto("ESTADÍSTICAS", "STATISTICS");
            lblSubtitulo.Text = Idioma.Texto("Jugador: ", "Player: ") + nombreJugador;
            Estilo.EstilizarBoton(btnCerrar, Idioma.Texto("Cerrar", "Close"), Estilo.Acento);
            Estilo.Redondear(panelTabla, 12);

            ResumenJugador r = new ResumenJugador(partidas);

            // Tarjetas de resumen.
            CrearTarjeta(0, r.Partidas.ToString(), Idioma.Texto("PARTIDAS", "MATCHES"), Color.White);
            CrearTarjeta(1, r.Victorias.ToString(), Idioma.Texto("VICTORIAS", "VICTORIES"), Color.LightGreen);
            CrearTarjeta(2, r.Derrotas.ToString(), Idioma.Texto("DERROTAS", "DEFEATS"), Color.IndianRed);
            CrearTarjeta(3, r.PorcentajeVictorias.ToString("0.#") + "%", Idioma.Texto("% DE VICTORIAS", "% WIN RATE"), Color.Gold);
            CrearTarjeta(4, r.DanoTotal.ToString(), Idioma.Texto("DAÑO TOTAL", "TOTAL DAMAGE"), Color.FromArgb(255, 150, 90));
            CrearTarjeta(5, r.PromedioTurnos.ToString("0.#"), Idioma.Texto("RONDAS POR PARTIDA", "ROUNDS PER MATCH"), Color.FromArgb(120, 190, 255));

            // Tabla de partidas (se llena con datos, por eso se crea por código).
            DataGridView dgv = CrearTabla(partidas);
            dgv.SetBounds(8, 8, 860, 324);
            panelTabla.Controls.Add(dgv);

            if (partidas.Count == 0)
            {
                Label vacio = Estilo.CrearEtiqueta(Idioma.Texto("Todavía no has jugado partidas. ¡Arma tu equipo y combate!", "You have not played any matches yet. Build your team and fight!"), 0, 150, 876, 30, 12f, true);
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
            this.lblTitulo.Text = "ESTADÍSTICAS";
            //
            // lblSubtitulo
            //
            this.lblSubtitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(165, 170, 200);
            this.lblSubtitulo.Location = new System.Drawing.Point(26, 50);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(600, 20);
            this.lblSubtitulo.TabIndex = 1;
            this.lblSubtitulo.Text = "Jugador: ";
            //
            // panelTabla
            //
            this.panelTabla.BackColor = System.Drawing.Color.FromArgb(205, 10, 12, 18);
            this.panelTabla.Location = new System.Drawing.Point(24, 168);
            this.panelTabla.Name = "panelTabla";
            this.panelTabla.Size = new System.Drawing.Size(876, 340);
            this.panelTabla.TabIndex = 2;
            //
            // btnCerrar
            //
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(96, 120, 220);
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.Location = new System.Drawing.Point(780, 524);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(120, 40);
            this.btnCerrar.TabIndex = 3;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.Cerrar_Click);
            //
            // FrmEstadisticas
            //
            this.BackColor = System.Drawing.Color.FromArgb(24, 26, 40);
            this.ClientSize = new System.Drawing.Size(924, 580);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblSubtitulo);
            this.Controls.Add(this.panelTabla);
            this.Controls.Add(this.btnCerrar);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmEstadisticas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Arena Elemental - Estadísticas";
            this.ResumeLayout(false);
        }
        private void Cerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Una tarjeta redondeada con un número grande y su nombre debajo.
        private void CrearTarjeta(int posicion, string valor, string texto, Color colorValor)
        {
            Panel tarjeta = new Panel();
            tarjeta.SetBounds(24 + posicion * 146, 84, 138, 72);
            tarjeta.BackColor = Color.FromArgb(205, 10, 12, 18);
            Controls.Add(tarjeta);
            Estilo.Redondear(tarjeta, 12);

            Label numero = Estilo.CrearEtiqueta(valor, 0, 8, 138, 34, 20f, true);
            numero.ForeColor = colorValor;
            numero.TextAlign = ContentAlignment.MiddleCenter;
            tarjeta.Controls.Add(numero);

            Label nombre = Estilo.CrearEtiqueta(texto, 0, 44, 138, 18, 7.5f, true);
            nombre.ForeColor = Estilo.TextoSuave;
            nombre.TextAlign = ContentAlignment.MiddleCenter;
            tarjeta.Controls.Add(nombre);
        }

        private DataGridView CrearTabla(List<EstadisticaPartida> partidas)
        {
            DataGridView dgv = new DataGridView();
            Estilo.EstilizarTabla(dgv);

            Estilo.AgregarColumna(dgv, "Fecha", Idioma.Texto("Fecha", "Date"), 150, "dd/MM/yyyy  HH:mm");
            Estilo.AgregarColumna(dgv, "Equipo", Idioma.Texto("Equipo", "Team"), 170, null);
            Estilo.AgregarColumna(dgv, "Escenario", Idioma.Texto("Escenario", "Scenario"), 150, null);
            Estilo.AgregarColumna(dgv, "Resultado", Idioma.Texto("Resultado", "Result"), 90, null);
            Estilo.AgregarColumna(dgv, "Turnos", Idioma.Texto("Rondas", "Rounds"), 70, null);
            Estilo.AgregarColumna(dgv, "DanoTotal", Idioma.Texto("Daño causado", "Damage dealt"), 100, null);
            dgv.DataSource = new List<EstadisticaPartida>(partidas);

            // El color del resultado se decide al dibujar cada celda (las filas aún no existen al asignar los datos).
            dgv.CellFormatting += Tabla_CellFormatting;
            dgv.ClearSelection();
            return dgv;
        }

        private void Tabla_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            if (e.ColumnIndex < 0 || dgv.Columns[e.ColumnIndex].Name != "Resultado") return;

            string resultado = Convert.ToString(e.Value);
            if (resultado == "Victoria") e.CellStyle.ForeColor = Color.LightGreen;
            else if (resultado == "Derrota") e.CellStyle.ForeColor = Color.IndianRed;
            else e.CellStyle.ForeColor = Estilo.TextoSuave;
            e.CellStyle.Font = fuenteResultado;

            // El resultado se guarda en español; aquí solo se traduce para mostrarlo.
            e.Value = Idioma.NombreResultado(resultado);
            e.FormattingApplied = true;
        }
    }
}
