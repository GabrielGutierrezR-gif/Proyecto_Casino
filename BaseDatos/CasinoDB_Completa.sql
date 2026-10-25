-- ============================================================
-- CasinoDB completa: estructura + datos (generado 2026-10-25 17:05)
-- BORRA la base CasinoDB que exista y la crea de nuevo.
-- Ejecutar en SQL Server Management Studio (F5).
-- ============================================================

USE master;
GO

IF DB_ID('CasinoDB') IS NOT NULL
BEGIN
    ALTER DATABASE CasinoDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CasinoDB;
END
GO

CREATE DATABASE CasinoDB;
GO

USE CasinoDB;
GO

-- ===================== TABLAS =====================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApuestasBlackjack](
	[IdApuesta] [int] IDENTITY(1,1) NOT NULL,
	[IdRonda] [int] NOT NULL,
	[IdUsuario] [int] NOT NULL,
	[Apuesta] [decimal](10, 2) NOT NULL,
	[Accion] [varchar](20) NULL,
	[Puntuacion] [int] NULL,
	[Resultado] [varchar](20) NULL,
	[Ganancia] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdApuesta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[ApuestasBlackjack] ADD  DEFAULT ((0)) FOR [Ganancia]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApuestasRuleta](
	[IdApuestaRuleta] [int] IDENTITY(1,1) NOT NULL,
	[IdPartida] [int] NOT NULL,
	[IdUsuario] [int] NOT NULL,
	[TipoApuesta] [varchar](20) NOT NULL,
	[ValorApuesta] [varchar](20) NOT NULL,
	[Monto] [decimal](10, 2) NOT NULL,
	[Resultado] [varchar](20) NULL,
	[Ganancia] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdApuestaRuleta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[ApuestasRuleta] ADD  DEFAULT ((0)) FOR [Ganancia]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Compras](
	[IdCompra] [int] IDENTITY(1,1) NOT NULL,
	[IdUsuario] [int] NOT NULL,
	[IdPaquete] [int] NOT NULL,
	[IdMetodoPago] [int] NOT NULL,
	[FechaCompra] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdCompra] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Compras] ADD  DEFAULT (getdate()) FOR [FechaCompra]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Configuracion](
	[Clave] [varchar](50) NOT NULL,
	[Valor] [varchar](100) NOT NULL,
	[Descripcion] [varchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[Clave] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JugadoresBlackjack](
	[IdJugadorSala] [int] IDENTITY(1,1) NOT NULL,
	[IdSala] [int] NOT NULL,
	[IdUsuario] [int] NOT NULL,
	[FechaUnion] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdJugadorSala] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_JugadorSala] UNIQUE NONCLUSTERED 
(
	[IdSala] ASC,
	[IdUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[JugadoresBlackjack] ADD  DEFAULT (getdate()) FOR [FechaUnion]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MetodosPago](
	[IdMetodoPago] [int] IDENTITY(1,1) NOT NULL,
	[IdUsuario] [int] NOT NULL,
	[TipoTarjeta] [varchar](20) NOT NULL,
	[NumeroTarjeta] [varchar](20) NOT NULL,
	[NombreTitular] [varchar](100) NOT NULL,
	[Activo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdMetodoPago] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[MetodosPago] ADD  CONSTRAINT [DF_MetodosPago_Activo]  DEFAULT ((1)) FOR [Activo]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaquetesFichas](
	[IdPaquete] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
	[CantidadFichas] [int] NOT NULL,
	[Precio] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPaquete] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PartidasRuleta](
	[IdPartida] [int] IDENTITY(1,1) NOT NULL,
	[NumeroGanador] [int] NOT NULL,
	[ColorGanador] [varchar](10) NOT NULL,
	[FechaPartida] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPartida] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[PartidasRuleta] ADD  DEFAULT (getdate()) FOR [FechaPartida]
GO
ALTER TABLE [dbo].[PartidasRuleta]  WITH CHECK ADD  CONSTRAINT [CK_Ruleta_Numero] CHECK  (([NumeroGanador]>=(0) AND [NumeroGanador]<=(36)))
GO
ALTER TABLE [dbo].[PartidasRuleta] CHECK CONSTRAINT [CK_Ruleta_Numero]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RondasBlackjack](
	[IdRonda] [int] IDENTITY(1,1) NOT NULL,
	[IdSala] [int] NOT NULL,
	[NumeroRonda] [int] NOT NULL,
	[Estado] [varchar](20) NOT NULL,
	[ResultadoDealer] [int] NULL,
	[FechaInicio] [datetime] NOT NULL,
	[FechaFin] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdRonda] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[RondasBlackjack] ADD  DEFAULT ('Pendiente') FOR [Estado]
GO
ALTER TABLE [dbo].[RondasBlackjack] ADD  DEFAULT (getdate()) FOR [FechaInicio]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SalasBlackjack](
	[IdSala] [int] IDENTITY(1,1) NOT NULL,
	[CodigoSala] [varchar](10) NOT NULL,
	[NombreSala] [varchar](50) NOT NULL,
	[TipoSala] [varchar](10) NOT NULL,
	[Estado] [varchar](20) NOT NULL,
	[FechaCreacion] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdSala] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[CodigoSala] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[SalasBlackjack] ADD  DEFAULT ('Esperando') FOR [Estado]
GO
ALTER TABLE [dbo].[SalasBlackjack] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[SalasBlackjack]  WITH CHECK ADD  CONSTRAINT [CK_Salas_Tipo] CHECK  (([TipoSala]='Privada' OR [TipoSala]='Publica'))
GO
ALTER TABLE [dbo].[SalasBlackjack] CHECK CONSTRAINT [CK_Salas_Tipo]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuarios](
	[IdUsuario] [int] IDENTITY(1,1) NOT NULL,
	[NombreUsuario] [varchar](50) NOT NULL,
	[Correo] [varchar](100) NOT NULL,
	[Password] [varchar](255) NOT NULL,
	[SaldoFichas] [decimal](10, 2) NOT NULL,
	[FechaRegistro] [datetime] NOT NULL,
	[Rol] [varchar](20) NOT NULL,
	[BienvenidaVista] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Correo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING ON

GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Usuarios_Correo] ON [dbo].[Usuarios]
(
	[Correo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Usuarios] ADD  DEFAULT ((0)) FOR [SaldoFichas]
GO
ALTER TABLE [dbo].[Usuarios] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[Usuarios] ADD  CONSTRAINT [DF_Usuarios_Rol]  DEFAULT ('Jugador') FOR [Rol]
GO
ALTER TABLE [dbo].[Usuarios] ADD  CONSTRAINT [DF_Usuarios_BienvenidaVista]  DEFAULT ((0)) FOR [BienvenidaVista]
GO

-- ===================== DATOS =====================
SET IDENTITY_INSERT [dbo].[ApuestasBlackjack] ON 

INSERT [dbo].[ApuestasBlackjack] ([IdApuesta], [IdRonda], [IdUsuario], [Apuesta], [Accion], [Puntuacion], [Resultado], [Ganancia]) VALUES (1, 1, 4, CAST(100.00 AS Decimal(10, 2)), N'Plantarse', 17, N'Empate', CAST(0.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasBlackjack] ([IdApuesta], [IdRonda], [IdUsuario], [Apuesta], [Accion], [Puntuacion], [Resultado], [Ganancia]) VALUES (3, 2, 4, CAST(70.00 AS Decimal(10, 2)), N'Doblar', 17, N'Pierde', CAST(-70.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasBlackjack] ([IdApuesta], [IdRonda], [IdUsuario], [Apuesta], [Accion], [Puntuacion], [Resultado], [Ganancia]) VALUES (4, 3, 4, CAST(10.00 AS Decimal(10, 2)), N'Plantarse', 17, N'Gana', CAST(10.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasBlackjack] ([IdApuesta], [IdRonda], [IdUsuario], [Apuesta], [Accion], [Puntuacion], [Resultado], [Ganancia]) VALUES (5, 4, 4, CAST(50.00 AS Decimal(10, 2)), N'Pedir', 23, N'Pierde', CAST(-50.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasBlackjack] ([IdApuesta], [IdRonda], [IdUsuario], [Apuesta], [Accion], [Puntuacion], [Resultado], [Ganancia]) VALUES (6, 5, 4, CAST(100.00 AS Decimal(10, 2)), N'Pedir', 21, N'Gana', CAST(100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasBlackjack] ([IdApuesta], [IdRonda], [IdUsuario], [Apuesta], [Accion], [Puntuacion], [Resultado], [Ganancia]) VALUES (7, 6, 4, CAST(10.00 AS Decimal(10, 2)), N'Plantarse', 7, N'Gana', CAST(10.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasBlackjack] ([IdApuesta], [IdRonda], [IdUsuario], [Apuesta], [Accion], [Puntuacion], [Resultado], [Ganancia]) VALUES (8, 7, 7, CAST(100.00 AS Decimal(10, 2)), N'Plantarse', 19, N'Empate', CAST(0.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasBlackjack] ([IdApuesta], [IdRonda], [IdUsuario], [Apuesta], [Accion], [Puntuacion], [Resultado], [Ganancia]) VALUES (9, 8, 7, CAST(100.00 AS Decimal(10, 2)), N'Doblar', 24, N'Pierde', CAST(-100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasBlackjack] ([IdApuesta], [IdRonda], [IdUsuario], [Apuesta], [Accion], [Puntuacion], [Resultado], [Ganancia]) VALUES (10, 9, 7, CAST(25.00 AS Decimal(10, 2)), N'Pedir', 25, N'Pierde', CAST(-25.00 AS Decimal(10, 2)))
SET IDENTITY_INSERT [dbo].[ApuestasBlackjack] OFF
GO

SET IDENTITY_INSERT [dbo].[ApuestasRuleta] ON 

INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (1, 1, 4, N'Color', N'Rojo', CAST(20.00 AS Decimal(10, 2)), N'Pierde', CAST(-20.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (2, 1, 4, N'Numero', N'17', CAST(10.00 AS Decimal(10, 2)), N'Pierde', CAST(-10.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (3, 2, 4, N'Numero', N'19', CAST(50.00 AS Decimal(10, 2)), N'Pierde', CAST(-50.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (4, 2, 4, N'Numero', N'20', CAST(50.00 AS Decimal(10, 2)), N'Pierde', CAST(-50.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (5, 2, 4, N'Docena', N'1', CAST(50.00 AS Decimal(10, 2)), N'Gana', CAST(100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (6, 3, 4, N'Docena', N'3', CAST(50.00 AS Decimal(10, 2)), N'Pierde', CAST(-50.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (7, 3, 4, N'Color', N'Negro', CAST(50.00 AS Decimal(10, 2)), N'Pierde', CAST(-50.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (8, 4, 4, N'Color', N'Negro', CAST(100.00 AS Decimal(10, 2)), N'Pierde', CAST(-100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (9, 5, 4, N'Color', N'Rojo', CAST(100.00 AS Decimal(10, 2)), N'Pierde', CAST(-100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (10, 5, 4, N'Docena', N'2', CAST(100.00 AS Decimal(10, 2)), N'Gana', CAST(200.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (11, 6, 4, N'Color', N'Rojo', CAST(100.00 AS Decimal(10, 2)), N'Gana', CAST(100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (12, 6, 4, N'Docena', N'2', CAST(100.00 AS Decimal(10, 2)), N'Pierde', CAST(-100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (13, 7, 4, N'Docena', N'2', CAST(100.00 AS Decimal(10, 2)), N'Pierde', CAST(-100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (14, 7, 4, N'Color', N'Rojo', CAST(100.00 AS Decimal(10, 2)), N'Gana', CAST(100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (15, 8, 4, N'Color', N'Rojo', CAST(100.00 AS Decimal(10, 2)), N'Pierde', CAST(-100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (16, 8, 4, N'Docena', N'2', CAST(100.00 AS Decimal(10, 2)), N'Pierde', CAST(-100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (17, 9, 4, N'Numero', N'25', CAST(50.00 AS Decimal(10, 2)), N'Pierde', CAST(-50.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (18, 10, 4, N'Color', N'Negro', CAST(50.00 AS Decimal(10, 2)), N'Gana', CAST(50.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (19, 10, 4, N'Color', N'Rojo', CAST(50.00 AS Decimal(10, 2)), N'Pierde', CAST(-50.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (20, 11, 4, N'Color', N'Rojo', CAST(100.00 AS Decimal(10, 2)), N'Gana', CAST(100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (21, 12, 4, N'Docena', N'2', CAST(50.00 AS Decimal(10, 2)), N'Gana', CAST(100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (22, 13, 4, N'Docena', N'1', CAST(10.00 AS Decimal(10, 2)), N'Pierde', CAST(-10.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (23, 13, 4, N'Docena', N'3', CAST(10.00 AS Decimal(10, 2)), N'Gana', CAST(20.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (24, 14, 4, N'Color', N'Negro', CAST(300.00 AS Decimal(10, 2)), N'Gana', CAST(300.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (25, 15, 4, N'ParImpar', N'Par', CAST(100.00 AS Decimal(10, 2)), N'Gana', CAST(100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (26, 16, 7, N'Docena', N'3', CAST(10.00 AS Decimal(10, 2)), N'Pierde', CAST(-10.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (27, 17, 7, N'ParImpar', N'Par', CAST(25.00 AS Decimal(10, 2)), N'Pierde', CAST(-25.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (28, 18, 7, N'Color', N'Negro', CAST(100.00 AS Decimal(10, 2)), N'Pierde', CAST(-100.00 AS Decimal(10, 2)))
INSERT [dbo].[ApuestasRuleta] ([IdApuestaRuleta], [IdPartida], [IdUsuario], [TipoApuesta], [ValorApuesta], [Monto], [Resultado], [Ganancia]) VALUES (29, 19, 7, N'ParImpar', N'Par', CAST(10.00 AS Decimal(10, 2)), N'Gana', CAST(10.00 AS Decimal(10, 2)))
SET IDENTITY_INSERT [dbo].[ApuestasRuleta] OFF
GO

SET IDENTITY_INSERT [dbo].[Compras] ON 

INSERT [dbo].[Compras] ([IdCompra], [IdUsuario], [IdPaquete], [IdMetodoPago], [FechaCompra]) VALUES (1, 4, 2, 1, CAST(N'2026-10-25T09:59:24.513' AS DateTime))
INSERT [dbo].[Compras] ([IdCompra], [IdUsuario], [IdPaquete], [IdMetodoPago], [FechaCompra]) VALUES (2, 4, 1, 1, CAST(N'2026-10-25T10:18:24.217' AS DateTime))
INSERT [dbo].[Compras] ([IdCompra], [IdUsuario], [IdPaquete], [IdMetodoPago], [FechaCompra]) VALUES (3, 4, 3, 2, CAST(N'2026-10-25T13:25:34.703' AS DateTime))
SET IDENTITY_INSERT [dbo].[Compras] OFF
GO

INSERT [dbo].[Configuracion] ([Clave], [Valor], [Descripcion]) VALUES (N'ApuestaMaximaBlackjack', N'1000', N'Apuesta máxima por mano en Blackjack')
INSERT [dbo].[Configuracion] ([Clave], [Valor], [Descripcion]) VALUES (N'ApuestaMinimaBlackjack', N'10', N'Apuesta mínima por mano en Blackjack')
INSERT [dbo].[Configuracion] ([Clave], [Valor], [Descripcion]) VALUES (N'FichasBienvenida', N'500', N'Cantidad de fichas asignadas al registrar un usuario')
INSERT [dbo].[Configuracion] ([Clave], [Valor], [Descripcion]) VALUES (N'MaxJugadoresPorSala', N'4', N'Máximo de jugadores humanos por sala de Blackjack')
INSERT [dbo].[Configuracion] ([Clave], [Valor], [Descripcion]) VALUES (N'MaxSalasActivas', N'5', N'Máximo de salas de Blackjack abiertas al mismo tiempo')
INSERT [dbo].[Configuracion] ([Clave], [Valor], [Descripcion]) VALUES (N'SegundosPorTurno', N'30', N'Segundos que tiene cada jugador para decidir su jugada')
GO

SET IDENTITY_INSERT [dbo].[MetodosPago] ON 

INSERT [dbo].[MetodosPago] ([IdMetodoPago], [IdUsuario], [TipoTarjeta], [NumeroTarjeta], [NombreTitular], [Activo]) VALUES (1, 4, N'Credito', N'1111', N'Jugador Uno', 1)
INSERT [dbo].[MetodosPago] ([IdMetodoPago], [IdUsuario], [TipoTarjeta], [NumeroTarjeta], [NombreTitular], [Activo]) VALUES (2, 4, N'Debito', N'7676', N'tarje', 0)
INSERT [dbo].[MetodosPago] ([IdMetodoPago], [IdUsuario], [TipoTarjeta], [NumeroTarjeta], [NombreTitular], [Activo]) VALUES (3, 4, N'Credito', N'5432', N'56767 567', 0)
INSERT [dbo].[MetodosPago] ([IdMetodoPago], [IdUsuario], [TipoTarjeta], [NumeroTarjeta], [NombreTitular], [Activo]) VALUES (5, 4, N'Credito', N'9999', N'Tarjeta Prueba', 0)
INSERT [dbo].[MetodosPago] ([IdMetodoPago], [IdUsuario], [TipoTarjeta], [NumeroTarjeta], [NombreTitular], [Activo]) VALUES (6, 4, N'Credito', N'4444', N'tarjet', 1)
SET IDENTITY_INSERT [dbo].[MetodosPago] OFF
GO

SET IDENTITY_INSERT [dbo].[PaquetesFichas] ON 

INSERT [dbo].[PaquetesFichas] ([IdPaquete], [Nombre], [CantidadFichas], [Precio]) VALUES (1, N'Paquete Basico', 100, CAST(2500.00 AS Decimal(10, 2)))
INSERT [dbo].[PaquetesFichas] ([IdPaquete], [Nombre], [CantidadFichas], [Precio]) VALUES (2, N'Paquete Medio', 500, CAST(10000.00 AS Decimal(10, 2)))
INSERT [dbo].[PaquetesFichas] ([IdPaquete], [Nombre], [CantidadFichas], [Precio]) VALUES (3, N'Paquete Grande', 1000, CAST(17500.00 AS Decimal(10, 2)))
SET IDENTITY_INSERT [dbo].[PaquetesFichas] OFF
GO

SET IDENTITY_INSERT [dbo].[PartidasRuleta] ON 

INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (1, 0, N'Verde', CAST(N'2026-10-25T09:59:56.273' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (2, 2, N'Negro', CAST(N'2026-10-25T10:11:16.170' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (3, 16, N'Rojo', CAST(N'2026-10-25T10:12:00.190' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (4, 21, N'Rojo', CAST(N'2026-10-25T10:18:36.867' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (5, 17, N'Negro', CAST(N'2026-10-25T10:19:20.730' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (6, 32, N'Rojo', CAST(N'2026-10-25T10:19:38.847' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (7, 25, N'Rojo', CAST(N'2026-10-25T10:19:53.250' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (8, 2, N'Negro', CAST(N'2026-10-25T10:20:06.733' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (9, 31, N'Negro', CAST(N'2026-10-25T10:20:30.913' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (10, 35, N'Negro', CAST(N'2026-10-25T10:20:41.980' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (11, 12, N'Rojo', CAST(N'2026-10-25T10:20:52.663' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (12, 13, N'Negro', CAST(N'2026-10-25T10:29:30.540' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (13, 27, N'Rojo', CAST(N'2026-10-25T11:50:53.207' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (14, 22, N'Negro', CAST(N'2026-10-25T13:17:52.093' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (15, 34, N'Rojo', CAST(N'2026-10-25T13:50:23.933' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (16, 10, N'Negro', CAST(N'2026-10-25T16:06:52.993' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (17, 7, N'Rojo', CAST(N'2026-10-25T16:07:51.850' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (18, 25, N'Rojo', CAST(N'2026-10-25T16:30:46.503' AS DateTime))
INSERT [dbo].[PartidasRuleta] ([IdPartida], [NumeroGanador], [ColorGanador], [FechaPartida]) VALUES (19, 10, N'Negro', CAST(N'2026-10-25T16:34:05.340' AS DateTime))
SET IDENTITY_INSERT [dbo].[PartidasRuleta] OFF
GO

SET IDENTITY_INSERT [dbo].[RondasBlackjack] ON 

INSERT [dbo].[RondasBlackjack] ([IdRonda], [IdSala], [NumeroRonda], [Estado], [ResultadoDealer], [FechaInicio], [FechaFin]) VALUES (1, 1, 1, N'Finalizada', 17, CAST(N'2026-10-25T10:02:31.260' AS DateTime), CAST(N'2026-10-25T10:02:55.433' AS DateTime))
INSERT [dbo].[RondasBlackjack] ([IdRonda], [IdSala], [NumeroRonda], [Estado], [ResultadoDealer], [FechaInicio], [FechaFin]) VALUES (2, 1, 2, N'Finalizada', 21, CAST(N'2026-10-25T10:13:52.180' AS DateTime), CAST(N'2026-10-25T10:14:14.343' AS DateTime))
INSERT [dbo].[RondasBlackjack] ([IdRonda], [IdSala], [NumeroRonda], [Estado], [ResultadoDealer], [FechaInicio], [FechaFin]) VALUES (3, 1, 3, N'Finalizada', 24, CAST(N'2026-10-25T10:14:43.367' AS DateTime), CAST(N'2026-10-25T10:14:59.630' AS DateTime))
INSERT [dbo].[RondasBlackjack] ([IdRonda], [IdSala], [NumeroRonda], [Estado], [ResultadoDealer], [FechaInicio], [FechaFin]) VALUES (4, 1, 4, N'Finalizada', 12, CAST(N'2026-10-25T10:15:42.643' AS DateTime), CAST(N'2026-10-25T10:15:56.570' AS DateTime))
INSERT [dbo].[RondasBlackjack] ([IdRonda], [IdSala], [NumeroRonda], [Estado], [ResultadoDealer], [FechaInicio], [FechaFin]) VALUES (5, 1, 5, N'Finalizada', 20, CAST(N'2026-10-25T10:16:20.317' AS DateTime), CAST(N'2026-10-25T10:16:37.830' AS DateTime))
INSERT [dbo].[RondasBlackjack] ([IdRonda], [IdSala], [NumeroRonda], [Estado], [ResultadoDealer], [FechaInicio], [FechaFin]) VALUES (6, 2, 1, N'Finalizada', 23, CAST(N'2026-10-25T16:17:31.757' AS DateTime), CAST(N'2026-10-25T16:17:41.593' AS DateTime))
INSERT [dbo].[RondasBlackjack] ([IdRonda], [IdSala], [NumeroRonda], [Estado], [ResultadoDealer], [FechaInicio], [FechaFin]) VALUES (7, 3, 1, N'Finalizada', 19, CAST(N'2026-10-25T16:31:34.177' AS DateTime), CAST(N'2026-10-25T16:31:58.027' AS DateTime))
INSERT [dbo].[RondasBlackjack] ([IdRonda], [IdSala], [NumeroRonda], [Estado], [ResultadoDealer], [FechaInicio], [FechaFin]) VALUES (8, 3, 2, N'Finalizada', 20, CAST(N'2026-10-25T16:32:14.563' AS DateTime), CAST(N'2026-10-25T16:32:22.240' AS DateTime))
INSERT [dbo].[RondasBlackjack] ([IdRonda], [IdSala], [NumeroRonda], [Estado], [ResultadoDealer], [FechaInicio], [FechaFin]) VALUES (9, 3, 3, N'Finalizada', 18, CAST(N'2026-10-25T16:32:38.130' AS DateTime), CAST(N'2026-10-25T16:32:57.197' AS DateTime))
SET IDENTITY_INSERT [dbo].[RondasBlackjack] OFF
GO

SET IDENTITY_INSERT [dbo].[SalasBlackjack] ON 

INSERT [dbo].[SalasBlackjack] ([IdSala], [CodigoSala], [NombreSala], [TipoSala], [Estado], [FechaCreacion]) VALUES (1, N'BRS2YQ', N'Mesa de Prueba', N'Publica', N'Cerrada', CAST(N'2026-10-25T10:00:59.487' AS DateTime))
INSERT [dbo].[SalasBlackjack] ([IdSala], [CodigoSala], [NombreSala], [TipoSala], [Estado], [FechaCreacion]) VALUES (2, N'MG8DPH', N'Prueba', N'Publica', N'Cerrada', CAST(N'2026-10-25T16:15:24.133' AS DateTime))
INSERT [dbo].[SalasBlackjack] ([IdSala], [CodigoSala], [NombreSala], [TipoSala], [Estado], [FechaCreacion]) VALUES (3, N'PXSKB9', N'Solitario', N'Publica', N'Cerrada', CAST(N'2026-10-25T16:31:04.250' AS DateTime))
SET IDENTITY_INSERT [dbo].[SalasBlackjack] OFF
GO

SET IDENTITY_INSERT [dbo].[Usuarios] ON 

INSERT [dbo].[Usuarios] ([IdUsuario], [NombreUsuario], [Correo], [Password], [SaldoFichas], [FechaRegistro], [Rol], [BienvenidaVista]) VALUES (4, N'Administrador', N'casinogabsa@gmail.com', N'$2a$11$s/MBYnSoyaa97LBZzQRybeRyJKQsW/tk/EcdlwGIcKPS174EKespG', CAST(2330.00 AS Decimal(10, 2)), CAST(N'2026-10-25T09:48:09.260' AS DateTime), N'Admin', 1)
INSERT [dbo].[Usuarios] ([IdUsuario], [NombreUsuario], [Correo], [Password], [SaldoFichas], [FechaRegistro], [Rol], [BienvenidaVista]) VALUES (7, N'sari', N'sarilacayo07@gmail.com', N'$2a$11$Z2NncY5lXuTExiXvQsR2pOSFf3jG6OqQGCtrxjY9fIOROpAPl.Tm2', CAST(250.00 AS Decimal(10, 2)), CAST(N'2026-10-25T14:00:55.220' AS DateTime), N'Jugador', 1)
SET IDENTITY_INSERT [dbo].[Usuarios] OFF
GO

-- ===================== RELACIONES =====================
ALTER TABLE [dbo].[ApuestasBlackjack]  WITH CHECK ADD  CONSTRAINT [FK_ApuestasBJ_Rondas] FOREIGN KEY([IdRonda])
REFERENCES [dbo].[RondasBlackjack] ([IdRonda])
GO
ALTER TABLE [dbo].[ApuestasBlackjack] CHECK CONSTRAINT [FK_ApuestasBJ_Rondas]
GO

ALTER TABLE [dbo].[ApuestasBlackjack]  WITH CHECK ADD  CONSTRAINT [FK_ApuestasBJ_Usuarios] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuarios] ([IdUsuario])
GO
ALTER TABLE [dbo].[ApuestasBlackjack] CHECK CONSTRAINT [FK_ApuestasBJ_Usuarios]
GO

ALTER TABLE [dbo].[ApuestasRuleta]  WITH CHECK ADD  CONSTRAINT [FK_ApuestasRuleta_Partida] FOREIGN KEY([IdPartida])
REFERENCES [dbo].[PartidasRuleta] ([IdPartida])
GO
ALTER TABLE [dbo].[ApuestasRuleta] CHECK CONSTRAINT [FK_ApuestasRuleta_Partida]
GO

ALTER TABLE [dbo].[ApuestasRuleta]  WITH CHECK ADD  CONSTRAINT [FK_ApuestasRuleta_Usuario] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuarios] ([IdUsuario])
GO
ALTER TABLE [dbo].[ApuestasRuleta] CHECK CONSTRAINT [FK_ApuestasRuleta_Usuario]
GO

ALTER TABLE [dbo].[Compras]  WITH CHECK ADD  CONSTRAINT [FK_Compras_MetodosPago] FOREIGN KEY([IdMetodoPago])
REFERENCES [dbo].[MetodosPago] ([IdMetodoPago])
GO
ALTER TABLE [dbo].[Compras] CHECK CONSTRAINT [FK_Compras_MetodosPago]
GO

ALTER TABLE [dbo].[Compras]  WITH CHECK ADD  CONSTRAINT [FK_Compras_Paquetes] FOREIGN KEY([IdPaquete])
REFERENCES [dbo].[PaquetesFichas] ([IdPaquete])
GO
ALTER TABLE [dbo].[Compras] CHECK CONSTRAINT [FK_Compras_Paquetes]
GO

ALTER TABLE [dbo].[Compras]  WITH CHECK ADD  CONSTRAINT [FK_Compras_Usuarios] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuarios] ([IdUsuario])
GO
ALTER TABLE [dbo].[Compras] CHECK CONSTRAINT [FK_Compras_Usuarios]
GO

ALTER TABLE [dbo].[JugadoresBlackjack]  WITH CHECK ADD  CONSTRAINT [FK_Jugadores_Salas] FOREIGN KEY([IdSala])
REFERENCES [dbo].[SalasBlackjack] ([IdSala])
GO
ALTER TABLE [dbo].[JugadoresBlackjack] CHECK CONSTRAINT [FK_Jugadores_Salas]
GO

ALTER TABLE [dbo].[JugadoresBlackjack]  WITH CHECK ADD  CONSTRAINT [FK_Jugadores_Usuarios] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuarios] ([IdUsuario])
GO
ALTER TABLE [dbo].[JugadoresBlackjack] CHECK CONSTRAINT [FK_Jugadores_Usuarios]
GO

ALTER TABLE [dbo].[MetodosPago]  WITH CHECK ADD  CONSTRAINT [FK_MetodosPago_Usuarios] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuarios] ([IdUsuario])
GO
ALTER TABLE [dbo].[MetodosPago] CHECK CONSTRAINT [FK_MetodosPago_Usuarios]
GO

ALTER TABLE [dbo].[RondasBlackjack]  WITH CHECK ADD  CONSTRAINT [FK_Rondas_Salas] FOREIGN KEY([IdSala])
REFERENCES [dbo].[SalasBlackjack] ([IdSala])
GO
ALTER TABLE [dbo].[RondasBlackjack] CHECK CONSTRAINT [FK_Rondas_Salas]
GO

-- ===================== PROCEDIMIENTOS =====================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE spHistorialCompras
    @IdUsuario INT,
    @Cantidad  INT = 20          -- si no se manda, trae 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Cantidad)
        c.IdCompra,
        c.FechaCompra,
        p.Nombre          AS Paquete,
        p.CantidadFichas,
        p.Precio,
        mp.TipoTarjeta,
        mp.NumeroTarjeta  AS UltimosDigitos
    FROM Compras c
    INNER JOIN PaquetesFichas p ON p.IdPaquete    = c.IdPaquete
    INNER JOIN MetodosPago   mp ON mp.IdMetodoPago = c.IdMetodoPago
    WHERE c.IdUsuario = @IdUsuario
    ORDER BY c.FechaCompra DESC;
END

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Últimas jugadas del usuario: Ruleta + Blackjack juntas 
CREATE   PROCEDURE spHistorialJuegos
    @IdUsuario INT,
    @Cantidad  INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Cantidad) *
    FROM (
        -- Parte 1: Ruleta (una fila por cada apuesta)
        SELECT
            'Ruleta'                                                 AS Juego,
            pr.FechaPartida                                          AS Fecha,
            CONCAT(ar.TipoApuesta, ': ', ar.ValorApuesta)            AS Detalle,
            CONCAT('Salió ', pr.NumeroGanador, ' ', pr.ColorGanador) AS ResultadoJuego,
            ar.Monto                                                 AS Apuesta,
            ar.Resultado,
            ar.Ganancia
        FROM ApuestasRuleta ar
        INNER JOIN PartidasRuleta pr ON pr.IdPartida = ar.IdPartida
        WHERE ar.IdUsuario = @IdUsuario

        UNION ALL

        -- Parte 2: Blackjack (una fila por cada mano terminada)
        SELECT
            'Blackjack',
            ISNULL(r.FechaFin, r.FechaInicio),
            CONCAT(s.NombreSala, ' - Ronda ', r.NumeroRonda),
            CONCAT('Tú ', ab.Puntuacion, ' vs Dealer ', r.ResultadoDealer),
            ab.Apuesta,
            ab.Resultado,
            ab.Ganancia
        FROM ApuestasBlackjack ab
        INNER JOIN RondasBlackjack r ON r.IdRonda = ab.IdRonda
        INNER JOIN SalasBlackjack  s ON s.IdSala  = r.IdSala
        WHERE ab.IdUsuario = @IdUsuario
          AND ab.Resultado IS NOT NULL          -- solo manos terminadas
          AND ab.Resultado <> 'Cancelada'       -- sin las rondas canceladas
    ) AS Historial
    ORDER BY Fecha DESC;
END

GO

