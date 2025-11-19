using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

class Server
{
    public Server(string[] prefixes, bool mongo = true)
    {
        _listener = new HttpListener();
        foreach (var prefix in prefixes)
        {
            _listener.Prefixes.Add(prefix);
        }
        _crawler = new Crawler();
        _mongo = mongo;

        _repositoryMongo = new RepositoryMongo("mongodb://localhost:27017");

        _repository = new RepositorySQL("server=localhost;user=root;database=WebCrawler;port=3306;");
        if (!_repository.ConnectToServer())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR CONNECTING TO MYSQL SERVER");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Connected to MySQL server");
            Console.ForegroundColor = ConsoleColor.White;
        }

        if (!_repositoryMongo.Connected)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR CONNECTING TO MONGODB SERVER");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Connected to MongoDB server");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public async Task Start()
    {
        _listener.Start();
        Console.WriteLine($"listening on: ");
        foreach (var prefix in _listener.Prefixes)
        {
            Console.WriteLine($"{prefix}");
        }

        try
        {
            while (true)
            {
                HttpListenerContext context = _listener.GetContext();
                await ProcessRequest(context);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            _listener.Stop();
        }

    }
    private async Task ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        Console.WriteLine($"{request.HttpMethod} request to {request.Url.AbsoluteUri}");

        if (request.HttpMethod == "POST")
        {
            if (request.Url.AbsolutePath == "/api/start-scraping")
            {
                await StartScraping(context);
                return;
            }
            else if (request.Url.AbsolutePath == "/api/login")
            {
                ValidateLoginCredentials(context);
                return;
            }
            else if (request.Url.AbsolutePath == "/api/register-user")
            {
                RegisterUser(context);
                return;
            }
        }
        else if (request.HttpMethod == "GET")
        {
            if (request.Url.AbsolutePath == "/api/get-notifications")
            {
                var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
                if (parsedUrl.Count == 1)
                {
                    GetNotifications(context);
                }
                else
                {
                    FilterNotifications(context);
                }
                return;
            }
            else if (request.Url.AbsolutePath == "/api/get-user")
            {
                GetUser(context);
                return;
            }
            else if (request.Url.AbsolutePath == "/api/get-articles")
            {
                var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
                if (parsedUrl.Count == 1)
                {
                    GetArticles(context);
                    return;
                }
                else if (parsedUrl.Count > 1)
                {
                    FilterArticles(context);
                    return;
                }
            }
            else if (request.Url.AbsolutePath == "/api/get-results")
            {
                GetResults(context);
                return;
            }
        }
        else if (request.HttpMethod == "PATCH")
        {
            if (request.Url.AbsolutePath == "/api/update-notif-read")
            {
                UpdateNotificationRead(context);
                return;
            }
            else if (request.Url.AbsolutePath == "/api/update-article-fav")
            {
                UpdateArticleFavorite(context);
                return;
            }
            else if (request.Url.AbsolutePath == "/api/discard-articles")
            {
                DiscardArticles(context);
                return;
            }
            else if (request.Url.AbsolutePath == "/api/edit-user")
            {
                EditUser(context);
                return;
            }
            else if (request.Url.AbsolutePath == "/api/edit-email")
            {
                EditEmail(context);
                return;
            }
            else if (request.Url.AbsolutePath == "/api/change-password")
            {
                ChangePassword(context);
                return;
            }
        }
        else if (request.HttpMethod == "DELETE")
        {
            if (request.Url.AbsolutePath == "/api/delete-notif")
            {
                DeleteNotification(context);
                return;
            }
            else if (request.Url.AbsolutePath == "/api/delete-results")
            {
                DeleteResults(context);
                return;
            }
            else if (request.Url.AbsolutePath == "/api/delete-user")
            {
                DeleteUser(context);
                return;
            }

        }

        string path = GetFilePath(request.Url.LocalPath);
        if (File.Exists(path))
        {
            ServeFile(path, response);
        }
        else
        {
            Console.WriteLine($"file does not exist {path}");
        }
    }

    private void FilterArticles(HttpListenerContext context)
    {
        var response = context.Response;
        var request = context.Request;

        var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);

        int id = string.IsNullOrEmpty(parsedUrl["id"]) ? -1 : Convert.ToInt32(parsedUrl["id"]);
        var titular = parsedUrl["titular"];
        var palabrasClave = parsedUrl["clave"];
        var tema = parsedUrl["tema"];
        var nombreFuente = parsedUrl["fuente"];
        var fecha1 = string.IsNullOrEmpty(parsedUrl["fecha1"]) ? null : parsedUrl["fecha1"];
        var fecha2 = string.IsNullOrEmpty(parsedUrl["fecha2"]) ? null : parsedUrl["fecha2"];

        List<ArticleSource> articles;
        if (_mongo)
        {
            articles = _repositoryMongo.FilterArticles(id.ToString(), titular, palabrasClave, tema, nombreFuente, fecha1, fecha2);
        }
        else
        {
            articles = _repository.FilterArticles(id, titular, palabrasClave, tema, nombreFuente, fecha1, fecha2);
        }
        string json = JsonSerializer.Serialize(articles);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        response.ContentType = GetContentType(".json");
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private void ChangePassword(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        if (request.HasEntityBody)
        {
            var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
            var idStr = parsedUrl["id"];
            int idInt = string.IsNullOrEmpty(idStr) ? -1 : Convert.ToInt32(idStr);

            var bodyStream = request.InputStream;
            var reader = new StreamReader(bodyStream, request.ContentEncoding);
            string body = reader.ReadToEnd();

            var newPassword = JsonSerializer.Deserialize<string>(body);
            bool success = false;
            if (_mongo)
            {
                // Mongo expects string id, ensure not null/empty and password not null
                if (!string.IsNullOrEmpty(idStr) && !string.IsNullOrEmpty(newPassword))
                    success = _repositoryMongo.ChangePassword(idStr, newPassword);
            }
            else
            {
                // SQL expects int id and password not null
                if (idInt != -1 && !string.IsNullOrEmpty(newPassword))
                    success = _repository.ChangePassword(idInt, newPassword!);
            }

            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(success));
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }

    private void DeleteUser(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
        var idStr = parsedUrl["id"];
        int idInt = string.IsNullOrEmpty(idStr) ? -1 : Convert.ToInt32(idStr);

        bool success = false;
        if (_mongo)
        {
            // Mongo expects string id
            if (!string.IsNullOrEmpty(idStr))
                success = _repositoryMongo.DeleteUser(idStr);
        }
        else
        {
            // SQL expects int id
            if (idInt != -1)
                success = _repository.DeleteUser(idInt);
        }
        Console.WriteLine(success);

        byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(success));
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private void EditEmail(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        if (request.HasEntityBody)
        {
            var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
            var idStr = parsedUrl["id"];
            int idInt = string.IsNullOrEmpty(idStr) ? -1 : Convert.ToInt32(idStr);

            var bodyStream = request.InputStream;
            var reader = new StreamReader(bodyStream, request.ContentEncoding);
            string body = reader.ReadToEnd();

            var newEmail = JsonSerializer.Deserialize<string>(body);
            System.Console.WriteLine(newEmail);
            bool success = false;
            if (_mongo)
            {
                if (!string.IsNullOrEmpty(idStr) && !string.IsNullOrEmpty(newEmail))
                    success = _repositoryMongo.EditEmail(idStr, newEmail);
            }
            else
            {
                if (idInt != -1 && !string.IsNullOrEmpty(newEmail))
                    success = _repository.EditEmail(idInt, newEmail);
            }

            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(success));
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }

    private void EditUser(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        if (request.HasEntityBody)
        {
            var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
            var idStr = parsedUrl["id"];
            int idInt = string.IsNullOrEmpty(idStr) ? -1 : Convert.ToInt32(idStr);

            var bodyStream = request.InputStream;
            var reader = new StreamReader(bodyStream, request.ContentEncoding);
            string body = reader.ReadToEnd();

            var newUser = JsonSerializer.Deserialize<Usuario>(body);
            bool success = false;
            if (_mongo)
            {
                if (!string.IsNullOrEmpty(idStr) && newUser != null && !string.IsNullOrEmpty(newUser.Nombres) && !string.IsNullOrEmpty(newUser.Apellidos))
                    success = _repositoryMongo.EditUser(idStr, newUser.Nombres, newUser.Apellidos);
            }
            else
            {
                if (idInt != -1 && newUser != null && !string.IsNullOrEmpty(newUser.Nombres) && !string.IsNullOrEmpty(newUser.Apellidos))
                    success = _repository.EditUser(idInt, newUser.Nombres, newUser.Apellidos);
            }

            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(success));
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }

    private void DeleteResults(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
        var idStr = parsedUrl["id"];
        int idInt = string.IsNullOrEmpty(idStr) ? -1 : Convert.ToInt32(idStr);
        bool success = false;
        if (_mongo)
        {
            if (!string.IsNullOrEmpty(idStr))
                success = _repositoryMongo.DeleteResults(idStr);
        }
        else
        {
            if (idInt != -1)
                success = _repository.DeleteResults(idInt);
        }
        Console.WriteLine($"success {success}");
        byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(success));
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
    private void DiscardArticles(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        if (request.HasEntityBody)
        {
            var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
            var idStr = parsedUrl["id"];
            int idInt = string.IsNullOrEmpty(idStr) ? -1 : Convert.ToInt32(idStr);
            var bodyStream = request.InputStream;
            var reader = new StreamReader(bodyStream, request.ContentEncoding);
            string body = reader.ReadToEnd();
            bool success = false;
            if (_mongo)
            {
                var discardIds = JsonSerializer.Deserialize<List<string>>(body);
                if (!string.IsNullOrEmpty(idStr) && discardIds != null)
                    success = _repositoryMongo.DiscardArticles(idStr, discardIds);
            }
            else
            {
                var discardIds = JsonSerializer.Deserialize<List<int>>(body);
                if (idInt != -1 && discardIds != null)
                    success = _repository.DiscardArticles(idInt, discardIds);
            }
            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(success));
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }

    private void GetResults(HttpListenerContext context)
    {
        var response = context.Response;
        var request = context.Request;
        var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
        var idStr = parsedUrl["id"];
        int idInt = string.IsNullOrEmpty(idStr) ? -1 : Convert.ToInt32(idStr);
        List<Result> results;
        if (_mongo)
        {
            results = _repositoryMongo.GetResults(idStr);
        }
        else
        {
            results = _repository.GetResults(idInt);
        }
        string json = JsonSerializer.Serialize(results);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        response.ContentType = GetContentType(".json");
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private void UpdateArticleFavorite(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        if (request.HasEntityBody)
        {
            var bodyStream = request.InputStream;
            var reader = new StreamReader(bodyStream, request.ContentEncoding);
            string body = reader.ReadToEnd();
            bool success = false;
            if (_mongo)
            {
                var articleId = JsonSerializer.Deserialize<string>(body);
                if (!string.IsNullOrEmpty(articleId))
                    success = _repositoryMongo.ToggleArticleFavorite(articleId);
            }
            else
            {
                var articleId = JsonSerializer.Deserialize<int>(body);
                success = _repository.ToggleArticleFavorite(articleId);
            }
            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(success));
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
    private void GetArticles(HttpListenerContext context)
    {
        var response = context.Response;
        var request = context.Request;

        var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
        int id = string.IsNullOrEmpty(parsedUrl["id"]) ? -1 : Convert.ToInt32(parsedUrl["id"]);
        var articles = _repository.GetArticlesAndSources(id);
        string json = JsonSerializer.Serialize(articles);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        response.ContentType = GetContentType(".json");
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
    private void RegisterUser(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        if (request.HasEntityBody)
        {
            var bodyStream = request.InputStream;
            var reader = new StreamReader(bodyStream, request.ContentEncoding);
            string body = reader.ReadToEnd();
            var newUser = JsonSerializer.Deserialize<Usuario>(body);
            bool success = false;
            if (_mongo)
            {
                if (newUser != null)
                    success = _repositoryMongo.RegisterUser(newUser);
            }
            else
            {
                if (newUser != null)
                    success = _repository.RegisterUser(newUser);
            }
            var json = JsonSerializer.Serialize(success);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
    private void GetUser(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
        if (parsedUrl == null)
        {
            response.StatusCode = 400;
            response.Close();
            return;
        }
        var idStr = parsedUrl["id"];
        int idInt = -1;
        object user;
        if (_mongo)
        {
            user = _repositoryMongo.GetUser(idStr);
        }
        else
        {
            idInt = string.IsNullOrEmpty(idStr) ? -1 : Convert.ToInt32(idStr);
            user = _repository.GetUser(idInt);
        }
        var json = JsonSerializer.Serialize(user);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private void ValidateLoginCredentials(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        if (!request.HasEntityBody)
        {
            response.StatusCode = 400;
            response.Close();
            return;
        }

        var bodyStream = request.InputStream;
        var reader = new StreamReader(bodyStream, request.ContentEncoding);
        string body = reader.ReadToEnd();
        var credenciales = JsonSerializer.Deserialize<Credenciales>(body);
        if (credenciales == null)
        {
            response.StatusCode = 400;
            response.Close();
            return;
        }

        Console.WriteLine(credenciales.Correo + credenciales.Contraseña);
        object idUsuario;
        if (_mongo)
        {
            // Mongo returns string ObjectId (or empty string when not found)
            idUsuario = _repositoryMongo.ValidateLogin(credenciales.Correo, credenciales.Contraseña);
        }
        else
        {
            // SQL returns int id (or -1 when not found)
            idUsuario = _repository.ValidateLogin(credenciales.Correo, credenciales.Contraseña);
        }

        var json = JsonSerializer.Serialize(idUsuario);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private void FilterNotifications(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
        if (parsedUrl == null)
        {
            response.StatusCode = 400;
            response.Close();
            return;
        }
        int? tipo = string.IsNullOrEmpty(parsedUrl["type"]) ? null : Convert.ToInt32(parsedUrl["type"]);
        bool? leido = string.IsNullOrEmpty(parsedUrl["read"]) ? null : Convert.ToInt32(parsedUrl["read"]) > 0;
        int id = string.IsNullOrEmpty(parsedUrl["id"]) ? -1 : Convert.ToInt32(parsedUrl["id"]);
        var notifications = _repository.FilterNotifications(id, tipo, leido);
        var responseObj = new
        {
            notifList = notifications
        };

        string json = JsonSerializer.Serialize(responseObj);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
    private void DeleteNotification(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
        if (parsedUrl == null)
        {
            response.StatusCode = 400;
            response.Close();
            return;
        }
        var idStr = parsedUrl["id"];
        int idInt = string.IsNullOrEmpty(idStr) ? -1 : Convert.ToInt32(idStr);
        Console.WriteLine($"DELETING NOTIF WITH ID {idStr}");
        if (_mongo)
        {
            if (!string.IsNullOrEmpty(idStr))
                _repositoryMongo.DeleteNotification(idStr);
        }
        else
        {
            if (idInt != -1)
                _repository.DeleteNotification(idInt);
        }
        byte[] buffer = Encoding.UTF8.GetBytes("");
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
    private void UpdateNotificationRead(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        if (request.HasEntityBody)
        {
            var bodyStream = request.InputStream;
            var reader = new StreamReader(bodyStream, request.ContentEncoding);
            string body = reader.ReadToEnd();
            if (_mongo)
            {
                var notifId = JsonSerializer.Deserialize<string>(body);
                if (!string.IsNullOrEmpty(notifId))
                    _repositoryMongo.UpdateReadNotification(notifId);
            }
            else
            {
                var notifId = JsonSerializer.Deserialize<int>(body);
                _repository.UpdateReadNotification(notifId);
            }
            byte[] buffer = Encoding.UTF8.GetBytes("");
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }

    private void GetNotifications(HttpListenerContext context)
    {
        var response = context.Response;
        var request = context.Request;

        var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
        if (parsedUrl == null)
        {
            response.StatusCode = 400;
            response.Close();
            return;
        }
        int id = string.IsNullOrEmpty(parsedUrl["id"]) ? -1 : Convert.ToInt32(parsedUrl["id"]);
        var notifications = _repository.GetNotifications(id);
        var responseObj = new
        {
            notifList = notifications
        };
        string json = JsonSerializer.Serialize(responseObj);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        response.ContentType = GetContentType(".json");
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private async Task StartScraping(HttpListenerContext context)
    {
        var response = context.Response;
        var request = context.Request;
        if (request.HasEntityBody)
        {
            var bodyStream = request.InputStream;
            var reader = new StreamReader(bodyStream, request.ContentEncoding);
            string body = reader.ReadToEnd();
            Console.WriteLine("REQUEST WITH BODY: " + body);
            var sources = JsonSerializer.Deserialize<List<string>>(body) ?? [];
            var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
            if (parsedUrl == null)
            {
                response.StatusCode = 400;
                response.Close();
                return;
            }
            int id = string.IsNullOrEmpty(parsedUrl["id"]) ? -1 : Convert.ToInt32(parsedUrl["id"]);

            var scrapedData = new List<(Articulo article, Fuente source)>();
            foreach (var source in sources)
            {
                var scrapedArticles = await _crawler.Crawl(source, _repository, id);
                scrapedData.AddRange(scrapedArticles);
            }

            var responseObj = new
            {
                articleList = scrapedData.Select(t => t.article).ToList(),
                sourceList = scrapedData.Select(t => t.source).ToList(),
                result = _crawler.LastResult
            };

            string json = JsonSerializer.Serialize(responseObj);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            response.ContentType = GetContentType(".json");
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
        else
        {
            Console.WriteLine("No urls given to start scraping");
        }
    }

    private string GetFilePath(string urlPath)
    {
        if (urlPath == "/" || urlPath == "")
        {
            return Path.Combine(_WebDirPath, "index.html");
        }
        var relPath = urlPath.TrimStart('/');
        relPath = relPath.Replace("..", "");
        return Path.Combine(_WebDirPath, relPath);
    }

    private void ServeFile(string filePath, HttpListenerResponse response)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        response.ContentType = GetContentType(Path.GetExtension(filePath));
        response.ContentLength64 = fileBytes.Length;
        response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
        response.OutputStream.Close();
        Console.WriteLine($"sending {filePath}");
    }
    private static string GetContentType(string fileExtension)
    {
        // Dictionary mapping file extensions to MIME types
        var contentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".html", "text/html; charset=utf-8" },      // Web pages
            { ".htm", "text/html; charset=utf-8" },       // Web pages
            { ".css", "text/css; charset=utf-8" },        // Stylesheets
            { ".js", "application/javascript; charset=utf-8" }, // JavaScript
            { ".json", "application/json; charset=utf-8" },     // JSON data
            { ".png", "image/png" },                      // Images
            { ".jpg", "image/jpeg" },                     // Images
            { ".jpeg", "image/jpeg" },                    // Images
            { ".gif", "image/gif" },                      // Images
            { ".ico", "image/x-icon" },                   // Favicons
            { ".svg", "image/svg+xml" },                  // Vector images
            { ".txt", "text/plain; charset=utf-8" }       // Plain text
        };

        // Look up the content type, or use default if not found
        return contentTypes.TryGetValue(fileExtension ?? string.Empty, out string contentType)
            ? contentType
            : "application/octet-stream"; // Default: treat as binary file
    }

    private readonly HttpListener _listener;
    private readonly Crawler _crawler;
    private readonly RepositorySQL _repository;
    private readonly RepositoryMongo _repositoryMongo;
    private const string _WebDirPath = "../../../../Interfaz web/";
    private readonly bool _mongo;
}
