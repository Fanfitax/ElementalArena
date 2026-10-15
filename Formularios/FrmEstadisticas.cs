using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Datos;

namespace ArenaElemental.Formularios
{
    /// <summary>Historial de partidas y totales de un jugador.</summary>
    public class FrmEstadisticas : Form
    {
        public FrmEstadisticas(string nombreJugador, IList<EstadisticaPartida> partidas)
        {
            Estilo.AplicarFormulario(this, "Arena Elemental - Estadísticas", 820, 520);

            ResumenJugador r = new ResumenJugador(partidas);

            Controls.Add(Estilo.CrearEtiqueta("Estadísticas de " + nombreJugador, 16, 12, 780, 28, 14f, true));
            Label resumen = Estilo.CrearEtiqueta(
                "Partidas: " + r.Partidas + "     Victorias: " + r.Victorias + "     Derrotas: " + r.Derrotas
                + "     % victorias: " + r.PorcentajeVictorias.ToString("0.#") + "%"
                + "\nDaño total causado: " + r.DanoTotal + "     Promedio de rondas por partida: " + r.PromedioTurnos.ToString("0.#"),
                16, 46, 780, 44, 10.5f, false);
            resumen.ForeColor = Color.Gold;
            Controls.Add(resumen);

            DataGridView dgv = new DataGridView();
            dgv.SetBounds(16, 100, 788, 360);
            dgv.AutoGenerateColumns = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.BackgroundColor = Estilo.Panel;
            dgv.BorderStyle = BorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Estilo.PanelClaro;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Estilo.Texto;
            dgv.DefaultCellStyle.BackColor = Estilo.Panel;
            dgv.DefaultCellStyle.ForeColor = Estilo.Texto;
            dgv.DefaultCellStyle.SelectionBackColor = Estilo.Acento;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            AgregarColumna(dgv, "Fecha", "Fecha", 140, "g");
            AgregarColumna(dgv, "Equipo", "Equipo", 170, null);
            AgregarColumna(dgv, "Escenario", "Escenario", 150, null);
            AgregarColumna(dgv, "Resultado", "Resultado", 90, null);
            AgregarColumna(dgv, "Turnos", "Rondas", 70, null);
            AgregarColumna(dgv, "DanoTotal", "Daño causado", 100, null);
            dgv.DataSource = new List<EstadisticaPartida>(partidas);

            foreach (DataGridViewRow fila in dgv.Rows)
            {
                string res = Convert.ToString(fila.Cells["Resultado"].Value);
                if (res == "Victoria") fila.DefaultCellStyle.ForeColor = Color.LightGreen;
                else if (res == "Derrota") fila.DefaultCellStyle.ForeColor = Color.IndianRed;
                else fila.DefaultCellStyle.ForeColor = Estilo.TextoSuave;
            }
            Controls.Add(dgv);

            Button cerrar = Estilo.CrearBoton("Cerrar", 704, 470, 100, 34);
            cerrar.Click += delegate { Close(); };
            Controls.Add(cerrar);
        }

        private static void AgregarColumna(DataGridView dgv, string propiedad, string titulo, int ancho, string formato)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.Name = propiedad;
            col.DataPropertyName = propiedad;
            col.HeaderText = titulo;
            col.Width = ancho;
            if (formato != null) col.DefaultCellStyle.Format = formato;
            dgv.Columns.Add(col);
        }
    }
}
