using System.Data;
using MySqlConnector;

class RepositorySQL
{
    public RepositorySQL(string connectionString)
    {
        Connection = new MySqlConnection(connectionString);
        Connected = true;
    }

    public bool ConnectToServer()
    {
        try
        {
            Connection.Open();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
            Connected = false;
        }
        return Connected;
    }

    public int RegisterScraping(int userId)
    {
        int resultId = -1;
        try
        {
            string query = "RegistrarResultado";
            var cmd = new MySqlCommand(query, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_idUsuario", userId);
            cmd.Parameters.AddWithValue("v_idResultado", -1);
            cmd.Parameters["v_idResultado"].Direction = ParameterDirection.Output;
            int rowsAffected = cmd.ExecuteNonQuery();
            resultId = Convert.ToInt32(cmd.Parameters["v_idResultado"].Value);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        if (resultId != -1)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Scraping attempt registered");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Error in registering scraping attempt");
            Console.ForegroundColor = ConsoleColor.White;
        }

        return resultId;
    }
    
    public bool GenerateNotification(int idResult, string message, int type)
    {
        var success = false;
        try
        {
            var procedure = "GenerarNotificacion";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_id_resultado", idResult);
            cmd.Parameters.AddWithValue("p_mensaje", message);
            cmd.Parameters.AddWithValue("p_tipo", type);
            var rowsAffected = cmd.ExecuteNonQuery();
            success = true;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return success;
    }
    
    public List<ArticleSource> FilterArticles(
        int userId,
        string? titular,
        string? palabrasClave,
        string? tema,
        string? nombreFuente,
        string? fecha1,
        string? fecha2
    )
    {
        var articulos = new List<ArticleSource>();
        try
        {
            var procedure = "FiltrarArticulos";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("idUsuarioP", userId);
            cmd.Parameters.AddWithValue("fecha1", fecha1);
            cmd.Parameters.AddWithValue("fecha2", fecha2);
            cmd.Parameters.AddWithValue("cointitulo", titular);
            cmd.Parameters.AddWithValue("claves", palabrasClave);
            cmd.Parameters.AddWithValue("temaBuscar", tema);
            cmd.Parameters.AddWithValue("fuentes", nombreFuente);
            var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var id = rdr.GetInt32("id");
                var temaArticulo = rdr.GetString("tema");
                var titularArticulo = rdr.GetString("titular");
                var subtitulo = rdr.GetString("subtitulo");
                var cuerpo = rdr.GetString("cuerpo");
                var favorito = rdr.GetBoolean("favorito");
                string fecha = rdr["fecha"] == DBNull.Value
                    ? string.Empty
                    : rdr.GetDateTime("fecha").ToString();
                var idResultado = rdr.GetInt32("idResultadoFK");
                var articulo = new Articulo(id, temaArticulo, titularArticulo, subtitulo, cuerpo, fecha, idResultado, favorito);

                var idFuente = rdr.GetInt32("idFuente");
                var url = rdr.GetString("url");
                var tipo = rdr.GetString("tipo");
                var nombre = rdr.GetString("nombre");
                var fuente = new Fuente(idFuente, url, tipo, nombre);

                articulos.Add(new ArticleSource(articulo, fuente));
            }
            rdr.Close();
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return articulos;
    }


    public bool SetResultFinished(int resultId, int articleCount)
    {
        var success = false;
        try
        {
            var query = $"UPDATE Resultado SET estado = 0, numArticulos = {articleCount} WHERE id = {resultId}";
            var cmd = new MySqlCommand(query, Connection);
            int rowsAffected = cmd.ExecuteNonQuery();
            success = true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return success;
    }

    public bool StoreArticleWithSource(Articulo a, Fuente f, int idResultado)
    {
        var success = true;
        try
        {
            DateTime? fechaDateTime = null;
            if (!string.IsNullOrEmpty(a.Fecha) && DateTime.TryParse(a.Fecha, out DateTime parsedDate))
            {
                fechaDateTime = parsedDate;
            }

            string query = "RegistrarArticulo";

            var cmd = new MySqlCommand(query, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@p_tema", a.Tema ?? "");
            cmd.Parameters.AddWithValue("@p_titular", a.Titular ?? "");
            cmd.Parameters.AddWithValue("@p_subtitulo", a.Subtitulo ?? "");
            cmd.Parameters.AddWithValue("@p_cuerpo", a.Cuerpo ?? "");
            cmd.Parameters.AddWithValue("@p_fecha", fechaDateTime ?? (object)DBNull.Value); // Use DBNull if parsing failed
            cmd.Parameters.AddWithValue("@p_idResultadoFK", idResultado);
            cmd.Parameters.AddWithValue("@p_favorito", false);
            cmd.Parameters.AddWithValue("@p_url", f.Url ?? "");
            cmd.Parameters.AddWithValue("@p_tipo", "articulo");
            cmd.Parameters.AddWithValue("@p_nombreFuente", f.Nombre ?? "");
            cmd.Parameters.Add("@p_idArticulo", MySqlDbType.Int32);
            cmd.Parameters["@p_idArticulo"].Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@p_idFuente", MySqlDbType.Int32);
            cmd.Parameters["@p_idArticulo"].Direction = ParameterDirection.Output;

            int rowsAffected = cmd.ExecuteNonQuery();
            int idArticulo = (int)cmd.Parameters["@p_idArticulo"].Value!;
            a.Id = idArticulo;
            int idFuente = (int)cmd.Parameters["@p_idFuente"].Value!;
            f.Id = idFuente;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
            success = false;
        }

        if (success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Article registered succesfully");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Article was not registered...");
            Console.ForegroundColor = ConsoleColor.White;
        }
        return success;
    }
    public bool ChangePassword(int userId, string newPassword)
    {
        var success = false;
        try
        {
            var procedure = "ActualizarContrasena";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("xnuevacontrasena", newPassword);
            cmd.Parameters.AddWithValue("idUsuario", userId);
            var rowsAffected = cmd.ExecuteNonQuery();
            success = true;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;

        }
        return success;
    }
    public bool DeleteUser(int userId)
    {
        var success = false;
        try
        {
            var procedure = "EliminarUsuario";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_id_usuario", userId);
            var rowsAffected = cmd.ExecuteNonQuery();
            success = true;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;

        }
        return success;
    }
    public bool EditEmail(int userId, string email)
    {
        var success = false;
        try
        {
            var procedure = "CambiarCorreoUsuario";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("idUsuarioP", userId);
            cmd.Parameters.AddWithValue("correoNuevo", email);
            var rowsAffected = cmd.ExecuteNonQuery();
            System.Console.WriteLine(rowsAffected);
            success = true;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;

        }
        return success;
    }
    public bool EditUser(int userId, string name, string lastName)
    {
        var success = false;
        try
        {
            var procedure = "CambiarNombreApellidoUsuario";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("idP", userId);
            cmd.Parameters.AddWithValue("nuevoNombre", name);
            cmd.Parameters.AddWithValue("nuevoApellido", lastName);
            var rowsAffected = cmd.ExecuteNonQuery();
            success = true;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;

        }
        return success;
    }
    public bool DeleteResults(int userId)
    {
        var success = false;
        try
        {
            var procedure = "eliminar_resultado";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_id_usuario", userId);
            var rowsAffected = cmd.ExecuteNonQuery();
            success = true;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;

        }
        return success;
    }
    public bool DiscardArticles(int userId, List<int> discardIds)
    {
        var success = false;
        try
        {
            foreach (var id in discardIds)
            {
                var procedure = "eliminarArticulo";
                var cmd = new MySqlCommand(procedure, Connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_id_usuario", userId);
                cmd.Parameters.AddWithValue("p_id_articulo", id);
                var rowsAffected = cmd.ExecuteNonQuery();
            }
            success = true;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;

        }
        return success;
    }
    public List<Result> GetResults(int userId)
    {
        var resultList = new List<Result>();
        try
        {
            var procedure = "VisualizarResultadoExtraccion";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("idUsuarioP", userId);
            var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var id = rdr.GetInt32("id");
                var idUsuario = rdr.GetInt32("idUsuarioFK");
                var estado = rdr.GetInt32("estado");
                var fecha = rdr.GetDateTime("fechaExtraccion").ToString();
                var cantidad = rdr.GetInt32("cantidad");
                var result = new Result(id, idUsuario, estado, fecha);
                result.Cantidad = cantidad;
                resultList.Add(result);
            }
            rdr.Close();
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return resultList;
    }
    public bool ToggleArticleFavorite(int articleId)
    {
        var success = false;
        try
        {
            var procedure = "toggle_favorito";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_id_articulo", articleId);
            var rowsAffected = cmd.ExecuteNonQuery();
            success = true;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return success;
    }
    public List<ArticleSource> GetArticlesAndSources(int userId)
    {
        var articulos = new List<ArticleSource>();
        try
        {
            var procedure = "ConsultarArticulos";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("idUsuarioP", userId);
            var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var id = rdr.GetInt32("id");
                var tema = rdr.GetString("tema");
                var titular = rdr.GetString("titular");
                var subtitulo = rdr.GetString("subtitulo");
                var cuerpo = rdr.GetString("cuerpo");
                var favorito = rdr.GetBoolean("favorito");
                string fecha = rdr["fecha"] == DBNull.Value
                    ? string.Empty
                    : rdr.GetDateTime("fecha").ToString();
                var idResultado = rdr.GetInt32("idResultadoFK");
                var articulo = new Articulo(id, tema, titular, subtitulo, cuerpo, fecha, idResultado, favorito);

                var idFuente = rdr.GetInt32("idFuente");
                var url = rdr.GetString("url");
                var tipo = rdr.GetString("tipo");
                var nombre = rdr.GetString("nombre");
                var fuente = new Fuente(idFuente, url, tipo, nombre);

                articulos.Add(new ArticleSource(articulo, fuente));
            }
            rdr.Close();
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return articulos;
    }
    public bool RegisterUser(Usuario u)
    {
        var success = true;
        try
        {
            string procedure = "RegistrarUsuario";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("xnombres", u.Nombres);
            cmd.Parameters.AddWithValue("xapellidos", u.Apellidos);
            cmd.Parameters.AddWithValue("xcontrasena", u.Contraseña);
            cmd.Parameters.AddWithValue("xemail", u.Correo);
            var rowsAffected = cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
            success = false;
        }
        return success;
    }
    public Usuario GetUser(int id)
    {
        try
        {
            string procedure = "ConsultarUsuario";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("idP", id);
            var rdr = cmd.ExecuteReader();
            var usuario = new Usuario(-1, "", "", "");
            while (rdr.Read())
            {
                var nombres = rdr.GetString("nombres");
                id = rdr.GetInt32("id");
                var apellidos = rdr.GetString("apellidos");
                var correo = rdr.GetString("correo");
                usuario.Id = id;
                usuario.Nombres = nombres;
                usuario.Apellidos = apellidos;
                usuario.Correo = correo;
            }
            rdr.Close();
            return usuario;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return new Usuario(-1, "", "", "");
    }

    public int ValidateLogin(string correo, string contraseña)
    {
        var id = -1;
        try
        {
            string procedure = "IniciarSesion";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("xemail", correo);
            cmd.Parameters.AddWithValue("xcontrasena", contraseña);
            cmd.Parameters.AddWithValue("xidUsuario", 0);
            cmd.Parameters["xidUsuario"].Direction = ParameterDirection.Output;
            int rowsAffected = cmd.ExecuteNonQuery();

            id = (int)cmd.Parameters["xidUsuario"].Value!;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return id;
    }

    public List<Notificacion> FilterNotifications(int idUsuario, int? tipo, bool? leido)
    {
        var notifications = new List<Notificacion>();
        try
        {
            string procedure = "FiltrarNotificacion";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("idUsuarioP", idUsuario);
            cmd.Parameters.AddWithValue("tipoP", tipo);
            cmd.Parameters.AddWithValue("leidoP", leido);
            var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var id = rdr.GetInt32("id");
                var mensaje = rdr.GetString("mensaje");
                var tipoNotif = rdr.GetInt32("tipo");
                var leidoNotif = rdr.GetBoolean("leido");
                var idResultado = rdr.GetInt32("idResultadoFK");
                var notif = new Notificacion(id, mensaje, tipoNotif, leidoNotif, idResultado);
                notifications.Add(notif);
            }
            rdr.Close();
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return notifications;
    }

    public List<Notificacion> GetNotifications(int idUsuario)
    {
        var notifications = new List<Notificacion>();
        try
        {
            string procedure = "ConsultarNotificaciones";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("idUsuarioP", idUsuario);
            cmd.Parameters.AddWithValue("mensajeP", "");
            cmd.Parameters["mensajeP"].Direction = ParameterDirection.Output;
            var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var id = rdr.GetInt32("id");
                var mensaje = rdr.GetString("mensaje");
                var tipo = rdr.GetInt32("tipo");
                var leido = rdr.GetBoolean("leido");
                var idResultado = rdr.GetInt32("idResultadoFK");
                var notif = new Notificacion(id, mensaje, tipo, leido, idResultado);
                notifications.Add(notif);
            }
            rdr.Close();
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return notifications;
    }

    public void UpdateReadNotification(int notifId)
    {
        try
        {
            string procedure = "AsignarLecturaNotificacion";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("idNotificacionP", notifId);
            var rowsAffected = cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public void DeleteNotification(int notifId)
    {
        try
        {
            string procedure = "eliminar_notificacion";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_id_notificacion", notifId);
            var rowsAffected = cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public int GetLastResultId()
    {
        var id = 0;
        try
        {
            string query = $"""
                SELECT LAST_INSERT_ID();
            """;
            var cmd = new MySqlCommand(query, Connection);
            var obj = cmd.ExecuteScalar();

            if (obj != null)
            {
                id = Convert.ToInt32(obj);
                return id;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return -1;
    }

    public Result? GetLastResult(int id)
    {
        Result result = default;
        try
        {
            string query = $"""
                SELECT * FROM Resultado WHERE {id} = id;
            """;
            var cmd = new MySqlCommand(query, Connection);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                result = new Result(
                    reader.GetInt32("id"),
                    reader.GetInt32("idUsuarioFK"),
                    reader.GetInt32("estado"),
                    reader.GetDateTime("fechaExtraccion").ToString()
                );
            }
            reader.Close();
            return result;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return result;
    }
    private readonly MySqlConnection Connection;
    public bool Connected { get; private set; }
}
