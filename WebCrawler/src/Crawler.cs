class Crawler
{
    public Crawler()
    {
        _urls = new Queue<Uri>();
        _parser = new Parser();
        _visited = [];
        _repository = new Repository("server=localhost;user=root;database=world;port=3306;");
    }

    public Crawler(List<string> urlList) {
        _urls = new Queue<Uri>();
        foreach (var url in urlList)
        {
            _urls.Enqueue(new Uri(url));
        }
        _parser = new Parser();
        _visited = new HashSet<Uri>();
        _repository = new Repository("server=localhost;user=root;port=3306;");
    }

    public async Task Crawl()
    {
        if(_urls.Count == 0){
            Console.WriteLine("Empty Queue!");
            return;
        }

        var successfulConnection = _repository.ConnectToServer();
        if (!successfulConnection)
        {
            Console.WriteLine("Database connection unsuccessful...");
            return;
        }
        else
        {
            Console.WriteLine("connected to database successfully.");
        }
        _repository.StoreHtml("");

        while (_urls.Count != 0 && _visited.Count < _siteLimit)
        {
            var currentUrl = _urls.Dequeue();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"visited:{_visited.Count} Queue:{_urls.Count}");

            if (_visited.Contains(currentUrl)) continue;
            var success = await _parser.loadHTML(currentUrl);

            if (!success) continue;
            var scrapedLinks = _parser.ExtractUrls(_visited);
            var html = _parser.GetHtml();
            _visited.Add(currentUrl);


            foreach (var link in scrapedLinks)
            {
                _urls.Enqueue(link);
            }
        }
    }

    private const int _siteLimit = 10;

    private readonly Queue<Uri> _urls;
    private readonly HashSet<Uri> _visited;
    private readonly Parser _parser;
    private readonly Repository _repository;
}
