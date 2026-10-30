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

        public FrmEstadisticas(string nombreJugador, List<EstadisticaPartida> partidas)
        {
            Estilo.AplicarFormulario(this, Idioma.Texto("Arena Elemental - Estadísticas", "Arena Elemental - Statistics"), 924, 580);

            ResumenJugador r = new ResumenJugador(partidas);

            Label titulo = Estilo.CrearEtiqueta(Idioma.Texto("ESTADÍSTICAS", "STATISTICS"), 24, 12, 600, 38, 22f, true);
            titulo.ForeColor = Color.Gold;
            Controls.Add(titulo);

            Label sub = Estilo.CrearEtiqueta(Idioma.Texto("Jugador: ", "Player: ") + nombreJugador, 26, 50, 600, 20, 10.5f, false);
            sub.ForeColor = Estilo.TextoSuave;
            Controls.Add(sub);

            // Tarjetas de resumen.
            CrearTarjeta(0, r.Partidas.ToString(), Idioma.Texto("PARTIDAS", "MATCHES"), Color.White);
            CrearTarjeta(1, r.Victorias.ToString(), Idioma.Texto("VICTORIAS", "VICTORIES"), Color.LightGreen);
            CrearTarjeta(2, r.Derrotas.ToString(), Idioma.Texto("DERROTAS", "DEFEATS"), Color.IndianRed);
            CrearTarjeta(3, r.PorcentajeVictorias.ToString("0.#") + "%", Idioma.Texto("% DE VICTORIAS", "% WIN RATE"), Color.Gold);
            CrearTarjeta(4, r.DanoTotal.ToString(), Idioma.Texto("DAÑO TOTAL", "TOTAL DAMAGE"), Color.FromArgb(255, 150, 90));
            CrearTarjeta(5, r.PromedioTurnos.ToString("0.#"), Idioma.Texto("RONDAS POR PARTIDA", "ROUNDS PER MATCH"), Color.FromArgb(120, 190, 255));

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
                Label vacio = Estilo.CrearEtiqueta(Idioma.Texto("Todavía no has jugado partidas. ¡Arma tu equipo y combate!", "You have not played any matches yet. Build your team and fight!"), 0, 150, 876, 30, 12f, true);
                vacio.ForeColor = Estilo.TextoSuave;
                vacio.TextAlign = ContentAlignment.MiddleCenter;
                marco.Controls.Add(vacio);
                vacio.BringToFront();
            }

            Button cerrar = new Button();
            cerrar.SetBounds(780, 524, 120, 40);
            Estilo.EstilizarBoton(cerrar, Idioma.Texto("Cerrar", "Close"), Estilo.Acento);
            cerrar.Click += Cerrar_Click;
            Controls.Add(cerrar);
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
