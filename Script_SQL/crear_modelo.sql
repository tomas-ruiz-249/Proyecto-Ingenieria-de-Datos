create database WebCrawler;
use WebCrawler;

create table Usuario(
    id int primary key auto_increment not null,
    nombres varchar(50) not null,
    apellidos varchar(50) not null,
    contraseña varchar(20) not null,
    correo varchar(50) not null 
);

create table Fuente(
    id int primary key auto_increment not null,
    url varchar(50) not null unique, 
    tipo varchar(50),
    nombre  varchar(50) not null unique
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
    tema varchar(50),
    titular varchar(100),
    subtitulo varchar(100),
    cuerpo varchar(100),
    fecha datetime,
    idResultadoFK int not null,
    favorito bool not null,
    foreign key (idResultadoFK) references Resultado(id)
);

create table Notificacion(
    id int primary key auto_increment not null,
	mensaje varchar(11) not null,
    tipo int not null, 
    idResultadoFK int not null,
	foreign key (idResultadoFK) references Resultado(id)
);

create table ArticuloDetalle(
    idArticuloFK int unique,
    idFuenteFK int unique,
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