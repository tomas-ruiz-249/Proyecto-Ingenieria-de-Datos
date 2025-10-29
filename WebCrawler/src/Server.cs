using System.Net;

class Server
{
    public Server(string prefix)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
        _crawler = new Crawler();
    }
    
    public void Start()
    {
        _listener.Start();
        Console.WriteLine($"listening on {_listener.Prefixes.AsEnumerable<string>().FirstOrDefault()}");
        try
        {
            while (true)
            {
                HttpListenerContext context = _listener.GetContext();
                ProcessRequest(context);
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            _listener.Stop();
        }

    }
    private void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        // Obtain a response object.
        HttpListenerResponse response = context.Response;

        // string path = request.Url.LocalPath;
        string path = GetFilePath(request.Url.LocalPath);

        if (File.Exists(path))
        {
            ServeFile(path, response);
        }
        else
        {
            Console.WriteLine($"path does not exist {path}");
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

	private HttpListener _listener;
    private Crawler _crawler;
    private const string _WebDirPath = "../../../../Interfaz web/";
}