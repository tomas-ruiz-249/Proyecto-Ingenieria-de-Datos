using HtmlAgilityPack;

class Parser
{
    public Parser()
    {
        _client = new HttpClient();
        _currentDoc = new HtmlDocument();
    }

    public async Task<bool> loadHTML(Uri url)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Parsing: " + url);
        var htmlStr = "";

        try
        {
            var response = await _client.GetAsync(url);
            htmlStr = await response.Content.ReadAsStringAsync();
        }
        catch (System.InvalidOperationException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("INVALID URL: " + url);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }
        catch (System.NotSupportedException){
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("SCHEME NOT SUPPORTED: " + url);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }
        catch (Exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR PARSING:" + url);
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        _currentDoc.LoadHtml(htmlStr);
        return true;
    }

    public List<Uri> ExtractUrls(HashSet<Uri> visited)
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
                var extractedUrl = new Uri(link);
                if (visited.Contains(extractedUrl))
                {
                    // Console.ForegroundColor = ConsoleColor.Magenta;
                    // Console.WriteLine("ALREADY VISITED, SKIPPING QUEUEING: " + link);
                    continue;
                }
                scrapedLinks.Add(extractedUrl);
            }
            catch (System.UriFormatException)
            {
                // Console.ForegroundColor = ConsoleColor.Red;
                // Console.WriteLine("INVALID URL" + link);
                continue;
            }

            // Console.ForegroundColor = ConsoleColor.Cyan;
            // Console.WriteLine('[' + link + "] ");
            // Console.ForegroundColor = ConsoleColor.White;
            // Console.WriteLine(node.InnerText);
            // Console.WriteLine("--------");
        }

        return scrapedLinks;
    }

    public string GetHtml()
    {
        return _currentDoc.DocumentNode.OuterHtml;
    }

    private readonly HttpClient _client;
    private readonly HtmlDocument _currentDoc;
}

