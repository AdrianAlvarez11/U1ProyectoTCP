using System;
using System.Collections.Generic;
using System.Text;

namespace AdivinaQuienCliente.Models
{
    public class Pokemon
    {
        public string Nombre { get; set; } = null!;
        public string Imagen => $"/Resources/Images/{Nombre}.png";
    }
}
