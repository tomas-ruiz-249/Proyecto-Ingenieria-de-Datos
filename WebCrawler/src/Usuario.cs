class Usuario
{
    public Usuario(int id, string nombres, string apellidos, string correo)
    {
        Id = id;
        Nombres = nombres;
        Apellidos = apellidos;
        Correo = correo;
    }
    public int Id { get; set; }
    public string Nombres { get; set; }
    public string Apellidos { get; set; }
    public string Correo { get; set; }
    public string Contraseña { get; set; }
}