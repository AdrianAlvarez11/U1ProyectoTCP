using AdivinaQuienServidor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
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

        Dispatcher dispatcher;

        private Vista _vistaActual = Vista.AbrirSala;

        public Vista VistaActual
        {
            get => _vistaActual;
            set { _vistaActual = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
