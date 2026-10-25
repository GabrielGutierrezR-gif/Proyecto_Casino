using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsSesion
    {
        public string Token { get; set; } = "";
        public clsPerfilUsuario Usuario { get; set; } = new();
        public bool PrimerIngreso { get; set; }        // la app muestra la bienvenida y el regalo de fichas
        public int FichasBienvenida { get; set; }
    }
}
