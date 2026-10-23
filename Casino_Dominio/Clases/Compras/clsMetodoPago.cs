using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsMetodoPago
    {
        public int IdMetodoPago { get; set; }
        public string TipoTarjeta { get; set; } = "";
        public string NumeroEnmascarado { get; set; } = "";  // ej: "**** **** **** 1234"
        public string NombreTitular { get; set; } = "";
    }

}
