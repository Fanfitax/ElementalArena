using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    /// <summary>
    /// Conecta dos PCs de la misma red (LAN) con sockets de .NET:
    ///  - El ANFITRIÓN usa un TcpListener: abre un puerto y espera.
    ///  - El INVITADO usa un TcpClient: se conecta a la IP y al puerto del anfitrión.
    ///
    /// Los mensajes son líneas de texto (cada una termina en un salto de línea). Para no usar hilos,
    /// los formularios le preguntan a esta clase cada cierto tiempo con un Timer
    /// ("¿ya se conectó alguien?", "¿llegó algo?"). Ninguna de esas preguntas se queda esperando:
    /// si no hay nada devuelven false o null.
    ///
    /// Mensajes que se envían:
    ///   CONFIG|nombreEquipo|id,id,id|escenario|semilla   el anfitrión anuncia su equipo, el escenario y la semilla
    ///   CONFIG|nombreEquipo|id,id,id                     el invitado anuncia su equipo
    ///   ACCION|tipo|numeroHabilidad|indice               una jugada del combate
    ///   RENDIRSE                                         el jugador abandona
    ///
    /// Los dos PCs crean el mismo combate (mismos equipos, escenario y semilla) y se pasan las jugadas,
    /// así que los dos combates siempre quedan iguales.
    /// </summary>
    public class RedLan
    {
        public const int PuertoPorDefecto = 8888;

        private TcpListener escuchador;        // solo lo usa el anfitrión
        private TcpClient cliente;             // la conexión con el otro PC
        private IAsyncResult conexionEnCurso;  // solo lo usa el invitado mientras se conecta
        private NetworkStream flujo;           // por aquí se envían y reciben los bytes
        private string recibido = "";          // texto que llegó y todavía no es una línea completa
        private bool desconectado;

        public bool EsAnfitrion { get; private set; }

        /// <summary>El último error, en texto, para mostrárselo al usuario.</summary>
        public string UltimoError { get; private set; }

        /// <summary>True si se perdió o se cerró la conexión.</summary>
        public bool Desconectado
        {
            get { return desconectado; }
        }

        // ---------- Anfitrión ----------

        /// <summary>Abre el puerto y empieza a esperar. Devuelve false si no se pudo (por ejemplo, puerto ocupado).</summary>
        public bool IniciarAnfitrion(int puerto)
        {
            try
            {
                EsAnfitrion = true;
                escuchador = new TcpListener(IPAddress.Any, puerto);
                escuchador.Start();
                return true;
            }
            catch (Exception ex)
            {
                UltimoError = "No se pudo abrir el puerto " + puerto + ": " + ex.Message;
                Cerrar();
                return false;
            }
        }

        /// <summary>Devuelve true cuando otro PC ya se conectó (en ese momento acepta la conexión).</summary>
        public bool HayClienteEsperando()
        {
            if (escuchador == null) return false;
            if (!escuchador.Pending()) return false;

            try
            {
                cliente = escuchador.AcceptTcpClient();
                flujo = cliente.GetStream();
                escuchador.Stop();
                escuchador = null;
                return true;
            }
            catch (Exception ex)
            {
                UltimoError = "Falló la conexión entrante: " + ex.Message;
                return false;
            }
        }

        // ---------- Invitado ----------

        /// <summary>Empieza a conectarse al anfitrión. Devuelve false si la IP está mal escrita.</summary>
        public bool IniciarConexion(string ip, int puerto)
        {
            IPAddress direccion;
            if (!IPAddress.TryParse(ip.Trim(), out direccion))
            {
                UltimoError = "La dirección IP \"" + ip + "\" no es válida (ejemplo: 192.168.1.20).";
                return false;
            }

            try
            {
                EsAnfitrion = false;
                cliente = new TcpClient();
                conexionEnCurso = cliente.BeginConnect(direccion, puerto, null, null);   // conecta sin congelar la ventana
                return true;
            }
            catch (Exception ex)
            {
                UltimoError = "No se pudo iniciar la conexión: " + ex.Message;
                Cerrar();
                return false;
            }
        }

        /// <summary>
        /// El Timer del formulario la llama mientras el invitado se conecta.
        /// Devuelve true cuando ya quedó conectado. Si falló, Desconectado queda en true.
        /// </summary>
        public bool ConexionTerminada()
        {
            if (conexionEnCurso == null) return false;
            if (!conexionEnCurso.IsCompleted) return false;

            try
            {
                cliente.EndConnect(conexionEnCurso);
                conexionEnCurso = null;
                flujo = cliente.GetStream();
                return true;
            }
            catch (Exception ex)
            {
                UltimoError = "No se pudo conectar con el anfitrión: " + ex.Message;
                desconectado = true;
                conexionEnCurso = null;
                return false;
            }
        }

        // ---------- Enviar y recibir ----------

        /// <summary>Envía una línea de texto al otro PC.</summary>
        public void EnviarTexto(string linea)
        {
            if (flujo == null || desconectado) return;

            try
            {
                byte[] datos = Encoding.UTF8.GetBytes(linea + "\n");
                flujo.Write(datos, 0, datos.Length);
            }
            catch (Exception)
            {
                desconectado = true;
            }
        }

        /// <summary>
        /// Devuelve la siguiente línea que llegó, o null si todavía no llegó una completa (no espera).
        /// Si el otro PC cerró la conexión, Desconectado pasa a true.
        /// </summary>
        public string LeerTexto()
        {
            if (flujo == null) return null;

            try
            {
                // 1) Pasar a "recibido" todo lo que haya llegado.
                while (cliente.Available > 0)
                {
                    byte[] datos = new byte[cliente.Available];
                    int leidos = flujo.Read(datos, 0, datos.Length);
                    recibido = recibido + Encoding.UTF8.GetString(datos, 0, leidos);
                }

                // 2) Si hay una línea completa, entregarla.
                int fin = recibido.IndexOf('\n');
                if (fin >= 0)
                {
                    string linea = recibido.Substring(0, fin).Trim();
                    recibido = recibido.Substring(fin + 1);
                    return linea;
                }

                // 3) Una línea sin fin tan larga no es de este juego: se corta la conexión.
                if (recibido.Length > 4096)
                {
                    desconectado = true;
                    return null;
                }

                // 4) Sin datos nuevos: si el otro PC cerró la conexión, se nota aquí.
                if (cliente.Client.Poll(0, SelectMode.SelectRead) && cliente.Available == 0)
                    desconectado = true;
            }
            catch (Exception)
            {
                desconectado = true;
            }
            return null;
        }

        /// <summary>Cierra la conexión (se puede llamar varias veces).</summary>
        public void Cerrar()
        {
            try
            {
                if (escuchador != null) escuchador.Stop();
                if (flujo != null) flujo.Close();
                if (cliente != null) cliente.Close();
            }
            catch (Exception)
            {
                // Si algo ya estaba cerrado no importa.
            }
            escuchador = null;
            flujo = null;
            cliente = null;
            desconectado = true;
        }

        // ---------- Mensajes del combate ----------

        /// <summary>Envía la jugada al otro PC.</summary>
        public void EnviarAccion(AccionCombate accion)
        {
            EnviarTexto("ACCION|" + accion.Tipo + "|" + accion.NumeroHabilidad + "|" + accion.Indice);
        }

        public void EnviarRendicion()
        {
            EnviarTexto("RENDIRSE");
        }

        /// <summary>Convierte un mensaje "ACCION|..." en una jugada. Devuelve null si el texto no sirve.</summary>
        public static AccionCombate TextoAAccion(string mensaje)
        {
            string[] partes = mensaje.Split('|');
            if (partes.Length != 4 || partes[0] != "ACCION") return null;

            int numero;
            int indice;
            if (!int.TryParse(partes[2], out numero)) return null;
            if (!int.TryParse(partes[3], out indice)) return null;

            string tipo = partes[1];
            if (tipo == "Atacar") return AccionCombate.Atacar();
            if (tipo == "Relevo") return AccionCombate.Relevar();
            if (tipo == "Pasar") return AccionCombate.Pasar();

            if (tipo == "Habilidad")
            {
                if (numero < 0 || numero >= Personaje.CantidadHabilidades) return null;
                if (indice >= 0) return AccionCombate.HabilidadSobreAliado(numero, indice);
                return AccionCombate.Habilidad(numero);
            }
            return null;
        }

        /// <summary>Las direcciones IPv4 de este PC, para decirle al otro jugador a cuál conectarse.</summary>
        public static List<string> DireccionesLocales()
        {
            List<string> direcciones = new List<string>();
            try
            {
                IPAddress[] todas = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                foreach (IPAddress ip in todas)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        direcciones.Add(ip.ToString());
                }
            }
            catch (Exception)
            {
                // Sin red no hay direcciones que mostrar.
            }
            return direcciones;
        }
    }
}
