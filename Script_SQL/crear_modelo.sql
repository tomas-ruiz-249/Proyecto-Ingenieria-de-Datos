create database BaseCrawler;
use BaseCrawler;

create table Usuario(
    id int primary key auto_increment not null,
    nombres varchar(50) not null,
    apellidos varchar(50) not null,
    contraseña varchar(20) not null,
    correo varchar(50) not null 
);

create table Fuente(
    id int primary key auto_increment not null,
    dominio varchar(50) not null, 
    tipo varchar(50),
    nombre  varchar(50) not null
);

create table Articulo(
    id int primary key auto_increment not null,
    tema varchar(50),
    titular varchar(100),
    subtitulo varchar(100),
    cuerpo varchar(100),
    fecha datetime,
    idResultado int not null,
    favortio bool not null
);

create table Resultado(
    id int primary key auto_increment not null,
    idUsuario int not null,
    estado  int not null,
    fechaExtraccion datetime not null,
	foreign key (idUsuario) references Usuario(id),
    foreign key (idUsuario) references Articulo(id)
);

create table Notificacion(
    id int primary key auto_increment not null,
	mensaje varchar(11) not null,
    tipo int not null, 
    idResultado int not null,
	foreign key (idResultado) references Resultado(id)
);

create table ArticuloDetalle(
    idArticulo int,
    idFuente int,
    foreign key (idArticulo) references Articulo(id),
	foreign key (idFuente) references Fuente(id)
);