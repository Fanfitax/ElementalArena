using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Modelos;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    /// <summary>Menú principal: pide el nombre del jugador y lanza una partida o las estadísticas.</summary>
    public class FrmMenu : Form
    {
        private Random azar = new Random();
        private TextBox txtJugador;

        public FrmMenu()
        {
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

            Label nota = Estilo.CrearEtiqueta("Las estadísticas se guardan solo mientras el programa esté abierto.", 16, 392, 448, 36, 8.5f, false);
            nota.ForeColor = Estilo.TextoSuave;
            nota.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(nota);

            AcceptButton = btnJugar;
        }

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
