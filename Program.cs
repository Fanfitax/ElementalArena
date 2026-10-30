using System;
using System.Windows.Forms;
using ArenaElemental.Datos;
using ArenaElemental.Formularios;

namespace ArenaElemental
{
    static class Program
    {
        // [STAThread] es necesario para que las ventanas de Windows Forms funcionen bien.
        [STAThread]
        static void Main()
        {
            // Primero se cargan los datos del juego: personajes, escenarios, lore y partidas guardadas.
            CatalogoBase.Cargar();
            LorePersonajes.Cargar();
            RegistroPartidas.Cargar();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMenu());
        }
    }
}
