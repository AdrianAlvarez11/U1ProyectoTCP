using System;
using System.Collections.Generic;
using System.Text;

namespace AdivinaQuienCliente.Models
{
    public enum Orden { UnirseSala, JugadorConectado, JugadorRechazado, SeleccionarPokemon, PokemonSeleccionado, IniciarPartida }
    public class Comandos
    {
        public Orden Comando { get; set; }

    }

    public class UnirseSalaComando : Comandos
    {
        public string NombreJugador { get; set; } = null!; //cliente-->servidor. se une
    }

    public class JugadorConectadoComando : Comandos
    {
        public string NombreServidor { get; set; } = null!;
        //servidor-->cliente. confirma al cliente que se unio
    }
    public class JugadorRechazadoComando : Comandos
    {
        //servidor-->cliente. rechazar por nombre duplicado. 
    }
    public class IniciarPartidaComando : Comandos
    {
        public string NombreRival { get; set; } = null!;
    }

    public class SeleccionarPokemonComando : Comandos
    {
        //servidor-->cliente. indicar que le toca seleccionar al cliente
    }

    public class PokemonSeleccionadoComando : Comandos
    {
        public string Pokemon { get; set; } = null!;
        //jugador-->servidor. 
    }
}
