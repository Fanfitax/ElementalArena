using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Datos;

namespace ArenaElemental.Formularios
{
    /// <summary>Historial de partidas de un jugador, con tarjetas de resumen arriba y la tabla de partidas abajo.</summary>
    public class FrmEstadisticas : FormularioModerno
    {
        private Font fuenteResultado = new Font("Segoe UI", 10f, FontStyle.Bold);

        public FrmEstadisticas(string nombreJugador, List<EstadisticaPartida> partidas)
        {
            Estilo.AplicarFormulario(this, "Arena Elemental - Estadísticas", 924, 580);

            ResumenJugador r = new ResumenJugador(partidas);

            Label titulo = Estilo.CrearEtiqueta("ESTADÍSTICAS", 24, 12, 600, 38, 22f, true);
            titulo.ForeColor = Color.Gold;
            Controls.Add(titulo);

            Label sub = Estilo.CrearEtiqueta("Jugador: " + nombreJugador, 26, 50, 600, 20, 10.5f, false);
            sub.ForeColor = Estilo.TextoSuave;
            Controls.Add(sub);

            // Tarjetas de resumen.
            CrearTarjeta(0, r.Partidas.ToString(), "PARTIDAS", Color.White);
            CrearTarjeta(1, r.Victorias.ToString(), "VICTORIAS", Color.LightGreen);
            CrearTarjeta(2, r.Derrotas.ToString(), "DERROTAS", Color.IndianRed);
            CrearTarjeta(3, r.PorcentajeVictorias.ToString("0.#") + "%", "% DE VICTORIAS", Color.Gold);
            CrearTarjeta(4, r.DanoTotal.ToString(), "DAÑO TOTAL", Color.FromArgb(255, 150, 90));
            CrearTarjeta(5, r.PromedioTurnos.ToString("0.#"), "RONDAS POR PARTIDA", Color.FromArgb(120, 190, 255));

            // Tabla de partidas.
            Panel marco = new Panel();
            marco.SetBounds(24, 168, 876, 340);
            marco.BackColor = Color.FromArgb(205, 10, 12, 18);
            Controls.Add(marco);
            Estilo.Redondear(marco, 12);

            DataGridView dgv = CrearTabla(partidas);
            dgv.SetBounds(8, 8, 860, 324);
            marco.Controls.Add(dgv);

            if (partidas.Count == 0)
            {
                Label vacio = Estilo.CrearEtiqueta("Todavía no has jugado partidas. ¡Arma tu equipo y combate!", 0, 150, 876, 30, 12f, true);
                vacio.ForeColor = Estilo.TextoSuave;
                vacio.TextAlign = ContentAlignment.MiddleCenter;
                marco.Controls.Add(vacio);
                vacio.BringToFront();
            }

            Button cerrar = new Button();
            cerrar.SetBounds(780, 524, 120, 40);
            Estilo.EstilizarBoton(cerrar, "Cerrar", Estilo.Acento);
            cerrar.Click += Cerrar_Click;
            Controls.Add(cerrar);
        }

        private void Cerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>Una tarjeta redondeada con un número grande y su nombre debajo.</summary>
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

            Estilo.AgregarColumna(dgv, "Fecha", "Fecha", 150, "dd/MM/yyyy  HH:mm");
            Estilo.AgregarColumna(dgv, "Equipo", "Equipo", 170, null);
            Estilo.AgregarColumna(dgv, "Escenario", "Escenario", 150, null);
            Estilo.AgregarColumna(dgv, "Resultado", "Resultado", 90, null);
            Estilo.AgregarColumna(dgv, "Turnos", "Rondas", 70, null);
            Estilo.AgregarColumna(dgv, "DanoTotal", "Daño causado", 100, null);
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
        }
    }
}
