-- Parámetros globales del casino (REQ8).
-- El backend los lee en cada petición, así que se pueden cambiar sin recompilar.
-- Se puede ejecutar varias veces: solo inserta las claves que todavía no existen.
USE CasinoDB;
GO

MERGE Configuracion AS destino
USING (VALUES
    ('FichasBienvenida',       '500',  'Cantidad de fichas asignadas al registrar un usuario'),
    ('MaxSalasActivas',        '5',    'Máximo de salas de Blackjack abiertas al mismo tiempo'),
    ('MaxJugadoresPorSala',    '4',    'Máximo de jugadores humanos por sala de Blackjack'),
    ('ApuestaMinimaBlackjack', '10',   'Apuesta mínima por mano en Blackjack'),
    ('ApuestaMaximaBlackjack', '1000', 'Apuesta máxima por mano en Blackjack'),
    ('SegundosPorTurno',       '30',   'Segundos que tiene cada jugador para decidir su jugada')
) AS origen (Clave, Valor, Descripcion)
ON destino.Clave = origen.Clave
WHEN NOT MATCHED THEN
    INSERT (Clave, Valor, Descripcion) VALUES (origen.Clave, origen.Valor, origen.Descripcion);
GO

-- Para dar acceso al panel de administración a un usuario, cambia el correo y ejecuta:
-- UPDATE Usuarios SET Rol = 'Admin' WHERE Correo = 'correo@ejemplo.com';
-- (El usuario debe cerrar sesión y volver a entrar para que su token tenga el nuevo rol.)
