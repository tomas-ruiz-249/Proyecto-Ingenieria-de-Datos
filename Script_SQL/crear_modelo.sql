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
);



INSERT INTO Usuario (nombres, apellidos, contraseña, correo) VALUES
('Ana', 'Pérez', 'ana123', 'ana@gmail.com'),
('Luis', 'Gómez', 'luis456', 'luis@hotmail.com'),
('María', 'Rodríguez', 'maria789', 'maria@yahoo.com'),
('Carlos', 'López', 'carloz1', 'carlos@gmail.com'),
('Sofía', 'Martínez', 'sofia22', 'sofia@outlook.com'),
('Jorge', 'García', 'jorge33', 'jorge@gmail.com'),
('Camila', 'Moreno', 'cami44', 'camila@hotmail.com'),
('David', 'Ramírez', 'david55', 'david@yahoo.com'),
('Lucía', 'Torres', 'lucia66', 'lucia@gmail.com'),
('Mateo', 'Hernández', 'mateo77', 'mateo@outlook.com');

INSERT INTO Fuente (dominio, tipo, nombre) VALUES
('www.eltiempo.com', 'noticias', 'El Tiempo'),
('www.portafolio.co', 'economía', 'Portafolio'),
('www.semana.com', 'revista', 'Semana'),
('www.caracol.com.co', 'radio', 'Caracol Radio'),
('www.elespectador.com', 'noticias', 'El Espectador'),
('www.bluradio.com', 'radio', 'Blu Radio'),
('www.infobae.com', 'internacional', 'Infobae'),
('www.larepublica.co', 'finanzas', 'La República'),
('www.pulzo.com', 'entretenimiento', 'Pulzo'),
('www.noticiasrcn.com', 'televisión', 'Noticias RCN');

INSERT INTO Resultado (idUsuarioFK, estado, fechaExtraccion) VALUES
(1, 1, NOW()),
(2, 1, NOW()),
(3, 0, NOW()),
(4, 1, NOW()),
(5, 1, NOW()),
(6, 1, NOW()),
(7, 0, NOW()),
(8, 1, NOW()),
(9, 1, NOW()),
(10, 1, NOW());

INSERT INTO Articulo (tema, titular, subtitulo, cuerpo, fecha, idResultadoFK, favorito) VALUES
('Política', 'El Congreso aprueba nueva ley electoral', 'Cambios en las elecciones 2026', 'El Congreso de la República aprobó hoy...', NOW(), 1, FALSE),
('Economía', 'El dólar alcanza su punto más alto del año', NULL, 'El precio del dólar subió a 4.200 pesos...', NOW(), 2, TRUE),
('Deportes', 'Colombia clasifica al Mundial', 'Gran triunfo ante Argentina', 'La selección logró su clasificación...', NOW(), 3, FALSE),
('Salud', 'Nuevas medidas contra el dengue', '', 'El Ministerio de Salud anunció nuevas campañas...', NOW(), 4, FALSE),
('Tecnología', 'Lanzan nuevo celular plegable', 'Innovación en pantallas flexibles', 'La marca X presentó su nuevo modelo...', NOW(), 5, FALSE),
('Entretenimiento', 'Nueva película rompe récords', NULL, 'La cinta superó los 2 millones de espectadores...', NOW(), 6, TRUE),
('Educación', 'Reformas en el sistema universitario', '', 'El Ministerio de Educación propone cambios...', NOW(), 7, FALSE),
('Ambiente', 'Incendios afectan la Amazonía', 'Se pierden miles de hectáreas', 'Expertos alertan sobre el impacto ambiental...', NOW(), 8, FALSE),
('Cultura', 'Festival de música regresa con fuerza', '', 'El evento más esperado del año vuelve...', NOW(), 9, FALSE),
('Internacional', 'Tensiones entre países aumentan', 'Crisis diplomática en curso', 'Las relaciones entre ambos países se deterioran...', NOW(), 10, FALSE);

INSERT INTO Notificacion (mensaje, tipo, idResultadoFK) VALUES
('Artículo nuevo', 1, 1),
('Actualización exitosa', 2, 2),
('Error en fuente', 3, 3),
('Nuevo resultado', 1, 4),
('Fuente añadida', 2, 5),
('Artículo nuevo', 1, 6),
('Actualización exitosa', 2, 7),
('Error en scraping', 3, 8),
('Nuevo resultado', 1, 9),
('Artículo nuevo', 1, 10);

INSERT INTO ArticuloDetalle (idArticuloFK, idFuenteFK) VALUES
(1, 1),
(2, 2),
(3, 3),
(4, 5),
(5, 7),
(6, 9),
(7, 8),
(8, 4),
(9, 6),
(10, 10);


#HU015 Registrar Usuario
Delimiter $$
Create Procedure RegistrarUsuario(IN xnombres VARCHAR(30), IN xapellidos VARCHAR(30), IN xcontrasena VARCHAR(30), IN xemail VARCHAR(30))
Begin
	Insert Into Usuario(nombres, apellidos, contraseña, correo)
    Values (xnombres, xapellidos, xcontrasena, xemail);
End $$
Delimiter ;
Call RegistrarUsuario("Jaime", "Gamba", "Saitama", "pepito@gmail.com");

#--------------------------------------------------------------------------------------------------------------------------------------------

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


#--------------------------------------------------------------------------------------------------------------------------------------------

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

#--------------------------------------------------------------------------------------------------------------------------------------------

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

#--------------------------------------------------------------------------------------------------------------------------------------------

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

#Trigger HU001
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

#--------------------------------------------------------------------------------------------------------------------------------------------

#HU002 Consultar Articulos
Delimiter $$
Create Procedure ConsultarArticulos()
Begin
	Select * from Articulo;
End $$
Delimiter ;

call ConsultarArticulos();

#--------------------------------------------------------------------------------------------------------------------------------------------

#HU033 Mostrar cantidad de articulos
Delimiter $$
Create Procedure MostrarCantidadArticulos()
Begin
	Select Count(*) FROM Articulos;
End $$
Delimiter ;

call MostrarCantidadArticulos();

#--------------------------------------------------------------------------------------------------------------------------------------------

#HU017 Consultar articulos (Despues scraping = total)
Delimiter $$
Create Procedure ConsultarCantidadArticulos(OUT total INT)
Begin
    Select COUNT(*) Into total From Articulo;
End $$

Delimiter ;

#--------------------------------------------------------------------------------------------------------------------------------------------

#HU040 Consultar Fuentes Alfabeticamente
Delimiter $$
Create Procedure FuentesAlfabeticamente()
Begin
    Select * from Fuente order by dominio desc;
End $$

Delimiter ;

call FuentesAlfabeticamente()

#--------------------------------------------------------------------------------------------------------------------------------------------
#Falta atributo enlace del articulo

#HU034 Copiar el enlace del articulo
DELIMITER $$
CREATE PROCEDURE ObtenerURL(IN idArticuloP INT, OUT urlP VARCHAR(50))
BEGIN
    SELECT url INTO urlP FROM Fuente f WHERE idArticuloP = (SELECT idArticuloFK FROM ArticuloDetalle a WHERE f.id = a.idFuenteFK);
END $$
DELIMITER ;

#--------------------------------------------------------------------------------------------------------------------------------------------

#HU006 Filtrar articulos por rango de fechas
Delimiter $$
Create Procedure FiltroArticulosRangoFechas(IN fecha1 datetime, IN fecha2 datetime)
Begin
	select * from Articulo where fecha between fecha1 and fecha2 order by fecha asc;
    
End $$

Delimiter ;

#--------------------------------------------------------------------------------------------------------------------------------------------

#HU007 Filtrar Articulos por coincidencias en titulo
Delimiter $$
Create Procedure FiltroArticuloCoincidenciasTitulo(IN palabras VARCHAR(50))
Begin
	select * from Articulo where titular like CONCAT('%', palabras, '%');
    
End $$

Delimiter ;

#--------------------------------------------------------------------------------------------------------------------------------------------

#HU008 Filtrar Por Palabras Clave
Delimiter $$
Create Procedure FiltroArticuloPalabrasClave(IN claves VARCHAR(100))
Begin
	select * from Articulo where titular like CONCAT('%', claves, '%') or subtitulo like CONCAT('%', claves, '%') or cuerpo like CONCAT('%', palabras, '%')
    order by fecha desc;
End $$

Delimiter ;

#--------------------------------------------------------------------------------------------------------------------------------------------

#HU009 Filtrar Articulos por tema
Delimiter $$
Create Procedure FiltroArticuloTema(IN temabuscar VARCHAR(100))
Begin
	select * from Articulo where tema = temabuscar;
    
End $$

Delimiter ;

#--------------------------------------------------------------------------------------------------------------------------------------------

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

#--------------------------------------------------------------------------------------------------------------------------------------------

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

#--------------------------------------------------------------------------------------------------------------------------------------------

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

#--------------------------------------------------------------------------------------------------------------------------------------------

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

#--------------------------------------------------------------------------------------------------------------------------------------------

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

#--------------------------------------------------------------------------------------------------------------------------------------------

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

#--------------------------------------------------------------------------------------------------------------------------------------------

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

#--------------------------------------------------------------------------------------------------------------------------------------------

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

#--------------------------------------------------------------------------------------------------------------------------------------------

#HU024 Cerrar sesion Simbolico en sql (FRONTEND BACKEND)
DELIMITER $$

CREATE PROCEDURE CerrarSesionUsuario(OUT mensaje VARCHAR(100))
BEGIN
    SET mensaje = "Sesión cerrada correctamente";
END $$

DELIMITER ;

DELIMITER $$

#--------------------------------------------------------------------------------------------------------------------------------------------

#HU035 CambiarCorreoe
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

DELIMITER $$

#--------------------------------------------------------------------------------------------------------------------------------------------

#HU038 Cambiar nombre y apellido
CREATE PROCEDURE CambiarNombreApellidoUsuario(IN correo VARCHAR(50),IN nuevoNombre VARCHAR(50),IN nuevoApellido VARCHAR(50),OUT mensaje VARCHAR(100)
)
BEGIN
    Update Usuario SET nombre = nuevoNombre, apellido = nuevoApellido Where correo = correo;
    SET mensaje = "Nombre y apellido actualizados correctamente";
END $$

DELIMITER ;
