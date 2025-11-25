public class Articulo
{
    public Articulo (
        string tema,
        string titular,
        string subtitulo,
        string cuerpo,
        string fecha,
        int idResultado,
        bool favorito) 
    {
        Tema = tema;
        Titular = titular;
        Subtitulo = subtitulo;
        Cuerpo = cuerpo;
        Fecha = fecha;
        IdResultado = idResultado;
        Favorito = favorito;
    }
    public Articulo (
        int id,
        string tema,
        string titular,
        string subtitulo,
        string cuerpo,
        string fecha,
        int idResultado,
        bool favorito) 
    {
        Id = id;
        Tema = tema;
        Titular = titular;
        Subtitulo = subtitulo;
        Cuerpo = cuerpo;
        Fecha = fecha;
        IdResultado = idResultado;
        Favorito = favorito;
    }

    public override string ToString()
    {
        return $"""
            tema: {Tema}

            titulo: {Titular}

            subtitulo({Subtitulo.Length}): {Subtitulo}

            cuerpo({Cuerpo.Length}): {Cuerpo}

            fecha: {Fecha}
        """;
    }
    public int Id { get; set; }
    public string MongoId { get; set; } = string.Empty;
    public string Tema { get; set; }
    public string Titular { get; set; }
    public string Subtitulo { get; set; }
    public string Cuerpo { get; set; }
    public string Fecha { get; set; }
    public int IdResultado { get; set; }
    // When using MongoDB, store the Resultado ObjectId here
    public string IdResultadoMongo { get; set; } = string.Empty;
    public bool Favorito { get; set; }
}
