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

        Dispatcher dispatcher;

        ClienteJuegoService service = new();

        public string? NombreCliente { get; set; }
        public string? NombreServidor { get; set; }
        public string? DireccionIP { get; set; } = "127.0.0.1";
        public string? Mensaje { get; set; } = "";

        private Vista _vistaActual = Vista.UnirseSala;

        public Vista VistaActual
        {
            get => _vistaActual;
            set { _vistaActual = value; OnPropertyChanged(); }
        }

        public ICommand UnirseSalaCommand { get; set; }
        public ClienteViewmodel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            UnirseSalaCommand = new RelayCommand(UnirseSala);

            service.ClienteAceptado += Service_ClienteAceptado;
            service.ClienteRechazado += Service_ClienteRechazado;
            service.ServerEscogePokemon += Service_ServerEscogePokemon;
            
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
                OnPropertyChanged(nameof(NombreServidor));
                Mensaje = $"Espera mientras {NombreServidor} escoje su pokemon";
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
