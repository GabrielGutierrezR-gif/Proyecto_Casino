using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsPerfilUsuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string Correo { get; set; } = "";
        public decimal SaldoFichas { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Rol { get; set; } = "";
    }

}
