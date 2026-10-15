using System;
using System.IO;

// Genera los efectos de sonido del juego (Sonidos\*.wav) por síntesis, sin ningún programa externo.
// NO forma parte del juego: se compila aparte.   csc GeneradorSonidos.cs   y luego   GeneradorSonidos.exe Sonidos
// Formato: WAV PCM de 16 bits, mono, 22050 Hz (el único que acepta System.Media.SoundPlayer).
class GeneradorSonidos
{
    const int Frecuencia = 22050;
    static Random azar = new Random(12345);

    static void Main(string[] args)
    {
        string carpeta = args.Length > 0 ? args[0] : "Sonidos";
        Directory.CreateDirectory(carpeta);

        Guardar(Path.Combine(carpeta, "ataque.wav"), Ataque());
        Guardar(Path.Combine(carpeta, "habilidad.wav"), Habilidad());
        Guardar(Path.Combine(carpeta, "relevo.wav"), Relevo());
        Guardar(Path.Combine(carpeta, "terremoto.wav"), Terremoto());
        Console.WriteLine("Sonidos generados en " + Path.GetFullPath(carpeta));
    }

    static double Ruido()
    {
        return azar.NextDouble() * 2 - 1;
    }

    // Golpe seco: un "thump" grave que baja de tono más un chasquido de ruido que se apaga muy rápido.
    static double[] Ataque()
    {
        double[] s = new double[(int)(0.30 * Frecuencia)];
        double fase = 0, filtro = 0;
        for (int i = 0; i < s.Length; i++)
        {
            double t = (double)i / Frecuencia;
            double frecuencia = 55 + 190 * Math.Exp(-t * 20);
            fase += 2 * Math.PI * frecuencia / Frecuencia;
            double ruido = Ruido();
            filtro += 0.35 * (ruido - filtro);          // paso bajo
            double agudo = ruido - filtro;              // lo que queda es la parte aguda del ruido
            s[i] = Math.Exp(-t * 11) * Math.Sin(fase) * 0.9 + Math.Exp(-t * 38) * agudo * 0.7;
        }
        return Normalizar(s, 0.80);
    }

    // Habilidad: un barrido brillante que sube de tono, con armónicos y un temblor rápido (efecto mágico).
    static double[] Habilidad()
    {
        double duracion = 0.70;
        double[] s = new double[(int)(duracion * Frecuencia)];
        double fase = 0;
        for (int i = 0; i < s.Length; i++)
        {
            double t = (double)i / Frecuencia;
            double avance = t / duracion;
            double frecuencia = 330 * Math.Pow(5.0, avance);                // de 330 Hz a 1650 Hz
            fase += 2 * Math.PI * frecuencia / Frecuencia;
            double tono = Math.Sin(fase) + 0.45 * Math.Sin(2 * fase) + 0.2 * Math.Sin(3 * fase) + 0.3 * Math.Sin(fase * 1.5);
            double temblor = 0.75 + 0.25 * Math.Sin(2 * Math.PI * 16 * t);
            double ataque = Math.Min(1.0, t / 0.03);
            double caida = Math.Pow(Math.Sin(Math.PI * Math.Min(1.0, avance * 0.92 + 0.04)), 0.6);
            s[i] = tono * temblor * ataque * caida;
        }
        return Normalizar(s, 0.60);
    }

    // Relevo: un "swoosh" de aire (ruido filtrado que sube y baja) y dos notas cortas al final.
    static double[] Relevo()
    {
        double duracion = 0.50;
        double[] s = new double[(int)(duracion * Frecuencia)];
        double paso = 0, banda = 0;
        for (int i = 0; i < s.Length; i++)
        {
            double t = (double)i / Frecuencia;
            double avance = t / duracion;
            double centro = 350 + 2200 * Math.Sin(Math.PI * Math.Min(1.0, avance * 1.3));
            double f = 2 * Math.Sin(Math.PI * centro / Frecuencia);
            double ruido = Ruido();
            // Filtro de estado variable (paso banda): deja pasar solo el ruido cercano a "centro".
            paso += f * banda;
            double alto = ruido - paso - 0.35 * banda;
            banda += f * alto;
            double envolvente = Math.Pow(Math.Sin(Math.PI * Math.Min(1.0, avance * 1.15)), 1.5);
            s[i] = banda * envolvente * 1.6;
        }
        AgregarNota(s, 0.27, 0.12, 660, 0.35);
        AgregarNota(s, 0.34, 0.14, 990, 0.35);
        return Normalizar(s, 0.65);
    }

    static void AgregarNota(double[] s, double inicio, double duracion, double frecuencia, double volumen)
    {
        int desde = (int)(inicio * Frecuencia);
        int cuantas = (int)(duracion * Frecuencia);
        for (int i = 0; i < cuantas && desde + i < s.Length; i++)
        {
            double t = (double)i / Frecuencia;
            s[desde + i] += volumen * Math.Sin(2 * Math.PI * frecuencia * t) * Math.Exp(-t * 18) * Math.Min(1.0, t / 0.005);
        }
    }

    // Terremoto: retumbar grave (ruido "marrón" muy filtrado + dos tonos bajos que vibran) con crujidos de roca.
    static double[] Terremoto()
    {
        double duracion = 1.50;
        double[] s = new double[(int)(duracion * Frecuencia)];
        double marron = 0, suave = 0;
        for (int i = 0; i < s.Length; i++)
        {
            double t = (double)i / Frecuencia;
            marron += 0.04 * (Ruido() - marron);
            suave += 0.25 * (marron - suave);
            double vibracion = 0.7 + 0.3 * Math.Sin(2 * Math.PI * 9 * t);
            double graves = Math.Sin(2 * Math.PI * 38 * t) * 0.6 + Math.Sin(2 * Math.PI * 57 * t) * 0.4;
            double crujido = 0;
            if (azar.NextDouble() < 0.0018) crujido = Ruido() * 0.9;   // golpecitos sueltos de roca
            double envolvente = Math.Min(1.0, t / 0.15) * Math.Min(1.0, (duracion - t) / 0.6);
            s[i] = (suave * 9.0 + graves * 0.55) * vibracion * envolvente + crujido * envolvente;
        }
        return Normalizar(s, 0.85);
    }

    static double[] Normalizar(double[] s, double pico)
    {
        double max = 0;
        foreach (double v in s) max = Math.Max(max, Math.Abs(v));
        if (max <= 0) return s;
        for (int i = 0; i < s.Length; i++) s[i] = s[i] / max * pico;
        // Los últimos milisegundos se apagan para que no suene un "clic" al terminar.
        int cola = Math.Min(s.Length, (int)(0.01 * Frecuencia));
        for (int i = 0; i < cola; i++) s[s.Length - 1 - i] *= (double)i / cola;
        return s;
    }

    static void Guardar(string ruta, double[] muestras)
    {
        using (BinaryWriter w = new BinaryWriter(File.Create(ruta)))
        {
            int bytesDatos = muestras.Length * 2;
            w.Write(new char[] { 'R', 'I', 'F', 'F' });
            w.Write(36 + bytesDatos);
            w.Write(new char[] { 'W', 'A', 'V', 'E' });
            w.Write(new char[] { 'f', 'm', 't', ' ' });
            w.Write(16);                    // tamaño del bloque fmt
            w.Write((short)1);              // PCM
            w.Write((short)1);              // mono
            w.Write(Frecuencia);
            w.Write(Frecuencia * 2);        // bytes por segundo
            w.Write((short)2);              // bytes por muestra
            w.Write((short)16);             // bits por muestra
            w.Write(new char[] { 'd', 'a', 't', 'a' });
            w.Write(bytesDatos);
            foreach (double m in muestras)
                w.Write((short)Math.Round(Math.Max(-1.0, Math.Min(1.0, m)) * 32767));
        }
    }
}
