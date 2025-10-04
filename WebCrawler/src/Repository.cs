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
            select * from Usuario;
            """;
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Console.WriteLine(rdr[0] + "|" + rdr[1] + "|" + rdr[2]);
            }
            rdr.Close();
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
