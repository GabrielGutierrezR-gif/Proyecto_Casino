using System;
using System.Collections.Generic;

namespace Casino_Dominio.Entidades;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Password { get; set; } = null!;

    public decimal SaldoFichas { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string Rol { get; set; } = null!;

    public virtual ICollection<ApuestasBlackjack> ApuestasBlackjacks { get; set; } = new List<ApuestasBlackjack>();

    public virtual ICollection<ApuestasRuletum> ApuestasRuleta { get; set; } = new List<ApuestasRuletum>();

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    public virtual ICollection<JugadoresBlackjack> JugadoresBlackjacks { get; set; } = new List<JugadoresBlackjack>();

    public virtual ICollection<MetodosPago> MetodosPagos { get; set; } = new List<MetodosPago>();
}
