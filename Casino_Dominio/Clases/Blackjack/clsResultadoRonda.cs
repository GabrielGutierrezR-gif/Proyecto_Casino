using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsResultadoRonda
    {
        public int NumeroRonda { get; set; }
        public List<clsCarta> CartasDealer { get; set; } = new();
        public int PuntosDealer { get; set; }
        public List<clsResultadoMano> Resultados { get; set; } = new();
    }
}
