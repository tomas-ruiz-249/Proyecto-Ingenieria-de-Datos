class Crawler
{
    public Crawler()
    {
        _urls = new Queue<Uri>();
        _parser = new Parser();
        _visited = [];
    }

    public Crawler(List<string> urlList) {
        _urls = new Queue<Uri>();
        foreach (var url in urlList)
        {
            _urls.Enqueue(new Uri(url));
        }
        _parser = new Parser();
        _visited = [];
    }

    public async Task Crawl(string startUrl, Repository repository)
    {
        if (!repository.Connected)
        {
            Console.WriteLine("No database connection.Crawling aborted...");
            return;
        }

        _parser.SetStartUrl(startUrl);
        _urls.Enqueue(new Uri(startUrl));
        repository.RegisterScraping(user);
        var resultId = repository.GetLastResultId();
        int articleCount = 0;

        while (_urls.Count != 0 && articleCount < _articleLimit)
        {
            var currentUrl = _urls.Dequeue();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"visited:{_visited.Count} Queue:{_urls.Count} article count: {articleCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (_visited.Contains(currentUrl)) continue;
            var success = await _parser.LoadArticleHTML(currentUrl);

            if (!success) continue;
            _visited.Add(currentUrl);
            var articulo = _parser.ParseArticle(resultId);

            var extractedUrls = _parser.ExtractUrls();

            foreach (var url in extractedUrls)
            {
                if (_visited.Contains(url))
                {
                    continue;
                }
                if(!currentUrl.Host.Contains(url.Host) && !url.Host.Contains(currentUrl.Host))
                {
                    continue;
                    
                }
                _urls.Enqueue(url);
            }

            if(articulo.Cuerpo.Length < 500)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("NO ES ARTICULO"); 
                Console.ForegroundColor = ConsoleColor.White;
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(articulo);
            Console.ForegroundColor = ConsoleColor.White;

            if(repository.StoreArticle(articulo, currentUrl, resultId))
            {
                articleCount++;
            }

        }
        _urls.Clear();
    }

    private const int _articleLimit = 10;

    private readonly Queue<Uri> _urls;
    private readonly HashSet<Uri> _visited;
    private readonly Parser _parser;
    private const int user = 1;
}
