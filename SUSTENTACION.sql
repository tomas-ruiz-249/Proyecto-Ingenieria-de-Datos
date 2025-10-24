use WebCrawler;

-- RQF 15 COMO PROCEDIMIENTO:
Delimiter $$
Create Procedure RegistrarUsuario(IN xnombres VARCHAR(30), IN xapellidos VARCHAR(30), IN xcontrasena VARCHAR(30), IN xemail VARCHAR(30))
Begin
	Insert Into Usuario(nombres, apellidos, contraseña, correo)
    Values (xnombres, xapellidos, xcontrasena, xemail);
End $$
Delimiter ;

-- RQF 28 COMO VISTA:
create view vistaNotificaciones as
select
	notificacion.*,
    resultado.idUsuarioFK
from notificacion
inner join resultado on resultado.id = notificacion.idResultadoFK;

select * from VistaNotificaciones where idUsuarioFK = 1;
