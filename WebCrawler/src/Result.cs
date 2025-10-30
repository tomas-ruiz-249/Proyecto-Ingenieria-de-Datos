class Result(int id, int idUsuario, int estado, DateTime fechaExtraccion)
{
    public int Id { get; set; } = id;
    public int IdUsuario { get; set; } = idUsuario;
    public int Estado { get; set; } = estado;
    public DateTime FechaExtraccion { get; set; } = fechaExtraccion;
}