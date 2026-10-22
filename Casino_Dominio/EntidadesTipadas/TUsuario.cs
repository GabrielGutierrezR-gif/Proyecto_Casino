using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.EntidadesTipadas
{
    public class TUsuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Password { get; set; } = null!;
        public decimal SaldoFichas { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Rol { get; set; } = null!;
    }
}
