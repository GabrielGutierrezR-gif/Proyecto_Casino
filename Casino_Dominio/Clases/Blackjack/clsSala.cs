using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsSala
    {
        public int IdSala { get; set; }
        public string CodigoSala { get; set; } = "";
        public string NombreSala { get; set; } = "";
        public string TipoSala { get; set; } = "";
        public string Estado { get; set; } = "";          // Esperando, Jugando, Cerrada
        public int CantidadJugadores { get; set; }
        public int MaxJugadores { get; set; }
        public List<string> Jugadores { get; set; } = new();   // nombres de los que están adentro
        public DateTime FechaCreacion { get; set; }
    }
}
