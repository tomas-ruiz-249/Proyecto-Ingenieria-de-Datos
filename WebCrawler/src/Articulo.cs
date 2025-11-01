public class Articulo(string tema, string titular, string subtitulo, string cuerpo, string fecha, int idResultado, bool favorito)
{
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
    public string Tema { get; set; } = tema;
    public string Titular { get; set; } = titular;
    public string Subtitulo { get; set; } = subtitulo;
    public string Cuerpo { get; set; } = cuerpo;
    public string Fecha { get; set; } = fecha;
    public int IdResultado { get; set; } = idResultado;
    public bool Favorito { get; set; } = favorito;
}
