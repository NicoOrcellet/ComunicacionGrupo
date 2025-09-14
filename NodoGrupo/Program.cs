using System.Net.Sockets;
using System.Text;

namespace NodoGrupo;

abstract class NodoGrupo
{
    static void Main()
    {
        // Conexión al coordinador (puerto 5000)
        TcpClient cliente = new TcpClient();
        cliente.Connect("127.0.0.1", 5000);
        NetworkStream stream = cliente.GetStream();

        Console.WriteLine("Conectado al coordinador (puerto 5000). Esperando mensajes...");

        byte[] buffer = new byte[1024];
        while (true)
        {
            try
            {
                // Esperamos mensaje del coordinador
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                
                // En caso de que se corte la conexión
                if (bytesRead == 0) break; 

                // Se decodifica el mensaje para luego mostrarlo
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("[Mensaje recibido] " + message);

                // Enviar ACK al coordinador
                byte[] ack = Encoding.UTF8.GetBytes("ACK");
                stream.Write(ack, 0, ack.Length);
            }
            catch 
            { 
                // si hay error, se termina el loop
                break;
            }
        }

        cliente.Close();
    }
}