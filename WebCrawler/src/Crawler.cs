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

    public async Task Crawl()
    {
        if(_urls.Count == 0){
            Console.WriteLine("Empty Queue!");
            return;
        }

        while(_urls.Count != 0 && _visited.Count < _siteLimit){
            var currentUrl = _urls.Dequeue();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"visited:{_visited.Count} Queue:{_urls.Count}");

            if (_visited.Contains(currentUrl)) continue;
            var scrapedLinks = await _parser.Parse(currentUrl, _visited);
            _visited.Add(currentUrl);

            foreach (var link in scrapedLinks)
            {
                _urls.Enqueue(link);
            }
        }
    }

    private const int _siteLimit = 999;

    private readonly Queue<Uri> _urls;
    private readonly HashSet<Uri> _visited;
    private readonly Parser _parser;
}
