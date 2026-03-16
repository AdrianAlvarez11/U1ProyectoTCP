using AdivinaQuienCliente.Models;
using AdivinaQuienCliente.Services;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AdivinaQuienCliente.Viewmodels
{
    public enum Vista
    {
        UnirseSala,
        ElegirPersonaje,
        EsperandoJugador,
        Juego,
        Resultados,
        JugadorDesconectado
    }

    public class ClienteViewmodel : INotifyPropertyChanged
    {
        public ObservableCollection<Pokemon> Pokemons { get; set; } = new ObservableCollection<Pokemon>()
        {
            new Pokemon { Nombre = "Alakazam", Habilitado = true },
            new Pokemon { Nombre = "Blaziken", Habilitado = true },
            new Pokemon { Nombre = "Bulbasaur", Habilitado = true },
            new Pokemon { Nombre = "Charizard", Habilitado = true },
            new Pokemon { Nombre = "Ditto", Habilitado = true },
            new Pokemon { Nombre = "Dragonite", Habilitado = true },
            new Pokemon { Nombre = "Eevee", Habilitado = true },
            new Pokemon { Nombre = "Gardevoir", Habilitado = true },
            new Pokemon { Nombre = "Gengar", Habilitado = true },
            new Pokemon { Nombre = "Greninja", Habilitado = true },
            new Pokemon { Nombre = "Groudon", Habilitado = true },
            new Pokemon { Nombre = "Gyarados", Habilitado = true },
            new Pokemon { Nombre = "Jigglypuff", Habilitado = true },
            new Pokemon { Nombre = "Lapras", Habilitado = true },
            new Pokemon { Nombre = "Lucario", Habilitado = true },
            new Pokemon { Nombre = "Machamp", Habilitado = true },
            new Pokemon { Nombre = "Meowth", Habilitado = true },
            new Pokemon { Nombre = "Mewtwo", Habilitado = true },
            new Pokemon { Nombre = "Onix", Habilitado = true },
            new Pokemon { Nombre = "Pikachu", Habilitado = true },
            new Pokemon { Nombre = "Rayquaza", Habilitado = true },
            new Pokemon { Nombre = "Snorlax", Habilitado = true },
            new Pokemon { Nombre = "Squirtle", Habilitado = true },
            new Pokemon { Nombre = "Umbreon", Habilitado = true }
        };

        Dispatcher dispatcher;

        ClienteJuegoService service = new();

        public string? NombreCliente { get; set; }
        public string? NombreServidor { get; set; }
        public string? DireccionIP { get; set; } = "127.0.0.1";
        public string? Mensaje { get; set; } = "";
        public Pokemon? MiPokemon { get; set; }

        public EstadoJuego? Juego { get; set; }


        private Vista _vistaActual = Vista.UnirseSala;

        public Vista VistaActual
        {
            get => _vistaActual;
            set { _vistaActual = value; OnPropertyChanged(); }
        }

        public ICommand UnirseSalaCommand { get; set; }
        public ICommand ElegirPokemonCommand { get; set; }
        public ICommand EnviarPreguntaCommand { get; set; }
        public ICommand EnviarRespuestaCommand { get; set; }
        public ICommand DescartarPokemonCommand { get; set; }
        public ICommand ModoAdivinarCommand { get; set; }
        public ICommand AdivinarCommand { get; set; }
        public ICommand VolverInicioCommand { get; set; }


        public bool EsMiTurno => Juego?.JugadorTurno?.Nombre == NombreCliente;

        public bool EsperandoPregunta { get; set; } = false;
        public bool EsperandoRespuesta { get; set; } = false;
        public bool Adivinando { get; set; } = false;
        public Pokemon? PokemonRival { get; set; }

        public ClienteViewmodel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            UnirseSalaCommand = new RelayCommand(UnirseSala);
            ElegirPokemonCommand= new RelayCommand<string> (ElegirPokemon);
            EnviarRespuestaCommand = new RelayCommand<string?>(EnviarRespuesta);
            EnviarPreguntaCommand = new RelayCommand(EnviarPregunta);
            DescartarPokemonCommand = new RelayCommand<string?>(DescartarPokemon);
            ModoAdivinarCommand = new RelayCommand(EntrarAdivinando);
            AdivinarCommand = new RelayCommand<string?>(Adivinar);
            VolverInicioCommand = new RelayCommand(VolverInicio);


            service.ClienteAceptado += Service_ClienteAceptado;
            service.ClienteRechazado += Service_ClienteRechazado;
            service.ServerEscogePokemon += Service_ServerEscogePokemon;
            service.PartidaIniciada += Service_PartidaIniciada;
            service.PreguntaRecibida += Service_PreguntaRecibida;
            service.TurnoCambiado += Service_TurnoCambiado;
            service.Gano += Service_Gano;
            service.Perdio += Service_Perdio;
            service.ServidorDesconectado += Service_ServidorDesconectado;
            service.ConexionFallida += Service_ConexionFallida;

        }

        private void Service_ConexionFallida()
        {
            Mensaje = "No se encontró una sala abierta con esa dirección IP";
            OnPropertyChanged(nameof(Mensaje));
        }

        private void VolverInicio()
        {
            VistaActual = Vista.UnirseSala;
            Juego = null;
            PokemonRival = null;
            Mensaje = "";
            NombreCliente = null;
        }

        private void Service_ServidorDesconectado()
        {
            VistaActual = Vista.JugadorDesconectado;
        }

        private void Service_Perdio(string pokemonRival)
        {
            var pokemon = Pokemons.Where(x => x.Nombre == pokemonRival).First();
            Mensaje = $"¡Perdiste!";
            PokemonRival = pokemon;
            VistaActual = Vista.Resultados;
            OnPropertyChanged(nameof(PokemonRival));
            OnPropertyChanged(nameof(Mensaje));
        }

        private void Service_Gano(string pokemonRival)
        {
            var pokemon = Pokemons.Where(x => x.Nombre == pokemonRival).First();
            PokemonRival = pokemon;
            VistaActual = Vista.Resultados;
            Mensaje = $"¡Felicidades! Ganaste";
            OnPropertyChanged(nameof(PokemonRival));
            OnPropertyChanged(nameof(Mensaje));
        }

        private void Adivinar(string? pokemon)
        {
            if (pokemon != null)
            {
                service.AdivinarPokemon(pokemon);
                Adivinando = false;
                OnPropertyChanged(nameof(Adivinando));
            }
        }

        private void DescartarPokemon(string? pokemon)
        {
            var pokemonlista = Pokemons.Where(x => x.Nombre == pokemon).First();
            pokemonlista.Habilitado = !pokemonlista.Habilitado;
            OnPropertyChanged(nameof(Pokemons));
        }
        private void EntrarAdivinando()
        {
            Adivinando = !Adivinando;
            if (Adivinando)
            {
                Mensaje = "Estas en modo adivinar, haz click en el pokemon que crees que es el del oponente";
            }
            else
            {
                Mensaje = "Es tu turno, haz una pregunta";
            }

            OnPropertyChanged(nameof(Adivinando));
            OnPropertyChanged(nameof(Mensaje));
        }
        private void Service_TurnoCambiado(EstadoJuego obj)
        {
            dispatcher.BeginInvoke(() =>
            {
                Juego = obj;
                OnPropertyChanged(nameof(Juego));
                OnPropertyChanged(nameof(EsMiTurno));
                if (EsMiTurno)
                {
                    Mensaje = "Es tu turno, haz una pregunta";
                    EsperandoPregunta = false;
                    EsperandoRespuesta = false;
                }
                else
                {
                    Mensaje = $"Espera a que {NombreServidor} envíe su pregunta";
                    EsperandoPregunta = true;
                    EsperandoRespuesta = false;
                }
                OnPropertyChanged(nameof(Mensaje));
                OnPropertyChanged(nameof(EsperandoPregunta));
                OnPropertyChanged(nameof(EsperandoRespuesta));
            });
        }

        private void EnviarPregunta()
        {
            if (string.IsNullOrWhiteSpace(Juego.Pregunta) || Juego.Pregunta.Length < 3)
            {
                Mensaje = "Escriba una pregunta para enviar";
                OnPropertyChanged(nameof(Mensaje));
                return;
            }

            service.EnviarPregunta(Juego.Pregunta);
            EsperandoRespuesta = true;
            Juego = service.Juego; //para actualizar el historial
            Mensaje = "Pregunta enviada, espera la respuesta";
            OnPropertyChanged(nameof(EsperandoRespuesta));
            OnPropertyChanged(nameof(Juego));
        }

        private void EnviarRespuesta(string? respuesta)
        {
            if (respuesta != null)
            {
                service.EnviarRespuesta(respuesta);
                EsperandoRespuesta = false;
                OnPropertyChanged(nameof(EsperandoRespuesta));
            }
        }

        private void Service_PreguntaRecibida(string obj)
        {
            dispatcher.BeginInvoke(() =>
            {
                Juego.Pregunta = obj;
                EsperandoPregunta = false;
                OnPropertyChanged(nameof(Juego));
                OnPropertyChanged(nameof(EsperandoPregunta));
                Mensaje = "Responde sinceramente";
                OnPropertyChanged(nameof(Mensaje));
            });
        }

        private void Service_PartidaIniciada(EstadoJuego obj)
        {
            dispatcher.BeginInvoke(() =>
            {
                Juego = obj;
                VistaActual = Vista.Juego;
                EsperandoPregunta = true;
                Mensaje = $"Espera a que {NombreServidor} envíe su pregunta";
                OnPropertyChanged(nameof(Juego));
                OnPropertyChanged(nameof(EsperandoPregunta));
                OnPropertyChanged(nameof(Mensaje));
            });
        }

        private void Service_ServerEscogePokemon()
        {
            Mensaje = "";
            OnPropertyChanged(Mensaje);
            VistaActual= Vista.ElegirPersonaje;
        }

        private void Service_ClienteRechazado()
        {
            dispatcher.BeginInvoke(() =>
            {
                Mensaje = "El nombre seleccionado ya esta siendo utilizado";
                OnPropertyChanged(nameof(Mensaje));
            });
        }

        private void Service_ClienteAceptado()
        {
            dispatcher.BeginInvoke(() =>
            {
                NombreServidor = service.Servidor.Nombre;
                VistaActual = Vista.EsperandoJugador;
                Pokemons.ToList().ForEach(x => x.Habilitado = true);
                OnPropertyChanged(nameof(NombreServidor));
                Mensaje = $"Espera mientras {NombreServidor} escoge su pokemon";
                OnPropertyChanged(nameof(Mensaje));
            });
        }

        private void UnirseSala()
        {
            if (string.IsNullOrWhiteSpace(NombreCliente) || NombreCliente.Length<3)
            {
                Mensaje = "Elija un nombre de al menos tres caracteres";
                OnPropertyChanged(nameof(Mensaje));
                return;
            }

            if (!IPAddress.TryParse(DireccionIP, out IPAddress ip))
            {
                Mensaje = "Escriba una direccion ip correcta";
                OnPropertyChanged(nameof(Mensaje));
                return;
            }

            service.Conectar(ip, NombreCliente);
        }

        public void ElegirPokemon(string? pokemon)
        {
            if (pokemon != null)
            {
                MiPokemon = Pokemons.Where(x => x.Nombre == pokemon).First();
                service.SeleccionarPokemonCliente(pokemon);
                OnPropertyChanged(nameof(MiPokemon));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
