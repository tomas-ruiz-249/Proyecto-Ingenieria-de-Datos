USE WebCrawler;

INSERT INTO Usuario (nombres, apellidos, contraseña, correo)
VALUES ("tomas", "ruiz", "1234", "tomasa.ruiz@urosario.edu.co");

-- HU001: Registar Articulo 
DELIMITER $$
CREATE PROCEDURE RegistrarArticulo(
    IN p_tema VARCHAR(500),
    IN p_titular VARCHAR(500),
    IN p_subtitulo VARCHAR(500),
    IN p_cuerpo TEXT,
    IN p_fecha DATETIME,
    IN p_idResultadoFK INT,
    IN p_favorito BOOL,
    IN p_url VARCHAR(500),
    IN p_tipo VARCHAR(50),
    IN p_nombreFuente VARCHAR(500)
)
BEGIN
    DECLARE v_idFuente INT;
    DECLARE v_idArticulo INT;

	INSERT INTO Articulo (tema, titular, subtitulo, cuerpo, fecha, idResultadoFK, favorito)
    VALUES (
        IFNULL(p_tema, ''),
        IFNULL(p_titular, ''),
        IFNULL(p_subtitulo, ''),
        IFNULL(p_cuerpo, ''),
        p_fecha,
        p_idResultadoFK,
        p_favorito
    );
    SET v_idArticulo = LAST_INSERT_ID();
	
    SELECT id INTO v_idFuente
    FROM Fuente
    WHERE url = p_url AND nombre = p_nombreFuente
    LIMIT 1;

    IF v_idFuente IS NULL THEN
        INSERT INTO Fuente (url, tipo, nombre)
        VALUES (p_url, p_tipo, p_nombreFuente);
        SET v_idFuente = LAST_INSERT_ID();
        
        INSERT INTO ArticuloDetalle (idArticuloFK, idFuenteFK)
		VALUES (v_idArticulo, v_idFuente);
    END IF;
END$$
DELIMITER ;


#HU002 Consultar Articulos
Delimiter $$
Create Procedure ConsultarArticulos(IN idUsuarioP INT)
Begin
	Select * from Articulo a
    INNER JOIN Resultado r ON r.id = a.idResultadoFK
    WHERE idUsuarioP = r.idUsuarioFK;
End $$
Delimiter ;


#HU003 Mostrar articulos mas recientes
Delimiter $$
Create Procedure VisualizarArticulosRecientes(IN idUsuarioP INT)
Begin
    select a.*, r.fechaExtraccion from Articulo a
    INNER JOIN Resultado r ON a.idResultadoFK = r.id
    WHERE r.idUsuarioFK = idUsuario
    ORDER BY r.fechaExtraccion DESC;
End $$
Delimiter ;


-- HU004 Descartar articulos ---------------------------------------
delimiter $$
create procedure eliminarArticulo(
    in p_id_articulo int
)
begin
    if exists (select 1 from articulo where id = p_id_articulo) then
        delete from articulodetalle where idarticulofk = p_id_articulo;
        delete from articulo where id = p_id_articulo;
        select 'el articulo fue eliminado correctamente' as mensaje;
    else
        select 'el articulo no existe' as mensaje;
    end if;
end$$
delimiter ;

-- HU005 Asignar articulos como favoritos-----------------------
delimiter $$
create procedure toggle_favorito(
    in p_id_articulo int,
    in p_favorito bool
)
begin
    if exists (select 1 from articulo where id = p_id_articulo) then
        update articulo
        set favorito = p_favorito
        where id = p_id_articulo;
        select 'el articulo fue actualizado correctamente' as mensaje;
    else
        select 'el articulo no existe' as mensaje;
    end if;
end$$
delimiter ;

#HU006 Filtrar articulos por rango de fechas
Delimiter $$
Create Procedure FiltroArticulosRangoFechas(IN idUsuarioP INT, IN fecha1 datetime, IN fecha2 datetime)
Begin
	select * from Articulo a
    inner join Resultado r on r.id = a.idResultadoFK
    where (fecha between fecha1 and fecha2) and (r.idUsuarioFK = idUsuarioP)
    order by fecha asc;
End $$
Delimiter ;

#HU007 Filtrar Articulos por coincidencias en titulo
Delimiter $$
Create Procedure FiltroArticuloCoincidenciasTitulo(IN idUsuarioP INT, IN palabras VARCHAR(50))
Begin
	select a.* from Articulo a
    INNER JOIN Resultado r ON r.id = a.idResultadoFK
    where titular like CONCAT('%', palabras, '%') and r.idUsuarioFK = idUsuarioP;
End $$
Delimiter ;


-- HU008: Filtrar artículos por palabras clave
DELIMITER $$
CREATE PROCEDURE FiltrarArticulosPorPalabraClave (
    IN idUsuarioP INT,
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
    INNER JOIN Resultado r ON r.id = a.idResultadoFK
    INNER JOIN ArticuloDetalle ad ON a.id = ad.idArticuloFK
    INNER JOIN Fuente f ON ad.idFuenteFK = f.id
    WHERE 
    (
        a.titular LIKE CONCAT('%', palabraClave, '%')
        OR a.subtitulo LIKE CONCAT('%', palabraClave, '%')
        OR a.cuerpo LIKE CONCAT('%', palabraClave, '%')
    )
        AND (r.idUsuarioFK = idUsuarioP)
    ORDER BY a.fecha DESC;
END $$
DELIMITER ;

#HU009 Filtrar Articulos por tema
Delimiter $$
Create Procedure FiltroArticuloTema(IN idUsuarioP INT, IN temabuscar VARCHAR(100))
Begin
	select * from Articulo a
    INNER JOIN Resultado r ON r.id = a.idResultadoFK
    where tema = temabuscar and r.idUsuarioFK = idUsuarioP;
End $$
Delimiter ;


#HU010 Filtrar Articulos la fuente correspondiente
Delimiter $$
Create Procedure FiltroArticuloFuente(IN idUsuarioP INT, IN fuentes VARCHAR(100))
Begin
	select * from Articulo a
    Inner Join ArticuloDetalle ad on a.id = ad.idArticuloFK
    Inner Join Fuente f on ad.idFuenteFK = f.id
    INNER JOIN Resultado r ON r.id = a.idResultadoFK
    Where f.nombre = fuentes AND r.idUsuarioFK = idUsuarioP
    Order by a.fecha desc;
End $$
Delimiter ;

-- HU011: Evitar articulos duplicados
DELIMITER $$
CREATE TRIGGER EvitarArticulosDuplicados
BEFORE INSERT ON Articulo
FOR EACH ROW
BEGIN 
	DECLARE v_url VARCHAR(500);
    DECLARE V_idArticulo INT;
    IF EXISTS(
		SELECT * FROM Articulo a
        WHERE NEW.titular = titular 
		AND NEW.subtitulo = subtitulo
	) THEN
		SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Articulo duplicado, inserción cancelada';
    END IF;
END$$
DELIMITER ;
-- SHOW TRIGGERS;

-- HU012 Actualizar articulo -------------------------------------------------
delimiter $$
create procedure actualizar_articulo(
    in p_url varchar(500),
    in p_titular varchar(500),
    in p_subtitulo varchar(500),
    in p_cuerpo text,
    in p_fecha datetime,
    in p_id_resultado int,
    in p_id_articulo int
)
begin
    update Articulo
    set 
    titular = p_titular,
    subtitulo = p_subtitulo,
    cuerpo = p_cuerpo,
    fecha = p_fecha
    where id = p_id_articulo;
    
    update Fuente f
    inner join ArticuloDetalle ad on ad.idFuenteFK = f.id
    inner join Articulo a on a.id = ad.idArticuloFK
    set f.url = p_url
    where a.id = p_id_articulo;
end$$
delimiter ;

-- HU013 Asignar Fuentes Iniciales ----------------------------------------------------------
#esto ocurre en el webcrawler

-- HU014 Conservar articulo -------------------------------------------------
#esto ocurre en la interfaz

#HU015 Mostrar cantidad fuentes
DELIMITER $$
CREATE PROCEDURE MostrarCantidadFuentes(IN idUsuarioP INT)
BEGIN
    SELECT COUNT(DISTINCT f.id) FROM Fuente f
    INNER JOIN ArticuloDetalle ad ON f.id = ad.idFuenteFK
    INNER JOIN Articulo a ON a.id = ad.idArticuloFK
    INNER JOIN Resultado r ON a.idResultadoFK = r.id
    WHERE r.idUsuarioFK = idUsuarioP;
END $$
DELIMITER ;

#HU016 Eliminar datos asociados a un usuario
DELIMITER $$
CREATE TRIGGER EliminarDatosAsociadosUsuario
BEFORE DELETE ON Usuario
FOR EACH ROW
BEGIN
    DELETE FROM Resultado WHERE r.idUsuarioFK = OLD.id;
END $$
DELIMITER ;

-- HU017: Consultar cantidad total de artículos 
DELIMITER $$
CREATE PROCEDURE MostrarCantidadArticulos(IN idUsuarioP INT)
BEGIN
	SELECT COUNT(a.id) AS ArticulosTotal FROM Articulo a
    INNER JOIN Resultado r ON a.idResultadoFK = r.id
    WHERE idUsuarioP = r.idUsuarioFK;
END $$
DELIMITER ;
 
-- HU018: Resultado de scraping 
DELIMITER $$
CREATE PROCEDURE RegistrarResultado(
    IN p_idUsuario INT
)
BEGIN
    DECLARE v_idResultado INT;
    DECLARE v_estado INT DEFAULT 2; #2: en proceso

    -- Crear un nuevo registro en la tabla Resultado
    INSERT INTO Resultado (idUsuarioFK, estado, fechaExtraccion)
    VALUES (p_idUsuario, v_estado, NOW());

    SET v_idResultado = LAST_INSERT_ID();

    -- Crear una notificación asociada al resultado
    INSERT INTO Notificacion (mensaje, tipo, idResultadoFK, leido)
    VALUES ('Scraping iniciado...', v_estado, v_idResultado, FALSE);
END$$
DELIMITER ;

-- HU019 Generar notificaciones ------------------------------------
delimiter $$
create procedure GenerarNotificacion(
    in p_id_resultado int,
    in p_mensaje varchar(50),
    in p_tipo int
)
begin
    insert into Notificacion (mensaje, tipo, leido, idResultadoFK) Values(p_mensaje, p_tipo, 0, p_id_resultado);
end$$
delimiter ;

-- HU020 Actualizar Resultado ------------------------------------------------------------
delimiter $$
create procedure actualizar_estado_resultado(
    in p_id_resultado int,
    in p_estado int
)
begin
    if exists (select 1 from resultado where id = p_id_resultado) then
        update resultado
        set estado = p_estado
        where id = p_id_resultado;
        select concat('Resultado con id ', p_id_resultado, ' actualizado a estado ', p_estado) as mensaje;
    else
        select concat('Error: Resultado con id ', p_id_resultado, ' no existe') as mensaje;
    end if;
end$$
delimiter ;

-- HU021 Eliminar Registros ------------------------------------------------ 
delimiter $$
create procedure eliminar_resultado(
    in p_id_resultado int
)
begin
    if exists (select 1 from Resultado where id = p_id_resultado) then
        delete from Resultado
        where id = p_id_resultado;
        select concat('Resultado con id ', p_id_resultado, ' y sus registros asociados han sido eliminados') as mensaje;
    else
        select concat('Error: Resultado con id ', p_id_resultado, ' no existe') as mensaje;
    end if;
end$$
delimiter ;


#HU022 Registrar Nuevo usuario en el sistema
Delimiter $$
Create Procedure RegistrarUsuario(IN xnombres VARCHAR(30), IN xapellidos VARCHAR(30), IN xcontrasena VARCHAR(30), IN xemail VARCHAR(30), OUT xmensaje VARCHAR(100))
Begin
    Insert Into Usuario(nombres, apellidos, contraseña, correo)
    Values (xnombres, xapellidos, xcontrasena, xemail);
    Set xmensaje = "El usuario se ha registrado correctamente";
End $$
Delimiter ;


#HU023 Iniciar sesion en la plataforma
Delimiter $$
Create Procedure IniciarSesion(IN xemail VARCHAR(30), IN xcontrasena VARCHAR(30), OUT xmensaje VARCHAR(100))
Begin
	IF EXISTS(Select 1 from Usuario where correo = xemail and contraseña = xcontrasena) THEN 
		Set xmensaje = "Inicio de sesion validado";
    ELSE 
		Set xmensaje = "El correo o la contrasena son incorrectos";
	End IF;
End $$
Delimiter ;

 #HU024 Cerrar sesion Simbolico en sql (FRONTEND BACKEND)
# esto ocurre en la interfaz


#HU025 Actualizar contraseña del usuario
Delimiter $$
Create Procedure ActualizarContrasena(IN xcorreo VARCHAR(50),IN xcontrasena VARCHAR(30),IN xnuevacontrasena VARCHAR(30), OUT xmensaje VARCHAR(200))
Begin
	IF EXISTS(Select 1 from Usuario where correo = xcorreo and contraseña = xcontrasena) THEN 
        update Usuario set contraseña = xnuevacontrasena where correo = xcorreo;
		Set xmensaje = "Se actualizo la contraseña correctamente";
    ELSE 
		Set xmensaje = "Credenciales incorrectas, verifica que esten bien escritas";
	End IF;
End $$
Delimiter ;

-- HU026 Eliminar Usuario---------------------------------------------- 
delimiter $$
create procedure EliminarUsuario(in p_id_usuario int)
begin
    delete from Usuario where id = p_id_usuario;
end $$
delimiter ;


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

#HU029 Marcar notificación como leída/no leída
DELIMITER $$
CREATE PROCEDURE AsignarLecturaNotificacion(
    IN idNotificacionP INT,
    IN leidaP BOOL
)
BEGIN
    UPDATE Notificacion SET leida = leidaP WHERE id = idNotificacionP;
END $$
DELIMITER ;

-- HU030 Eliminar notificacion --------------------------------------------
delimiter $$
create procedure eliminar_notificacion(
    in p_id_notificacion int
)
begin
    if exists (select 1 from notificacion where id = p_id_notificacion) then
        delete from notificacion where id = p_id_notificacion;
        select 'la notificacion fue eliminada correctamente' as mensaje;
    else
        select 'la notificacion no existe' as mensaje;
    end if;
end$$
delimiter ;

#HU031 Visualizar articulos almacenados
DELIMITER $$
CREATE PROCEDURE VisualizarAlmacenados(IN idUsuarioP INT, OUT xmensaje varchar(100))
BEGIN
    DECLARE v_numArticulos INT;
    SELECT COUNT(*) into v_numArticulos FROM Articulo a
    INNER JOIN Resultado r ON r.id = a.idResultadoFK
    WHERE r.idUsuarioFK = idUsuarioP;
    
    IF v_numArticulos > 0 THEN
        SELECT a.* FROM Articulo a
        INNER JOIN Resultado r ON r.id = a.idResultadoFK
        WHERE r.idUsuarioFK = idUsuarioP;
        SET xmensaje = "consulta exitosa...";
    ELSE
        SET xmensaje = "No hay articulos registrados para este usuario...";
    END IF;
END $$
DELIMITER ;

#HU032 Visualizar los resultados de la extracción de artículos
DELIMITER $$
CREATE PROCEDURE VisualizarResultadoExtraccion(
    IN idResultadoP INT
)
BEGIN
	SELECT r.*, COUNT(a.id) AS cantidad FROM Resultado r
    INNER JOIN Articulo a ON a.idResultadoFK = r.id
    WHERE r.id = idResultadoP
    GROUP BY r.id;
END $$
DELIMITER ;

#HU033 Copiar enlace de artículo
DELIMITER $$
CREATE PROCEDURE ObtenerURL(
	IN idArticuloP INT
)
BEGIN
	SELECT url FROM Fuente f 
    WHERE f.id = (SELECT idFuenteFK FROM ArticuloDetalle WHERE idArticuloFK = idArticuloP);
END $$
DELIMITER ;

#HU034 CambiarCorreo
DELIMITER $$
CREATE PROCEDURE CambiarCorreoUsuario(IN idUsuarioP INT, IN correoActual VARCHAR(50),IN correoNuevo VARCHAR(50),OUT mensaje VARCHAR(100))
BEGIN
    Update Usuario Set correo = correoNuevo Where correo = correoActual AND id = idUsuarioP;
    IF ROW_COUNT() > 0 THEN
        SET mensaje = "Se actualizo el correo exitosamente...";
    ELSE
        SET mensaje = "Error al actualizar el correo, verifica que el nuevo sea diferente al anterior";
    END IF;
END $$
DELIMITER ;

#HU035 Filtrar notificaciones por estado
DELIMITER $$
CREATE PROCEDURE EstadoNotificacion(IN idUsuarioP INT, IN tipoP INT, OUT mensajeP varchar(100))
BEGIN
    IF EXISTS (
        SELECT 1 FROM Resultado r 
        INNER JOIN Notificacion n ON n.idResultadoFK = r.id
        WHERE r.idUsuarioFK = idUsuarioP AND n.tipo = tipoP
    ) THEN
		SELECT n.*, r.id , r.estado, r.fechaExtraccion FROM Notificacion n
		INNER JOIN Resultado r ON n.idResultadoFK = r.id
		WHERE n.tipo = tipoP AND idUsuarioP = r.idUsuarioFK;
		SET mensajeP = "exito";
	ELSE
		SET mensajeP = "No hay notificaciones del tipo especificado";
	END IF;
END $$
DELIMITER ;

#HU036 Filtrar articulos por busqueda avanzada 
DELIMITER $$
Create Procedure FiltroArticuloBusquedaAvanzada(
    IN idUsuarioP INT,
    IN fecha1 datetime,
    IN fecha2 datetime, 
    IN cointitulo VARCHAR(100),
    IN claves VARCHAR(100), 
    IN temabuscar VARCHAR(100), 
    IN fuentes VARCHAR(100))
BEGIN
	SELECT a.*, f.nombre as "fuente", f.url from Articulo a
    INNER JOIN ArticuloDetalle ad on a.id = ad.idArticuloFK
    INNER JOIN Fuente f on ad.idFuenteFK = f.id
    INNER JOIN Resultado r ON r.id = a.idResultadoFK
	WHERE (fecha1 IS NOT NULL AND fecha2 IS NOT NULL AND Articulo.fecha BETWEEN fecha1 and fecha2)
		and ((cointitulo IS NULL OR Articulo.titular like CONCAT('%', cointitulo, '%')))
		and (claves IS NULL OR Articulo.titular like CONCAT('%', claves, '%') or Articulo.subtitulo like CONCAT('%', claves, '%') or Articulo.cuerpo like CONCAT('%', claves, '%'))
		and (temabuscar IS NULL OR Articulo.tema = temabuscar)
		and (fuentes IS NULL OR Fuente.dominio = fuentes)
        and idUsuarioP = r.idUsuarioFK
    Order by Articulo.fecha desc;
END $$
DELIMITER ;


#HU037 Cambiar nombre y apellido
DELIMITER $$
CREATE PROCEDURE CambiarNombreApellidoUsuario(IN idP INT,IN nuevoNombre VARCHAR(50),IN nuevoApellido VARCHAR(50),OUT mensaje VARCHAR(100)
)
BEGIN
    Update Usuario SET nombre = nuevoNombre, apellido = nuevoApellido Where id = idP;
    SET mensaje = "Nombre y apellido actualizados correctamente";
END $$
DELIMITER ;

#HU038 Evitar correos duplicados
DELIMITER $$
CREATE TRIGGER EvitarCorreosDuplicadosIns
BEFORE INSERT ON Usuario
FOR EACH ROW
BEGIN
    IF EXISTS (SELECT 1 FROM Usuario WHERE NEW.correo = correo) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'No se pudo crear el usuario, el correo ya existe!';
    END IF;
END$$
DELIMITER ;

DELIMITER $$
CREATE TRIGGER EvitarCorreosDuplicadosUpdate
BEFORE UPDATE ON Usuario
FOR EACH ROW
BEGIN
    IF EXISTS (SELECT 1 FROM Usuario WHERE NEW.correo = correo AND OLD.correo != NEW.correo) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Este correo ya existe, intente con otro.';
    END IF;
END $$
DELIMITER ;

#HU039 Consultar fuentes ordenadas alfabéticamente
DELIMITER $$
Create Procedure FuentesAlfabeticamente(
    in idUsuarioP int
)
Begin
    Select * from Fuente f
    inner join ArticuloDetalle ad on ad.idFuenteFK = f.id
    inner join Articulo a on a.id = ad.idArticuloFK
    inner join Resultado r on r.id = a.idResultadoFK
    where r.idUsuarioFK = idUsuarioP
    order by nombre desc;
End $$
DELIMITER ;

#HU040 Eliminar datos asociados a un articulo
DELIMITER $$
CREATE TRIGGER EliminarDatosAsociadosArticulo
BEFORE DELETE ON Articulo
FOR EACH ROW
BEGIN
    DECLARE idFuenteV INT;
    SELECT idFuenteFK INTO idFuenteV FROM ArticuloDetalle WHERE idArticuloFK = OLD.id;

    DELETE FROM ArticuloDetalle WHERE idArticuloFK = OLD.id;
    DELETE FROM Fuente WHERE id = idFuenteV;
END $$;
DELIMITER ;

#HU041 Eliminar datos asociados a un resultado
DELIMITER $$
CREATE TRIGGER EliminarDatosAsociadosResultado
BEFORE DELETE ON Resultado
FOR EACH ROW
BEGIN
	DELETE FROM Notificacion WHERE idResultadoFK = OLD.id;
    DELETE FROM Articulo WHERE idResultadoFK = OLD.id;
END $$
DELIMITER ;

----------------------------------------------------------------------------------------------------- PUNTOS SUSTENTACION JUAN OBANDO ---------------------------------------

-- RQF 15 COMO PROCEDIMIENTO:
Delimiter $$
Create Procedure RegistrarUsuario(IN xnombres VARCHAR(30), IN xapellidos VARCHAR(30), IN xcontrasena VARCHAR(30), IN xemail VARCHAR(30))
Begin
	Insert Into Usuario(nombres, apellidos, contraseña, correo)
    Values (xnombres, xapellidos, xcontrasena, xemail);
End $$
Delimiter ;

-- RQF 28 COMO VISTA:
create view vistaNotificaciones as
select
	notificacion.*,
    resultado.idUsuarioFK
from notificacion
inner join resultado on resultado.id = notificacion.idResultadoFK;

select * from VistaNotificaciones where idUsuarioFK = 1;
