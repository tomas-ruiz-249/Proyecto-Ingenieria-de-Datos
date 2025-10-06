using MySqlConnector;
class Repository
{
    public Repository(string connectionString)
    {
        conn = new MySqlConnection(connectionString);
    }

    public bool ConnectToServer()
    {
        try
        {
            conn.Open();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
        return true;
    }

    public void StoreHtml(string html)
    {
        try
        {
            string query = """
                use WebCrawler;
                insert into Usuario (nombre, contraseña) values("tomas", "1234");
            """;
            MySqlCommand cmd = new MySqlCommand(query, conn);
            int rowsAffected = cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Console.WriteLine("query unsuccessful....");
            return;
        }
        Console.WriteLine("query sucessfull....");
    }
    MySqlConnection conn;
}
