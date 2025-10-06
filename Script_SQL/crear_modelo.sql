drop database WebCrawler;
create database WebCrawler;
use WebCrawler;

create table Usuario(
    id int primary key auto_increment not null,
    nombre varchar(30) not null,
    contraseña varchar(20) not null
);
select * from Usuario;

create table Resultado(
    id int primary key auto_increment not null,
    url varchar(500),
    estado  int,
    fecha_extraccion datetime,
    id_fuenteFK int,
    foreign key (id_fuenteFK) references Fuente(id)
);

create table Fuente(
    id int primary key auto_increment not null,
    dominio varchar(50),
    tipo int,
    nombre_fuente varchar(50)
);

create table Articulo(
    id int primary key auto_increment not null,
    tema varchar(50),
    titular varchar(100),
    subtitulo varchar(100),
    cuerpo varchar(100),
    fecha datetime,
    id_usuario int,
    id_resultado int,
    foreign key (id_usuario) references Usuario(id),
    foreign key (id_resultado) references Resultado(id)
);

create table Notificacion(
    id int primary key auto_increment not null,
	mensaje varchar(11),
    tipo int
);

create table Notificacion_resultado(
    id_notificacion int,
    id_resultado int,
    foreign key (id_notificacion) references Notificacion(id),
    foreign key (id_resultado) references Resultado(id)
);