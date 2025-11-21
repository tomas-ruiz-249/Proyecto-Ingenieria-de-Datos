class Fuente
{
    public Fuente(int id, string url, string tipo, string nombre)
    {
        Id = id;
        Url = url;
        Tipo = tipo;
        Nombre = nombre;
    }
    public Fuente(string url, string tipo, string nombre)
    {
        Url = url;
        Tipo = tipo;
        Nombre = nombre;
    }

    public int Id { get; set; }
    public string Url { get; set; }
    public string Tipo { get; set; }
    public string Nombre { get; set; }
    // When using MongoDB, store the Fuente ObjectId string here
    public string MongoId { get; set; } = string.Empty;
}