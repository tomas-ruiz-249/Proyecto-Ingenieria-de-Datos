using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;

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
        foreach(var prefix in prefixes)
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
        foreach(var prefix in _listener.Prefixes)
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
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            _listener.Stop();
        }

    }
    private async Task ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        Console.WriteLine($"{request.HttpMethod} request to {request.Url.AbsolutePath}");

        if (request.Url.AbsolutePath == "/api/start-scraping" && request.HttpMethod == "POST")
        {
            await StartScraping(context);
            return;
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

            var articles = new List<Articulo>();
            foreach(var source in sources)
            {
                var scrapedArticles = await _crawler.Crawl(source, _repository);
                articles.AddRange(scrapedArticles);
            }

            var responseObj = new
            {
                articleList = articles,
                result = _crawler.LastResult
            };

            string json = JsonSerializer.Serialize(responseObj);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            response.ContentType = GetContentType(".json");
            response.OutputStream.Write(buffer, 0, buffer.Length);
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