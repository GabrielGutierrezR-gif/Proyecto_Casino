using System;
using System.Collections.Generic;

namespace Casino_Dominio.Entidades;

public partial class PaquetesFicha
{
    public int IdPaquete { get; set; }

    public string Nombre { get; set; } = null!;

    public int CantidadFichas { get; set; }

    public decimal Precio { get; set; }

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
