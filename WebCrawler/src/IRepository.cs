interface IRepository
{
    public bool Connected { get; }
    public bool RegisterScraping(int userId);
    public bool GenerateNotification(int idResult, string message, int type);
    public List<ArticleSource> FilterArticles(
        int userId,
        string? titular,
        string? palabrasClave,
        string? tema,
        string? nombreFuente,
        string? fecha1,
        string? fecha2
    );
    public bool SetResultFinished(int resultId, int articleCount);

    public bool StoreArticleWithSource(Articulo a, Fuente f, int idResultado);
    public bool ChangePassword(int userId, string newPassword);
    public bool DeleteUser(int userId);
    public bool EditEmail(int userId, string email);
    public bool EditUser(int userId, string name, string lastName);
    public bool DeleteResults(int userId);
    public bool DiscardArticles(int userId, List<int> discardIds);
    public List<Result> GetResults(int userId);
    public bool ToggleArticleFavorite(int articleId);
    public List<ArticleSource> GetArticlesAndSources(int userId);
    public bool RegisterUser(Usuario u);
    public Usuario GetUser(int id);
    public int ValidateLogin(string correo, string contraseña);
    public List<Notificacion> FilterNotifications(int idUsuario, int? tipo, bool? leido);
    public List<Notificacion> GetNotifications(int idUsuario);
    public void UpdateReadNotification(int notifId);
    public void DeleteNotification(int notifId);
    public int GetLastResultId();
    public Result? GetLastResult(int id);
}