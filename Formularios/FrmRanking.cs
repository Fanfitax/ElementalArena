using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Datos;

namespace ArenaElemental.Formularios
{
    /// <summary>
    /// El ranking de todos los jugadores: un podio con los tres primeros y la tabla completa debajo.
    /// El jugador actual aparece resaltado en la tabla. Los puntos son: victoria 3, derrota 1, abandono 0.
    /// </summary>
    public class FrmRanking : FormularioModerno
    {
        private static Color Oro = Color.FromArgb(255, 205, 60);
        private static Color Plata = Color.FromArgb(205, 212, 225);
        private static Color Bronce = Color.FromArgb(214, 140, 80);

        private string nombreJugador;
        private Font fuenteNegrita = new Font("Segoe UI", 10f, FontStyle.Bold);
        private DataGridView tabla;

        public FrmRanking(string nombreJugador)
        {
            this.nombreJugador = nombreJugador;
            Estilo.AplicarFormulario(this, "Arena Elemental - Ranking", 924, 600);

            List<PuestoRanking> ranking = RegistroPartidas.ObtenerRanking();

            Label titulo = Estilo.CrearEtiqueta("RANKING", 24, 12, 600, 38, 22f, true);
            titulo.ForeColor = Color.Gold;
            Controls.Add(titulo);

            Label sub = Estilo.CrearEtiqueta(
                "Puntos: victoria = " + PuestoRanking.PuntosPorVictoria + "  ·  derrota = " + PuestoRanking.PuntosPorDerrota + "  ·  abandono = 0",
                26, 50, 700, 20, 10.5f, false);
            sub.ForeColor = Estilo.TextoSuave;
            Controls.Add(sub);

            // Podio: los tres primeros.
            Color[] colores = new Color[] { Oro, Plata, Bronce };
            for (int i = 0; i < 3; i++)
            {
                PuestoRanking puesto = null;
                if (i < ranking.Count) puesto = ranking[i];
                CrearTarjetaPodio(i, puesto, colores[i]);
            }

            // Tabla completa.
            Panel marco = new Panel();
            marco.SetBounds(24, 190, 876, 330);
            marco.BackColor = Color.FromArgb(205, 10, 12, 18);
            Controls.Add(marco);
            Estilo.Redondear(marco, 12);

            DataGridView dgv = CrearTabla(ranking);
            tabla = dgv;
            dgv.SetBounds(8, 8, 860, 314);
            marco.Controls.Add(dgv);

            if (ranking.Count == 0)
            {
                Label vacio = Estilo.CrearEtiqueta("Todavía no hay partidas. ¡Juega una y aparece en el ranking!", 0, 150, 876, 30, 12f, true);
                vacio.ForeColor = Estilo.TextoSuave;
                vacio.TextAlign = ContentAlignment.MiddleCenter;
                marco.Controls.Add(vacio);
                vacio.BringToFront();
            }

            Button cerrar = new Button();
            cerrar.SetBounds(780, 540, 120, 40);
            Estilo.EstilizarBoton(cerrar, "Cerrar", Estilo.Acento);
            cerrar.Click += Cerrar_Click;
            Controls.Add(cerrar);
        }

        private void Cerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>Al mostrarse la ventana el DataGridView selecciona la primera fila: se quita para que solo se vea el resaltado del jugador.</summary>
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

        /// <summary>Una tarjeta del podio con el puesto, el nombre y los puntos. Si no hay jugador queda vacía.</summary>
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

            Label lugar = Estilo.CrearEtiqueta((posicion + 1) + ".º", 16, 10, 70, 70, 28f, true);
            lugar.ForeColor = color;
            lugar.TextAlign = ContentAlignment.MiddleCenter;
            tarjeta.Controls.Add(lugar);

            string nombre = "—";
            string detalle = "Puesto libre";
            if (puesto != null)
            {
                nombre = puesto.Jugador;
                detalle = puesto.Puntos + " pts  ·  " + puesto.Victorias + " V / " + puesto.Derrotas + " D";
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
            Estilo.AgregarColumna(dgv, "Jugador", "Jugador", 170, null);
            Estilo.AgregarColumna(dgv, "Puntos", "Puntos", 70, null);
            Estilo.AgregarColumna(dgv, "Partidas", "Partidas", 70, null);
            Estilo.AgregarColumna(dgv, "Victorias", "Victorias", 70, null);
            Estilo.AgregarColumna(dgv, "Derrotas", "Derrotas", 70, null);
            Estilo.AgregarColumna(dgv, "Efectividad", "% victorias", 85, null);
            Estilo.AgregarColumna(dgv, "DanoTotal", "Daño total", 85, null);
            dgv.DataSource = ranking;

            // Los colores se deciden al dibujar cada celda (las filas aún no existen al asignar los datos).
            dgv.CellFormatting += Tabla_CellFormatting;
            dgv.ClearSelection();
            return dgv;
        }

        /// <summary>Medalla de color para los tres primeros y fila resaltada para el jugador actual.</summary>
        private void Tabla_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            PuestoRanking puesto = dgv.Rows[e.RowIndex].DataBoundItem as PuestoRanking;
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
