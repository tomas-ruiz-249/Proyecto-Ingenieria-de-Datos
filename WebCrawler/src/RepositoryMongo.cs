
using MongoDB.Bson;
using MongoDB.Driver;

class RepositoryMongo
{
    public RepositoryMongo(string connectionString)
    {
        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString), "You must provide a value for MONGODB_URL");
        }
        client = new MongoClient(connectionString);
        DB = client.GetDatabase("WebCrawlerCompass");
        Articulos = DB.GetCollection<BsonDocument>("Articulo");
        Usuarios = DB.GetCollection<BsonDocument>("Usuario");
        Notificaciones = DB.GetCollection<BsonDocument>("Notificacion");
        Connected = true;
    }
    public bool ChangePassword(int userId, string newPassword)
    {
        throw new NotImplementedException();
    }

    public void DeleteNotification(int notifId)
    {
        throw new NotImplementedException();
    }

    public bool DeleteResults(int userId)
    {
        throw new NotImplementedException();
    }

    public bool DeleteUser(int userId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", userId);
        var result = Usuarios.DeleteOne(filter);
        return result.DeletedCount > 0;
    }

    public bool DiscardArticles(int userId, List<int> discardIds)
    {
        throw new NotImplementedException();
    }

    public bool EditEmail(int userId, string email)
    {
        throw new NotImplementedException();
    }

    public bool EditUser(int userId, string name, string lastName)
    {
        throw new NotImplementedException();
    }

    public List<ArticleSource> FilterArticles(int userId, string? titular, string? palabrasClave, string? tema, string? nombreFuente, string? fecha1, string? fecha2)
    {
        throw new NotImplementedException();
    }

    public List<Notificacion> FilterNotifications(int idUsuario, int? tipo, bool? leido)
    {
        throw new NotImplementedException();
    }

    public bool GenerateNotification(int idResult, string message, int type)
    {
        throw new NotImplementedException();
    }

    public List<ArticleSource> GetArticlesAndSources(int userId)
    {
        throw new NotImplementedException();
    }

    public Result? GetLastResult(int id)
    {
        throw new NotImplementedException();
    }

    public int GetLastResultId()
    {
        throw new NotImplementedException();
    }

    public List<Notificacion> GetNotifications(int idUsuario)
    {
        throw new NotImplementedException();
    }

    public List<Result> GetResults(int userId)
    {
        throw new NotImplementedException();
    }

    public Usuario GetUser(int id)
    {
        throw new NotImplementedException();
    }

    public bool RegisterScraping(int userId)
    {
        throw new NotImplementedException();
    }

    public bool RegisterUser(Usuario u)
    {
        throw new NotImplementedException();
    }

    public bool SetResultFinished(int resultId, int articleCount)
    {
        throw new NotImplementedException();
    }

    public bool StoreArticleWithSource(Articulo a, Fuente f, int idResultado)
    {
        throw new NotImplementedException();
    }

    public bool ToggleArticleFavorite(int articleId)
    {
        throw new NotImplementedException();
    }

    public void UpdateReadNotification(int notifId)
    {
        throw new NotImplementedException();
    }

    public int ValidateLogin(string correo, string contraseña)
    {
        throw new NotImplementedException();
    }
    public bool Connected  { get;  private set;}
    private readonly MongoClient client;
    private readonly IMongoDatabase DB;
    private readonly IMongoCollection<BsonDocument> Articulos;
    private readonly IMongoCollection<BsonDocument> Usuarios;
    private readonly IMongoCollection<BsonDocument> Notificaciones;
}