using System;
using System.Collections.Generic;

namespace Casino_Dominio.Entidades;

public partial class JugadoresBlackjack
{
    public int IdJugadorSala { get; set; }

    public int IdSala { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaUnion { get; set; }

    public virtual SalasBlackjack IdSalaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
