USE WebCrawler;

-- 1. Insert users
INSERT INTO Usuario (nombres, apellidos, contraseña, correo) VALUES
('Juan', 'Pérez', 'pass123', 'juan.perez@email.com'),
('María', 'Gómez', 'securepass', 'maria.g@email.com'),
('Carlos', 'López', 'clave123', 'carlos.lopez@email.com'),
('Ana', 'Rodríguez', 'ana2024', 'ana.rod@email.com'),
('Pedro', 'Martínez', 'pedro123', 'pedro.m@email.com');

-- 2. Insert sources
INSERT INTO Fuente (url, tipo, nombre) VALUES
('https://news.example.com/rss', 'RSS', 'Example News'),
('https://techblog.com/feed', 'RSS', 'Tech Blog'),
('https://sportsnews.com/api', 'API', 'Sports News'),
('https://politics.com/rss', 'RSS', 'Politics Daily'),
('https://health.org/feed', 'RSS', 'Health Updates'),
('https://economy.com/api', 'API', 'Economy Watch'),
('https://science.org/rss', 'RSS', 'Science Journal');

-- 3. Insert results
INSERT INTO Resultado (idUsuarioFK, estado, fechaExtraccion) VALUES
(1, 1, '2024-01-15 10:30:00'),
(1, 2, '2024-01-16 14:45:00'),
(2, 1, '2024-01-17 09:15:00'),
(3, 3, '2024-01-18 16:20:00'),
(4, 1, '2024-01-19 11:00:00'),
(5, 2, '2024-01-20 13:30:00'),
(2, 1, '2024-01-21 08:45:00');

-- 4. Insert articles
INSERT INTO Articulo (tema, titular, subtitulo, cuerpo, fecha, idResultadoFK, favorito) VALUES
('Tecnología', 'Nueva IA Revoluciona Medicina', 'Avances en diagnóstico con IA', 'Sistema de IA detecta enfermedades con 95% de precisión en estudios recientes.', '2024-01-15 10:00:00', 1, true),
('Deportes', 'Equipo Local Gana Campeonato', 'Victoria histórica en tiempo extra', 'Equipo local gana título nacional después de 20 años en partido emocionante.', '2024-01-16 14:30:00', 2, false),
('Política', 'Nueva Ley de Datos Aprobada', 'Regulación para empresas tech', 'Congreso aprueba legislación que protege datos personales de ciudadanos.', '2024-01-17 09:00:00', 3, true),
('Salud', 'Nuevo Tratamiento para Diabetes', 'Estudio muestra resultados prometedores', 'Tratamiento innovador podría cambiar vida de millones de pacientes diabéticos.', '2024-01-18 16:00:00', 4, false),
('Economía', 'Mercado Alcanza Máximo Histórico', 'Inversores optimistas', 'Índices bursátiles rompen récords por crecimiento económico sostenido.', '2024-01-19 10:45:00', 5, true),
('Ciencia', 'Misión Encuentra Agua en Marte', 'Hallazgo aumenta posibilidad de vida', 'Rover confirma agua líquida bajo superficie marciana en descubrimiento clave.', '2024-01-20 13:15:00', 6, false),
('Tech', 'Nuevo Smartphone Plegable', 'Innovación en diseño móvil', 'Compañía presenta dispositivo plegable con características revolucionarias.', '2024-01-21 08:30:00', 7, true);

-- 5. Insert notifications
INSERT INTO Notificacion (mensaje, tipo, leido, idResultadoFK) VALUES
('Éxito', 1, true, 1),
('Error', 2, false, 2),
('Completado', 1, true, 3),
('Límite', 3, false, 4),
('Éxito', 1, true, 5),
('Error conex', 2, true, 6),
('Finalizado', 1, false, 7);

-- 6. Finally, insert article details - use the actual IDs that were generated
INSERT INTO ArticuloDetalle (idArticuloFK, idFuenteFK) VALUES
(1, 1),  -- First article with first source
(2, 2),  -- Second article with second source
(3, 3),  -- Third article with third source
(4, 4),  -- Fourth article with fourth source
(5, 5),  -- Fifth article with fifth source
(6, 6),  -- Sixth article with sixth source
(7, 7);  -- Seventh article with seventh source

#HU034 Copiar enlace de artículo
DELIMITER $$
CREATE PROCEDURE ObtenerURL(
	IN idArticuloP INT,
    OUT urlP VARCHAR(50)
)
BEGIN
	SELECT url INTO urlP FROM Fuente f 
    WHERE idArticuloP = (SELECT idArticuloFK FROM ArticuloDetalle a WHERE f.id = a.idFuenteFK);
END $$
DELIMITER ;

#HU027 Crear notificación para el usuario
DELIMITER $$
CREATE PROCEDURE CrearNotificacion(
	IN idResultadoP INT,
    IN mensajeP VARCHAR(11),
    IN tipoP INT
)
BEGIN
	INSERT INTO Notificacion (mensaje, tipo, leido, idResultadoFK) VALUES (mensajeP, tipoP, TRUE, idResultadoP);
END $$
DELIMITER ;

#HU041 Eliminar datos asociados a un resultado
DELIMITER $$
CREATE TRIGGER EliminarDatosAsociadosResultado
BEFORE DELETE ON Resultado
FOR EACH ROW
BEGIN
	DELETE FROM Notificacion n WHERE n.idResultadoFK = OLD.id;
    DELETE FROM Articulo a WHERE a.idResultadoFK = OLD.id;
END $$
DELIMITER ;

#HU016 Eliminar datos asociados a un usuario
DELIMITER $$
CREATE TRIGGER EliminarDatosAsociadosUsuario
BEFORE DELETE ON Usuario
FOR EACH ROW
BEGIN
    DELETE FROM Resultado r WHERE r.idUsuarioFK = OLD.id;
END $$
DELIMITER ;

#HU039 Consultar fuentes ordenadas alfabéticamente
DELIMITER $$
CREATE PROCEDURE ConsultarFuentesAlfabeticamente()
BEGIN
    SELECT * FROM Fuente ORDER BY nombre ASC;
END $$
DELIMITER ;

#HU028 Consultar notificaciones del usuario
DELIMITER $$
CREATE PROCEDURE ConsultarNotificaciones(
    IN idUsuarioP int,
    OUT mensajeP varchar(30)
)
BEGIN
    DECLARE numNotificaciones INT;
    SELECT COUNT(*) INTO numNotificaciones FROM Notificacion n
    INNER JOIN Resultado r ON r.id = n.idResultadoFK
    WHERE r.idUsuarioFK = idUsuarioP;

    IF numNotificaciones = 0 THEN
        SET mensajeP = "No hay notificaciones";
    ELSE 
        SET mensajeP = CONCAT("Hay ", numNotificaciones, " notificaciones");
        SELECT * FROM Notificacion n
        INNER JOIN Resultado r ON r.id = n.idResultadoFK
        WHERE r.idUsuarioFK = idUsuarioP;
    END IF;
END $$
DELIMITER ;