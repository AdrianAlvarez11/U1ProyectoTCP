using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AdivinaQuienCliente.Models
{
    public class Pokemon : INotifyPropertyChanged
    {
        public string Nombre { get; set; } = null!;
        public string Imagen => $"/Resources/Images/{Nombre}.png";
        private bool _habilitado;

        public bool Habilitado
        {
            get => _habilitado;
            set
            {
                if (_habilitado != value)
                {
                    _habilitado = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Habilitado)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
