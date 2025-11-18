class Usuario
{
    public Usuario()
    {
        
    }
    public Usuario(int id, string nombres, string apellidos, string correo)
    {
        Id = id;
        Nombres = nombres;
        Apellidos = apellidos;
        Correo = correo;
    }
    
    public override string ToString()
    {
        return $"""
            Id: {Id}

            Nombres: {Nombres}

            Apellidos: {Apellidos}

            Correo: {Correo}

            Contraseña: {Contraseña}
        """;
    }
    public int Id { get; set; }
    public string Nombres { get; set; }
    public string Apellidos { get; set; }
    public string Correo { get; set; }
    public string Contraseña { get; set; }
}