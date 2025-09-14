using System.Net.Sockets;
using System.Text;

namespace NodoExterno;

abstract class NodoExterno
{
    static void Main()
    {
        //Se conecta al coordinador (puerto 6000)
        TcpClient client = new TcpClient();
        client.Connect("127.0.0.1", 6000);
        NetworkStream stream = client.GetStream();

        Console.WriteLine("Escribí mensajes para enviar al grupo. CLOSE para desconectarte.");
        string? line;
        byte[] buffer = new byte[1024];
        
        while (!string.IsNullOrEmpty(line = Console.ReadLine()))
        {
            // Enviar mensaje al coordinador
            byte[] data = Encoding.UTF8.GetBytes(line);
            stream.Write(data, 0, data.Length);

            if (line == "CLOSE") break;

            // Esperar respuesta del coordinador (ACK o ERROR)
            int bytes = stream.Read(buffer, 0, buffer.Length);
            string respuesta = Encoding.UTF8.GetString(buffer, 0, bytes);
            Console.WriteLine("[Coordinador] " + respuesta);
        }

        client.Close();
    }
}