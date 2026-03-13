using AdivinaQuienServidor.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace AdivinaQuienServidor.Services
{
    public class ServidorJuegoService
    {
        TcpListener? Servidor;

        public Jugador? JugadorServer { get; set; }
        public Jugador? JugadorCliente { get; set; }
        public EstadoJuego? Juego { get; set; }

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

        int puertoEscucha = 11000;

        bool salaAbierta = false;

        public event Action<string>? ClienteConectado; //despues de cliente conectado, servidor debe escoger pokemon
        public event Action? PartidaIniciada;
        public event Action? ClienteDesconectado;


        public void AbrirSala(string nombre)
        {
            if (salaAbierta == false)
            {
                JugadorServer = new();
                JugadorServer.Nombre= nombre;
                salaAbierta = true;

                Thread hilo = new(RecibirCliente);
                hilo.IsBackground = true;
                hilo.Start();
            }
        }

        private void RecibirCliente()
        {
            IPEndPoint ipserver = new(IPAddress.Any, puertoEscucha);
            Servidor = new(ipserver);
            Servidor.Start();

            while (salaAbierta)
            {
                try
                {
                    var clienteNuevo = Servidor.AcceptTcpClient();
                    Thread.Sleep(100);
                    var stream = clienteNuevo.GetStream();

                    byte[] buffer = new byte[clienteNuevo.Available];
                    stream.ReadExactly(buffer, 0, buffer.Length);

                    var json = Encoding.UTF8.GetString(buffer);

                    var conectarcomando = JsonSerializer.Deserialize<UnirseSalaComando>(json);

                    if (conectarcomando != null)
                    {
                        var nombre = conectarcomando.NombreJugador;

                        var cliente = new Jugador
                        {
                            Conexion = clienteNuevo,
                            Nombre = nombre ?? ""
                        };

                        JugadorCliente = cliente;


                        if (JugadorServer.Nombre == nombre)
                            {
                                //si ya hay uno con ese nombre lo rechazo

                                var rechazarCommand = new JugadorRechazadoComando
                                {
                                    Comando = Orden.JugadorRechazado
                                };

                                EnviarComando(rechazarCommand);

                                clienteNuevo.Close();
                            }
                            else
                            {
                                
                                ClienteConectado?.Invoke(JugadorCliente.Nombre);


                                var bienvenido = new JugadorConectadoComando
                                {
                                    Comando = Orden.JugadorConectado,
                                    NombreServidor=JugadorServer.Nombre
                                };

                                EnviarComando(bienvenido);

                                Thread hiloEscuchar = new Thread(EscucharCliente);
                                hiloEscuchar.IsBackground = true;
                                hiloEscuchar.Start(clienteNuevo);

                                salaAbierta = false;
                                Servidor.Stop();
                        }
                        }
                    
                }
                catch
                {

                }
            }
        }

        public void PokemonServidorSeleccionado(string pokemon)
        {
            if (!PokemonValidos.Contains(pokemon))
                return;

            if (JugadorServer != null)
            {
                JugadorServer.Pokemon = pokemon;
            }

            if (JugadorCliente != null && JugadorCliente.Conexion != null)
            {
                var comando = new SeleccionarPokemonComando
                {
                    Comando = Orden.SeleccionarPokemon
                };

                EnviarComando(comando);
            }
        }
        private void EscucharCliente() 
        {

            if (JugadorCliente != null && JugadorCliente.Conexion!=null)
            {
                var client = JugadorCliente.Conexion;
                try
                {
                    while (client.Connected)
                    {
                        if (client.Available > 0 /*&& !client.Client.Poll(1000, SelectMode.SelectRead)*/)
                        {
                            var stream = client.GetStream();
                            var buffer = new byte[client.Available];
                            stream.ReadExactly(buffer, 0, buffer.Length);
                            var json = Encoding.UTF8.GetString(buffer);

                            var comando = JsonSerializer.Deserialize<Comandos>(json);
                            if (comando != null)
                            {
                                switch (comando.Comando)
                                {
                                    case Orden.PokemonSeleccionado:
                                        var pokemonSeleccionado = JsonSerializer.Deserialize<PokemonSeleccionadoComando>(json);
                                        if (pokemonSeleccionado != null)
                                        {
                                            if (PokemonValidos.Contains(pokemonSeleccionado.Pokemon))
                                            {
                                                JugadorCliente.Pokemon = pokemonSeleccionado.Pokemon;
                                            }

                                            if (JugadorCliente.Pokemon!=null && JugadorServer != null && JugadorServer.Pokemon != null )
                                            {
                                                //si ambos escogieron pokemon, iniciar la partida
                                                Juego = new()
                                                {
                                                    JugadorTurno = JugadorServer,
                                                    Ronda = 1,
                                                    Historial = new() { "----Partida Iniciada----"}
                                                    
                                                };

                                                var comandoIniciar = new IniciarPartidaComando
                                                {
                                                    Comando = Orden.IniciarPartida,
                                                    NombreRival = JugadorServer.Nombre
                                                };


                                                EnviarComando(comandoIniciar);

                                                PartidaIniciada?.Invoke();

                                                //falta completar el service de cliente, la navegacion y los viewmodels. cambiar vistas a usercontrols. bu
                                            }
                                        }
                                        break;


                                    default: break;
                                }

                            }
                        }
                    }
                }
                catch 
                { 

                }
                finally
                {
                    if (JugadorCliente.Conexion != null)
                    {
                        Juego = null;
                        JugadorServer = null;
                        JugadorCliente= null;
                        ClienteDesconectado?.Invoke();

                    }
                }
                
            }
        }


        private void EnviarComando(object comando)
        {
            if (JugadorCliente != null && JugadorCliente.Conexion != null)
            {
                var stream = JugadorCliente.Conexion.GetStream();
                var json = JsonSerializer.Serialize(comando);

                var buffer = Encoding.UTF8.GetBytes(json);

                stream.Write(buffer, 0, buffer.Length);
            }
                
        }


    }
}
