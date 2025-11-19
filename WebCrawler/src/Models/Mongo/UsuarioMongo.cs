using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class UsuarioMongo
{
	// Mongo primary id
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	[BsonElement("_id")]
	public string? Id { get; set; }

	[BsonElement("nombres")]
	public string Nombres { get; set; } = string.Empty;

	[BsonElement("apellidos")]
	public string Apellidos { get; set; } = string.Empty;

	[BsonElement("correo")]
	public string Correo { get; set; } = string.Empty;

	// Field name contains a special character in the DB; map explicitly.
	[BsonElement("contraseña")]
	public string Contraseña { get; set; } = string.Empty;

	[BsonElement("Resultado")]
	public Resultado? Resultado { get; set; }

	[BsonElement("articulos")]
	public List<ArticuloRef> Articulos { get; set; } = new List<ArticuloRef>();
}

[BsonIgnoreExtraElements]
public class Resultado
{
	[BsonRepresentation(BsonType.ObjectId)]
	[BsonElement("_id")]
	public string? Id { get; set; }

	[BsonElement("fechaExtraccion")]
	public DateTime FechaExtraccion { get; set; }

	[BsonElement("estado")]
	public int Estado { get; set; }
}

[BsonIgnoreExtraElements]
public class ArticuloRef
{
	[BsonElement("idArticulo")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? IdArticulo { get; set; }

	[BsonElement("descartado")]
	public bool Descartado { get; set; }

	[BsonElement("favorito")]
	public bool Favorito { get; set; }
}
