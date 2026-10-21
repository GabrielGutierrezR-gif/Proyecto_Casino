using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.EntidadesTipadas
{
    public class TCompra
    {
        public int IdCompra { get; set; }
        public int IdUsuario { get; set; }
        public int IdPaquete { get; set; }
        public int IdMetodoPago { get; set; }
        public DateTime FechaCompra { get; set; }
    }
}
