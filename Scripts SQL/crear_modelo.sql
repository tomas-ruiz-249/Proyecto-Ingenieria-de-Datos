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
    descartado bool not null,
    numArticulos int not null default 0,
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
    foreign key (idResultadoFK) references Resultado(id)
);

create table Notificacion(
    id int primary key auto_increment not null,
	mensaje varchar(500) not null,
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

create table ArticulosUsuario(
	idUsuarioFK int not null,
    idArticulo int not null,
    descartado bool not null,
    favorito bool not null,
    foreign key (idUsuarioFK) references Usuario(id)
);
