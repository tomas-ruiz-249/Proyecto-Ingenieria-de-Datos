using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class NotificacionMongo
{
    // Mongo primary id
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("_id")]
    public string? Id { get; set; }

    [BsonElement("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [BsonElement("tipo")]
    public int Tipo { get; set; }

    [BsonElement("leido")]
    public bool Leido { get; set; }

    // Reference to Resultado stored as ObjectId
    [BsonElement("idResultado")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? IdResultado { get; set; }
}
