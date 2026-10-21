using System;
using System.Collections.Generic;

namespace Casino_Dominio.Entidades;

public partial class SalasBlackjack
{
    public int IdSala { get; set; }

    public string CodigoSala { get; set; } = null!;

    public string NombreSala { get; set; } = null!;

    public string TipoSala { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<JugadoresBlackjack> JugadoresBlackjacks { get; set; } = new List<JugadoresBlackjack>();

    public virtual ICollection<RondasBlackjack> RondasBlackjacks { get; set; } = new List<RondasBlackjack>();
}
