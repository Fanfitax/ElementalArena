using System;
using System.Collections.Generic;
using System.Media;

namespace ArenaElemental.Formularios
{
    // Efectos de sonido del combate con System.Media.SoundPlayer (viene con .NET, sin librerías externas).
    // Los archivos .wav están en la carpeta "Sonidos": ataque.wav, habilidad.wav, relevo.wav y terremoto.wav.
    // Si un archivo no existe o está dañado, simplemente no suena: el juego nunca se cae por un sonido.
    public static class Sonidos
    {
        // Dos listas paralelas: el reproductor número i es del archivo número i.
        // Si el archivo no se pudo cargar se guarda null para no volver a buscarlo.
        private static List<string> archivos = new List<string>();
        private static List<SoundPlayer> reproductores = new List<SoundPlayer>();
        private static bool activados = true;

        // Permite silenciar todos los efectos (por ejemplo desde un botón de la interfaz).
        public static bool Activados
        {
            get { return activados; }
            set { activados = value; }
        }

        // Ataque básico.
        public static void Ataque()
        {
            Reproducir("ataque.wav");
        }

        // Habilidad especial (de ataque o de utilidad).
        public static void Habilidad()
        {
            Reproducir("habilidad.wav");
        }

        // Relevo de personaje.
        public static void Relevo()
        {
            Reproducir("relevo.wav");
        }

        // Cuando se activa el Terremoto.
        public static void Terremoto()
        {
            Reproducir("terremoto.wav");
        }

        private static void Reproducir(string archivo)
        {
            if (!activados) return;

            // Si el archivo todavía no está en la lista, se agrega (con null hasta que se cargue bien).
            int posicion = archivos.IndexOf(archivo);
            if (posicion == -1)
            {
                archivos.Add(archivo);
                reproductores.Add(null);
                posicion = archivos.Count - 1;
                CargarReproductor(archivo, posicion);
            }

            try
            {
                SoundPlayer reproductor = reproductores[posicion];
                if (reproductor != null) reproductor.Play();   // Play() no detiene el juego: suena en segundo plano
            }
            catch (Exception)
            {
                reproductores[posicion] = null;   // sin tarjeta de sonido: ya no se vuelve a intentar
            }
        }

        private static void CargarReproductor(string archivo, int posicion)
        {
            try
            {
                string ruta = Estilo.BuscarEnCarpeta("Sonidos", archivo);
                if (ruta != null)
                {
                    SoundPlayer reproductor = new SoundPlayer(ruta);
                    reproductor.Load();   // lanza una excepción si el .wav está dañado
                    reproductores[posicion] = reproductor;
                }
            }
            catch (Exception)
            {
                reproductores[posicion] = null;   // dañado: ya no se vuelve a intentar
            }
        }
    }
}
