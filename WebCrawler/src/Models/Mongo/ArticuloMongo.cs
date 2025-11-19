using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class ArticuloMongo
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	[BsonElement("_id")]
	public string? Id { get; set; }

	[BsonElement("idResultado")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? IdResultado { get; set; }

	[BsonElement("tema")]
	public string? Tema { get; set; }

	[BsonElement("titular")]
	public string? Titular { get; set; }

	[BsonElement("cuerpo")]
	public string? Cuerpo { get; set; }

	[BsonElement("fecha")]
	public DateTime? Fecha { get; set; }

	[BsonElement("fuente")]
	public FuenteMongo Fuente { get; set; } = new FuenteMongo();
}

[BsonIgnoreExtraElements]
public class FuenteMongo
{
	[BsonElement("id")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("url")]
	public string Url { get; set; } = string.Empty;

	[BsonElement("nombre")]
	public string Nombre { get; set; } = string.Empty;

	[BsonElement("tipo")]
	public string Tipo { get; set; } = string.Empty;
}
