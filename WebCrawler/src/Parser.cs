using HtmlAgilityPack;

class Parser
{
    public Parser()
    {
        _client = new HttpClient();
        _repository = new Repository();
    }

    public async Task<List<Uri>> Parse(Uri url, HashSet<Uri> visited)
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
            return new List<Uri>();
        }
        catch (System.NotSupportedException){
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("SCHEME NOT SUPPORTED: " + url);
            Console.ForegroundColor = ConsoleColor.White;
            return new List<Uri>();
        }
        catch (Exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR PARSING:" + url);
            Console.ForegroundColor = ConsoleColor.White;
            return new List<Uri>();
        }

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlStr);
        return ParseUrls(htmlDoc, url, visited);
    }

    private List<Uri> ParseUrls(HtmlDocument doc, Uri url, HashSet<Uri> visited)
    {
        var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
        var scrapedLinks = new List<Uri>();
        if (linkNodes == null) return scrapedLinks;

        foreach (var node in linkNodes)
        {
            var link = node.Attributes["href"]?.Value;
            if (link == null) continue;

            if (!link.Contains("http"))
            {
                link = url.Scheme + "://" + url.Host + link;
            }
            
            try
            {
                var formattedUrl = new Uri(link);
                if (visited.Contains(formattedUrl) || formattedUrl == url)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("ALREADY VISITED, SKIPPING QUEUEING: " + link);
                    continue;
                }
                scrapedLinks.Add(formattedUrl);
            }
            catch (System.UriFormatException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("INVALID URL" + link);
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine('[' + link + "] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(node.InnerText);
            Console.WriteLine("--------");
        }

        return scrapedLinks;
    }

    private readonly HttpClient _client;
    private readonly Repository _repository;
}

