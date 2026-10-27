using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsEstadoMesa
    {
        public int IdSala { get; set; }
        public string CodigoSala { get; set; } = "";
        public string NombreSala { get; set; } = "";
        public string Fase { get; set; } = "";               // Apuestas o Jugando
        public int NumeroRonda { get; set; }
        public List<clsCarta> CartasDealer { get; set; } = new();
        public int? PuntosDealer { get; set; }               // solo cuenta las cartas visibles
        public List<clsJugadorMesa> Jugadores { get; set; } = new();
        public int? TurnoIdUsuario { get; set; }             // a quién le toca (null si nadie)
        public DateTime? FinTurnoUtc { get; set; }           // para mostrar el contador de 30 s
        public DateTime? FinApuestasUtc { get; set; }        // para mostrar "apuestas cierran en X s"
    }
}
