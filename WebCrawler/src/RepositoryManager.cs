
class RepositoryManager 
{
    private readonly RepositorySQL RepoSQL;
    private readonly RepositoryMongo RepoMongo;

    public bool Connected => throw new NotImplementedException();

    public bool ChangePassword(int userId, string newPassword)
    {
        var success = false;
        if (
            RepoSQL.ChangePassword(userId, newPassword) &&
            RepoMongo.ChangePassword(userId, newPassword) 
        )
        {
            success = true;
        }
        return success;
    }

    public bool ConnectToServer()
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
        throw new NotImplementedException();
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
}