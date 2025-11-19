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
    public string Mensaje { get; set; }
    public int Tipo { get; set; }
    public bool Leido { get; set; }
    public int IdResultado { get; set; }
}