using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsMetodoPagoRequest
    {
        public string TipoTarjeta { get; set; } = "";    // "Credito" o "Debito"
        public string NumeroTarjeta { get; set; } = "";  // número completo (solo se guardan los últimos 4)
        public string NombreTitular { get; set; } = "";
    }

}
