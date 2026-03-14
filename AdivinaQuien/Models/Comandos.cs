using System;
using System.Collections.Generic;
using System.Text;

namespace AdivinaQuienServidor.Models
{
    public enum Orden { UnirseSala, JugadorConectado, JugadorRechazado, SeleccionarPokemon, PokemonSeleccionado, IniciarPartida, Pregunta, Respuesta, CambiarTurno, Adivinar, Ganar, Perder }
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
        public Jugador JugadorTurno { get; set; } = null!;

        public List<string>? Historial { get; set; }
        public int Ronda { get; set; }
    }

    public class SeleccionarPokemonComando : Comandos
    {
        //servidor-->cliente
    }

    public class PokemonSeleccionadoComando : Comandos
    {
        public string Pokemon { get; set; } = null!;
        //cliente-->servidor 
    }

    public class PreguntaComando : Comandos
    {
        public string Pregunta { get; set; } = null!;
    }
    public class RespuestaComando : Comandos
    {
        public string Respuesta { get; set; } = null!;
    }

    public class CambiarTurnoComando : Comandos
    {
        public Jugador JugadorTurno { get; set; } = null!;
        public List<string>? Historial { get; set; }
        public int Ronda { get; set; }

    }

    public class AdivinarComando : Comandos
    {
        public string Pokemon { get; set; } = null!;
    }

    public class GanarComando : Comandos
    {
        //servidor-->cliente. Indicar que el cliente gano
        public string PokemonRival { get; set; } = null!;
    }

    public class PerderComando : Comandos
    {
        //servidor-->cliente. Indicar que el cliente perdio
        public string PokemonRival { get; set; } = null!;
    }




}
