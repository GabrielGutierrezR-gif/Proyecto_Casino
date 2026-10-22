using System;
using System.Collections.Generic;

namespace Casino_Dominio.Entidades;

public partial class Compra
{
    public int IdCompra { get; set; }

    public int IdUsuario { get; set; }

    public int IdPaquete { get; set; }

    public int IdMetodoPago { get; set; }

    public DateTime FechaCompra { get; set; }

    public virtual MetodosPago IdMetodoPagoNavigation { get; set; } = null!;

    public virtual PaquetesFicha IdPaqueteNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
