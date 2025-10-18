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

#HU001 Registrar Articulo
Delimiter $$
Create Procedure RegistrarArticulo(IN xTema VARCHAR(50),IN xTitular VARCHAR(100),IN xSubtitulo VARCHAR(100),IN xCuerpo VARCHAR(100),IN xFecha DATETIME,IN xIdResultado INT,IN xIdFuente INT,OUT mensaje VARCHAR(100))
Begin
	Insert Into Articulo (tema, titular, subtitulo, cuerpo, fecha, idResultadoFK, favorito)
    Values (
        xTema,xTitular,IFNULL(xSubtitulo, ''),IFNULL(xCuerpo, ''),xFecha,xIdResultado,FALSE
    );
    -- Guardacion de la fuente
    Insert Into ArticuloDetalle (idArticuloFK, idFuenteFK)
    Values (LAST_INSERT_ID(), pIdFuente);

    SET mensaje = 'Artículo registrado correctamente';
End $$
Delimiter ;

DELIMITER $$
CREATE TRIGGER NotificacionArticuloAgregado
    AFTER INSERT
    ON Articulo
    FOR EACH ROW
BEGIN
    Insert into Notificacion(mensaje, tipo, leido, idResultadoFK)
    Values ('Artículo nuevo registrado', 1, FALSE, NEW.idResultadoFK);
END $$
DELIMITER ;

-- HU001: Registar Articulo 
DELIMITER $$
CREATE PROCEDURE RegistrarArticulo(
    IN p_tema VARCHAR(50),
    IN p_titular VARCHAR(100),
    IN p_subtitulo VARCHAR(100),
    IN p_cuerpo VARCHAR(100),
    IN p_fecha DATETIME,
    IN p_idResultadoFK INT,
    IN p_favorito BOOL,
    IN p_url VARCHAR(50),
    IN p_tipo VARCHAR(50),
    IN p_nombreFuente VARCHAR(50)
)
BEGIN
    DECLARE v_idFuente INT;
    DECLARE v_idArticulo INT;

    SELECT id INTO v_idFuente
    FROM Fuente
    WHERE url in (p_url) AND nombre in (p_nombreFuente)
    LIMIT 1;

    IF v_idFuente IS NULL THEN
        INSERT INTO Fuente (url, tipo, nombre)
        VALUES (p_url, p_tipo, p_nombreFuente);
        SET v_idFuente = LAST_INSERT_ID();
    END IF;

    INSERT INTO Articulo (tema, titular, subtitulo, cuerpo, fecha, idResultadoFK, favorito)
    VALUES (
        NULLIF(p_tema, ''),
        NULLIF(p_titular, ''),
        NULLIF(p_subtitulo, ''),
        NULLIF(p_cuerpo, ''),
        p_fecha,
        p_idResultadoFK,
        p_favorito
    );

    SET v_idArticulo = LAST_INSERT_ID();

    INSERT INTO ArticuloDetalle (idArticuloFK, idFuenteFK)
    VALUES (v_idArticulo, v_idFuente);

END$$
DELIMITER ;


#HU002 Consultar Articulos
Delimiter $$
Create Procedure ConsultarArticulos()
Begin
	Select * from Articulo;
End $$
Delimiter ;

call ConsultarArticulos();

#HU003 Mostrar articulos mas recientes
Delimiter $$
Create Procedure VisualizarArticulosRecientes()
Begin
	#select * from Articulo order by fecha desc; OTRA OPCION 
    select Articulo.*, Resultado.fechaExtraccion from Articulo
    #El primero es la otra tabla que quiero unir
    INNER JOIN Resultado ON Articulo.idResultadoFK = Resultado.id
    ORDER BY Resultado.fechaExtraccion DESC;
End $$
Delimiter ;

-- HU003:  Mostrar artículos más recientes
DELIMITER $$
CREATE PROCEDURE mostrarArticulosRecientes()
BEGIN
    SELECT 
        id,
        tema,
        titular,
        subtitulo,
        fecha
    FROM Articulo
    ORDER BY fecha DESC;
END $$
DELIMITER ;


#HU006 Filtrar articulos por rango de fechas
Delimiter $$
Create Procedure FiltroArticulosRangoFechas(IN fecha1 datetime, IN fecha2 datetime)
Begin
	select * from Articulo where fecha between fecha1 and fecha2 order by fecha asc;
End $$
Delimiter ;

#HU007 Filtrar Articulos por coincidencias en titulo
Delimiter $$
Create Procedure FiltroArticuloCoincidenciasTitulo(IN palabras VARCHAR(50))
Begin
	select * from Articulo where titular like CONCAT('%', palabras, '%');
    
End $$
Delimiter ;

#HU008 Filtrar Por Palabras Clave
Delimiter $$
Create Procedure FiltroArticuloPalabrasClave(IN claves VARCHAR(100))
Begin
	select * from Articulo where titular like CONCAT('%', claves, '%') or subtitulo like CONCAT('%', claves, '%') or cuerpo like CONCAT('%', palabras, '%')
    order by fecha desc;
End $$
Delimiter ;

-- HU008: Filtrar artículos por palabras clave
DELIMITER $$
CREATE PROCEDURE FiltrarArticulosPorPalabraClave (
    IN palabraClave VARCHAR(100)
)
BEGIN
    SELECT 
        a.id,
        a.tema,
        a.titular,
        a.subtitulo,
        a.cuerpo,
        a.fecha,
        a.favorito,
        f.url AS urlFuente,
        f.tipo AS tipoFuente,
        f.nombre AS nombreFuente
    FROM Articulo a
    INNER JOIN ArticuloDetalle ad ON a.id = ad.idArticuloFK
    INNER JOIN Fuente f ON ad.idFuenteFK = f.id
    WHERE 
        a.titular LIKE CONCAT('%', palabraClave, '%')
        OR a.subtitulo LIKE CONCAT('%', palabraClave, '%')
        OR a.cuerpo LIKE CONCAT('%', palabraClave, '%')
    ORDER BY a.fecha DESC;
END $$
DELIMITER ;

#HU009 Filtrar Articulos por tema
Delimiter $$
Create Procedure FiltroArticuloTema(IN temabuscar VARCHAR(100))
Begin
	select * from Articulo where tema = temabuscar;
    
End $$
Delimiter ;


#HU010 Filtrar Articulos la fuente correspondiente
Delimiter $$
Create Procedure FiltroArticuloFuente(IN fuentes VARCHAR(100))
Begin
	select * from Articulo 
    #Unir con cadenas de 1 a 2 y de 2 a 3, articulo a articulodetalle y de articulodetalle a fuente
    Inner Join ArticuloDetalle on Articulo.id = ArticuloDetalle.idArticuloFK
    Inner Join Fuente on ArticuloDetalle.idFuenteFK = Fuente.id
    Where Fuente.dominio = fuentes
    #No olvidar el orden de fechas de articulos
    Order by Articulo.fecha desc;
    
End $$

-- HU011: Evitar articulos duplicados
DELIMITER $$
CREATE PROCEDURE RegistrarFuenteSinDuplicados(
    IN p_url VARCHAR(50),
    IN p_tipo VARCHAR(50),
    IN p_nombre VARCHAR(50)
)
BEGIN
    DECLARE existe INT;

    SELECT COUNT(*) INTO existe
    FROM Fuente
    WHERE nombre in (p_nombre) AND url in (p_url);

    IF existe > 0 THEN
        SELECT CONCAT('La fuente "', p_nombre, '" con URL "', p_url, '" ya está registrada.') AS mensaje;
    ELSE
        INSERT INTO Fuente (url, tipo, nombre)
        VALUES (p_url, p_tipo, p_nombre);

        SELECT CONCAT('Fuente "', p_nombre, '" registrada exitosamente.') AS mensaje;
    END IF;
END$$
DELIMITER ;

#HU015 Registrar Usuario
Delimiter $$
Create Procedure RegistrarUsuario(IN xnombres VARCHAR(30), IN xapellidos VARCHAR(30), IN xcontrasena VARCHAR(30), IN xemail VARCHAR(30))
Begin
	Insert Into Usuario(nombres, apellidos, contraseña, correo)
    Values (xnombres, xapellidos, xcontrasena, xemail);
End $$
Delimiter ;

#HU016 Eliminar datos asociados a un usuario
DELIMITER $$
CREATE TRIGGER EliminarDatosAsociadosUsuario
BEFORE DELETE ON Usuario
FOR EACH ROW
BEGIN
    DELETE FROM Resultado r WHERE r.idUsuarioFK = OLD.id;
END $$
DELIMITER ;

#HU017 Consultar articulos (Despues scraping = total)
Delimiter $$
Create Procedure ConsultarCantidadArticulos(OUT total INT)
Begin
    Select COUNT(*) Into total From Articulo;
End $$

Delimiter ;

-- HU017: Consultar cantidad total de artículos 
Delimiter $$
CREATE PROCEDURE MostrarCantidadArticulos()
BEGIN
	SELECT Count(*) AS ArticulosTotal FROM Articulo;
END $$
Delimiter ;
 
-- HU018: Notificación de scraping 
DELIMITER $$
CREATE TRIGGER RegistrarYNotificacion
AFTER INSERT ON Articulo
FOR EACH ROW
BEGIN
    DECLARE v_idResultado INT;
    DECLARE v_idUsuario INT DEFAULT 1;  -- Usuario que ejecuta el scraping (ajústalo según tu app)
    DECLARE v_estado INT DEFAULT 1;     -- 1 = exitoso

    -- Crear un nuevo registro en la tabla Resultado
    INSERT INTO Resultado (idUsuarioFK, estado, fechaExtraccion)
    VALUES (v_idUsuario, v_estado, NOW());

    SET v_idResultado = LAST_INSERT_ID();

    -- Crear una notificación asociada al resultado
    INSERT INTO Notificacion (mensaje, tipo, idResultadoFK)
    VALUES ('Scraping EXITOSO', v_estado, v_idResultado);
END$$
DELIMITER ;

#HU022 Registrar Nuevo usuario en el sistema
Delimiter $$
Create Procedure RegistrarNuevoUsuario(IN xnombres VARCHAR(30), IN xapellidos VARCHAR(30), IN xcontrasena VARCHAR(30), IN xemail VARCHAR(30), OUT xmensaje VARCHAR(100))
Begin
	IF(Select 1 from Usuario where correo = xemail) THEN 
		Set xmensaje = "El correo ya esta registrado intente de nuevo con otra direccion de correo";
    ELSE 
		Insert Into Usuario(nombres, apellidos, contraseña, correo)
		Values (xnombres, xapellidos, xcontrasena, xemail);
        Set xmensaje = "El usuario se ha registrado correctamente";
	End IF;
End $$
Delimiter ;

-- HU022: Registrar un usuario nuevo
DELIMITER $$
CREATE PROCEDURE RegistrarNuevoUsuario(
    IN p_nombres VARCHAR(50),
    IN p_apellidos VARCHAR(50),
    IN p_contraseña VARCHAR(20),
    IN p_correo VARCHAR(50)
)
BEGIN
    DECLARE v_existente INT;

    SELECT COUNT(*) INTO v_existente
    FROM Usuario
    WHERE correo in (p_correo);

    IF v_existente > 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'El correo ya está registrado. Intente con otro.';
    ELSE
        INSERT INTO Usuario (nombres, apellidos, contraseña, correo)
        VALUES (p_nombres, p_apellidos, p_contraseña, p_correo);

        SELECT 'Usuario registrado exitosamente.' AS Mensaje, LAST_INSERT_ID() AS ID_Usuario;
    END IF;
END$$
DELIMITER ;

#HU023 Iniciar sesion en la plataforma
Delimiter $$
Create Procedure IniciarSesion(IN xemail VARCHAR(30), IN xcontrasena VARCHAR(30), OUT xmensaje VARCHAR(100))
Begin
	IF EXISTS(Select 1 from Usuario where correo = xemail and contraseña = xcontrasena) THEN 
		Set xmensaje = "Informacion Correcta Inicio de sesion validado";
    ELSE 
		Set xmensaje = "El correo o la contrasena son incorrectos";
	End IF;
End $$
Delimiter ;
SET @mensaje = '';
CALL IniciarSesion("pepito@gmail.com", "Saitama", @mensaje);
SELECT @mensaje;

-- HU023: Iniciar sesión en la plataforma  
DELIMITER $$
CREATE PROCEDURE IniciarSesion(
    IN p_correo VARCHAR(50),
    IN p_contraseña VARCHAR(20)
)
BEGIN
    DECLARE v_idUsuario INT;
    DECLARE v_estado INT;

    SELECT id INTO v_idUsuario
    FROM Usuario
    WHERE correo in (p_correo) AND contraseña in (p_contraseña)
    LIMIT 1;

    IF v_idUsuario IS NOT NULL THEN
        SET v_estado = 1;
        INSERT INTO Resultado (idUsuarioFK, estado, fechaExtraccion)
        VALUES (v_idUsuario, v_estado, NOW());
        SELECT 'Inicio de sesión exitoso.' AS Mensaje, v_idUsuario AS ID_Usuario;

    ELSE
        SET v_estado = 0;
        INSERT INTO Resultado (idUsuarioFK, estado, fechaExtraccion)
        VALUES (1, v_estado, NOW());
        SELECT 'Error: Correo o contraseña incorrectos.' AS Mensaje;
    END IF;
END$$
DELIMITER ;

#HU024 Cerrar sesion Simbolico en sql (FRONTEND BACKEND)
DELIMITER $$

CREATE PROCEDURE CerrarSesionUsuario(OUT mensaje VARCHAR(100))
BEGIN
    SET mensaje = "Sesión cerrada correctamente";
END $$

DELIMITER ;

DELIMITER $$

-- HU024: Cerrar sesión
DELIMITER $$
CREATE PROCEDURE CerrarSesion(
    IN p_idUsuario INT
)
BEGIN
    DECLARE v_sesionActiva INT;

    SELECT COUNT(*) INTO v_sesionActiva
    FROM Resultado
    WHERE idUsuarioFK in (p_idUsuario) AND estado = 1;

    IF v_sesionActiva > 0 THEN
        INSERT INTO Resultado (idUsuarioFK, estado, fechaExtraccion)
        VALUES (p_idUsuario, 2, NOW());

        SELECT 'Sesión cerrada correctamente. Redirigiendo a la pantalla principal...' AS Mensaje;

    ELSE
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Error: No hay una sesión activa para este usuario.';
    END IF;
END$$
DELIMITER ;

#HU025 Actualizar contraseña del usuario
Delimiter $$
Create Procedure ActualizarContrasena(IN xcorreo VARCHAR(50),IN xcontrasena VARCHAR(30),IN xnuevacontrasena VARCHAR(30), OUT xmensaje VARCHAR(200))
Begin
	IF EXISTS(Select 1 from Usuario where correo = xcorreo and contraseña = xcontrasena) THEN 
		Set xmensaje = "Informacion Correcta a";
        update Usuario set Usuario.contrasena = xnuevacontrasena where correo = xcorreo;
    ELSE 
		Set xmensaje = "La contrasena es incorrecta";
	End IF;
End $$

Delimiter ;

-- HU025: Actualizar contraseña
DELIMITER $$
CREATE PROCEDURE ActualizarContraseñaUsuario(
    IN p_correo VARCHAR(50),
    IN p_contraseñaActual VARCHAR(20),
    IN p_nuevaContraseña VARCHAR(20)
)
BEGIN
    DECLARE v_idUsuario INT;

    SELECT id INTO v_idUsuario
    FROM Usuario
    WHERE correo in (p_correo) AND contraseña in (p_contraseñaActual)
    LIMIT 1;

    IF v_idUsuario IS NOT NULL THEN
        UPDATE Usuario
        SET contraseña = p_nuevaContraseña
        WHERE id in (v_idUsuario);

        SELECT 'Contraseña actualizada exitosamente.' AS Mensaje;

    ELSE
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Error: La contraseña actual es incorrecta o el usuario no existe.';
    END IF;
END$$
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

#HU024 Cerrar Sesion de Usuario 
#Agregar nuevo tributo para el estado de inicio o cierre de sesion 
ALTER TABLE Usuario ADD COLUMN sesion_activa BOOLEAN DEFAULT FALSE;

Delimiter $$
Create Procedure CerrarSesion(IN idx VARCHAR(30), OUT xmensaje VARCHAR(100))
Begin
	IF(Select 1 from Usuario where id = idx and sesion_activa = TRUE) THEN 
		Update Usuario set sesion_activa = FALSE where id = idx;
        set xmensaje = "Se ha cerrado la sesion";
    ELSE 
		Set xmensaje = "El usuario no se encuentra activo";
	End IF;
End $$
Delimiter ;

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

#HU031 Visualizar articulos almacenados
Delimiter $$
Create Procedure VisualizarAlmacenados(OUT xmensaje varchar(100))
Begin
	
	IF exists (SELECT COUNT(*) FROM Articulo) >= 1 then
		select * from Articulo Order by Articulo.fecha desc;
	Else
		Set xmensaje = "El estado introducido es invalido";
	END IF;
    
End $$

Delimiter ;

#HU032 Visualizar los resultados de la extracción de artículos
Delimiter $$
Create Procedure VisualizarResultadoExtraccion()
Begin
#Resultado puede ser todo o seleccionar
	select Resultado.*, count(Articulo.id) as cantidad from Resultado
    left join Articulo on Articulo.idResultadoFK = Resultado.id
    GROUP BY Resultado.id, Resultado.idUsuarioFK, Resultado.estado, Resultado.fechaExtraccion
    ORDER BY Resultado.fechaExtraccion DESC;
End $$

Delimiter ;

#HU033 Mostrar cantidad de articulos
Delimiter $$
Create Procedure MostrarCantidadArticulos()
Begin
	Select Count(*) FROM Articulos;
End $$
Delimiter ;

call MostrarCantidadArticulos();

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

#HU034 Copiar el enlace del articulo
DELIMITER $$
CREATE PROCEDURE ObtenerURL(IN idArticuloP INT, OUT urlP VARCHAR(50))
BEGIN
    SELECT url INTO urlP FROM Fuente f WHERE idArticuloP = (SELECT idArticuloFK FROM ArticuloDetalle a WHERE f.id = a.idFuenteFK);
END $$
DELIMITER ;

#HU035 CambiarCorreoe
DELIMITER $$
CREATE PROCEDURE CambiarCorreoUsuario(IN correoActual VARCHAR(50),IN correoNuevo VARCHAR(50),OUT mensaje VARCHAR(100))
BEGIN
    IF EXISTS (Select 1 From Usuario Where correo = correoNuevo) THEN
        SET mensaje = "El correo ya existe, no se puede cambiar";
    ELSE
        Update Usuario Set correo = correoNuevo Where correo = correoActual;
        Set mensaje = "Correo actualizado correctamente";
    END IF;
END $$
DELIMITER ;

#HU036 Filtrar notificaciones por estado
Delimiter $$
Create Procedure EstadoNotificacion(IN xestado INT, OUT xmensaje varchar(100))
Begin
	
    IF exists (Select 1 from Resultado where estado = xestado) then
		select Notificacion.*, Resultado.id , Resultado.estado, Resultado.fechaExtraccion, Resultado.idUsuarioFK from Notificacion
		Inner Join Resultado ON Notificacion.idResultadoFK = Resultado.id
		WHERE Resultado.estado = xestado;
	Else
		Set xmensaje = "El estado introducido es invalido";
	END IF;
    
    
End $$

Delimiter ;

#HU037 Filtrar articulos por busqueda avanzada 
Delimiter $$
Create Procedure FiltroArticuloBusquedaAvanzada(IN fecha1 datetime, IN fecha2 datetime, IN cointitulo VARCHAR(100),IN claves VARCHAR(100), IN temabuscar VARCHAR(100), IN fuentes VARCHAR(100))
Begin
	
	select Articulo.*, Fuente.nombre, Fuente.dominio from articulo
    #Unir con cadenas de 1 a 2 y de 2 a 3, articulo a articulodetalle y de articulodetalle a fuente
    Inner Join ArticuloDetalle on Articulo.id = ArticuloDetalle.idArticuloFK
    Inner Join Fuente on ArticuloDetalle.idFuenteFK = Fuente.id
    #Fecha
	WHERE (fecha1 IS NULL OR fecha2 IS NULL OR Articulo.fecha BETWEEN fecha1 and fecha2)
		and ((cointitulo IS NULL OR Articulo.titular like CONCAT('%', cointitulo, '%')))
		and (claves IS NULL OR Articulo.titular like CONCAT('%', claves, '%') or Articulo.subtitulo like CONCAT('%', claves, '%') or Articulo.cuerpo like CONCAT('%', claves, '%'))
		and (temabuscar IS NULL OR Articulo.tema = temabuscar)
		and (fuentes IS NULL OR Fuente.dominio = fuentes)
    Order by Articulo.fecha desc;
    
End $$

Delimiter ;

#HU038 Cambiar nombre y apellido
CREATE PROCEDURE CambiarNombreApellidoUsuario(IN correo VARCHAR(50),IN nuevoNombre VARCHAR(50),IN nuevoApellido VARCHAR(50),OUT mensaje VARCHAR(100)
)
BEGIN
    Update Usuario SET nombre = nuevoNombre, apellido = nuevoApellido Where correo = correo;
    SET mensaje = "Nombre y apellido actualizados correctamente";
END $$

DELIMITER ;

#HU039 Consultar fuentes ordenadas alfabéticamente
DELIMITER $$
CREATE PROCEDURE ConsultarFuentesAlfabeticamente()
BEGIN
    SELECT * FROM Fuente ORDER BY nombre ASC;
END $$
DELIMITER ;

#HU040 Consultar Fuentes Alfabeticamente
Delimiter $$
Create Procedure FuentesAlfabeticamente()
Begin
    Select * from Fuente order by dominio desc;
End $$

Delimiter ;

call FuentesAlfabeticamente()

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