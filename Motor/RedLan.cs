using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ArenaElemental.Datos;
using ArenaElemental.Modelos;

namespace ArenaElemental.Motor
{
    /// <summary>
    /// Conexión entre dos PCs de la misma red local (LAN) con sockets nativos: un jugador es el ANFITRIÓN
    /// (usa TcpListener y espera) y el otro es el INVITADO (usa TcpClient y se conecta por IP y puerto).
    ///
    /// Los mensajes son líneas de texto (terminan en salto de línea). Todo se consulta desde un Timer del
    /// formulario (no hay hilos): "HayClienteEsperando", "ConexionTerminada" y "LeerTexto" nunca se quedan
    /// esperando; si no hay nada devuelven false o null.
    ///
    /// Mensajes que se envían:
    ///   CONFIG|nombreEquipo|id,id,id|escenario|semilla   (el anfitrión: equipo, escenario y semilla del azar)
    ///   CONFIG|nombreEquipo|id,id,id                     (el invitado: su equipo)
    ///   ACCION|tipo|numeroHabilidad|indice               (una jugada del combate)
    ///   RENDIRSE                                         (el jugador abandona)
    /// Los dos combates arrancan idénticos (mismos equipos, escenario y semilla) y reciben las mismas jugadas
    /// en el mismo orden, así que siempre quedan iguales en los dos PCs.
    /// </summary>
    public class RedLan
    {
        public const int PuertoPorDefecto = 8888;

        private TcpListener escuchador;       // solo el anfitrión
        private TcpClient cliente;            // la conexión con el otro PC
        private IAsyncResult conexionEnCurso; // solo el invitado, mientras se conecta
        private NetworkStream flujo;
        private List<byte> recibido = new List<byte>();   // bytes que ya llegaron y todavía no forman una línea completa
        private bool desconectado;

        public bool EsAnfitrion { get; private set; }

        /// <summary>Explica en texto lo último que falló (puerto ocupado, IP inválida, etc.).</summary>
        public string UltimoError { get; private set; }

        /// <summary>True si la conexión con el otro PC se perdió o se cerró.</summary>
        public bool Desconectado
        {
            get { return desconectado; }
        }

        // ---------- Anfitrión ----------

        /// <summary>Empieza a esperar a que otro PC se conecte a este puerto. Devuelve false si no se pudo (puerto ocupado).</summary>
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

        /// <summary>True cuando otro PC ya se conectó (en ese momento se acepta la conexión).</summary>
        public bool HayClienteEsperando()
        {
            if (escuchador == null || !escuchador.Pending()) return false;
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
                conexionEnCurso = cliente.BeginConnect(direccion, puerto, null, null);
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
        /// Se llama una y otra vez desde un Timer mientras el invitado se conecta.
        /// Devuelve true cuando ya está conectado. Si falló, Desconectado queda en true y UltimoError dice por qué.
        /// </summary>
        public bool ConexionTerminada()
        {
            if (conexionEnCurso == null || !conexionEnCurso.IsCompleted) return false;
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
        /// Devuelve la siguiente línea recibida o null si todavía no llegó una completa (no espera).
        /// Si el otro PC cerró la conexión, Desconectado pasa a true.
        /// </summary>
        public string LeerTexto()
        {
            if (flujo == null) return null;

            try
            {
                // 1) Se pasa a "recibido" lo que haya llegado por la red.
                while (cliente.Available > 0)
                {
                    byte[] datos = new byte[cliente.Available];
                    int leidos = flujo.Read(datos, 0, datos.Length);
                    for (int i = 0; i < leidos; i++)
                        recibido.Add(datos[i]);
                }

                // 2) Si ya hay una línea completa (termina en el byte 10 = salto de línea), se entrega.
                int fin = recibido.IndexOf(10);
                if (fin >= 0)
                {
                    byte[] linea = recibido.GetRange(0, fin).ToArray();
                    recibido.RemoveRange(0, fin + 1);
                    return Encoding.UTF8.GetString(linea).Trim();
                }

                // 3) Una línea sin fin tan larga no es de este juego: se corta la conexión.
                if (recibido.Count > 4096)
                {
                    desconectado = true;
                    return null;
                }

                // 4) Sin datos nuevos: si el otro PC cerró la conexión, se detecta aquí.
                if (cliente.Client.Poll(0, SelectMode.SelectRead) && cliente.Available == 0)
                    desconectado = true;
            }
            catch (Exception)
            {
                desconectado = true;
            }
            return null;
        }

        /// <summary>Cierra todo (se puede llamar varias veces).</summary>
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
                // al cerrar no importa si algo ya estaba cerrado
            }
            escuchador = null;
            flujo = null;
            cliente = null;
            desconectado = true;
        }

        // ---------- Jugadas ----------

        public void EnviarAccion(AccionCombate accion)
        {
            EnviarTexto("ACCION|" + accion.Tipo + "|" + accion.NumeroHabilidad + "|" + accion.Indice);
        }

        public void EnviarRendicion()
        {
            EnviarTexto("RENDIRSE");
        }

        /// <summary>Convierte un mensaje "ACCION|..." en la jugada. Devuelve null si el texto no es una jugada válida.</summary>
        public static AccionCombate TextoAAccion(string mensaje)
        {
            string[] partes = mensaje.Split('|');
            if (partes.Length != 4 || partes[0] != "ACCION") return null;

            int numero, indice;
            if (!int.TryParse(partes[2], out numero) || !int.TryParse(partes[3], out indice)) return null;

            if (partes[1] == "Atacar") return AccionCombate.Atacar();
            if (partes[1] == "Relevo") return AccionCombate.Relevar();
            if (partes[1] == "Pasar") return AccionCombate.Pasar();
            if (partes[1] == "Habilidad")
            {
                if (numero < 0 || numero >= Personaje.CantidadHabilidades) return null;
                if (indice >= 0) return AccionCombate.HabilidadSobreAliado(numero, indice);
                return AccionCombate.Habilidad(numero);
            }
            return null;
        }

        // ---------- Ayudas ----------

        /// <summary>Las direcciones IPv4 de este PC (para decirle al otro jugador a cuál conectarse).</summary>
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
                // sin red no hay direcciones que mostrar
            }
            return direcciones;
        }
    }

    /// <summary>
    /// Lo que cada jugador anuncia antes del combate: nombre de su equipo y los 3 personajes (como números de
    /// la lista de CatalogoBase). El anfitrión además manda el escenario y la semilla del azar.
    /// </summary>
    public class ConfigPartida
    {
        public string NombreEquipo;
        public List<Personaje> Equipo = new List<Personaje>();
        public int IndiceEscenario = -1;   // solo viene del anfitrión
        public int Semilla;                // solo viene del anfitrión

        /// <summary>Arma el mensaje CONFIG para enviarlo por la red.</summary>
        public string ATexto()
        {
            StringBuilder ids = new StringBuilder();
            foreach (Personaje p in Equipo)
            {
                if (ids.Length > 0) ids.Append(",");
                ids.Append(CatalogoBase.Personajes.IndexOf(p));
            }

            // Los separadores "|" y los saltos de línea no pueden ir dentro del nombre.
            string nombre = NombreEquipo.Replace('|', '/').Replace('\n', ' ').Replace('\r', ' ');
            string texto = "CONFIG|" + nombre + "|" + ids;
            if (IndiceEscenario >= 0) texto += "|" + IndiceEscenario + "|" + Semilla;
            return texto;
        }

        /// <summary>Lee un mensaje CONFIG. Devuelve null si el texto está mal formado o pide personajes que no existen.</summary>
        public static ConfigPartida DeTexto(string mensaje)
        {
            string[] partes = mensaje.Split('|');
            if (partes[0] != "CONFIG" || (partes.Length != 3 && partes.Length != 5)) return null;
            if (partes[1].Trim().Length == 0) return null;

            ConfigPartida config = new ConfigPartida();
            config.NombreEquipo = partes[1].Trim();

            string[] ids = partes[2].Split(',');
            if (ids.Length != EquipoCombate.TamanoEquipo) return null;
            foreach (string id in ids)
            {
                int numero;
                if (!int.TryParse(id, out numero)) return null;
                if (numero < 0 || numero >= CatalogoBase.Personajes.Count) return null;
                Personaje p = CatalogoBase.Personajes[numero];
                if (config.Equipo.Contains(p)) return null;   // repetido
                config.Equipo.Add(p);
            }

            if (partes.Length == 5)
            {
                if (!int.TryParse(partes[3], out config.IndiceEscenario)) return null;
                if (config.IndiceEscenario < 0 || config.IndiceEscenario >= CatalogoBase.Escenarios.Count) return null;
                if (!int.TryParse(partes[4], out config.Semilla)) return null;
            }
            return config;
        }
    }
}
