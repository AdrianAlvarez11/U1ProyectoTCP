using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace AdivinaQuienCliente.Models
{
    public class Jugador
    {
        public TcpClient? Conexion { get; set; }
       public string Nombre { get; set; } = null!;
        public string? Pokemon { get; set; }
    }
}
