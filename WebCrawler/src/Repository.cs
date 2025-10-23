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
            Console.WriteLine(ex.ToString());
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
            Console.WriteLine(ex.ToString());
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

    public bool StoreArticle(Articulo a, Uri url, int idResultado)
    {
        var success = true;
        try
        {
            DateTime? fechaDateTime = null;
            if (!string.IsNullOrEmpty(a.Fecha) && DateTime.TryParse(a.Fecha, out DateTime parsedDate))
            {
                fechaDateTime = parsedDate;
            }

            string query = @"
				CALL RegistrarArticulo(
                    @tema,
                    @titular,
                    @subtitulo,
                    @cuerpo,
                    @fecha,
                    @idResultado,
                    @favorito,
                    @url,
                    @tipo,
                    @nombreFuente
                );
			";

            var cmd = new MySqlCommand(query, Connection);
            cmd.Parameters.AddWithValue("@tema", a.Tema ?? "");
            cmd.Parameters.AddWithValue("@titular", a.Titular ?? "");
            cmd.Parameters.AddWithValue("@subtitulo", a.Subtitulo ?? "");
            cmd.Parameters.AddWithValue("@cuerpo", a.Cuerpo ?? "");
            cmd.Parameters.AddWithValue("@fecha", fechaDateTime ?? (object)DBNull.Value); // Use DBNull if parsing failed
            cmd.Parameters.AddWithValue("@idResultado", idResultado);
            cmd.Parameters.AddWithValue("@favorito", false);
            cmd.Parameters.AddWithValue("@url", url.AbsolutePath ?? "");
            cmd.Parameters.AddWithValue("@tipo", "articulo");
            cmd.Parameters.AddWithValue("@nombreFuente", url.AbsolutePath ?? "");

            int rowsAffected = cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.ToString());
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
            Console.WriteLine(ex.ToString());
            Console.ForegroundColor = ConsoleColor.White;
        }
        return -1;

    }
    private MySqlConnection Connection;
    public bool Connected { get; private set; }
}
