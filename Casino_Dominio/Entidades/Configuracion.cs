using System;
using System.Collections.Generic;

namespace Casino_Dominio.Entidades;

public partial class Configuracion
{
    public string Clave { get; set; } = null!;

    public string Valor { get; set; } = null!;

    public string? Descripcion { get; set; }
}
