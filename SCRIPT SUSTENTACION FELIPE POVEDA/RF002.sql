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
    leido bool not null,
    idResultadoFK int not null,
	foreign key (idResultadoFK) references Resultado(id)
);

create table ArticuloDetalle(
    idArticuloFK int unique not null,
    idFuenteFK int unique not null,
    foreign key (idArticuloFK) references Articulo(id),
	foreign key (idFuenteFK) references Fuente(id)
);

INSERT INTO Usuario (nombres, apellidos, contraseña, correo)
VALUES ("Juan", "Poveda", "vacaciones2025", "juanf.poveda@urosario.edu.co");

INSERT INTO Resultado(idUsuarioFK, estado, fechaExtraccion)
VALUES (1,1,NOW());

INSERT INTO Articulo(tema, titular, subtitulo, cuerpo, fecha, idResultadoFk, favorito)
VALUES 
('Tecnología', 
 'La inteligencia artificial revoluciona la educación', 
 'Herramientas digitales transforman la forma de aprender', 
 'En los últimos años, la inteligencia artificial ha permitido crear plataformas educativas personalizadas, adaptando los contenidos al ritmo y estilo de aprendizaje de cada estudiante.', 
 NOW(), 
 1, 
 TRUE),

('Ciencia', 
 'Descubren nueva especie en la selva amazónica', 
 'Un hallazgo que sorprende a la comunidad científica', 
 'Un grupo de biólogos ha identificado una nueva especie de rana fluorescente en una zona remota de la Amazonía. El descubrimiento podría aportar información clave sobre la biodiversidad tropical.', 
 NOW(), 
 1, 
 FALSE),

('Deportes', 
 'El equipo nacional logra histórico triunfo', 
 'Una victoria que quedará en los libros', 
 'En un partido lleno de emoción, la selección nacional venció al campeón defensor con un marcador de 3-2, asegurando su pase a la final después de una década de espera.', 
 NOW(), 
 1, 
 TRUE);
 

 -- Requisito funcional RQ002: Consultar Articulos
Delimiter $$
Create Procedure ConsultarArticulos(IN idUsuarioP INT)
Begin
	Select * from Articulo a
    INNER JOIN Resultado r ON r.id = a.idResultadoFK
    WHERE idUsuarioP = r.idUsuarioFK;
End $$
Delimiter ;

-- HU002: Consultar artículos del usuario
CALL ConsultarArticulos(1);