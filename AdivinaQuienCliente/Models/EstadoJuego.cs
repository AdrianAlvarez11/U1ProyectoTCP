using System;
using System.Collections.Generic;
using System.Text;

namespace AdivinaQuienCliente.Models
{
    public class 
        EstadoJuego
    {
        public Jugador? JugadorTurno { get; set; } 
        public int Ronda { get; set; }
        public string? Pregunta { get; set; }
        public List<string> Historial { get; set; } = new();


    }
}
