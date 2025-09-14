using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Coordinador;

abstract class Coordinador
{
    // Lista con todos los nodos del grupo conectados
    private static List<TcpClient> nodosGrupo = new List<TcpClient>();
    
    // Objeto usado para "lock" y evitar problemas cuando varios hilos
    // intentan leer/modificar la lista de nodos al mismo tiempo
    private static object lockObj = new object();

    static void Main(string[] args)
    {
        // TcpListener es un server que escucha conexiones entrantes
        // Se crea uno para los nodos del grupo (puerto 5000)
        TcpListener listenerNodos = new TcpListener(IPAddress.Any, 5000);
        
        // Otro para los nodos externos (puerto 6000)
        TcpListener listenerExterno = new TcpListener(IPAddress.Any, 6000);

        // Se arrancan ambos servidores
        listenerNodos.Start();
        listenerExterno.Start();

        Console.WriteLine("Coordinador escuchando...");

        // Hilo que acepta conexiones de los nodos del grupo
        Thread hiloNodos = new Thread(() =>
        {
            while (true)
            {
                // Se bloquea hasta que un nodo se conecte
                TcpClient nodo = listenerNodos.AcceptTcpClient();
                
                // Se agrega el nodo a la lista compartida
                lock (lockObj) nodosGrupo.Add(nodo);
                
                Console.WriteLine("Nodo conectado");
            }
        });
        hiloNodos.Start();

        // Hilo que acepta conexiones de los nodos externos
        Thread hiloExternos = new Thread(() =>
        {
            while (true)
            {
                // Espera un cliente externo
                TcpClient externo = listenerExterno.AcceptTcpClient();
                Console.WriteLine("Cliente externo conectado.");
                
                // Por cada cliente externo creamos un hilo independiente
                new Thread(() => HandleExterno(externo)).Start();
            }
        });
        hiloExternos.Start();
    }

    // Maneja la comunicación con un nodo externo
    private static void HandleExterno(TcpClient externo)
    {
        try
        {
            // Obtenemos el canal de comunicación con el nodo externo(stream)
            NetworkStream streamExterno = externo.GetStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                // Se lee lo que manda el cliente externo
                int bytes = streamExterno.Read(buffer, 0, buffer.Length);
                
                // conexión cerrada
                if (bytes == 0) break;
                
                // bytes → texto
                string message = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();

                // Si se manda "CLOSE", se termina la comunicación
                if (message.ToUpper() == "CLOSE")
                {
                    Console.WriteLine("Cliente externo terminó la comunicación");
                    break;
                }

                Console.WriteLine($"Mensaje recibido de externo: {message}");

                // Reenviar a todos los nodos
                List<TcpClient> fallidos = new List<TcpClient>();
                
                // Se protege la lista de nodos
                lock (lockObj)
                {
                    foreach (var nodo in nodosGrupo)
                    {
                        try
                        {
                            // Se manda el mensaje al nodo
                            NetworkStream nodoStream = nodo.GetStream();
                            byte[] data = Encoding.UTF8.GetBytes(message);
                            nodoStream.Write(data, 0, data.Length);

                            // Esperar respuesta del nodo (ACK o texto)
                            nodoStream.ReadTimeout = 3000; // 3 seg
                            byte[] ackBuffer = new byte[1024];
                            int ackBytes = nodoStream.Read(ackBuffer, 0, ackBuffer.Length);
                            string respuesta = Encoding.UTF8.GetString(ackBuffer, 0, ackBytes).Trim();

                            //Comprobar que el mensaje no sea el ACK para responder
                            if (respuesta != "ACK")
                            {
                                Console.WriteLine($"[Nodo] {respuesta}");
                                fallidos.Add(nodo);
                            }
                        }
                        catch
                        {
                            // Sí hay algún error, como timeout, desconexión, etc.
                            fallidos.Add(nodo);
                        }
                    }
                }

                // Se muestra en consola el resultado
                if (fallidos.Count > 0)
                {
                    Console.WriteLine($"No todos los nodos recibieron el mensaje ({fallidos.Count} fallidos).");

                    // Enviar aviso de error al externo
                    byte[] failMsg = Encoding.UTF8.GetBytes("ERROR: " + fallidos.Count + " nodos no respondieron");
                    streamExterno.Write(failMsg, 0, failMsg.Length);
                }
                else
                {
                    Console.WriteLine("Mensaje entregado correctamente a todos los nodos.");

                    // Enviar ACK al externo
                    byte[] ackMsg = Encoding.UTF8.GetBytes("ACK");
                    streamExterno.Write(ackMsg, 0, ackMsg.Length);
                }
            }
        }
        catch
        {
            // No hay implementación en caso de error
        }
        finally
        {
            // Cerrar la conexión al terminar
            externo.Close();
            Console.WriteLine("Cliente externo desconectado");
        }
    }
}