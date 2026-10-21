using System;
using System.Collections.Generic;

namespace Casino_Dominio.Entidades;

public partial class ApuestasRuletum
{
    public int IdApuestaRuleta { get; set; }

    public int IdPartida { get; set; }

    public int IdUsuario { get; set; }

    public string TipoApuesta { get; set; } = null!;

    public string ValorApuesta { get; set; } = null!;

    public decimal Monto { get; set; }

    public string? Resultado { get; set; }

    public decimal Ganancia { get; set; }

    public virtual PartidasRuletum IdPartidaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
