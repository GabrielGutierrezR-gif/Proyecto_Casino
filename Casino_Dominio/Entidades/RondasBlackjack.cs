using System;
using System.Collections.Generic;

namespace Casino_Dominio.Entidades;

public partial class RondasBlackjack
{
    public int IdRonda { get; set; }

    public int IdSala { get; set; }

    public int NumeroRonda { get; set; }

    public string Estado { get; set; } = null!;

    public int? ResultadoDealer { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public virtual ICollection<ApuestasBlackjack> ApuestasBlackjacks { get; set; } = new List<ApuestasBlackjack>();

    public virtual SalasBlackjack IdSalaNavigation { get; set; } = null!;
}
