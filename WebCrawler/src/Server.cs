using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

class Server
{
    public Server(string prefix)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
        _crawler = new Crawler();

        _repository = new Repository("server=localhost;user=root;database=WebCrawler;port=3306;");
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
    }
    public Server(string[] prefixes)
    {
        _listener = new HttpListener();
        foreach (var prefix in prefixes)
        {
            _listener.Prefixes.Add(prefix);
        }
        _crawler = new Crawler();

        _repository = new Repository("server=localhost;user=root;database=WebCrawler;port=3306;");
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

    private void DeleteUser(HttpListenerContext context)
    {

    }

    private void EditEmail(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        if (request.HasEntityBody)
        {
            var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
            int id = string.IsNullOrEmpty(parsedUrl["id"]) ? -1 : Convert.ToInt32(parsedUrl["id"]);

            var bodyStream = request.InputStream;
            var reader = new StreamReader(bodyStream, request.ContentEncoding);
            string body = reader.ReadToEnd();

            var newEmail = JsonSerializer.Deserialize<string>(body);

            System.Console.WriteLine(newEmail);
            var success = _repository.EditEmail(id, newEmail);

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
            int id = string.IsNullOrEmpty(parsedUrl["id"]) ? -1 : Convert.ToInt32(parsedUrl["id"]);

            var bodyStream = request.InputStream;
            var reader = new StreamReader(bodyStream, request.ContentEncoding);
            string body = reader.ReadToEnd();

            var newUser = JsonSerializer.Deserialize<Usuario>(body);

            var success = _repository.EditUser(id, newUser.Nombres, newUser.Apellidos);

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
        int id = string.IsNullOrEmpty(parsedUrl["id"]) ? -1 : Convert.ToInt32(parsedUrl["id"]);

        var success = _repository.DeleteResults(id);
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
            int id = string.IsNullOrEmpty(parsedUrl["id"]) ? -1 : Convert.ToInt32(parsedUrl["id"]);

            var bodyStream = request.InputStream;
            var reader = new StreamReader(bodyStream, request.ContentEncoding);
            string body = reader.ReadToEnd();
            var discardIds = JsonSerializer.Deserialize<List<int>>(body);

            var success = _repository.DiscardArticles(id, discardIds);

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
        int id = string.IsNullOrEmpty(parsedUrl["id"]) ? -1 : Convert.ToInt32(parsedUrl["id"]);
        var results = _repository.GetResults(id);

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

            var articleId = JsonSerializer.Deserialize<int>(body);
            var success = _repository.ToggleArticleFavorite(articleId);
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
            var success = _repository.RegisterUser(newUser);

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
        var id = Convert.ToInt32(parsedUrl["id"]);
        var user = _repository.GetUser(id);
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
        if (request.HasEntityBody)
        {
            var bodyStream = request.InputStream;
            var reader = new StreamReader(bodyStream, request.ContentEncoding);
            string body = reader.ReadToEnd();
            var credenciales = JsonSerializer.Deserialize<Credenciales>(body);
            Console.WriteLine(credenciales.Correo + credenciales.Contraseña);
            var idUsuario = _repository.ValidateLogin(credenciales.Correo, credenciales.Contraseña);

            var json = JsonSerializer.Serialize(idUsuario);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }

    private void FilterNotifications(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        var parsedUrl = HttpUtility.ParseQueryString(request.Url.Query);
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
        var id = Convert.ToInt32(parsedUrl["id"]);
        Console.WriteLine($"DELETING NOTIF WITH ID {id}");
        _repository.DeleteNotification(id);
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
            var notifId = JsonSerializer.Deserialize<int>(body);
            _repository.UpdateReadNotification(notifId);
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
        return contentTypes.TryGetValue(fileExtension, out string contentType)
            ? contentType
            : "application/octet-stream"; // Default: treat as binary file
    }

    private readonly HttpListener _listener;
    private readonly Crawler _crawler;
    private readonly Repository _repository;
    private const string _WebDirPath = "../../../../Interfaz web/";
}