class Result(int id, int idUsuario, int estado, string fechaExtraccion)
{
    public int Id { get; set; } = id;
    public int IdUsuario { get; set; } = idUsuario;
    public int Estado { get; set; } = estado;
    public string FechaExtraccion { get; set; } = fechaExtraccion;
    public int Cantidad { get; set; }
    public string? MongoId { get; set; } // ObjectId as string
    public string? MongoIdUsuario { get; set; } // ObjectId as string for IdUsuario
}