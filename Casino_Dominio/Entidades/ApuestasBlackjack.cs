using System;
using System.Collections.Generic;

namespace Casino_Dominio.Entidades;

public partial class ApuestasBlackjack
{
    public int IdApuesta { get; set; }

    public int IdRonda { get; set; }

    public int IdUsuario { get; set; }

    public decimal Apuesta { get; set; }

    public string? Accion { get; set; }

    public int? Puntuacion { get; set; }

    public string? Resultado { get; set; }

    public decimal Ganancia { get; set; }

    public virtual RondasBlackjack IdRondaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
