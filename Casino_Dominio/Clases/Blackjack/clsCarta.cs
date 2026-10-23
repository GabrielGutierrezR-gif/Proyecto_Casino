using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public  class clsCarta
    {
        public string Valor { get; set; } = "";   // A, 2-10, J, Q, K  (vacío si está oculta)
        public string Palo { get; set; } = "";    // Corazones, Diamantes, Treboles, Picas
        public bool Oculta { get; set; }          // la segunda carta del dealer va boca abajo
    }
}
