class Credenciales
{
    public Credenciales(string correo, string contraseña)
    {
        Correo = correo;
        Contraseña = contraseña;
    }
    public string Correo { get; set; }
    public string Contraseña { get; set; }
}