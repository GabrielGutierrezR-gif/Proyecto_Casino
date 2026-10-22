using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_LogicaNegocios.Blackjack
{
    // Un jugador sentado en la mesa (solo vive en memoria)
    public class JugadorMesa
    {
        public int IdUsuario { get; }
        public string Nombre { get; }
        public bool Conectado { get; set; }
        public bool SalirAlTerminar { get; set; }      // abandonó a mitad de ronda
        public List<Mano> Manos { get; } = new();      // vacía = no ha apostado

        public JugadorMesa(int idUsuario, string nombre)
        {
            IdUsuario = idUsuario;
            Nombre = nombre;
        }
    }
}