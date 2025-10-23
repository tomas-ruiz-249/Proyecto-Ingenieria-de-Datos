DROP DATABASE WebCrawler;
create database WebCrawler;
use WebCrawler;

create table Usuario(
    id int primary key auto_increment not null,
    nombres varchar(50) not null,
    apellidos varchar(50) not null,
    contraseña varchar(30) not null,
    correo varchar(50) not null 
);

create table Fuente(
    id int primary key auto_increment not null,
    url varchar(500) not null, 
    tipo varchar(50),
    nombre  varchar(500) not null
);

create table Resultado(
    id int primary key auto_increment not null,
    idUsuarioFK int not null,
    estado  int not null,
    fechaExtraccion datetime not null,
	foreign key (idUsuarioFK) references Usuario(id)
);

create table Articulo(
    id int primary key auto_increment not null,
    tema varchar(500),
    titular varchar(500),
    subtitulo varchar(500),
    cuerpo text,
    fecha datetime,
    idResultadoFK int not null,
    favorito bool not null,
    foreign key (idResultadoFK) references Resultado(id)
);

create table Notificacion(
    id int primary key auto_increment not null,
	mensaje varchar(50) not null,
    tipo int not null, 
    idResultadoFK int not null,
	foreign key (idResultadoFK) references Resultado(id)
);

create table ArticuloDetalle(
    idArticuloFK int unique not null,
    idFuenteFK int unique not null,
    foreign key (idArticuloFK) references Articulo(id),
	foreign key (idFuenteFK) references Fuente(id)
);

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
call toggle_favorito(1, true);

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

/* select * from resultado;
select * from notificacion;
call eliminar_resultado_antiguo(2);
set sql_safe_updates = 0; */

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