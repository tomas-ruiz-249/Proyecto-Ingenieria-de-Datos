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
    url varchar(50) not null, 
    tipo varchar(50),
    nombre  varchar(50) not null
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
    leido bool not null,
    idResultadoFK int not null,
	foreign key (idResultadoFK) references Resultado(id)
);

create table ArticuloDetalle(
    idArticuloFK int unique,
    idFuenteFK int unique,
    foreign key (idArticuloFK) references Articulo(id),
	foreign key (idFuenteFK) references Fuente(id)
<<<<<<< HEAD
);
=======
);

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
>>>>>>> b64434d (organizados hu felipe)
