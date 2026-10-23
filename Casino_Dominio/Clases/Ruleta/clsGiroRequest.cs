using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsGiroRequest
    {
        public List<clsApuestaRuleta> Apuestas { get; set; } = new();
    }
}
