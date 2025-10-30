using System.Text;
using HtmlAgilityPack;

class Parser
{
    public Parser()
    {
        _client = new HttpClient();
        _currentDoc = new HtmlDocument();
    }
    public void SetStartUrl(string startUrl)
    {
        _startUrl = new Uri(startUrl);
    }

    public async Task<bool> LoadArticleHTML(Uri url)
    {
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.Write("Parsing: " + url);
        Console.ForegroundColor = ConsoleColor.White;
        var htmlStr = "";

        try
        {
            var response = await _client.GetAsync(url);
            htmlStr = await response.Content.ReadAsStringAsync();
        }
        catch (System.InvalidOperationException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("(INVALID URL)");
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }
        catch (System.NotSupportedException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("(SCHEME NOT SUPPORTED:" + url.Scheme + ")");
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        _currentDoc.LoadHtml(htmlStr);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("(HTML loaded)");
        Console.ForegroundColor = ConsoleColor.White;
        _currentUrl = url;
        return true;
    }

    public Articulo ParseArticle(int idResult)
    {
        string tema, titular, subtitulo, cuerpo, fecha;
        bool favorito = false;

        tema = _currentDoc.DocumentNode.SelectSingleNode(
            "//meta[contains(@name, 'keyword')]"
        )?.GetAttributeValue("content", "") ?? "";

        titular = _currentDoc.DocumentNode.SelectSingleNode(
            "//meta[@property='og:title']"
        )?.GetAttributeValue("content", "") ?? "";

        fecha = _currentDoc.DocumentNode.SelectSingleNode(
            "//meta[@property='article:published_time']"
        )?.GetAttributeValue("content", "") ?? "";

        subtitulo = _currentDoc.DocumentNode.SelectSingleNode(
            "//meta[@property='og:description']"
        )?.GetAttributeValue("content", "") ?? "";

        var cuerpoNodes = _currentDoc.DocumentNode.SelectNodes(
            @"
            //article//p | 
            //div[contains(@class, 'article')]//p | 
            //div[contains(@class, 'content')]//p |
            //div[contains(@class, 'post-content')]//p
            "
        );
        
        if(cuerpoNodes != null)
        {
            var sb = new StringBuilder();
            foreach(var node in cuerpoNodes)
            {
                // Console.ForegroundColor = ConsoleColor.Cyan;
                // Console.WriteLine("--" + node.InnerText + "--");
                // Console.ForegroundColor = ConsoleColor.White;
                sb.Append(node.InnerText);
            }
            cuerpo = sb.ToString();
        }
        else
        {
            cuerpo = "";
        }
        
        var articulo = new Articulo(tema, titular, subtitulo, cuerpo, fecha, idResult, favorito);
        return articulo;
    }

    public List<Uri> ExtractUrls()
    {
        var linkNodes = _currentDoc.DocumentNode.SelectNodes("//a[@href]");
        var scrapedLinks = new List<Uri>();
        if (linkNodes == null) return scrapedLinks;

        foreach (var node in linkNodes)
        {
            var link = node.Attributes["href"]?.Value;
            if (link == null) continue;

            try
            {
                var extractedUrl = new Uri(link, UriKind.RelativeOrAbsolute);
                if (!extractedUrl.IsAbsoluteUri)
                {
                    extractedUrl = new Uri(_startUrl, extractedUrl);
                }
                scrapedLinks.Add(extractedUrl);
            }
            catch (System.UriFormatException)
            {
                // Console.ForegroundColor = ConsoleColor.Red;
                // Console.WriteLine("INVALID URL" + link);
                // Console.ForegroundColor = ConsoleColor.White;
                continue;
            }
        }

        return scrapedLinks;
    }

    private readonly HttpClient _client;
    private readonly HtmlDocument _currentDoc;
    private Uri? _currentUrl;
    private Uri? _startUrl;
}

