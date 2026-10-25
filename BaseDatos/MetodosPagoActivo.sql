-- Eliminar métodos de pago.
-- Activo = 0 → la tarjeta ya no aparece ni se puede usar, pero las compras hechas con ella se conservan.
-- Se puede ejecutar varias veces.
USE CasinoDB;
GO

IF COL_LENGTH('MetodosPago', 'Activo') IS NULL
BEGIN
    ALTER TABLE MetodosPago
        ADD Activo BIT NOT NULL
            CONSTRAINT DF_MetodosPago_Activo DEFAULT (1);
END
GO
