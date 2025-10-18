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
);