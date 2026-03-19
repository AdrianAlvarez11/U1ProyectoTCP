using AdivinaQuienServidor.Models;
using AdivinaQuienServidor.Services;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
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
        public ICommand DescartarPokemonCommand { get; set; }
        public ICommand ModoAdivinarCommand { get; set; }
        public ICommand AdivinarCommand { get; set; }
        public ICommand VolverAJugarCommand { get; set; }
        public ICommand VolverInicioCommand { get; set; }
        public ICommand SalirCommand { get; set; }


        public EstadoJuego? Juego { get; set; }

        public bool EsMiTurno => Juego?.JugadorTurno?.Nombre == NombreServidor;

        public bool EsperandoPregunta { get; set; } = false;
        public bool EsperandoRespuesta { get; set; } = false;

        public bool Adivinando {  get; set; } = false;

        public Pokemon? PokemonRival {  get; set; }


        public ServidorViewmodel()
        {
            hiloUI = Dispatcher.CurrentDispatcher;
            AbrirSalaCommand = new RelayCommand(AbrirSala);
            EnviarPreguntaCommand = new RelayCommand(EnviarPregunta);
            ElegirPokemonCommand = new RelayCommand<string?>(ElegirPokemon);
            EnviarRespuestaCommand = new RelayCommand<string?>(EnviarRespuesta);
            DescartarPokemonCommand = new RelayCommand<string?>(DescartarPokemon);
            ModoAdivinarCommand = new RelayCommand(EntrarAdivinando);
            AdivinarCommand = new RelayCommand<string?>(Adivinar);
            VolverAJugarCommand = new RelayCommand(VolverAJugar);
            VolverInicioCommand = new RelayCommand(VolverInicio);
            SalirCommand = new RelayCommand(SalirDelJuego);


            service.ClienteConectado += Service_ClienteConectado;
            service.PartidaIniciada += Service_PartidaIniciada1;
            service.PreguntaEnviada += Service_PreguntaEnviada;
            service.PreguntaRecibida += Service_PreguntaRecibida;
            service.TurnoCambiado += Service_TurnoCambiado;
            service.Gano += Service_Gano;
            service.Perdio += Service_Perdio;
            service.ClienteDesconectado += Service_ClienteDesconectado;
        }

        private void SalirDelJuego()
        {
            Application.Current.Shutdown();
        }

        private void Service_ClienteDesconectado()
        {
            VistaActual = Vista.JugadorDesconectado;

        }

        private void VolverInicio()
        {
            VistaActual= Vista.AbrirSala;
            Juego = null;
            PokemonRival = null;
            Mensaje = "";
            NombreServidor = null;
            NombreCliente = null;
            OnPropertyChanged(nameof(Mensaje));
            OnPropertyChanged(nameof(Juego));
            OnPropertyChanged(nameof(NombreCliente));
            OnPropertyChanged(nameof(NombreServidor));
            OnPropertyChanged(nameof(PokemonRival));

        }

        private void VolverAJugar()
        {
            service.VolverAJugar();
            Juego = null;
            PokemonRival = null;
            Mensaje = "";
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

        private void EntrarAdivinando()
        {
            Adivinando = !Adivinando;
                if(Adivinando)
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

        private void DescartarPokemon(string? pokemon)
        {
            var pokemonlista = Pokemons.Where(x => x.Nombre == pokemon).First();
            pokemonlista.Habilitado = !pokemonlista.Habilitado;
            OnPropertyChanged(nameof(Pokemons));
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
                Pokemons.ToList().ForEach(x => x.Habilitado = true);

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
