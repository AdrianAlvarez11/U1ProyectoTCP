using AdivinaQuienServidor.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;

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
        public event Action<EstadoJuego>? PartidaIniciada;
        public event Action? ClienteDesconectado;
        public event Action? PreguntaEnviada;
        public event Action? RespuestaEnviada;
        public event Action<string>? PreguntaRecibida;
        public event Action<EstadoJuego>? TurnoCambiado;
        public event Action<string>? Gano;
        public event Action<string>? Perdio;

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

            byte[] buffer = new byte[1024];
            while (salaAbierta)
            {
                try
                {
                    var clienteNuevo = Servidor.AcceptTcpClient();
                    Thread.Sleep(100);
                    var stream = clienteNuevo.GetStream();

                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    var json = Encoding.UTF8.GetString(buffer, 0, bytes);


                    var conectarcomando = JsonSerializer.Deserialize<UnirseSalaComando>(json);

                    if (conectarcomando != null)
                    {
                        var nombre = conectarcomando.NombreJugador;

                        var cliente = new Jugador
                        {
                            Conexion = clienteNuevo,
                            Nombre = nombre ?? ""
                        };


                        if (JugadorServer.Nombre == nombre)
                            {
                                //si ya hay uno con ese nombre lo rechazo

                                var rechazarCommand = new JugadorRechazadoComando
                                {
                                    Comando = Orden.JugadorRechazado
                                };

                                JugadorCliente = cliente;
                                EnviarComando(rechazarCommand);
                                Thread.Sleep(500);
                                JugadorCliente = null;

                                clienteNuevo.Close();
                            }
                            else
                            {
                                JugadorCliente = cliente;
                            
                                var bienvenido = new JugadorConectadoComando
                                {
                                    Comando = Orden.JugadorConectado,
                                    NombreServidor=JugadorServer.Nombre
                                };

                                EnviarComando(bienvenido);
                                ClienteConectado?.Invoke(JugadorCliente.Nombre);

                                Thread hiloEscuchar = new Thread(EscucharCliente);
                                hiloEscuchar.IsBackground = true;
                                hiloEscuchar.Start();

                                salaAbierta = false;
                                Servidor.Stop();
                        }
                        }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        public void VolverAJugar() 
        {
            Juego = new();
             if (JugadorServer != null)
             {
                JugadorServer.Pokemon = null;
             }
             if (JugadorCliente != null)
             {
                JugadorCliente.Pokemon = null;
             }

            var bienvenido = new JugadorConectadoComando
            {
                Comando = Orden.JugadorConectado,
                NombreServidor = JugadorServer.Nombre
            };

            EnviarComando(bienvenido);
            ClienteConectado?.Invoke(JugadorCliente.Nombre);
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
                byte[] buffer = new byte[1024];
                try
                {
                    while (true)
                    {
                        var stream = client.GetStream();
                        int bytes = stream.Read(buffer, 0, buffer.Length);

                        if (bytes == 0)
                        {
                            // el cliente se desconectó, manda a finally y avisa
                            break;
                        }

                        var json = Encoding.UTF8.GetString(buffer, 0, bytes);

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
                                                    JugadorTurno= JugadorServer,
                                                    Ronda=1,
                                                    Historial = Juego.Historial
                                                };


                                                EnviarComando(comandoIniciar);

                                                PartidaIniciada?.Invoke(Juego);

                                            }
                                        }
                                        break;

                                    case Orden.Pregunta:
                                        var pregunta = JsonSerializer.Deserialize<PreguntaComando>(json);
                                        if (pregunta != null)
                                        {
                                            Juego.Pregunta = pregunta.Pregunta;

                                            Application.Current.Dispatcher.Invoke(() =>
                                            {
                                                Juego.Historial.Add($"{Juego.Ronda}. {JugadorCliente.Nombre}: {pregunta.Pregunta}");
                                            });
                                            PreguntaRecibida?.Invoke(pregunta.Pregunta);
                                        }
                                        break;

                                    case Orden.Respuesta:
                                        var respuesta = JsonSerializer.Deserialize<RespuestaComando>(json);
                                        if (respuesta != null)
                                        {

                                            Application.Current.Dispatcher.Invoke(() =>
                                            {
                                                Juego.Historial.Add($"{Juego.Ronda}. {JugadorCliente.Nombre}: {respuesta.Respuesta}");
                                            });
                                            //cambiar turno manda respuesta en historial 
                                            CambiarTurno();
                                        }
                                        break;

                                        case Orden.Adivinar:
                                            var adivinar = JsonSerializer.Deserialize<AdivinarComando>(json);
                                        if (adivinar != null)
                                        {
                                            if (adivinar.Pokemon == JugadorServer.Pokemon)
                                            {
                                                //gana el cliente, manda ganar
                                                var comandoGanar = new GanarComando
                                                {
                                                    Comando = Orden.Ganar,
                                                    PokemonRival = JugadorServer.Pokemon
                                                };
                                                EnviarComando(comandoGanar);
                                                Perdio?.Invoke(JugadorCliente.Pokemon);
                                            }
                                            else
                                            {
                                                //no adivina, manda intento de adivinar a cliente y cambia turno

                                                Application.Current.Dispatcher.Invoke(() =>
                                                {
                                                    Juego.Historial.Add($"{Juego.Ronda}. {JugadorCliente.Nombre} intentó adivinar: {adivinar.Pokemon} - Incorrecto");
                                                });
                                                CambiarTurno();
                                            }
                                        }
                                        break;

                                    default: break;
                                }

                            
                        }
                    }
             
 
                }
                catch (Exception ex)
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

        public void EnviarPregunta(string pregunta)
        {
            if (JugadorCliente != null && JugadorCliente.Conexion != null)
            {
                Juego.Pregunta = pregunta;
                Juego.Historial.Add($"{Juego.Ronda}. {JugadorServer.Nombre}: {pregunta}");
                var comando = new PreguntaComando
                {
                    Comando = Orden.Pregunta,
                    Pregunta = pregunta
                };
                EnviarComando(comando);
                PreguntaEnviada?.Invoke(); 
            }
        }

        public void EnviarRespuesta(string respuesta)
        {
            if (JugadorCliente != null && JugadorCliente.Conexion != null)
            {
                Juego.Historial.Add($"{Juego.Ronda}. {JugadorServer.Nombre}: {respuesta}");
                var comando = new RespuestaComando
                {
                    Comando = Orden.Respuesta,
                    Respuesta = respuesta
                };
                EnviarComando(comando);
                RespuestaEnviada?.Invoke(); 

                CambiarTurno();
            }
        }

        public void CambiarTurno()
        {
            if (JugadorServer != null && JugadorCliente != null)
            {
                Juego.JugadorTurno = Juego.JugadorTurno == JugadorServer ? JugadorCliente : JugadorServer;
                Juego.Ronda++;
                Juego.Pregunta = null;
                var comando = new CambiarTurnoComando
                {
                    Comando = Orden.CambiarTurno,
                    JugadorTurno = Juego.JugadorTurno.Nombre,
                    Historial = Juego.Historial,
                    Ronda=Juego.Ronda
                };
                EnviarComando(comando);
                TurnoCambiado?.Invoke(Juego);
            }
        }

        public void AdivinarPokemon(string pokemon)
        {
            if (JugadorCliente != null && JugadorCliente.Conexion != null)
            {
                if (!PokemonValidos.Contains(pokemon))
                    return;

                if(JugadorCliente.Pokemon != null && pokemon == JugadorCliente.Pokemon)
                {
                    //gana el servidor, manda perder a cliente
                    var comandoPerder = new PerderComando
                    {
                        Comando = Orden.Perder,
                        PokemonRival = JugadorServer.Pokemon
                    };
                    EnviarComando(comandoPerder);

                    Gano?.Invoke(pokemon);
                }
                else
                {
                    //no adivina, manda intento de adivinar a cliente y cambia turno
                    Juego.Historial.Add($"{Juego.Ronda}. {JugadorServer.Nombre} intentó adivinar:{pokemon} - Incorrecto");
                    CambiarTurno(); 
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
