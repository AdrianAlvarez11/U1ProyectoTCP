using AdivinaQuienCliente.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Net.Sockets;
using System.Printing;
using System.Text;
using System.Text.Json;

namespace AdivinaQuienCliente.Services
{
    public class ClienteJuegoService
    {
        public Jugador Servidor { get; set; } = new();
        public Jugador? JugadorCliente { get; set; }
        public List<string> PokemonValidos = new List<string>()
        {
            "Alakazam",
            "Blaziken",
            "Bulbasaur",
            "Charizard",
            "Ditto",
            "Dragonite",
            "Eevee",
            "Gardevoir",
            "Gengar",
            "Greninja",
            "Groudon",
            "Gyarados",
            "Jigglypuff",
            "Lapras",
            "Lucario",
            "Machamp",
            "Meowth",
            "Mewtwo",
            "Onix",
            "Pikachu",
            "Rayquaza",
            "Snorlax",
            "Squirtle",
            "Umbreon"
        };

        int puertoRemoto = 11000;

        public event Action? ClienteAceptado;
        public event Action? ClienteRechazado;
        public event Action? ServerEscogePokemon;
        public event Action? PartidaIniciada;

        public void Conectar(IPAddress serverIP, string nombreJugador)
        {
            if(string.IsNullOrWhiteSpace(nombreJugador) || nombreJugador.Length < 3)
            {
                throw new ArgumentException("Tu nombre debe tener al menos 3 caracteres");
            }

            if(Servidor.Conexion == null)
            {
                Servidor.Conexion = new();
                IPEndPoint endpoint = new IPEndPoint(serverIP, puertoRemoto);

                Servidor.Conexion.Connect(endpoint);

                if (Servidor.Conexion.Connected)
                {
                    var unirseCommand = new UnirseSalaComando
                    {
                        Comando = Orden.UnirseSala,
                        NombreJugador = nombreJugador
                    };

                    JugadorCliente = new() { Nombre = nombreJugador };

                    Thread hiloRecibir = new Thread(RecibirMensaje);
                    hiloRecibir.IsBackground = true;
                    hiloRecibir.Start();

                    EnviarComando(unirseCommand);
                }

            }
        }

        private void EnviarComando(object comando)
        {
            if (Servidor != null && Servidor.Conexion != null)
            {
                var stream = Servidor.Conexion.GetStream();
                var json = JsonSerializer.Serialize(comando);

                var buffer = Encoding.UTF8.GetBytes(json);

                stream.Write(buffer, 0, buffer.Length);
            }
        }

        private void RecibirMensaje()
        {
            try
            {
                if(Servidor!=null && Servidor.Conexion!= null)
                {
                    while (Servidor.Conexion.Connected)
                    {
                        if(Servidor.Conexion.Available > 0/* && !Servidor.Conexion.Client.Poll(1000, SelectMode.SelectRead*/)
                        {
                            var stream = Servidor.Conexion.GetStream();
                            var buffer = new byte[Servidor.Conexion.Available];
                            stream.ReadExactly(buffer, 0, buffer.Length);
                            var json = Encoding.UTF8.GetString(buffer);

                            var comando = JsonSerializer.Deserialize<Comandos>(json);
                            if (comando != null)
                            {
                                switch (comando.Comando)
                                {
                                    case Orden.JugadorConectado:
                                        var bienvenido = JsonSerializer.Deserialize<JugadorConectadoComando>(json);
                                        if (bienvenido != null)
                                        {
                                            Servidor.Nombre = bienvenido.NombreServidor;
                                            ClienteAceptado?.Invoke();
                                        }
                                        break;

                                    case Orden.JugadorRechazado:
                                        Servidor.Conexion.Close();
                                        Servidor.Conexion = null;
                                        ClienteRechazado?.Invoke();
                                        return;


                                    case Orden.SeleccionarPokemon:
                                        ServerEscogePokemon?.Invoke();
                                        break;


                                    default: break;
                                }

                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }


        public void SeleccionarPokemonCliente(string pokemon)
        {
            if (!PokemonValidos.Contains(pokemon))
                return;

            if (JugadorCliente != null)
            {
                JugadorCliente.Pokemon = pokemon;
            }

            if (Servidor != null && Servidor.Conexion != null)
            {
                var comando = new PokemonSeleccionadoComando
                {
                    Comando = Orden.PokemonSeleccionado,
                    Pokemon = pokemon
                };

                EnviarComando(comando);

                PartidaIniciada?.Invoke();
            }
        }
    }
}
