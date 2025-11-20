class Crawler
{
    public Crawler()
    {
        _urls = new Queue<Uri>();
        _parser = new Parser();
        _visited = new HashSet<Uri>();
    }

    public Crawler(List<string> urlList) {
        _urls = new Queue<Uri>();
        foreach (var url in urlList)
        {
            _urls.Enqueue(new Uri(url));
        }
        _parser = new Parser();
        _visited = new HashSet<Uri>();
    }

    public async Task<List<(Articulo article, Fuente source)>> Crawl(string startUrl, RepositorySQL repository, int userId)
    {
        var scrapedArticles = new List<(Articulo article, Fuente source)>();
        if (!repository.Connected)
        {
            Console.WriteLine("No database connection.Crawling aborted...");
            return scrapedArticles;
        }

        _parser.SetStartUrl(startUrl);
        _urls.Enqueue(new Uri(startUrl));
        repository.RegisterScraping(userId);
        var resultId = repository.GetLastResultId();
        LastResult = repository.GetLastResult(resultId) ?? new Result(-1, -1, -1, string.Empty);
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
            var fuente = _parser.ParseSource();

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

            if(repository.StoreArticleWithSource(articulo, fuente, resultId))
            {
                articleCount++;
                var articleSourceTuple = (articulo, fuente);
                scrapedArticles.Add(articleSourceTuple);
            }
            else
            {
                repository.GenerateNotification(resultId, $"Error registrando articulo {articulo.Titular}", 1);
            }

        }
        _urls.Clear();
        _visited.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"scraping attempt ended with {articleCount} articles found and registered in the database...");
        Console.ForegroundColor = ConsoleColor.White;
        repository.SetResultFinished(resultId,articleCount);
        repository.GenerateNotification(resultId, $"Se han encontrado {articleCount} articulos", 0);
        return scrapedArticles;
    }

    // Overload for Mongo repository: same behavior but using string ids
    public async Task<List<(Articulo article, Fuente source)>> Crawl(string startUrl, RepositoryMongo repository, string userId)
    {
        var scrapedArticles = new List<(Articulo article, Fuente source)>();
        if (!repository.Connected)
        {
            Console.WriteLine("No database connection.Crawling aborted...");
            return scrapedArticles;
        }

        _parser.SetStartUrl(startUrl);
        _urls.Enqueue(new Uri(startUrl));
        repository.RegisterScraping(userId);
        var resultId = repository.GetLastResultId();
        LastResult = repository.GetResult(resultId);
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
            var articulo = _parser.ParseArticle(-1);
            articulo.IdResultadoMongo = resultId;
            var fuente = _parser.ParseSource();

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

            if(repository.StoreArticleWithSource(articulo, fuente, resultId))
            {
                articleCount++;
                var articleSourceTuple = (articulo, fuente);
                scrapedArticles.Add(articleSourceTuple);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Article registered succesfully");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                repository.GenerateNotification(resultId, $"Error registrando articulo {articulo.Titular}", 1);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Article was not registered...");
                Console.ForegroundColor = ConsoleColor.White;
            }

        }
        _urls.Clear();
        _visited.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"scraping attempt ended with {articleCount} articles found and registered in the database...");
        Console.ForegroundColor = ConsoleColor.White;
        repository.SetResultFinished(resultId,articleCount);
        repository.GenerateNotification(resultId, $"Se han encontrado {articleCount} articulos", 0);
        return scrapedArticles;
    }

    public Result LastResult { get; private set; }
    private const int _articleLimit = 10;
    private readonly Queue<Uri> _urls;
    private readonly HashSet<Uri> _visited;
    private readonly Parser _parser;
}
