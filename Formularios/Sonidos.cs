using System;
using System.Collections.Generic;
using System.Media;

namespace ArenaElemental.Formularios
{
    /// <summary>
    /// Efectos de sonido del combate con System.Media.SoundPlayer (viene con .NET, sin librerías externas).
    /// Los archivos .wav están en la carpeta "Sonidos": ataque.wav, habilidad.wav, relevo.wav y terremoto.wav.
    /// Si un archivo no existe o está dañado, simplemente no suena: el juego nunca se cae por un sonido.
    /// </summary>
    public static class Sonidos
    {
        // Un SoundPlayer por archivo. Si el archivo no se pudo cargar se guarda null para no volver a buscarlo.
        private static Dictionary<string, SoundPlayer> reproductores = new Dictionary<string, SoundPlayer>();
        private static bool activados = true;

        /// <summary>Permite silenciar todos los efectos (por ejemplo desde un botón de la interfaz).</summary>
        public static bool Activados
        {
            get { return activados; }
            set { activados = value; }
        }

        /// <summary>Ataque básico.</summary>
        public static void Ataque()
        {
            Reproducir("ataque.wav");
        }

        /// <summary>Habilidad especial (de ataque o de utilidad).</summary>
        public static void Habilidad()
        {
            Reproducir("habilidad.wav");
        }

        /// <summary>Relevo de personaje.</summary>
        public static void Relevo()
        {
            Reproducir("relevo.wav");
        }

        /// <summary>Cuando se activa el Terremoto.</summary>
        public static void Terremoto()
        {
            Reproducir("terremoto.wav");
        }

        private static void Reproducir(string archivo)
        {
            if (!activados) return;

            try
            {
                SoundPlayer reproductor;
                if (!reproductores.TryGetValue(archivo, out reproductor))
                {
                    string ruta = Estilo.BuscarEnCarpeta("Sonidos", archivo);
                    reproductor = null;
                    if (ruta != null)
                    {
                        reproductor = new SoundPlayer(ruta);
                        reproductor.Load();   // lanza una excepción si el .wav está dañado
                    }
                    reproductores[archivo] = reproductor;
                }

                if (reproductor != null) reproductor.Play();   // Play() no detiene el juego: suena en segundo plano
            }
            catch (Exception)
            {
                reproductores[archivo] = null;   // dañado o sin tarjeta de sonido: ya no se vuelve a intentar
            }
        }
    }
}
