class Fuente
{
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
}