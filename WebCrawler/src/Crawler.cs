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

    public async Task Crawl(string startUrl)
    {
        if(_urls.Count == 0){
            Console.WriteLine("Empty Queue!");
            return;
        }


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
}
