using System;
using System.Collections.Generic;

namespace Casino_Dominio.Entidades;

public partial class MetodosPago
{
    public int IdMetodoPago { get; set; }

    public int IdUsuario { get; set; }

    public string TipoTarjeta { get; set; } = null!;

    public string NumeroTarjeta { get; set; } = null!;

    public string NombreTitular { get; set; } = null!;

    public bool Activo { get; set; } = true;

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
