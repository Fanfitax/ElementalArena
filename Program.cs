using System;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Formularios;

namespace ArenaElemental
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string aviso;
            IRepositorio repo = RepositorioFactory.Crear(out aviso);
            if (aviso != null)
                MessageBox.Show(aviso, "Arena Elemental", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Application.Run(new FrmMenu(repo));
        }
    }
}
