class Crawler
{
    public Crawler()
    {
        _urls = new Queue<Uri>();
        _parser = new Parser();
        _visited = [];
        _repository = new Repository("server=localhost;user=root;database=WebCrawler;port=3306;");
        if (!_repository.ConnectToServer())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Database connection unsuccessful...");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Connected to database successfully.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public Crawler(List<string> urlList) {
        _urls = new Queue<Uri>();
        foreach (var url in urlList)
        {
            _urls.Enqueue(new Uri(url));
        }
        _parser = new Parser();
        _visited = [];

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

    public async Task Crawl(string startUrl)
    {
        if (!_repository.Connected)
        {
            Console.WriteLine("No database connection.Crawling aborted...");
            return;
        }

        _parser.SetStartUrl(startUrl);
        _urls.Enqueue(new Uri(startUrl));
        _repository.RegisterScraping(user);
        var resultId = _repository.GetLastResultId();
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
                    // Console.ForegroundColor = ConsoleColor.DarkBlue;
                    // Console.WriteLine(url.Host + " was already visited");
                    // Console.ForegroundColor = ConsoleColor.White;
                    continue;
                }
                if(!currentUrl.Host.Contains(url.Host) && !url.Host.Contains(currentUrl.Host))
                {
                    // Console.ForegroundColor = ConsoleColor.DarkBlue;
                    // Console.WriteLine(url.AbsolutePath + " diff domain from " + currentUrl.AbsolutePath);
                    // Console.ForegroundColor = ConsoleColor.White;
                    continue;
                    
                }
                // Console.ForegroundColor = ConsoleColor.DarkBlue;
                // Console.WriteLine("adding " + url.AbsolutePath + "to queue");
                // Console.ForegroundColor = ConsoleColor.White;
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

            if(_repository.StoreArticle(articulo, currentUrl, resultId))
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
    private readonly Repository _repository;
    private const int user = 1;
}
