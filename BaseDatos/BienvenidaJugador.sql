-- Mensaje de bienvenida en el primer ingreso.
-- BienvenidaVista = 0 → al iniciar sesión la app muestra la bienvenida y el regalo de fichas.
-- Se puede ejecutar varias veces.
USE CasinoDB;
GO

IF COL_LENGTH('Usuarios', 'BienvenidaVista') IS NULL
BEGIN
    ALTER TABLE Usuarios
        ADD BienvenidaVista BIT NOT NULL
            CONSTRAINT DF_Usuarios_BienvenidaVista DEFAULT (0);

    -- Los usuarios que ya existían ya entraron antes: no se les muestra
    EXEC('UPDATE Usuarios SET BienvenidaVista = 1');
END
GO
