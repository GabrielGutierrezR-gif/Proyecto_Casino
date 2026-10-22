using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsSesion
    {
        public string Token { get; set; } = "";
        public clsPerfilUsuario Usuario { get; set; } = new();
    }
}
