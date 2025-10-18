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
        IFNULL(p_tema, ''),
        IFNULL(p_titular, ''),
        IFNULL(p_subtitulo, ''),
        IFNULL(p_cuerpo, ''),
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


#HU003 Mostrar articulos mas recientes
Delimiter $$
Create Procedure VisualizarArticulosRecientes()
Begin
    select a.*, r.fechaExtraccion from Articulo a
    INNER JOIN Resultado r ON a.idResultadoFK = r.id
    ORDER BY r.fechaExtraccion DESC;
End $$
Delimiter ;

/* -- HU003:  Mostrar artículos más recientes
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
DELIMITER ; */



-- HU004 Descartar articulos ---------------------------------------
delimiter $$
create procedure eliminarArticulo(
    in p_id_articulo int,
    in p_confirmacion bool
)
begin
    if not p_confirmacion then
        select 'eliminacion cancelada por el usuario' as mensaje;
    else
        if exists (select 1 from articulo where id = p_id_articulo) then
            delete from articulodetalle where idarticulofk = p_id_articulo;
            delete from articulo where id = p_id_articulo;
            select 'el articulo fue eliminado correctamente' as mensaje;
        else
            select 'el articulo no existe' as mensaje;
        end if;
    end if;
end$$
delimiter ;
call eliminarArticulo(4, true);
select * from articulo;

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
	select * from Articulo a
    Inner Join ArticuloDetalle ad on a.id = ad.idArticuloFK
    Inner Join Fuente f on ad.idFuenteFK = f.id
    Where f.nombre = fuentes
    Order by a.fecha desc;
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

-- HU012 Actualizar articulo -------------------------------------------------
delimiter $$
create procedure actualizar_articulo(
    in p_url varchar(50),
    in p_titular varchar(100),
    in p_subtitulo varchar(100),
    in p_cuerpo varchar(100),
    in p_fecha datetime,
    in p_id_resultado int
)
begin
    declare v_id_articulo int;

    select a.id as id_articulo into v_id_articulo
    from articulo a
    join articulodetalle ad on ad.idarticulofk = a.id
    join fuente f on f.id = ad.idfuentefk
    where f.url = p_url
    limit 1;

    if v_id_articulo is not null then
        if (select a.titular as titular from articulo a where a.id = v_id_articulo) <> p_titular
           or (select a.subtitulo as subtitulo from articulo a where a.id = v_id_articulo) <> p_subtitulo
           or (select a.cuerpo as cuerpo from articulo a where a.id = v_id_articulo) <> p_cuerpo
        then
            update articulo
            set titular = p_titular,
                subtitulo = p_subtitulo,
                cuerpo = p_cuerpo,
                fecha = p_fecha
            where id = v_id_articulo;

            insert into notificacion(mensaje, tipo, leido, idresultadofk)
            values('art_actualizado', 1, false, p_id_resultado);
        end if;
    end if;
end$$
delimiter ;
call actualizar_articulo(
    'www.portafolio.co',                          -- url de la fuente
    'El dólar sigue subiendo y rompe récord',      -- nuevo titular
    'Impacto en la economía colombiana',          -- nuevo subtitulo
    'El precio del dólar alcanzó 4.250 pesos...', -- nuevo cuerpo
    now(),                                        -- fecha de actualización
    2                                             -- idResultadoFK
);

-- HU013 Asignar Fuentes Iniciales ----------------------------------------------------------
create table FuentesIniciales(
    id int primary key auto_increment not null,
    idFuenteFK int not null,
    foreign key (idFuenteFK) references Fuente(id)
);

delimiter $$
create procedure registrar_fuente_inicial(
    in p_id_fuente int
)
begin
    if exists (select 1 from Fuente where id = p_id_fuente) then
        insert ignore into FuentesIniciales(idFuenteFK)
        values(p_id_fuente);

        select concat('Fuente con id ', p_id_fuente, ' registrada como fuente inicial') as mensaje;
    else
        select concat('Error: Fuente con id ', p_id_fuente, ' no existe') as mensaje;
    end if;
end$$
delimiter ;
call registrar_fuente_inicial(2);

-- HU014 Conservar articulo -------------------------------------------------
delimiter $$
create procedure conservarArticulo(
    in p_id_articulo int,
    in p_conservar boolean
)
begin
    declare v_titular varchar(100);

    if exists (select 1 from articulo where id = p_id_articulo) then
        select titular into v_titular from articulo where id = p_id_articulo;

        if p_conservar then
			rollback;
        else
            delete from articulodetalle where idarticulofk = p_id_articulo;
            delete from articulo where id = p_id_articulo;
        end if;
    else
        select 'el articulo no existe' as mensaje;
    end if;
end$$
delimiter ;
call conservarArticulo(8, false);
select * from articulo;
-- HU011: Evitar articulos duplicados
DELIMITER $$
CREATE TRIGGER EvitarArticulosDuplicados
BEFORE INSERT ON Articulo
FOR EACH ROW
BEGIN 
    IF EXISTS(SELECT * FROM Articulo 
                WHERE NEW.titular = titular 
                AND NEW.subtitulo = subtitulo
                AND NEW.cuerpo = cuerpo
                AND NEW.fecha = fecha) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Articulo duplicado, inserción cancelada';
    END IF;
END$$
DELIMITER ;

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
    DELETE FROM Resultado r WHERE r.idUsuarioFK = OLD.id;
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
    IN p_idUsuario,
)
BEGIN
    DECLARE v_idResultado INT;
    DECLARE v_estado INT DEFAULT 2; #2: en proceso

    -- Crear un nuevo registro en la tabla Resultado
    INSERT INTO Resultado (idUsuarioFK, estado, fechaExtraccion)
    VALUES (p_idUsuario, v_estado, NOW());

    SET v_idResultado = LAST_INSERT_ID();

    -- Crear una notificación asociada al resultado
    INSERT INTO Notificacion (mensaje, tipo, idResultadoFK)
    VALUES ('Scraping iniciado...', v_estado, v_idResultado);
END$$
DELIMITER ;

-- HU019 Generar notificaciones ------------------------------------
delimiter $$
create procedure registrar_articulo( ##Genera notificacion cuando registra un articulo o genera error 
    in p_tema varchar(50),
    in p_titular varchar(100),
    in p_subtitulo varchar(100),
    in p_cuerpo varchar(100),
    in p_fecha datetime,
    in p_id_resultado int
)
begin
    declare exit handler for sqlexception
    begin
        insert into notificacion(mensaje, tipo, leido, idresultadofk)
        values('art_err', 2, false, p_id_resultado);
    end;

    insert into articulo(tema, titular, subtitulo, cuerpo, fecha, idresultadofk, favorito)
    values(p_tema, p_titular, p_subtitulo, p_cuerpo, p_fecha, p_id_resultado, false);

    insert into notificacion(mensaje, tipo, leido, idresultadofk)
    values('art_ok', 1, false, p_id_resultado);
end$$
delimiter ;

delimiter $$
create procedure notificar_fin_busqueda( ##Genera notificacion cuando termina la busqueda o genera error
    in p_id_resultado int,
    in p_exito bool
)
begin
    if p_exito then
        insert into notificacion(mensaje, tipo, leido, idresultadofk)
        values('bus_ok', 3, false, p_id_resultado);
    else
        insert into notificacion(mensaje, tipo, leido, idresultadofk)
        values('bus_err', 4, false, p_id_resultado);
    end if;
end$$
delimiter ;
call registrar_articulo('politica', 'nuevo debate', 'senado', 'texto', now(), 4);
call notificar_fin_busqueda(5, true);
call notificar_fin_busqueda(5, false);

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
call actualizar_estado_resultado(3, 1);
select * from resultado;

-- HU021 Eliminar Registros ------------------------------------------------ (problema de safeKEY)
delimiter $$
create procedure eliminar_resultado_antiguo(
    in p_id_resultado int
)
begin
    if exists (select 1 from Resultado where id = p_id_resultado) then
        delete from ArticuloDetalle
        where idArticuloFK in (select id from Articulo where idResultadoFK = p_id_resultado);

        delete from Articulo
        where idResultadoFK = p_id_resultado;

        delete from Notificacion
        where idResultadoFK = p_id_resultado;

        delete from Resultado
        where id = p_id_resultado;

        select concat('Resultado con id ', p_id_resultado, ' y sus registros asociados han sido eliminados') as mensaje;
    else
        select concat('Error: Resultado con id ', p_id_resultado, ' no existe') as mensaje;
    end if;
end$$
delimiter ;
call eliminar_resultado_antiguo(3);

#HU022 Registrar Nuevo usuario en el sistema
Delimiter $$
Create Procedure RegistrarUsuario(IN xnombres VARCHAR(30), IN xapellidos VARCHAR(30), IN xcontrasena VARCHAR(30), IN xemail VARCHAR(30), OUT xmensaje VARCHAR(100))
Begin
	IF EXISTS(Select 1 from Usuario where correo = xemail) THEN 
		Set xmensaje = "El correo ya esta registrado... intente de nuevo con otra direccion de correo";
    ELSE 
		Insert Into Usuario(nombres, apellidos, contraseña, correo)
		Values (xnombres, xapellidos, xcontrasena, xemail);
        Set xmensaje = "El usuario se ha registrado correctamente";
	End IF;
End $$
Delimiter ;


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

/* -- HU023: Iniciar sesión en la plataforma  
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
DELIMITER ; */

/* #HU024 Cerrar sesion Simbolico en sql (FRONTEND BACKEND)
 DELIMITER $$
CREATE PROCEDURE CerrarSesionUsuario(OUT mensaje VARCHAR(100))
BEGIN
    SET mensaje = "Sesión cerrada correctamente";
END $$
DELIMITER ; */

/*
-- HU024: Cerrar sesión
DELIMITER $$

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

-- HU026 Eliminar Usuario---------------------------------------------- 
delimiter $$
create procedure eliminarArticulosUsuario(in p_id_usuario int)
begin
    declare v_existe int;
    declare v_cant int;

    select count(*) into v_existe
    from articulo a
    inner join resultado r on a.idresultadofk = r.id
    where r.idusuariofk = p_id_usuario;

    if v_existe = 0 then
        signal sqlstate '45000'
        set message_text = 'no existen articulos para este usuario.';
    else
        delete ad from articulodetalle ad
        inner join articulo a on ad.idarticulofk = a.id
        inner join resultado r on a.idresultadofk = r.id
        where r.idusuariofk = p_id_usuario;

        delete a from articulo a
        inner join resultado r on a.idresultadofk = r.id
        where r.idusuariofk = p_id_usuario;

        select v_existe as articulos_eliminados;
    end if;
end $$
delimiter ;
call eliminarArticulosUsuario(2);

#HU027 Crear notificación para el usuario
DELIMITER $$
CREATE PROCEDURE CrearNotificacion(
	IN idResultadoP INT,
    IN mensajeP VARCHAR(11),
    IN tipoP INT
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

*/

#HU025 Actualizar contraseña del usuario
Delimiter $$
Create Procedure ActualizarContrasena(IN xcorreo VARCHAR(50),IN xcontrasena VARCHAR(30),IN xnuevacontrasena VARCHAR(30), OUT xmensaje VARCHAR(200))
Begin
	IF EXISTS(Select 1 from Usuario where correo = xcorreo and contraseña = xcontrasena) THEN 
		Set xmensaje = "Informacion Correcta";
        update Usuario set Usuario.contrasena = xnuevacontrasena where correo = xcorreo;
    ELSE 
		Set xmensaje = "La contrasena es incorrecta";
	End IF;
End $$

Delimiter ;

/* -- HU025: Actualizar contraseña
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
DELIMITER ; */

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

-- HU030 Eliminar notificacion --------------------------------------------
delimiter $$
create procedure eliminar_notificacion(
    in p_id_notificacion int,
    in p_confirmacion bool
)
begin
    if not p_confirmacion then
        select 'eliminacion cancelada por el usuario' as mensaje;
    else
        if exists (select 1 from notificacion where id = p_id_notificacion) then
            delete from notificacion where id = p_id_notificacion;
            select 'la notificacion fue eliminada correctamente' as mensaje;
        else
            select 'la notificacion no existe' as mensaje;
        end if;
    end if;
end$$
delimiter ;
call eliminar_notificacion(23, true);
call eliminar_notificacion(10, false);
select * from notificacion;

#HU031 Visualizar articulos almacenados
DELIMITER $$
CREATE PROCEDURE VisualizarAlmacenados(IN idUsuarioP INT, OUT xmensaje varchar(100))
BEGIN
    DECLARE v_numArticulos INT;
    SELECT COUNT(*) into v_numArticulos FROM Articulo a
    INNER JOIN Resultado r ON r.id = a.idResultadoFK
    WHERE r.idUsuarioFK = idUsuarioP;
    
    IF v_numArticulos > 0 THEN
        SELECT a.* into v_numArticulos FROM Articulo a
        INNER JOIN Resultado r ON r.id = a.idResultadoFK
        WHERE r.idUsuarioFK = idUsuarioP;
    ELSE
        SET xmensaje = "No hay articulos registrados para este usuario...";
    END IF;
END $$
DELIMITER ;

#HU032 Visualizar los resultados de la extracción de artículos
DELIMITER $$
CREATE PROCEDURE VisualizarResultadoExtraccion()
BEGIN
	SELECT r.*, COUNT(Articulo.id) AS cantidad FROM Resultado r
    INNER JOIN Articulo a ON a.idResultadoFK = r.id
    GROUP BY r.id, r.idUsuarioFK, r.estado, r.fechaExtraccion
    ORDER BY r.fechaExtraccion DESC;
END $$
DELIMITER ;

#repetido
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
)
BEGIN
	SELECT url FROM Fuente f 
    WHERE idArticuloP = (SELECT idArticuloFK FROM ArticuloDetalle a WHERE f.id = a.idFuenteFK);
END $$
DELIMITER ;

/* #HU034 Copiar el enlace del articulo
DELIMITER $$
CREATE PROCEDURE ObtenerURL(IN idArticuloP INT, OUT urlP VARCHAR(50))
BEGIN
    SELECT url INTO urlP FROM Fuente f WHERE idArticuloP = (SELECT idArticuloFK FROM ArticuloDetalle a WHERE f.id = a.idFuenteFK);
END $$
DELIMITER ; */

#HU035 CambiarCorreo
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
DELIMITER $$
CREATE PROCEDURE EstadoNotificacion(IN idUsuarioP IN estadoP INT, OUT mensajeP varchar(100))
BEGIN
    IF EXISTS (SELECT 1 FROM Resultado WHERE estado = estadoP) THEN
		SELECT n.*, r.id , r.estado, r.fechaExtraccion FROM Notificacion n
		INNER JOIN Resultado r ON n.idResultadoFK = Resultado.id
		WHERE r.estado = estadoP AND idUsuarioP = r.idUsuarioFK;
	ELSE
		SET mensajeP = "El estado introducido es invalido";
	END IF;
END $$
DELIMITER ;

#HU037 Filtrar articulos por busqueda avanzada 
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
    INNER JOIN Fuente f on ArticuloDetalle.idFuenteFK = Fuente.id
    INNER JOIN Resultado r ON r.id = a.idResultadoFK
    WHERE idUsuarioP = r.idUsuarioFK;
    #Fecha
	WHERE (fecha1 IS NOT NULL AND fecha2 IS NOT NULL AND Articulo.fecha BETWEEN fecha1 and fecha2)
		and ((cointitulo IS NULL OR Articulo.titular like CONCAT('%', cointitulo, '%')))
		and (claves IS NULL OR Articulo.titular like CONCAT('%', claves, '%') or Articulo.subtitulo like CONCAT('%', claves, '%') or Articulo.cuerpo like CONCAT('%', claves, '%'))
		and (temabuscar IS NULL OR Articulo.tema = temabuscar)
		and (fuentes IS NULL OR Fuente.dominio = fuentes)
    Order by Articulo.fecha desc;
END $$
DELIMITER ;


#HU038 Cambiar nombre y apellido
DELIMITER $$
CREATE PROCEDURE CambiarNombreApellidoUsuario(IN idP INT,IN nuevoNombre VARCHAR(50),IN nuevoApellido VARCHAR(50),OUT mensaje VARCHAR(100)
)
BEGIN
    Update Usuario SET nombre = nuevoNombre, apellido = nuevoApellido Where id = idP;
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
    Select * from Fuente order by nombre desc;
End $$

Delimiter ;

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