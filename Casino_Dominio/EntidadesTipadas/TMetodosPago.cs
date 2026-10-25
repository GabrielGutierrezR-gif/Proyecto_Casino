using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.EntidadesTipadas
{
    public class TMetodosPago
    {
        public int IdMetodoPago { get; set; }
        public int IdUsuario { get; set; }
        public string TipoTarjeta { get; set; } = null!;
        public string NumeroTarjeta { get; set; } = null!;
        public string NombreTitular { get; set; } = null!;
        public bool Activo { get; set; } = true;
    }
}
