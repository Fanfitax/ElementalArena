using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Modelos;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    /// <summary>Menú principal: identifica al jugador y lanza partidas o estadísticas.</summary>
    public class FrmMenu : Form
    {
        private readonly IRepositorio repo;
        private readonly Random azar = new Random();
        private TextBox txtJugador;

        public FrmMenu(IRepositorio repo)
        {
            this.repo = repo;
            Estilo.AplicarFormulario(this, "Arena Elemental", 480, 440);

            Label titulo = Estilo.CrearEtiqueta("ARENA ELEMENTAL", 0, 24, 480, 44, 24f, true);
            titulo.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(titulo);

            Label sub = Estilo.CrearEtiqueta("Combate estratégico por turnos y relevos\n" + Estilo.Triangulo, 0, 72, 480, 44, 10f, false);
            sub.TextAlign = ContentAlignment.MiddleCenter;
            sub.ForeColor = Color.Gold;
            Controls.Add(sub);

            Controls.Add(Estilo.CrearEtiqueta("Nombre del jugador", 90, 140, 300, 22, 10.5f, true));
            txtJugador = new TextBox();
            txtJugador.SetBounds(90, 164, 300, 28);
            txtJugador.Font = new Font("Segoe UI", 12f);
            txtJugador.MaxLength = 30;
            txtJugador.Text = Environment.UserName;
            Controls.Add(txtJugador);

            Button btnJugar = Estilo.CrearBoton("Nueva partida", 90, 212, 300, 48);
            btnJugar.BackColor = Color.FromArgb(46, 160, 96);
            btnJugar.Click += delegate { NuevaPartida(); };
            Controls.Add(btnJugar);

            Button btnStats = Estilo.CrearBoton("Mis estadísticas", 90, 270, 300, 42);
            btnStats.Click += delegate { VerEstadisticas(); };
            Controls.Add(btnStats);

            Button btnSalir = Estilo.CrearBoton("Salir", 90, 322, 300, 36);
            btnSalir.BackColor = Estilo.PanelClaro;
            btnSalir.Click += delegate { Close(); };
            Controls.Add(btnSalir);

            Label modo = Estilo.CrearEtiqueta(repo.Descripcion, 16, 392, 448, 36, 8.5f, false);
            modo.ForeColor = Estilo.TextoSuave;
            modo.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(modo);

            AcceptButton = btnJugar;
        }

        /// <summary>Valida el nombre y devuelve el id del jugador (null si hubo un problema).</summary>
        private int? ObtenerJugador(out string nombre)
        {
            nombre = txtJugador.Text.Trim();
            if (nombre.Length == 0)
            {
                MessageBox.Show("Escribe tu nombre de jugador.", "Falta el nombre", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtJugador.Focus();
                return null;
            }
            try
            {
                return repo.ObtenerOCrearJugador(nombre);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo registrar al jugador:\n" + ex.Message, "Base de datos",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private void NuevaPartida()
        {
            string nombre;
            int? idJugador = ObtenerJugador(out nombre);
            if (idJugador == null) return;

            IList<Personaje> catalogo;
            IList<Escenario> escenarios;
            try
            {
                catalogo = repo.ObtenerPersonajes();
                escenarios = repo.ObtenerEscenarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo cargar el catálogo:\n" + ex.Message, "Base de datos",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (FrmSeleccionEquipo seleccion = new FrmSeleccionEquipo(catalogo, escenarios, nombre))
            {
                if (seleccion.ShowDialog(this) != DialogResult.OK) return;

                int idEquipo;
                try
                {
                    idEquipo = repo.GuardarEquipo(idJugador.Value, seleccion.NombreEquipo, seleccion.EquipoElegido);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo guardar el equipo:\n" + ex.Message, "Base de datos",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                EquipoCombate propio = new EquipoCombate(seleccion.NombreEquipo, seleccion.EquipoElegido, true);
                EquipoCombate rival = new EquipoCombate("Rival CPU", SortearRival(catalogo), false);
                Combate combate = new Combate(seleccion.EscenarioElegido, propio, rival);

                using (FrmCombate frm = new FrmCombate(repo, idJugador.Value, idEquipo, combate))
                {
                    frm.ShowDialog(this);
                }
            }
        }

        private List<Personaje> SortearRival(IList<Personaje> catalogo)
        {
            List<Personaje> pool = new List<Personaje>(catalogo);
            List<Personaje> elegidos = new List<Personaje>();
            for (int i = 0; i < EquipoCombate.TamanoEquipo; i++)
            {
                int k = azar.Next(pool.Count);
                elegidos.Add(pool[k]);
                pool.RemoveAt(k);
            }
            return elegidos;
        }

        private void VerEstadisticas()
        {
            string nombre;
            int? idJugador = ObtenerJugador(out nombre);
            if (idJugador == null) return;
            try
            {
                IList<EstadisticaPartida> partidas = repo.ObtenerPartidas(idJugador.Value);
                using (FrmEstadisticas frm = new FrmEstadisticas(nombre, partidas))
                {
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar las estadísticas:\n" + ex.Message, "Base de datos",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
