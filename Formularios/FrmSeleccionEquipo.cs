using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArenaElemental.Modelos;
using ArenaElemental.Motor;

namespace ArenaElemental.Formularios
{
    /// <summary>Pantalla para armar el equipo de 3 personajes (el orden define quién inicia) y elegir escenario.</summary>
    public class FrmSeleccionEquipo : Form
    {
        private readonly IList<Escenario> escenarios;
        private readonly List<Personaje> equipo = new List<Personaje>();

        private ListView lvCatalogo;
        private ListBox lstEquipo;
        private Label lblDetalle;
        private Label lblEscenario;
        private TextBox txtNombreEquipo;
        private ComboBox cboEscenario;
        private Button btnAgregar, btnQuitar, btnSubir, btnBajar, btnCombatir, btnCancelar;

        public FrmSeleccionEquipo(IList<Personaje> catalogo, IList<Escenario> escenarios, string nombreJugador)
        {
            this.escenarios = escenarios;
            ConstruirInterfaz(nombreJugador);
            foreach (Personaje p in catalogo) AgregarFilaCatalogo(p);
            foreach (Escenario s in escenarios) cboEscenario.Items.Add(s);
            if (cboEscenario.Items.Count > 0) cboEscenario.SelectedIndex = 0;
            if (lvCatalogo.Items.Count > 0) lvCatalogo.Items[0].Selected = true;
            ActualizarEstado();
        }

        public List<Personaje> EquipoElegido { get { return new List<Personaje>(equipo); } }
        public Escenario EscenarioElegido { get { return cboEscenario.SelectedItem as Escenario; } }
        public string NombreEquipo { get { return txtNombreEquipo.Text.Trim(); } }

        private void ConstruirInterfaz(string nombreJugador)
        {
            Estilo.AplicarFormulario(this, "Arena Elemental - Arma tu equipo", 980, 590);

            Controls.Add(Estilo.CrearEtiqueta("Catálogo de personajes (doble clic para agregar)", 16, 12, 600, 22, 11f, true));

            lvCatalogo = new ListView();
            lvCatalogo.SetBounds(16, 40, 620, 290);
            lvCatalogo.View = View.Details;
            lvCatalogo.FullRowSelect = true;
            lvCatalogo.MultiSelect = false;
            lvCatalogo.HideSelection = false;
            lvCatalogo.GridLines = false;
            lvCatalogo.BackColor = Estilo.Panel;
            lvCatalogo.ForeColor = Estilo.Texto;
            lvCatalogo.BorderStyle = BorderStyle.None;
            lvCatalogo.Columns.Add("Nombre", 110);
            lvCatalogo.Columns.Add("Elemento", 75);
            lvCatalogo.Columns.Add("HP", 45);
            lvCatalogo.Columns.Add("ATK", 45);
            lvCatalogo.Columns.Add("DEF", 45);
            lvCatalogo.Columns.Add("ENG", 45);
            lvCatalogo.Columns.Add("EMP", 45);
            lvCatalogo.Columns.Add("Habilidad", 190);
            lvCatalogo.SelectedIndexChanged += delegate { MostrarDetalle(); };
            lvCatalogo.DoubleClick += delegate { AgregarSeleccionado(); };
            Controls.Add(lvCatalogo);

            lblDetalle = Estilo.CrearEtiqueta(string.Empty, 16, 340, 620, 100, 10f, false);
            lblDetalle.BackColor = Estilo.Panel;
            lblDetalle.Padding = new Padding(8, 6, 8, 6);
            Controls.Add(lblDetalle);

            btnAgregar = Estilo.CrearBoton("Agregar al equipo  >>", 16, 450, 220, 38);
            btnAgregar.Click += delegate { AgregarSeleccionado(); };
            Controls.Add(btnAgregar);

            Label tri = Estilo.CrearEtiqueta("Triángulo elemental:  " + Estilo.Triangulo + "\nVentaja x1.5  ·  Desventaja x0.5",
                                             16, 502, 620, 44, 10f, false);
            tri.ForeColor = Color.Gold;
            Controls.Add(tri);

            // Columna derecha
            Controls.Add(Estilo.CrearEtiqueta("Equipo de " + nombreJugador + " (el 1.º inicia)", 656, 12, 310, 22, 11f, true));

            lstEquipo = new ListBox();
            lstEquipo.SetBounds(656, 40, 308, 84);
            lstEquipo.BackColor = Estilo.Panel;
            lstEquipo.ForeColor = Estilo.Texto;
            lstEquipo.BorderStyle = BorderStyle.None;
            lstEquipo.Font = new Font("Segoe UI", 10.5f);
            lstEquipo.ItemHeight = 24;
            Controls.Add(lstEquipo);

            btnQuitar = Estilo.CrearBoton("Quitar", 656, 130, 98, 30);
            btnSubir = Estilo.CrearBoton("Subir", 760, 130, 98, 30);
            btnBajar = Estilo.CrearBoton("Bajar", 864, 130, 100, 30);
            btnQuitar.Click += delegate { QuitarSeleccionado(); };
            btnSubir.Click += delegate { MoverSeleccionado(-1); };
            btnBajar.Click += delegate { MoverSeleccionado(1); };
            Controls.Add(btnQuitar);
            Controls.Add(btnSubir);
            Controls.Add(btnBajar);

            Controls.Add(Estilo.CrearEtiqueta("Nombre del equipo", 656, 176, 308, 20, 10f, true));
            txtNombreEquipo = new TextBox();
            txtNombreEquipo.SetBounds(656, 198, 308, 26);
            txtNombreEquipo.MaxLength = 40;
            txtNombreEquipo.Text = "Equipo de " + nombreJugador;
            txtNombreEquipo.TextChanged += delegate { ActualizarEstado(); };
            Controls.Add(txtNombreEquipo);

            Controls.Add(Estilo.CrearEtiqueta("Escenario", 656, 238, 308, 20, 10f, true));
            cboEscenario = new ComboBox();
            cboEscenario.SetBounds(656, 260, 308, 26);
            cboEscenario.DropDownStyle = ComboBoxStyle.DropDownList;
            cboEscenario.SelectedIndexChanged += delegate { MostrarEscenario(); };
            Controls.Add(cboEscenario);

            lblEscenario = Estilo.CrearEtiqueta(string.Empty, 656, 292, 308, 70, 9.5f, false);
            lblEscenario.ForeColor = Estilo.TextoSuave;
            Controls.Add(lblEscenario);

            btnCombatir = Estilo.CrearBoton("¡A COMBATIR!", 656, 440, 308, 52);
            btnCombatir.BackColor = Color.FromArgb(46, 160, 96);
            btnCombatir.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            btnCombatir.Click += delegate { DialogResult = DialogResult.OK; Close(); };
            Controls.Add(btnCombatir);

            btnCancelar = Estilo.CrearBoton("Cancelar", 656, 500, 308, 34);
            btnCancelar.BackColor = Estilo.PanelClaro;
            btnCancelar.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancelar);
        }

        private void AgregarFilaCatalogo(Personaje p)
        {
            ListViewItem item = new ListViewItem(p.Nombre);
            item.SubItems.Add(p.Elemento.ToString());
            item.SubItems.Add(p.HpMax.ToString());
            item.SubItems.Add(p.Atk.ToString());
            item.SubItems.Add(p.Defensa.ToString());
            item.SubItems.Add(p.EnergiaMax.ToString());
            item.SubItems.Add(p.Empatia.ToString());
            item.SubItems.Add(p.NombreHabilidad);
            item.BackColor = Estilo.FondoDe(p.Elemento);
            item.ForeColor = Estilo.Texto;
            item.Tag = p;
            lvCatalogo.Items.Add(item);
        }

        private Personaje CatalogoSeleccionado()
        {
            if (lvCatalogo.SelectedItems.Count == 0) return null;
            return (Personaje)lvCatalogo.SelectedItems[0].Tag;
        }

        private void MostrarDetalle()
        {
            Personaje p = CatalogoSeleccionado();
            lblDetalle.Text = p == null ? string.Empty : Estilo.DescripcionCompleta(p);
        }

        private void MostrarEscenario()
        {
            Escenario s = EscenarioElegido;
            lblEscenario.Text = s == null ? string.Empty : s.Descripcion + "\nBonus: +" + s.BonusAtaquePct + "% de ATK a las cartas de " + s.ElementoAfin + ".";
        }

        private void AgregarSeleccionado()
        {
            Personaje p = CatalogoSeleccionado();
            if (p == null) return;
            if (equipo.Count >= EquipoCombate.TamanoEquipo)
            {
                MessageBox.Show("El equipo ya tiene " + EquipoCombate.TamanoEquipo + " personajes. Quita uno primero.",
                                "Equipo completo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            foreach (Personaje existente in equipo)
            {
                if (existente.Id == p.Id)
                {
                    MessageBox.Show(p.Nombre + " ya está en tu equipo.", "Repetido", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            equipo.Add(p);
            RefrescarEquipo(equipo.Count - 1);
        }

        private void QuitarSeleccionado()
        {
            int i = lstEquipo.SelectedIndex;
            if (i < 0) return;
            equipo.RemoveAt(i);
            RefrescarEquipo(Math.Min(i, equipo.Count - 1));
        }

        private void MoverSeleccionado(int delta)
        {
            int i = lstEquipo.SelectedIndex;
            int j = i + delta;
            if (i < 0 || j < 0 || j >= equipo.Count) return;
            Personaje tmp = equipo[i];
            equipo[i] = equipo[j];
            equipo[j] = tmp;
            RefrescarEquipo(j);
        }

        private void RefrescarEquipo(int seleccionar)
        {
            lstEquipo.Items.Clear();
            for (int i = 0; i < equipo.Count; i++)
                lstEquipo.Items.Add((i + 1) + ".  " + equipo[i].Nombre + "  [" + equipo[i].Elemento + "]");
            if (seleccionar >= 0 && seleccionar < lstEquipo.Items.Count) lstEquipo.SelectedIndex = seleccionar;
            ActualizarEstado();
        }

        private void ActualizarEstado()
        {
            btnCombatir.Enabled = equipo.Count == EquipoCombate.TamanoEquipo
                                  && EscenarioElegido != null
                                  && NombreEquipo.Length > 0;
            btnCombatir.Text = equipo.Count == EquipoCombate.TamanoEquipo
                ? "¡A COMBATIR!"
                : "Elige " + (EquipoCombate.TamanoEquipo - equipo.Count) + " personaje(s) más";
            btnCombatir.BackColor = btnCombatir.Enabled ? Color.FromArgb(46, 160, 96) : Estilo.PanelClaro;
        }
    }
}
