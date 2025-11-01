using System.Data;
using MySqlConnector;
class Repository
{
    public Repository(string connectionString)
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

    public bool RegisterScraping(int userId)
    {
        var success = true;
        try
        {
            string query = $"""
                USE WebCrawler;
                CALL RegistrarResultado({userId})
            """;
            var cmd = new MySqlCommand(query, Connection);
            int rowsAffected = cmd.ExecuteNonQuery();
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
            Console.WriteLine("Scraping attempt registered");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Error in registering scraping attempt");
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

    public List<Notificacion> GetNotifications()
    {
        var notifications = new List<Notificacion>();
        try
        {
            string procedure = "ConsultarNotificaciones";
            var cmd = new MySqlCommand(procedure, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("idUsuarioP", 1);
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
        catch(Exception e)
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
                    reader.GetDateTime("fechaExtraccion")
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
    private MySqlConnection Connection;
    public bool Connected { get; private set; }
}
