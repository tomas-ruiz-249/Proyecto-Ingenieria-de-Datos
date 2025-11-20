class Notificacion
{
    public Notificacion(int id, string mensaje, int tipo, bool leido, int idResultado)
    {
        Id = id;
        Mensaje = mensaje;
        Tipo = tipo;
        Leido = leido;
        IdResultado = idResultado;
    }

    public int Id { get; set; }
<<<<<<< HEAD
=======
    public string MongoId { get; set; } = string.Empty;
>>>>>>> 58b0877 (mongo scraping)
    public string Mensaje { get; set; }
    public int Tipo { get; set; }
    public bool Leido { get; set; }
    public int IdResultado { get; set; }
}