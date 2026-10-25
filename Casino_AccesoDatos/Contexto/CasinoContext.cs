using System;
using System.Collections.Generic;
using Casino_Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Casino_AccesoDatos.Contexto;

public partial class CasinoContext : DbContext
{
    public CasinoContext()
    {
    }

    public CasinoContext(DbContextOptions<CasinoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ApuestasBlackjack> ApuestasBlackjacks { get; set; }

    public virtual DbSet<ApuestasRuletum> ApuestasRuleta { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<Configuracion> Configuracions { get; set; }

    public virtual DbSet<JugadoresBlackjack> JugadoresBlackjacks { get; set; }

    public virtual DbSet<MetodosPago> MetodosPagos { get; set; }

    public virtual DbSet<PaquetesFicha> PaquetesFichas { get; set; }

    public virtual DbSet<PartidasRuletum> PartidasRuleta { get; set; }

    public virtual DbSet<RondasBlackjack> RondasBlackjacks { get; set; }

    public virtual DbSet<SalasBlackjack> SalasBlackjacks { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

   
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApuestasBlackjack>(entity =>
        {
            entity.HasKey(e => e.IdApuesta).HasName("PK__Apuestas__2F50138677A47A8A");

            entity.ToTable("ApuestasBlackjack");

            entity.Property(e => e.Accion)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Apuesta).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Ganancia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Resultado)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.IdRondaNavigation).WithMany(p => p.ApuestasBlackjacks)
                .HasForeignKey(d => d.IdRonda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApuestasBJ_Rondas");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.ApuestasBlackjacks)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApuestasBJ_Usuarios");
        });

        modelBuilder.Entity<ApuestasRuletum>(entity =>
        {
            entity.HasKey(e => e.IdApuestaRuleta).HasName("PK__Apuestas__62412901F88C6720");

            entity.Property(e => e.Ganancia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Resultado)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TipoApuesta)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ValorApuesta)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.IdPartidaNavigation).WithMany(p => p.ApuestasRuleta)
                .HasForeignKey(d => d.IdPartida)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApuestasRuleta_Partida");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.ApuestasRuleta)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApuestasRuleta_Usuario");
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.IdCompra).HasName("PK__Compras__0A5CDB5CEAFCEFF1");

            entity.Property(e => e.FechaCompra)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdMetodoPagoNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdMetodoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compras_MetodosPago");

            entity.HasOne(d => d.IdPaqueteNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdPaquete)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compras_Paquetes");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compras_Usuarios");
        });

        modelBuilder.Entity<Configuracion>(entity =>
        {
            entity.HasKey(e => e.Clave).HasName("PK__Configur__E8181E1060D29AA0");

            entity.ToTable("Configuracion");

            entity.Property(e => e.Clave)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Valor)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<JugadoresBlackjack>(entity =>
        {
            entity.HasKey(e => e.IdJugadorSala).HasName("PK__Jugadore__68A80648D97319E9");

            entity.ToTable("JugadoresBlackjack");

            entity.HasIndex(e => new { e.IdSala, e.IdUsuario }, "UQ_JugadorSala").IsUnique();

            entity.Property(e => e.FechaUnion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdSalaNavigation).WithMany(p => p.JugadoresBlackjacks)
                .HasForeignKey(d => d.IdSala)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Jugadores_Salas");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.JugadoresBlackjacks)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Jugadores_Usuarios");
        });

        modelBuilder.Entity<MetodosPago>(entity =>
        {
            entity.HasKey(e => e.IdMetodoPago).HasName("PK__MetodosP__6F49A9BEB5927324");

            entity.ToTable("MetodosPago");

            entity.Property(e => e.NombreTitular)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NumeroTarjeta)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TipoTarjeta)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_MetodosPago_Activo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.MetodosPagos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MetodosPago_Usuarios");
        });

        modelBuilder.Entity<PaquetesFicha>(entity =>
        {
            entity.HasKey(e => e.IdPaquete).HasName("PK__Paquetes__DE278F8BC846D7D4");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<PartidasRuletum>(entity =>
        {
            entity.HasKey(e => e.IdPartida).HasName("PK__Partidas__6ED660C711DE0080");

            entity.Property(e => e.ColorGanador)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.FechaPartida)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<RondasBlackjack>(entity =>
        {
            entity.HasKey(e => e.IdRonda).HasName("PK__RondasBl__D3E8E37EEFFA4993");

            entity.ToTable("RondasBlackjack");

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdSalaNavigation).WithMany(p => p.RondasBlackjacks)
                .HasForeignKey(d => d.IdSala)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rondas_Salas");
        });

        modelBuilder.Entity<SalasBlackjack>(entity =>
        {
            entity.HasKey(e => e.IdSala).HasName("PK__SalasBla__A04F9B3B76A502AC");

            entity.ToTable("SalasBlackjack");

            entity.HasIndex(e => e.CodigoSala, "UQ__SalasBla__44BC63888BBE9ABF").IsUnique();

            entity.Property(e => e.CodigoSala)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Esperando");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreSala)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TipoSala)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__5B65BF97D5EF77EC");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A1988951123").IsUnique();

            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Jugador", "DF_Usuarios_Rol");
            entity.Property(e => e.SaldoFichas).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.BienvenidaVista).HasDefaultValue(false, "DF_Usuarios_BienvenidaVista");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
