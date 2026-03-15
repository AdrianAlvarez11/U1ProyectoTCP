using AdivinaQuienServidor.Models;
using AdivinaQuienServidor.Services;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace AdivinaQuienServidor.Viewmodels
{
    public enum Vista
    {
        AbrirSala,
        ElegirPersonaje,
        EsperandoJugador,
        Juego,
        Resultados,
        JugadorDesconectado
    }
    public class ServidorViewmodel : INotifyPropertyChanged
    {
        public ObservableCollection<Pokemon> Pokemons { get; set; } = new ObservableCollection<Pokemon>()
        {
            new Pokemon { Nombre = "Alakazam" },
            new Pokemon { Nombre = "Blaziken" },
            new Pokemon { Nombre = "Bulbasaur" },
            new Pokemon { Nombre = "Charizard" },
            new Pokemon { Nombre = "Ditto" },
            new Pokemon { Nombre = "Dragonite" },
            new Pokemon { Nombre = "Eevee" },
            new Pokemon { Nombre = "Gardevoir" },
            new Pokemon { Nombre = "Gengar" },
            new Pokemon { Nombre = "Greninja" },
            new Pokemon { Nombre = "Groudon" },
            new Pokemon { Nombre = "Gyarados" },
            new Pokemon { Nombre = "Jigglypuff" },
            new Pokemon { Nombre = "Lapras" },
            new Pokemon { Nombre = "Lucario" },
            new Pokemon { Nombre = "Machamp" },
            new Pokemon { Nombre = "Meowth" },
            new Pokemon { Nombre = "Mewtwo" },
            new Pokemon { Nombre = "Onix" },
            new Pokemon { Nombre = "Pikachu" },
            new Pokemon { Nombre = "Rayquaza" },
            new Pokemon { Nombre = "Snorlax" },
            new Pokemon { Nombre = "Squirtle" },
            new Pokemon { Nombre = "Umbreon" }
        };

        Dispatcher hiloUI;

        ServidorJuegoService service = new();

        private Vista _vistaActual = Vista.AbrirSala;
        private string? nombreServidor;

        public Vista VistaActual
        {
            get => _vistaActual;
            set { _vistaActual = value; OnPropertyChanged(); }
        }
        public string? NombreServidor { get => nombreServidor; set { nombreServidor = value; OnPropertyChanged(); } }
        public string? NombreCliente { get; set; }

        public string Mensaje { get; set; } = "";

        public Pokemon? MiPokemon { get; set; }

        public ICommand AbrirSalaCommand { get; set; }
        public ICommand ElegirPokemonCommand { get; set; }
        public ICommand EnviarPreguntaCommand { get; set; }
        public ICommand EnviarRespuestaCommand { get; set; }

        public EstadoJuego? Juego { get; set; }

        public bool EsMiTurno => Juego?.JugadorTurno?.Nombre == NombreServidor;

        public bool EsperandoPregunta { get; set; } = false;
        public bool EsperandoRespuesta { get; set; } = false;


        public ServidorViewmodel()
        {
            hiloUI = Dispatcher.CurrentDispatcher;
            AbrirSalaCommand = new RelayCommand(AbrirSala);
            EnviarPreguntaCommand = new RelayCommand(EnviarPregunta);
            ElegirPokemonCommand = new RelayCommand<string?>(ElegirPokemon);
            EnviarRespuestaCommand = new RelayCommand<string?>(EnviarRespuesta);
            service.ClienteConectado += Service_ClienteConectado;
            service.PartidaIniciada += Service_PartidaIniciada1;
            service.PreguntaEnviada += Service_PreguntaEnviada;
            service.PreguntaRecibida += Service_PreguntaRecibida;
            service.TurnoCambiado += Service_TurnoCambiado;
        }

        private void Service_PreguntaRecibida(string obj)
        {
            hiloUI.BeginInvoke(() =>
            {
                Juego.Pregunta = obj;
                EsperandoPregunta = false;
                OnPropertyChanged(nameof(Juego));
                OnPropertyChanged(nameof(EsperandoPregunta));
                Mensaje = "Responde sinceramente";
                OnPropertyChanged(nameof(Mensaje));
            });

        }

        private void Service_TurnoCambiado(EstadoJuego obj)
        {
            hiloUI.BeginInvoke(() =>
            {
                Juego = obj;
                OnPropertyChanged(nameof(Juego));
                OnPropertyChanged(nameof(EsMiTurno));
                if(EsMiTurno)
                {
                    Mensaje = "Es tu turno, haz una pregunta";
                    EsperandoPregunta = false;
                    EsperandoRespuesta = false;
                }
                else
                {
                    Mensaje = $"Espera a que {NombreCliente} envíe su pregunta";
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
            if(string.IsNullOrWhiteSpace(Juego.Pregunta) || Juego.Pregunta.Length<3)
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

            if(respuesta != null)
            {
                service.EnviarRespuesta(respuesta);
                EsperandoRespuesta = false;
                OnPropertyChanged(nameof(EsperandoRespuesta));
            }
            
        }

        private void Service_PreguntaEnviada()
        {
            hiloUI.BeginInvoke(() =>
            {
                EsperandoRespuesta = true;
                Juego = service.Juego;
                OnPropertyChanged(nameof(EsperandoRespuesta));
                OnPropertyChanged(nameof(Juego));
            });
        }

        private void Service_PartidaIniciada1(EstadoJuego obj)
        {
            hiloUI.BeginInvoke(() =>
            {
                Juego = obj;
                VistaActual = Vista.Juego;
                Mensaje = "Es tu turno, haz una pregunta";
                OnPropertyChanged(nameof(Juego));
                OnPropertyChanged(nameof(Mensaje));
            });
        }



        private void Service_ClienteConectado(string obj)
        {
            hiloUI.BeginInvoke(() =>
            {
                NombreCliente = obj;
                Mensaje = "Se unio el jugador: " + obj;
                VistaActual = Vista.ElegirPersonaje;

                OnPropertyChanged(nameof(Mensaje));
            });
            
            
        }

        public void ElegirPokemon(string? pokemon)
        {
            if (pokemon != null)
            {
                MiPokemon = Pokemons.Where(x => x.Nombre == pokemon).First();
                service.PokemonServidorSeleccionado(pokemon);
                VistaActual = Vista.EsperandoJugador;
                Mensaje = $"Espera mientras {NombreCliente} escoge su pokemon";
                OnPropertyChanged(nameof(Mensaje));
                OnPropertyChanged(nameof(MiPokemon));
            }
        }
        private void AbrirSala()
        {
            if(!string.IsNullOrWhiteSpace(NombreServidor) && NombreServidor.Length >= 3)
            {
                service.AbrirSala(NombreServidor);
                VistaActual= Vista.EsperandoJugador;
                Mensaje = "";
            }
            else
            {
                Mensaje = "Escoja un nombre de al menos 3 caracteres";   
            }
            OnPropertyChanged(nameof(Mensaje));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
