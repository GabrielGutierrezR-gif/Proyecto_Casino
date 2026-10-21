using System;
using System.Collections.Generic;

namespace Casino_Dominio.Entidades;

public partial class PartidasRuletum
{
    public int IdPartida { get; set; }

    public int NumeroGanador { get; set; }

    public string ColorGanador { get; set; } = null!;

    public DateTime FechaPartida { get; set; }

    public virtual ICollection<ApuestasRuletum> ApuestasRuleta { get; set; } = new List<ApuestasRuletum>();
}
